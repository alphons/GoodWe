namespace GoodWe;

/// <summary>
/// ES/EM/BP family (platform 105) single-phase hybrid inverter.
/// </summary>
public class EsInverter : Inverter
{
    private const ushort RuntimeOffset = 25700;
    private const ushort RuntimeCount = 125;

    private static readonly Sensor[] RuntimeSensors =
    [
        new TimestampSensor("timestamp", 25700, "Timestamp"),
        new VoltageSensor("vpv1", 25703, "PV1 Voltage", SensorKind.PV),
        new CurrentSensor("ipv1", 25704, "PV1 Current", SensorKind.PV),
        new Power4Sensor("ppv1", 25705, "PV1 Power", SensorKind.PV),
        new VoltageSensor("vpv2", 25707, "PV2 Voltage", SensorKind.PV),
        new CurrentSensor("ipv2", 25708, "PV2 Current", SensorKind.PV),
        new Power4Sensor("ppv2", 25709, "PV2 Power", SensorKind.PV),
        new VoltageSensor("vgrid", 25721, "On-grid Voltage", SensorKind.AC),
        new CurrentSensor("igrid", 25722, "On-grid Current", SensorKind.AC),
        new FrequencySensor("fgrid", 25723, "On-grid Frequency", SensorKind.AC),
        new PowerSignedSensor("pgrid", 25724, "On-grid Power", SensorKind.AC),
        new PowerSignedSensor("active_power", 25725, "Active Power", SensorKind.GRID),
        new VoltageSensor("vbattery1", 25740, "Battery Voltage", SensorKind.BAT),
        new CurrentSignedSensor("ibattery1", 25741, "Battery Current", SensorKind.BAT),
        new Power4SignedSensor("pbattery1", 25742, "Battery Power", SensorKind.BAT),
        new IntegerSensor("battery_mode", 25744, "Battery Mode code", "", SensorKind.BAT),
        new EnumSensor("battery_mode_label", 25744, Constants.BatteryModes, "Battery Mode", SensorKind.BAT),
        new TempSensor("temperature", 25752, "Inverter Temperature", SensorKind.AC),
        new IntegerSensor("battery_soc", 25754, "Battery State of Charge", "%", SensorKind.BAT),
        new Energy4Sensor("e_total", 25761, "Total PV Generation", SensorKind.PV),
        new Energy4Sensor("e_day", 25763, "Today's PV Generation", SensorKind.PV),
    ];

    public EsInverter(InverterProtocol protocol) : base(protocol) { }

    public override async Task ReadDeviceInfoAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await SendReadAsync(25000, 100, ct);
            var data = resp.ResponseData;
            SerialNumber = ReadAscii(data, 25001, 25000, 16);
            ModelName = ReadAscii(data, 25021, 25000, 16).Trim('\0', ' ');
            RatedPower = SensorHelper.PeekU16At(data, 25041, 25000);
            DspVersion = SensorHelper.PeekU16At(data, 25051, 25000);
            ArmVersion = SensorHelper.PeekU16At(data, 25056, 25000);
            Firmware = $"DSP:{DspVersion}";
            ArmFirmware = $"ARM:{ArmVersion}";
        }
        catch
        {
            ModelName ??= "Unknown ES";
            SerialNumber ??= "Unknown";
        }
    }

    private static string ReadAscii(byte[] data, int startReg, int baseReg, int charCount)
    {
        var chars = new char[charCount];
        for (int i = 0; i < charCount / 2; i++)
        {
            int off = (startReg + i - baseReg) * 2;
            if (off + 1 >= data.Length) break;
            chars[i * 2] = (char)data[off];
            chars[i * 2 + 1] = (char)data[off + 1];
        }
        return new string(chars).Trim('\0', ' ');
    }

    public override async Task<Dictionary<string, object?>> ReadRuntimeDataAsync(CancellationToken ct = default)
    {
        var result = new Dictionary<string, object?>();
        var resp = await SendReadAsync(RuntimeOffset, RuntimeCount, ct);
        foreach (var sensor in RuntimeSensors)
        {
            try { result[sensor.Id] = sensor.Read(resp); }
            catch { result[sensor.Id] = null; }
        }

        var raw = resp.ResponseData;
        result["ppv"] = Math.Max(0, SensorHelper.PeekS32At(raw, 25705, RuntimeOffset))
                      + Math.Max(0, SensorHelper.PeekS32At(raw, 25709, RuntimeOffset));
        return result;
    }

    public override async Task<Dictionary<string, object?>> ReadSettingsDataAsync(CancellationToken ct = default)
    {
        var result = new Dictionary<string, object?>();
        var resp = await SendReadAsync(25200, 30, ct);
        resp.Seek(25200);
        result["operation_mode"] = SensorHelper.ReadU16(resp);
        return result;
    }

    public override async Task<int> GetGridExportLimitAsync(CancellationToken ct = default)
    {
        var resp = await SendReadAsync(25025, 1, ct);
        resp.Seek(25025);
        return SensorHelper.ReadU16(resp);
    }

    public override Task SetGridExportLimitAsync(int exportLimitW, CancellationToken ct = default) =>
        SendWriteAsync(25025, (ushort)exportLimitW, ct);

    public override async Task<OperationMode> GetOperationModeAsync(CancellationToken ct = default)
    {
        var resp = await SendReadAsync(25200, 1, ct);
        resp.Seek(25200);
        return (OperationMode)SensorHelper.ReadU16(resp);
    }

    public override Task SetOperationModeAsync(OperationMode mode, CancellationToken ct = default) =>
        SendWriteAsync(25200, (ushort)mode, ct);

    public override async Task<int> GetBatterySocAsync(CancellationToken ct = default)
    {
        var resp = await SendReadAsync(25754, 1, ct);
        resp.Seek(25754);
        return SensorHelper.ReadU16(resp);
    }
}
