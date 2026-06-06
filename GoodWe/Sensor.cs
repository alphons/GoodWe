namespace GoodWe;

public abstract class Sensor
{
	public string Id { get; }
	public int Offset { get; }
	public string Name { get; }
	public string Unit { get; }
	public SensorKind? Kind { get; }

	protected Sensor(string id, int offset, string name, string unit, SensorKind? kind)
	{
		Id = id;
		Offset = offset;
		Name = name;
		Unit = unit;
		Kind = kind;
	}

	public object? Read(ProtocolResponse data)
	{
		data.Seek(Offset);
		return ReadValue(data);
	}

	protected abstract object? ReadValue(ProtocolResponse data);
}

// ── helpers ──────────────────────────────────────────────────────────────────

public static class SensorHelper
{
	public static ushort ReadU16(ProtocolResponse d) =>
		(ushort)(d.Read(2) is var b ? (b[0] << 8) | b[1] : 0);

	public static short ReadS16(ProtocolResponse d) =>
		(short)ReadU16(d);

	public static uint ReadU32(ProtocolResponse d) =>
		(uint)(d.Read(4) is var b ? (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3] : 0);

	public static int ReadS32(ProtocolResponse d) =>
		(int)ReadU32(d);

	public static ulong ReadU64(ProtocolResponse d) =>
		(ulong)ReadU32(d) << 32 | ReadU32(d);

	// Read raw 2 bytes at absolute register address without moving the stream cursor
	public static ushort PeekU16At(byte[] responseData, int register, int firstAddress) =>
		(ushort)((responseData[(register - firstAddress) * 2] << 8) |
				  responseData[(register - firstAddress) * 2 + 1]);

	public static int PeekS16At(byte[] responseData, int register, int firstAddress) =>
		(short)PeekU16At(responseData, register, firstAddress);

	public static uint PeekU32At(byte[] responseData, int register, int firstAddress)
	{
		int off = (register - firstAddress) * 2;
		return (uint)((responseData[off] << 24) | (responseData[off + 1] << 16) |
					   (responseData[off + 2] << 8) | responseData[off + 3]);
	}

	public static int PeekS32At(byte[] responseData, int register, int firstAddress) =>
		(int)PeekU32At(responseData, register, firstAddress);
}

// ── concrete sensor types ─────────────────────────────────────────────────────

public class VoltageSensor : Sensor
{
	public VoltageSensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "V", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		 SensorHelper.ReadU16(d) is var a && a != 0xffff ? a / 10.0 : 0.0;
}

public class CurrentSensor : Sensor
{
	public CurrentSensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "A", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		SensorHelper.ReadU16(d) is var a && a != 0xffff ? a / 10.0 : 0.0;
}

public class CurrentSignedSensor : Sensor
{
	public CurrentSignedSensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "A", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		SensorHelper.ReadS16(d) / 10.0;
}

public class FrequencySensor : Sensor
{
	public FrequencySensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "Hz", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		SensorHelper.ReadU16(d) / 100.0;
}

public class PowerSensor : Sensor
{
	public PowerSensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "W", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		(int)SensorHelper.ReadU16(d);
}

public class PowerSignedSensor : Sensor
{
	public PowerSignedSensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "W", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		(int)SensorHelper.ReadS16(d);
}

public class Power4Sensor : Sensor
{
	public Power4Sensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "W", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		(long)SensorHelper.ReadU32(d);
}

public class Power4SignedSensor : Sensor
{
	public Power4SignedSensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "W", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		(long)SensorHelper.ReadS32(d);
}

public class EnergySensor : Sensor
{
	public EnergySensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "kWh", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		SensorHelper.ReadU16(d) / 10.0;
}

public class Energy4Sensor : Sensor
{
	public Energy4Sensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "kWh", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		SensorHelper.ReadU32(d) / 10.0;
}

