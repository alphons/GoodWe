namespace GoodWe;

/// <summary>
/// ET/EH/BT/BH family (platform 205 / 745 / 753) hybrid battery inverter.
/// </summary>
public class EtInverter(InverterProtocol protocol) : Inverter(protocol)
{
	// 0x88B8 = 35000, count 0x21 = 33
	private const ushort DeviceInfoOffset = 0x88B8;
	private const ushort DeviceInfoCount = 0x21;

	// 0x891C = 35100, count 0x7D = 125
	private const ushort RuntimeOffset = 0x891C;
	private const ushort RuntimeCount = 0x7D;

	// 0x9088 = 37000, count 0x18 = 24
	private const ushort BatteryOffset = 0x9088;
	private const ushort BatteryCount = 0x18;

	private const ushort SettingsOffset = 47500;
	private const ushort SettingsCount = 60;

	private static readonly Sensor[] RuntimeSensors =
	[
		new TimestampSensorET("timestamp", 35100, "Timestamp"),
		new VoltageSensor("vpv1", 35103, "PV1 Voltage", SensorKind.PV),
		new CurrentSensor("ipv1", 35104, "PV1 Current", SensorKind.PV),
		new Power4Sensor("ppv1", 35105, "PV1 Power", SensorKind.PV),
		new VoltageSensor("vpv2", 35107, "PV2 Voltage", SensorKind.PV),
		new CurrentSensor("ipv2", 35108, "PV2 Current", SensorKind.PV),
		new Power4Sensor("ppv2", 35109, "PV2 Power", SensorKind.PV),
		new VoltageSensor("vpv3", 35111, "PV3 Voltage", SensorKind.PV),
		new CurrentSensor("ipv3", 35112, "PV3 Current", SensorKind.PV),
		new Power4Sensor("ppv3", 35113, "PV3 Power", SensorKind.PV),
		new VoltageSensor("vpv4", 35115, "PV4 Voltage", SensorKind.PV),
		new CurrentSensor("ipv4", 35116, "PV4 Current", SensorKind.PV),
		new Power4Sensor("ppv4", 35117, "PV4 Power", SensorKind.PV),
		new VoltageSensor("vgrid", 35121, "On-grid L1 Voltage", SensorKind.AC),
		new CurrentSensor("igrid", 35122, "On-grid L1 Current", SensorKind.AC),
		new FrequencySensor("fgrid", 35123, "On-grid L1 Frequency", SensorKind.AC),
		new PowerSignedSensor("pgrid", 35125, "On-grid L1 Power", SensorKind.AC),
		new VoltageSensor("vgrid2", 35126, "On-grid L2 Voltage", SensorKind.AC),
		new CurrentSensor("igrid2", 35127, "On-grid L2 Current", SensorKind.AC),
		new FrequencySensor("fgrid2", 35128, "On-grid L2 Frequency", SensorKind.AC),
		new PowerSignedSensor("pgrid2", 35130, "On-grid L2 Power", SensorKind.AC),
		new VoltageSensor("vgrid3", 35131, "On-grid L3 Voltage", SensorKind.AC),
		new CurrentSensor("igrid3", 35132, "On-grid L3 Current", SensorKind.AC),
		new FrequencySensor("fgrid3", 35133, "On-grid L3 Frequency", SensorKind.AC),
		new PowerSignedSensor("pgrid3", 35135, "On-grid L3 Power", SensorKind.AC),
		new IntegerSensor("grid_mode", 35136, "Grid Mode code", "", SensorKind.PV),
		new EnumSensor("grid_mode_label", 35136, Constants.GridModes, "Grid Mode", SensorKind.PV),
		new PowerSignedSensor("total_inverter_power", 35138, "Total Power", SensorKind.AC),
		new PowerSignedSensor("active_power", 35140, "Active Power", SensorKind.GRID),
		new VoltageSensor("backup_v1", 35145, "Back-up L1 Voltage", SensorKind.UPS),
		new CurrentSensor("backup_i1", 35146, "Back-up L1 Current", SensorKind.UPS),
		new FrequencySensor("backup_f1", 35147, "Back-up L1 Frequency", SensorKind.UPS),
		new PowerSignedSensor("backup_p1", 35150, "Back-up L1 Power", SensorKind.UPS),
		new PowerSignedSensor("load_ptotal", 35172, "Load", SensorKind.AC),
		new IntegerSensor("ups_load", 35173, "UPS Load", "%", SensorKind.UPS),
		new TempSensor("temperature_air", 35174, "Inverter Temperature (Air)", SensorKind.AC),
		new TempSensor("temperature", 35176, "Inverter Temperature (Radiator)", SensorKind.AC),
		new VoltageSensor("vbattery1", 35180, "Battery Voltage", SensorKind.BAT),
		new CurrentSignedSensor("ibattery1", 35181, "Battery Current", SensorKind.BAT),
		new Power4SignedSensor("pbattery1", 35182, "Battery Power", SensorKind.BAT),
		new IntegerSensor("battery_mode", 35184, "Battery Mode code", "", SensorKind.BAT),
		new EnumSensor("battery_mode_label", 35184, Constants.BatteryModes, "Battery Mode", SensorKind.BAT),
		new IntegerSensor("warning_code", 35185, "Warning code"),
		new IntegerSensor("safety_country", 35186, "Safety Country code", "", SensorKind.AC),
		new EnumSensor("safety_country_label", 35186, Constants.SafetyCountries, "Safety Country", SensorKind.AC),
		new IntegerSensor("work_mode", 35187, "Work Mode code"),
		new EnumSensor("work_mode_label", 35187, Constants.WorkModesET, "Work Mode"),
		new IntegerSensor("operation_mode", 35188, "Operation Mode code"),
		new LongSensor("error_codes", 35189, "Error Codes"),
		new BitmapSensor("errors", 35189, Constants.ErrorCodes, "Errors"),
		new Energy4Sensor("e_total", 35191, "Total PV Generation", SensorKind.PV),
		new Energy4Sensor("e_day", 35193, "Today's PV Generation", SensorKind.PV),
		new Energy4Sensor("e_total_exp", 35195, "Total Energy (export)", SensorKind.AC),
		new LongSensor("h_total", 35197, "Hours Total", "h", SensorKind.PV),
		new EnergySensor("e_day_exp", 35199, "Today Energy (export)", SensorKind.AC),
		new Energy4Sensor("e_total_imp", 35200, "Total Energy (import)", SensorKind.AC),
		new EnergySensor("e_day_imp", 35202, "Today Energy (import)", SensorKind.AC),
		new Energy4Sensor("e_load_total", 35203, "Total Load", SensorKind.AC),
		new EnergySensor("e_load_day", 35205, "Today Load", SensorKind.AC),
		new Energy4Sensor("e_bat_charge_total", 35206, "Total Battery Charge", SensorKind.BAT),
		new EnergySensor("e_bat_charge_day", 35208, "Today Battery Charge", SensorKind.BAT),
		new Energy4Sensor("e_bat_discharge_total", 35209, "Total Battery Discharge", SensorKind.BAT),
		new EnergySensor("e_bat_discharge_day", 35211, "Today Battery Discharge", SensorKind.BAT),
		new BitmapSensor("diagnose_result_label", 35220, Constants.DiagStatusCodes, "Diag Status"),
	];

