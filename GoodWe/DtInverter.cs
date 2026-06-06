namespace GoodWe;

/// <summary>
/// DT/MS/D-NS/XS family — grid-only (no battery) 3-phase inverter.
/// comm_addr = 0x7F, runtime data at 0x7594 (30100), device info at 0x7531 (30001).
/// </summary>
public class DtInverter(InverterProtocol protocol) : Inverter(protocol)
{
	// 0x7531 = 30001, count 0x28 = 40
	private const ushort DevInfoOffset = 0x7531;
	private const ushort DevInfoCount = 0x28;

	// 0x7594 = 30100, count 0x49 = 73
	private const ushort RuntimeOffset = 0x7594;
	private const ushort RuntimeCount = 0x49;

	// 0x75F3 = 30195, count 0x10 = 16  (through 30210 leakage_current)
	private const ushort MeterOffset = 0x75F3;
	private const ushort MeterCount = 0x10;

	private static readonly Sensor[] RuntimeSensors =
	[
		new TimestampSensorDT("timestamp",            30100, "Timestamp"),
		new VoltageSensor("vpv1",                   30103, "PV1 Voltage",           SensorKind.PV),
		new CurrentSensor("ipv1",                   30104, "PV1 Current",           SensorKind.PV),
		new VoltageSensor("vpv2",                   30105, "PV2 Voltage",           SensorKind.PV),
		new CurrentSensor("ipv2",                   30106, "PV2 Current",           SensorKind.PV),
		new VoltageSensor("vpv3",                   30107, "PV3 Voltage",           SensorKind.PV),
		new CurrentSensor("ipv3",                   30108, "PV3 Current",           SensorKind.PV),
		new VoltageSensor("vpv4",                   30109, "PV4 Voltage",           SensorKind.PV),
		new CurrentSensor("ipv4",                   30110, "PV4 Current",           SensorKind.PV),
		new VoltageSensor("vline1",                 30115, "On-grid L1-L2 Voltage", SensorKind.AC),
		new VoltageSensor("vline2",                 30116, "On-grid L2-L3 Voltage", SensorKind.AC),
		new VoltageSensor("vline3",                 30117, "On-grid L3-L1 Voltage", SensorKind.AC),
		new VoltageSensor("vgrid1",                 30118, "On-grid L1 Voltage",    SensorKind.AC),
		new VoltageSensor("vgrid2",                 30119, "On-grid L2 Voltage",    SensorKind.AC),
		new VoltageSensor("vgrid3",                 30120, "On-grid L3 Voltage",    SensorKind.AC),
		new CurrentSensor("igrid1",                 30121, "On-grid L1 Current",    SensorKind.AC),
		new CurrentSensor("igrid2",                 30122, "On-grid L2 Current",    SensorKind.AC),
		new CurrentSensor("igrid3",                 30123, "On-grid L3 Current",    SensorKind.AC),
		new FrequencySensor("fgrid1",               30124, "On-grid L1 Frequency",  SensorKind.AC),
		new FrequencySensor("fgrid2",               30125, "On-grid L2 Frequency",  SensorKind.AC),
		new FrequencySensor("fgrid3",               30126, "On-grid L3 Frequency",  SensorKind.AC),
		new Power4Sensor("total_inverter_power",    30127, "Total Power",           SensorKind.AC),
		new IntegerSensor("work_mode",              30129, "Work Mode code"),
		new EnumSensor("work_mode_label",           30129, Constants.WorkModes, "Work Mode"),
		new LongSensor("error_codes",               30130, "Error Codes"),
		new BitmapSensor("errors",                  30130, Constants.ErrorCodes, "Errors"),
		new IntegerSensor("warning_code",           30132, "Warning code"),
		new PowerSignedSensor("total_input_power",  30137, "Total Input Power",     SensorKind.PV),
		new TempSensor("temperature",               30141, "Inverter Temperature",  SensorKind.AC),
		new TempSensor("temperature_heatsink",      30142, "Heatsink Temperature",  SensorKind.AC),
		new EnergySensor("e_day",                   30144, "Today's PV Generation", SensorKind.PV),
		new Energy4Sensor("e_total",                30145, "Total PV Generation",   SensorKind.PV),
		new LongSensor("h_total",                   30147, "Hours Total",           "h", SensorKind.PV),
		new IntegerSensor("safety_country",         30149, "Safety Country code",   "", SensorKind.AC),
		new EnumSensor("safety_country_label",      30149, Constants.SafetyCountries, "Safety Country", SensorKind.AC),
		new IntegerSensor("funbit",                 30162, "Function Bit",          "", SensorKind.PV),
		new VoltageSensor("vbus",                   30163, "Bus Voltage",           SensorKind.PV),
		new VoltageSensor("vnbus",                  30164, "NBus Voltage",          SensorKind.PV),
		new LongSensor("derating_mode",             30165, "Derating Mode code"),
		new BitmapSensor("derating_mode_label",     30165, Constants.DeratingModeCodes, "Derating Mode"),
		new IntegerSensor("rssi",                   30172, "RSSI"),
	];