// Energy4W: 4-byte unsigned, divide by 1000 (smart-meter kWh)
public class Energy4WSensor : Sensor
{
	public Energy4WSensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "kWh", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		SensorHelper.ReadU32(d) / 1000.0;
}

// CurrentSmA: 2-byte signed, value in mA → display in A
public class CurrentSmASensor : Sensor
{
	public CurrentSmASensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "A", kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		SensorHelper.ReadS16(d) / 1000.0;
}

public class TempSensor : Sensor
{
	public TempSensor(string id, int offset, string name, SensorKind? kind = null)
		: base(id, offset, name, "°C", kind) { }
	protected override object? ReadValue(ProtocolResponse d)
	{
		short raw = SensorHelper.ReadS16(d);
		return raw is -1 or short.MinValue ? null : (object)(raw / 10.0);
	}
}

public class IntegerSensor : Sensor
{
	public IntegerSensor(string id, int offset, string name, string unit = "", SensorKind? kind = null)
		: base(id, offset, name, unit, kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		(int)SensorHelper.ReadU16(d);

	//SensorHelper.ReadU16(d) is var a && a == 0xffff ? "" : $"{(int)a}";
}

public class IntegerSignedSensor : Sensor
{
	public IntegerSignedSensor(string id, int offset, string name, string unit = "", SensorKind? kind = null)
		: base(id, offset, name, unit, kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		(int)SensorHelper.ReadS16(d);
}

public class LongSensor : Sensor
{
	public LongSensor(string id, int offset, string name, string unit = "", SensorKind? kind = null)
		: base(id, offset, name, unit, kind) { }
	protected override object? ReadValue(ProtocolResponse d) =>
		(long)SensorHelper.ReadU32(d);
}

public class EnumSensor : Sensor
{
	private readonly IReadOnlyDictionary<int, string> _labels;
	public EnumSensor(string id, int offset, IReadOnlyDictionary<int, string> labels, string name, SensorKind? kind = null)
		: base(id, offset, name, "", kind) => _labels = labels;
	protected override object? ReadValue(ProtocolResponse d)
	{
		int v = SensorHelper.ReadU16(d);
		return _labels.TryGetValue(v, out var label) ? label : $"Unknown({v})";
	}
}

public class TimestampSensor : Sensor
{
	public TimestampSensor(string id, int offset, string name)
		: base(id, offset, name, "", null) { }
	protected override object? ReadValue(ProtocolResponse d)
	{
		var b = d.Read(12);
		try
		{
			return new DateTime(2000 + b[0], b[1], b[2], b[4], b[6], b[8]);
		}
		catch { return null; }
	}
}

// Bitmap sensor — decodes bit flags to comma-separated label string
public class BitmapSensor : Sensor
{
	private readonly IReadOnlyDictionary<int, string> _labels;
	public BitmapSensor(string id, int offset, IReadOnlyDictionary<int, string> labels, string name, SensorKind? kind = null)
		: base(id, offset, name, "", kind) => _labels = labels;
	protected override object? ReadValue(ProtocolResponse d)
	{
		uint val = SensorHelper.ReadU32(d);
		var flags = new List<string>();
		for (int bit = 0; bit < 32; bit++)
			if ((val & (1u << bit)) != 0 && _labels.TryGetValue(bit, out var lbl))
				flags.Add(lbl);
		return flags.Count > 0 ? string.Join(", ", flags) : "OK";
	}
}

// Calculated sensor with a delegate
public class CalculatedSensor : Sensor
{
	private readonly Func<byte[], int, object?> _getter;
	private readonly int _firstAddress;

	public CalculatedSensor(string id, Func<byte[], int, object?> getter, string name, string unit, SensorKind? kind, int firstAddress)
		: base(id, 0, name, unit, kind)
	{
		_getter = getter;
		_firstAddress = firstAddress;
	}

	public object? Compute(byte[] responseData) => _getter(responseData, _firstAddress);
	protected override object? ReadValue(ProtocolResponse d) => null; // not used directly
}
