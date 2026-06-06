using System.Net;
using System.Net.Sockets;

namespace GoodWe;

public static class GoodWeClient
{
    /// <summary>
    /// Connect to a GoodWe inverter. Auto-detects the inverter family.
    /// </summary>
    public static async Task<Inverter> ConnectAsync(
        string host,
        int port = Constants.GoodWeUdpPort,
        string? family = null,
        byte commAddr = 0,
        int timeout = 2,
        int retries = 3,
        CancellationToken ct = default)
    {
        if (family != null)
        {
            byte addr = commAddr != 0 ? commAddr : DefaultCommAddr(family);
            var proto = new UdpInverterProtocol(host, port, addr, timeout, retries);
            var inv = CreateInverter(family, proto);
            await inv.ReadDeviceInfoAsync(ct);
            return inv;
        }

        return await DiscoverFamilyAsync(host, port, commAddr, timeout, retries, ct);
    }

    /// <summary>
    /// Connect via Modbus/TCP (port 502).
    /// </summary>
    public static async Task<Inverter> ConnectTcpAsync(
        string host,
        int port = Constants.GoodWeTcpPort,
        string? family = null,
        byte commAddr = 0x01,
        int timeout = 5,
        int retries = 3,
        CancellationToken ct = default)
    {
        if (family != null)
        {
            byte addr = commAddr != 0x01 ? commAddr : DefaultCommAddr(family);
            var proto = new TcpInverterProtocol(host, port, addr, timeout, retries);
            var inv = CreateInverter(family, proto);
            await inv.ReadDeviceInfoAsync(ct);
            return inv;
        }

        var protocol = new TcpInverterProtocol(host, port, commAddr, timeout, retries);
        return await DiscoverFamilyAsync(host, port, commAddr, timeout, retries, ct, tcp: true);
    }

    /// <summary>
    /// Broadcast scan to find inverters on the local network.
    /// Returns the IP address of the first responding inverter, or null.
    /// </summary>
    public static async Task<string?> SearchInvertersAsync(int timeoutMs = 3000, CancellationToken ct = default)
    {
        using var udp = new UdpClient();
        udp.EnableBroadcast = true;
        udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        // GoodWe discovery packet
        var payload = Convert.FromHexString("F7030001000121e3");
        await udp.SendAsync(payload, payload.Length, new IPEndPoint(IPAddress.Broadcast, Constants.GoodWeUdpPort));

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeoutMs);

        try
        {
            var result = await udp.ReceiveAsync(cts.Token);
            return result.RemoteEndPoint.Address.ToString();
        }
        catch (OperationCanceledException) { return null; }
    }

    private static async Task<Inverter> DiscoverFamilyAsync(
        string host, int port, byte commAddr, int timeout, int retries, CancellationToken ct,
        bool tcp = false)
    {
        foreach (var family in new[] { "ET", "ES", "DT" })
        {
            byte addr = commAddr != 0 ? commAddr : DefaultCommAddr(family);
            InverterProtocol proto = tcp
                ? new TcpInverterProtocol(host, port, addr, timeout, retries)
                : new UdpInverterProtocol(host, port, addr, timeout, retries);
            try
            {
                var inv = CreateInverter(family, proto);
                await inv.ReadDeviceInfoAsync(ct);
                return inv;
            }
            catch (Exception ex) when (ex is RequestFailedException or MaxRetriesException)
            {
                await proto.DisposeAsync();
                // try next family
            }
        }
        throw new InverterError("Could not identify inverter family. Check the host and network connection.");
    }

    private static byte DefaultCommAddr(string family) =>
        family.ToUpperInvariant() switch
        {
            "DT" or "MS" or "XS" => DtInverter.CommAddr,  // 0x7F
            _ => 0xF7,                                      // ET, ES
        };

    private static Inverter CreateInverter(string family, InverterProtocol protocol) =>
        family.ToUpperInvariant() switch
        {
            "ET" or "EH" or "BT" or "BH" => new EtInverter(protocol),
            "ES" or "EM" or "BP" => new EsInverter(protocol),
            "DT" or "MS" or "XS" => new DtInverter(protocol),
            _ => throw new ArgumentException($"Unknown inverter family: {family}")
        };
}