	private static readonly Sensor[] MeterSensors =
	[
		new Power4SignedSensor("meter_active_power",    30195, "Meter Active Power",            SensorKind.GRID),
		new Energy4WSensor("meter_e_total_exp",         30197, "Meter Total Energy (export)",   SensorKind.GRID),
		new Energy4WSensor("meter_e_total_imp",         30199, "Meter Total Energy (import)",   SensorKind.GRID),
		new IntegerSensor("meter_comm_status",          30209, "Meter Comm Status code",        "", SensorKind.GRID),
		new EnumSensor("meter_comm_label",              30209, Constants.MeterCommStatus, "Meter Comm Status", SensorKind.GRID),
		new CurrentSmASensor("leakage_current",         30210, "Leakage Current",               SensorKind.PV),
	];

	public override async Task ReadDeviceInfoAsync(CancellationToken ct = default)
	{
		// Device info response data (raw payload after header strip):
		// [6:22]  = serial number (ASCII)
		// [22:32] = model name (ASCII)
		// [66:68] = DSP1 version
		// [68:70] = DSP2 version
		// [70:72] = ARM version
		var resp = await SendReadAsync(DevInfoOffset, DevInfoCount, ct);
		var data = resp.ResponseData;

		SerialNumber = DecodeAscii(data, 6, 16);
		ModelName = DecodeAscii(data, 22, 10);
		DspVersion = data.Length > 67 ? (data[66] << 8) | data[67] : 0;
		ArmVersion = data.Length > 71 ? (data[70] << 8) | data[71] : 0;
		Firmware = $"DSP:{DspVersion}";
		ArmFirmware = $"{ArmVersion}";
	}

	private static string DecodeAscii(byte[] data, int start, int length)
	{
		if (start >= data.Length) return string.Empty;
		int end = Math.Min(start + length, data.Length);
		return System.Text.Encoding.ASCII.GetString(data, start, end - start)
					 .Trim('\0', ' ');
	}

	public override async Task<Dictionary<string, object?>> ReadRuntimeDataAsync(CancellationToken ct = default)
	{
		var result = new Dictionary<string, object?>();

		var resp = await SendReadAsync(RuntimeOffset, RuntimeCount, ct);
		var raw = resp.ResponseData;

		foreach (var sensor in RuntimeSensors)
		{
			try { result[sensor.Id] = sensor.Read(resp); }
			catch { result[sensor.Id] = null; }
		}

		// Calculated: ppv = sum of V*I per string
		double ppv = 0;
		foreach (var (vReg, iReg) in new[] { (30103, 30104), (30105, 30106), (30107, 30108), (30109, 30110) })
		{
			double v = SensorHelper.PeekU16At(raw, vReg, RuntimeOffset) / 10.0;
			if (v > 6500)
				continue;
			double i = SensorHelper.PeekU16At(raw, iReg, RuntimeOffset) / 10.0;
			ppv += Math.Round(v * i);
		}
		result["ppv"] = (long)ppv;

		// Calculated grid powers
		foreach (var (vReg, iReg, key, name) in new[]
		{
			(30118, 30121, "pgrid1", "On-grid L1 Power"),
			(30119, 30122, "pgrid2", "On-grid L2 Power"),
			(30120, 30123, "pgrid3", "On-grid L3 Power"),
		})
		{
			double v = SensorHelper.PeekU16At(raw, vReg, RuntimeOffset) / 10.0;
			double i = SensorHelper.PeekU16At(raw, iReg, RuntimeOffset) / 10.0;
			result[key] = (long)Math.Round(v * i);
		}

		// Meter data
		try
		{
			var meterResp = await SendReadAsync(MeterOffset, MeterCount, ct);
			foreach (var sensor in MeterSensors)
			{
				try { result[sensor.Id] = sensor.Read(meterResp); }
				catch { result[sensor.Id] = null; }
			}
			if (result.TryGetValue("meter_active_power", out var map))
				result["house_consumption"] = Math.Abs((long)ppv - Convert.ToInt64(map ?? 0L));
		}
		catch (RequestFailedException) { /* meter is optional */ }

		return result;
	}

	public override async Task<Dictionary<string, object?>> ReadSettingsDataAsync(CancellationToken ct = default)
	{
		var result = new Dictionary<string, object?>();
		try
		{
			var resp = await SendReadAsync(40326, 20, ct);
			resp.Seek(40327);
			if (SensorHelper.ReadU16(resp) == 1)
				result["grid_export_limit"] = "Enabled";
			else
				result["grid_export_limit"] = "Disabled";
			result["grid_export_limit_value"] = SensorHelper.ReadU16(resp);
		}
		catch { /* optional */ }
		return result;
	}

	public override async Task<int> GetGridExportLimitAsync(CancellationToken ct = default)
	{
		var resp = await SendReadAsync(40328, 2, ct);
		resp.Seek(40328);
		return SensorHelper.ReadU16(resp);
	}

	public override Task SetGridExportLimitAsync(bool exportEnabled, CancellationToken ct = default) =>
		SendWriteAsync(40327, exportEnabled ? (ushort)1 : (ushort) 0, ct);

	public override Task SetGridExportLimitValueAsync(int exportLimitW, CancellationToken ct = default) =>
		SendWriteAsync(40328, (ushort)exportLimitW, ct);

	public override Task<OperationMode> GetOperationModeAsync(CancellationToken ct = default) =>
		Task.FromResult(OperationMode.General);

	public override Task SetOperationModeAsync(OperationMode mode, CancellationToken ct = default) =>
		Task.CompletedTask;

	public override Task<int> GetBatterySocAsync(CancellationToken ct = default) =>
		Task.FromResult(0);

	public const byte CommAddr = 0x7F;
}
