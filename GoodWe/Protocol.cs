using System.Net;
using System.Net.Sockets;

namespace GoodWe;

public class ProtocolResponse
{
    public byte[] RawData { get; }
    private readonly ProtocolCommand? _command;
    private int _position;
    private readonly byte[] _responseData;

    public ProtocolResponse(byte[] rawData, ProtocolCommand? command)
    {
        RawData = rawData;
        _command = command;
        _responseData = command?.TrimResponse(rawData) ?? rawData;
    }

    public void Seek(int address)
    {
        _position = _command?.GetOffset(address) ?? address;
    }

    public byte[] Read(int size)
    {
        var result = new byte[size];
        Array.Copy(_responseData, _position, result, 0, size);
        _position += size;
        return result;
    }

    public byte[] ResponseData => _responseData;
}

public abstract class ProtocolCommand
{
    protected byte[] Request { get; set; }
    public int FirstAddress { get; protected set; }
    public int Value { get; protected set; }
    public Func<byte[], bool> Validator { get; protected set; }

    protected ProtocolCommand(byte[] request, Func<byte[], bool> validator)
    {
        Request = request;
        Validator = validator;
    }

    public virtual byte[] RequestBytes() => Request;

    public abstract byte[] TrimResponse(byte[] raw);
    public abstract int GetOffset(int address);

    public override string ToString() => Convert.ToHexString(Request).ToLower();
}

public class ModbusRtuCommand : ProtocolCommand
{
    public ModbusRtuCommand(byte[] request, byte cmd, ushort offset, ushort value)
        : base(request, data => Modbus.ValidateRtuResponse(data, cmd, offset, value))
    {
        FirstAddress = offset;
        Value = value;
    }

    public override byte[] TrimResponse(byte[] raw) => raw[5..^2];
    public override int GetOffset(int address) => (address - FirstAddress) * 2;
}

public class ModbusRtuReadCommand : ModbusRtuCommand
{
    public ModbusRtuReadCommand(byte commAddr, ushort offset, ushort count)
        : base(Modbus.CreateRtuRequest(commAddr, Modbus.ReadCmd, offset, count), Modbus.ReadCmd, offset, count) { }
}

public class ModbusRtuWriteCommand : ModbusRtuCommand
{
    public ModbusRtuWriteCommand(byte commAddr, ushort register, ushort value)
        : base(Modbus.CreateRtuRequest(commAddr, Modbus.WriteCmd, register, value), Modbus.WriteCmd, register, value) { }
}

public class ModbusRtuWriteMultiCommand : ModbusRtuCommand
{
    public ModbusRtuWriteMultiCommand(byte commAddr, ushort offset, byte[] values)
        : base(Modbus.CreateRtuMultiRequest(commAddr, Modbus.WriteMultiCmd, offset, values),
               Modbus.WriteMultiCmd, offset, (ushort)(values.Length / 2)) { }
}

public class ModbusTcpCommand : ProtocolCommand
{
    private static int _tcpTx = 0;
    public ModbusTcpCommand(byte[] request, byte cmd, ushort offset, ushort value)
        : base(request, data => Modbus.ValidateTcpResponse(data, cmd, offset, value))
    {
        FirstAddress = offset;
        Value = value;
    }

    public override byte[] RequestBytes()
    {
        int tx = Interlocked.Increment(ref _tcpTx) & 0xFFFF;
        if (tx == 0) tx = 1;
        Request[0] = (byte)(tx >> 8);
        Request[1] = (byte)(tx & 0xFF);
        return Request;
    }

    public override byte[] TrimResponse(byte[] raw) => raw[9..];
    public override int GetOffset(int address) => (address - FirstAddress) * 2;
}

public class ModbusTcpReadCommand : ModbusTcpCommand
{
    public ModbusTcpReadCommand(byte commAddr, ushort offset, ushort count)
        : base(Modbus.CreateTcpRequest(commAddr, Modbus.ReadCmd, offset, count), Modbus.ReadCmd, offset, count) { }
}

public class ModbusTcpWriteCommand : ModbusTcpCommand
{
    public ModbusTcpWriteCommand(byte commAddr, ushort register, ushort value)
        : base(Modbus.CreateTcpRequest(commAddr, Modbus.WriteCmd, register, value), Modbus.WriteCmd, register, value) { }
}

public abstract class InverterProtocol : IAsyncDisposable
{
    protected readonly string Host;
    protected readonly int Port;
    protected readonly byte CommAddr;
    public int Timeout { get; set; }
    public int Retries { get; set; }

    protected InverterProtocol(string host, int port, byte commAddr, int timeout, int retries)
    {
        Host = host;
        Port = port;
        CommAddr = commAddr;
        Timeout = timeout;
        Retries = retries;
    }

