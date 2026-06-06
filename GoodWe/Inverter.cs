namespace GoodWe;

public abstract class Inverter : IAsyncDisposable
{
    protected readonly InverterProtocol Protocol;

    public string? ModelName { get; protected set; }
    public string? SerialNumber { get; protected set; }
    public int RatedPower { get; protected set; }
    public string? Firmware { get; protected set; }
    public string? ArmFirmware { get; protected set; }
    public int DspVersion { get; protected set; }
    public int ArmVersion { get; protected set; }

    protected Inverter(InverterProtocol protocol)
    {
        Protocol = protocol;
    }

    public abstract Task ReadDeviceInfoAsync(CancellationToken ct = default);
    public abstract Task<Dictionary<string, object?>> ReadRuntimeDataAsync(CancellationToken ct = default);
    public abstract Task<Dictionary<string, object?>> ReadSettingsDataAsync(CancellationToken ct = default);
    public abstract Task<int> GetGridExportLimitAsync(CancellationToken ct = default);
    public abstract Task SetGridExportLimitAsync(int exportLimitW, CancellationToken ct = default);
    public abstract Task<OperationMode> GetOperationModeAsync(CancellationToken ct = default);
    public abstract Task SetOperationModeAsync(OperationMode mode, CancellationToken ct = default);
    public abstract Task<int> GetBatterySocAsync(CancellationToken ct = default);

    protected async Task<ProtocolResponse> SendReadAsync(ushort offset, ushort count, CancellationToken ct)
    {
        var cmd = Protocol.ReadCommand(offset, count);
        return await Protocol.SendAsync(cmd, ct);
    }

    protected async Task SendWriteAsync(ushort register, ushort value, CancellationToken ct)
    {
        var cmd = Protocol.WriteCommand(register, value);
        await Protocol.SendAsync(cmd, ct);
    }

    public ValueTask DisposeAsync() => Protocol.DisposeAsync();
}