	private static readonly Sensor[] BatterySensors =
	[
		new IntegerSensor("battery_bms", 37000, "Battery BMS", "", SensorKind.BAT),
		new IntegerSensor("battery_status", 37002, "Battery Status", "", SensorKind.BAT),
		new TempSensor("battery_temperature", 37003, "Battery Temperature", SensorKind.BAT),
		new IntegerSensor("battery_charge_limit", 37004, "Battery Charge Limit", "A", SensorKind.BAT),
		new IntegerSensor("battery_discharge_limit", 37005, "Battery Discharge Limit", "A", SensorKind.BAT),
		new IntegerSensor("battery_soc", 37007, "Battery State of Charge", "%", SensorKind.BAT),
		new IntegerSensor("battery_soh", 37008, "Battery State of Health", "%", SensorKind.BAT),
		new BitmapSensor("battery_error", 37012, Constants.BmsAlarmCodes, "Battery Error", SensorKind.BAT),
	];

	public override async Task ReadDeviceInfoAsync(CancellationToken ct = default)
	{
		// Response payload byte offsets (Python source: et.py read_device_info):
		//   [0:2]   modbus_version  (reg 35000)
		//   [2:4]   rated_power     (reg 35001)
		//   [4:6]   ac_output_type  (reg 35002)
		//   [6:22]  serial_number   (regs 35003-35010, 16 ASCII chars)
		//   [22:32] model_name      (regs 35011-35015, 10 ASCII chars)
		//   [32:34] dsp1_version    (reg 35016)
		//   [34:36] dsp2_version    (reg 35017)
		//   [36:38] dsp_svn_version (reg 35018)
		//   [38:40] arm_version     (reg 35019)
		var resp = await SendReadAsync(DeviceInfoOffset, DeviceInfoCount, ct);
		var d = resp.ResponseData;

		RatedPower = d.Length >= 4 ? (d[2] << 8) | d[3] : 0;
		SerialNumber = DecodeAscii(d, 6, 16);
		ModelName = DecodeAscii(d, 22, 10);
		int dsp1 = d.Length >= 34 ? (d[32] << 8) | d[33] : 0;
		int dsp2 = d.Length >= 36 ? (d[34] << 8) | d[35] : 0;
		int arm = d.Length >= 40 ? (d[38] << 8) | d[39] : 0;
		DspVersion = dsp1;
		ArmVersion = arm;
		Firmware = $"DSP:{dsp1}.{dsp2}";
		ArmFirmware = $"ARM:{arm:X4}";
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

		var runtimeResp = await SendReadAsync(RuntimeOffset, RuntimeCount, ct);
		ReadSensors(RuntimeSensors, runtimeResp, result);

		// Add calculated total PV power
		var raw = runtimeResp.ResponseData;
		long ppv = Math.Max(0, SensorHelper.PeekS32At(raw, 35105, RuntimeOffset))
				 + Math.Max(0, SensorHelper.PeekS32At(raw, 35109, RuntimeOffset))
				 + Math.Max(0, SensorHelper.PeekS32At(raw, 35113, RuntimeOffset))
				 + Math.Max(0, SensorHelper.PeekS32At(raw, 35117, RuntimeOffset));
		result["ppv"] = ppv;
		result["ppv_name"] = "PV Power";
		result["ppv_unit"] = "W";

		// House consumption = ppv + pbattery1 - active_power
		if (result.TryGetValue("pbattery1", out var bat) && result.TryGetValue("active_power", out var ap))
		{
			long pbat = Convert.ToInt64(bat ?? 0);
			long pact = Convert.ToInt64(ap ?? 0);
			result["house_consumption"] = ppv + pbat - pact;
		}

		// Battery sensors (separate register block)
		try
		{
			var batResp = await SendReadAsync(BatteryOffset, BatteryCount, ct);
			ReadSensors(BatterySensors, batResp, result);
		}
		catch (RequestFailedException) { /* battery block optional */ }

		return result;
	}