    public abstract Task<ProtocolResponse> SendAsync(ProtocolCommand command, CancellationToken ct = default);
    public abstract ModbusRtuReadCommand ReadCommand(ushort offset, ushort count);
    public abstract ModbusRtuWriteCommand WriteCommand(ushort register, ushort value);

    public abstract ValueTask DisposeAsync();
}

public class UdpInverterProtocol : InverterProtocol
{
    private UdpClient? _udp;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public UdpInverterProtocol(string host, int port, byte commAddr, int timeout = 1, int retries = 3)
        : base(host, port, commAddr, timeout, retries) { }

    public override ModbusRtuReadCommand ReadCommand(ushort offset, ushort count) =>
        new(CommAddr, offset, count);

    public override ModbusRtuWriteCommand WriteCommand(ushort register, ushort value) =>
        new(CommAddr, register, value);

    private UdpClient GetOrCreateUdp()
    {
        if (_udp == null)
        {
            _udp = new UdpClient();
            _udp.Connect(Host, Port);
        }
        return _udp;
    }

    public override async Task<ProtocolResponse> SendAsync(ProtocolCommand command, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            byte[]? partialData = null;
            int partialMissing = 0;

            for (int attempt = 0; attempt <= Retries; attempt++)
            {
                var udp = GetOrCreateUdp();
                var payload = command.RequestBytes();
                await udp.SendAsync(payload, payload.Length);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(Timeout));

                try
                {
                    while (true)
                    {
                        var result = await udp.ReceiveAsync(cts.Token);
                        var data = result.Buffer;

                        if (partialData != null && partialMissing == data.Length)
                        {
                            var combined = new byte[partialData.Length + data.Length];
                            partialData.CopyTo(combined, 0);
                            data.CopyTo(combined, partialData.Length);
                            data = combined;
                            partialData = null;
                            partialMissing = 0;
                        }

                        try
                        {
                            if (command.Validator(data))
                                return new ProtocolResponse(data, command);
                        }
                        catch (PartialResponseException ex)
                        {
                            partialData = data;
                            partialMissing = ex.Expected - ex.Length;
                            cts.CancelAfter(TimeSpan.FromSeconds(Timeout));
                        }
                        catch (RequestRejectedException)
                        {
                            throw;
                        }
                    }
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    // timeout — retry
                }
            }

            throw new MaxRetriesException();
        }
        finally
        {
            _lock.Release();
        }
    }

    public override ValueTask DisposeAsync()
    {
        _udp?.Dispose();
        _udp = null;
        return ValueTask.CompletedTask;
    }
}

public class TcpInverterProtocol : InverterProtocol
{
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public TcpInverterProtocol(string host, int port, byte commAddr, int timeout = 5, int retries = 3)
        : base(host, port, commAddr, timeout, retries) { }

    public override ModbusRtuReadCommand ReadCommand(ushort offset, ushort count) =>
        new(CommAddr, offset, count);

    public override ModbusRtuWriteCommand WriteCommand(ushort register, ushort value) =>
        new(CommAddr, register, value);

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_tcp?.Connected == true) return;
        _tcp?.Dispose();
        _tcp = new TcpClient();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        await _tcp.ConnectAsync(Host, Port, cts.Token);
        _stream = _tcp.GetStream();
    }

    public override async Task<ProtocolResponse> SendAsync(ProtocolCommand command, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            for (int attempt = 0; attempt <= Retries; attempt++)
            {
                try
                {
                    await EnsureConnectedAsync(ct);
                    var payload = command.RequestBytes();
                    await _stream!.WriteAsync(payload, ct);

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(Timeout));

                    var buffer = new byte[1024];
                    byte[]? partialData = null;
                    int partialMissing = 0;

                    while (true)
                    {
                        int read = await _stream.ReadAsync(buffer, cts.Token);
                        if (read == 0) throw new RequestFailedException("Connection closed");

                        byte[] data = buffer[..read];
                        if (partialData != null && partialMissing == data.Length)
                        {
                            var combined = new byte[partialData.Length + data.Length];
                            partialData.CopyTo(combined, 0);
                            data.CopyTo(combined, partialData.Length);
                            data = combined;
                            partialData = null;
                        }

                        try
                        {
                            if (command.Validator(data))
                                return new ProtocolResponse(data, command);
                        }
                        catch (PartialResponseException ex)
                        {
                            partialData = data;
                            partialMissing = ex.Expected - ex.Length;
                            cts.CancelAfter(TimeSpan.FromSeconds(Timeout));
                        }
                    }
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    _tcp?.Dispose();
                    _tcp = null;
                }
                catch (IOException)
                {
                    _tcp?.Dispose();
                    _tcp = null;
                }
            }

            throw new MaxRetriesException();
        }
        finally
        {
            _lock.Release();
        }
    }

    public override ValueTask DisposeAsync()
    {
        _tcp?.Dispose();
        return ValueTask.CompletedTask;
    }
}
