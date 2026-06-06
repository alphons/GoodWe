using GoodWe;

if (args.Length == 0)
{
	Console.WriteLine("Usage: GoodWe.Console <inverter-ip> [udp|tcp] [family]");
	Console.WriteLine("  family: ET (default), ES, DT");
	Console.WriteLine();
	Console.WriteLine("Example: GoodWe.Console 192.168.1.100");
	Console.WriteLine("         GoodWe.Console 192.168.1.100 udp DT");
	Console.WriteLine("         GoodWe.Console 192.168.1.100 tcp ET");
	return 1;
}

string host = args[0];
string transport = args.Length > 1 ? args[1].ToLower() : "udp";
string family = args.Length > 2 ? args[2] : "Unknown";

Console.WriteLine($"Connecting to {host} ({transport.ToUpper()}{(family != null ? $" / {family}" : "")}) ...");

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

try
{
	bool tcp = transport == "tcp";

	await using var inverter = await GoodWeClient.ConnectAsync(host,
		tcp: tcp, family: Enum.Parse<FamilyEnum>(family!), ct: cts.Token);

	if (inverter == null)
		return 1;

	// ── Device info ────────────────────────────────────────────────────────
	Console.WriteLine();
	PrintHeader("Device Info");
	PrintRow("Model", inverter.ModelName);
	PrintRow("Serial", inverter.SerialNumber);
	PrintRow("Rated Power", $"{inverter.RatedPower} W");
	PrintRow("Firmware", inverter.Firmware);
	PrintRow("ARM Firmware", inverter.ArmFirmware);

	// ── Runtime data ───────────────────────────────────────────────────────
	Console.WriteLine();
	PrintHeader("Runtime Data");
	var data = await inverter.ReadRuntimeDataAsync(cts.Token);
	PrintAllData(data);

	// ── Settings ───────────────────────────────────────────────────────────
	Console.WriteLine();
	PrintHeader("Settings");

	var opMode = await inverter.GetOperationModeAsync(cts.Token);
	PrintRow("Operation Mode", opMode.ToString());

	var dictSettings = await inverter.ReadSettingsDataAsync(CancellationToken.None);

	PrintRow("Grid Export Limit", $"{dictSettings["grid_export_limit"]}");
	PrintRow("Grid Export Limit Value", $"{dictSettings["grid_export_limit_value"]} %");

	Console.WriteLine();
	Console.WriteLine("Press any key to exit...");
	Console.ReadKey(intercept: true);
	return 0;
}
catch (MaxRetriesException)
{
	Console.Error.WriteLine($"Error: no response from {host} after retries. Check IP and network.");
	return 2;
}
catch (InverterError ex)
{
	Console.Error.WriteLine($"Inverter error: {ex.Message}");
	return 3;
}
catch (OperationCanceledException)
{
	Console.Error.WriteLine("Timed out.");
	return 4;
}

// ── helpers ──────────────────────────────────────────────────────────────────

static void PrintHeader(string title)
{
	Console.WriteLine($"=== {title} ===");
}

static void PrintRow(string label, string? value)
{
	Console.WriteLine($"  {label,-30}: {value ?? "n/a"}");
}

static void PrintAllData(Dictionary<string, object?> data)
{
	// Separate into measurements (have numeric/typed values) and labels/codes
	// Sort by key so output is predictable
	foreach (var kv in data.OrderBy(k => k.Key))
	{
		if (kv.Value is null) continue;

		string display = kv.Value switch
		{
			double d => $"{d:F2}",
			float f => $"{f:F2}",
			DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
			_ => kv.Value.ToString() ?? ""
		};

		Console.WriteLine($"  {kv.Key,-35}: {display}");
	}
}