	private static void ReadSensors(Sensor[] sensors, ProtocolResponse resp, Dictionary<string, object?> result)
	{
		foreach (var sensor in sensors)
		{
			try { result[sensor.Id] = sensor.Read(resp); }
			catch { result[sensor.Id] = null; }
		}
	}

	public override async Task<Dictionary<string, object?>> ReadSettingsDataAsync(CancellationToken ct = default)
	{
		var result = new Dictionary<string, object?>();
		var resp = await SendReadAsync(SettingsOffset, SettingsCount, ct);
		// operation mode at 47500 offset 0
		resp.Seek(SettingsOffset);
		result["operation_mode"] = SensorHelper.ReadU16(resp);
		resp.Seek(47509);
		result["grid_export_limit"] = (int)SensorHelper.ReadU16(resp);
		resp.Seek(47515);
		result["battery_dod"] = (int)SensorHelper.ReadU16(resp);
		return result;
	}

	public override async Task<int> GetGridExportLimitAsync(CancellationToken ct = default)
	{
		var resp = await SendReadAsync(47509, 1, ct);
		resp.Seek(47509);
		return SensorHelper.ReadU16(resp);
	}

	public override async Task SetGridExportLimitAsync(int exportLimitW, CancellationToken ct = default)
	{
		await SendWriteAsync(47509, (ushort)exportLimitW, ct);
	}

	public override async Task<OperationMode> GetOperationModeAsync(CancellationToken ct = default)
	{
		var resp = await SendReadAsync(47500, 1, ct);
		resp.Seek(47500);
		return (OperationMode)SensorHelper.ReadU16(resp);
	}

	public override async Task SetOperationModeAsync(OperationMode mode, CancellationToken ct = default)
	{
		await SendWriteAsync(47500, (ushort)mode, ct);
	}

	public override async Task<int> GetBatterySocAsync(CancellationToken ct = default)
	{
		var resp = await SendReadAsync(37007, 1, ct);
		resp.Seek(37007);
		return SensorHelper.ReadU16(resp);
	}

	public static string[] ModelTags => ["ET", "EH", "BT", "BH", "GEH", "GE-", "HYD"];
}
