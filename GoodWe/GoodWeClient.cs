using System.Net;
using System.Net.Sockets;

namespace GoodWe;

public static class GoodWeClient
{
	public static async Task<Inverter?> ConnectAsync(string host,
		bool tcp = true,
		FamilyEnum family = FamilyEnum.Unknown,
		byte commAddr = 0,
		int timeout = 2,
		int retries = 3,
		CancellationToken ct = default)
	{
		if (tcp)
			return await ConnectTcpAsync(host, Constants.GoodWeTcpPort,
				family, commAddr, timeout, retries, ct);
		else
			return await ConnectUdpAsync(host, Constants.GoodWeUdpPort, 
				family, commAddr, timeout, retries, ct);
	}
	/// <summary>
	/// Connect to a GoodWe inverter. Auto-detects the inverter family.
	/// </summary>
	private static async Task<Inverter> ConnectUdpAsync(
		string host,
		int port = Constants.GoodWeUdpPort,
		FamilyEnum family = FamilyEnum.Unknown,
		byte commAddr = 0,
		int timeout = 2,
		int retries = 3,
		CancellationToken ct = default)
	{
		if (family != FamilyEnum.Unknown)
		{
			using CancellationTokenSource cts = new(1000);
			byte addr = commAddr != 0 ? commAddr : DefaultCommAddr(family);
			var proto = new UdpInverterProtocol(host, port, addr, timeout, retries);
			var inv = CreateInverter(family, proto);
			await inv.ReadDeviceInfoAsync(cts.Token);
			return inv;
		}

		return await DiscoverFamilyAsync(host, port, commAddr, timeout, retries, tcp: false);
	}

	/// <summary>
	/// Connect via Modbus/TCP (port 502).
	/// </summary>
	private static async Task<Inverter?> ConnectTcpAsync(
		string host,
		int port = Constants.GoodWeTcpPort,
		FamilyEnum family = FamilyEnum.Unknown,
		byte commAddr = 0x01,
		int timeout = 5,
		int retries = 3,
		CancellationToken ct = default)
	{
		try
		{
			if (family != FamilyEnum.Unknown)
			{
				using CancellationTokenSource cts = new(1000);
				byte addr = commAddr != 0x01 ? commAddr : DefaultCommAddr(family);
				var proto = new TcpInverterProtocol(host, port, addr, timeout, retries);
				var inv = CreateInverter(family, proto);
				await inv.ReadDeviceInfoAsync(cts.Token);
				return inv;
			}

			return await DiscoverFamilyAsync(host, port, commAddr, timeout, retries, tcp: true);
		}
		catch
		{
			return default;
		}
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
		string host, int port, byte commAddr, int timeout, int retries,
		bool tcp = false)
	{
		foreach (var family in new[] { FamilyEnum.ET, FamilyEnum.ES, FamilyEnum.DT })
		{
			byte addr = commAddr != 0 ? commAddr : DefaultCommAddr(family);
			InverterProtocol proto = tcp
				? new TcpInverterProtocol(host, port, addr, timeout, retries)
				: new UdpInverterProtocol(host, port, addr, timeout, retries);
			try
			{
				using CancellationTokenSource cts = new (1000);
				var inv = CreateInverter(family, proto);
				await inv.ReadDeviceInfoAsync(cts.Token);
				if (inv.ModelName!.Contains("Unknown"))
					throw new RequestFailedException("Unknown");
				return inv;
			}
			catch (Exception ex) when (ex is RequestFailedException or MaxRetriesException or OperationCanceledException)
			{
				await proto.DisposeAsync();
				// try next family
			}
		}
		throw new InverterError("Could not identify inverter family. Check the host and network connection.");
	}

	private static byte DefaultCommAddr(FamilyEnum family) =>
		family switch
		{
			FamilyEnum.DT or FamilyEnum.MS or FamilyEnum.XS
				 => DtInverter.CommAddr,  // 0x7F
			_ => 0xF7,                    // ET, ES
		};

	private static Inverter CreateInverter(FamilyEnum family, InverterProtocol protocol) =>
		family switch
		{
			FamilyEnum.ET or FamilyEnum.EH or FamilyEnum.BT or FamilyEnum.BH 
				=> new EtInverter(protocol),
			FamilyEnum.ES or FamilyEnum.EM or FamilyEnum.BP
				=> new EsInverter(protocol),
			FamilyEnum.DT or FamilyEnum.MS or FamilyEnum.XS
				=> new DtInverter(protocol),
			_ => throw new ArgumentException($"Unknown inverter family: {family}")
		};
}
