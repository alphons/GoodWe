namespace GoodWe;

public static class Modbus
{
    public const byte ReadCmd = 0x03;
    public const byte WriteCmd = 0x06;
    public const byte WriteMultiCmd = 0x10;

    private static readonly ushort[] Crc16Table = BuildCrc16Table();

    private static ushort[] BuildCrc16Table()
    {
        var table = new ushort[256];
        for (int i = 0; i < 256; i++)
        {
            int buffer = i << 1;
            ushort crc = 0;
            for (int j = 8; j > 0; j--)
            {
                buffer >>= 1;
                if (((buffer ^ crc) & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
            table[i] = crc;
        }
        return table;
    }

    public static ushort Checksum(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;
        foreach (byte b in data)
            crc = (ushort)((crc >> 8) ^ Crc16Table[(crc ^ b) & 0xFF]);
        return crc;
    }

    public static byte[] CreateRtuRequest(byte commAddr, byte cmd, ushort offset, ushort value)
    {
        var data = new byte[8];
        data[0] = commAddr;
        data[1] = cmd;
        data[2] = (byte)(offset >> 8);
        data[3] = (byte)(offset & 0xFF);
        data[4] = (byte)(value >> 8);
        data[5] = (byte)(value & 0xFF);
        ushort checksum = Checksum(data.AsSpan(0, 6));
        data[6] = (byte)(checksum & 0xFF);
        data[7] = (byte)(checksum >> 8);
        return data;
    }

    public static byte[] CreateRtuMultiRequest(byte commAddr, byte cmd, ushort offset, byte[] values)
    {
        var data = new byte[7 + values.Length + 2];
        data[0] = commAddr;
        data[1] = cmd;
        data[2] = (byte)(offset >> 8);
        data[3] = (byte)(offset & 0xFF);
        data[4] = 0;
        data[5] = (byte)(values.Length / 2);
        data[6] = (byte)values.Length;
        values.CopyTo(data, 7);
        ushort checksum = Checksum(data.AsSpan(0, 7 + values.Length));
        data[7 + values.Length] = (byte)(checksum & 0xFF);
        data[8 + values.Length] = (byte)(checksum >> 8);
        return data;
    }

    public static byte[] CreateTcpRequest(byte commAddr, byte cmd, ushort offset, ushort value)
    {
        var data = new byte[12];
        data[0] = 0; data[1] = 1; // transaction id (updated per-request)
        data[2] = 0; data[3] = 0; // protocol id
        data[4] = 0; data[5] = 6; // length
        data[6] = commAddr;
        data[7] = cmd;
        data[8] = (byte)(offset >> 8);
        data[9] = (byte)(offset & 0xFF);
        data[10] = (byte)(value >> 8);
        data[11] = (byte)(value & 0xFF);
        return data;
    }

    public static byte[] CreateTcpMultiRequest(byte commAddr, byte cmd, ushort offset, byte[] values)
    {
        var data = new byte[13 + values.Length];
        data[0] = 0; data[1] = 1;
        data[2] = 0; data[3] = 0;
        data[4] = 0; data[5] = (byte)(7 + values.Length);
        data[6] = commAddr;
        data[7] = cmd;
        data[8] = (byte)(offset >> 8);
        data[9] = (byte)(offset & 0xFF);
        data[10] = 0;
        data[11] = (byte)(values.Length / 2);
        data[12] = (byte)values.Length;
        values.CopyTo(data, 13);
        return data;
    }

    private static readonly IReadOnlyDictionary<byte, string> FailureCodes = new Dictionary<byte, string>
    {
        { 1, "ILLEGAL FUNCTION" }, { 2, "ILLEGAL DATA ADDRESS" }, { 3, "ILLEGAL DATA VALUE" },
        { 4, "SLAVE DEVICE FAILURE" }, { 6, "SLAVE DEVICE BUSY" }, { 11, "GATEWAY TARGET FAILED" },
    };

    public static bool ValidateRtuResponse(byte[] data, byte cmd, ushort offset, ushort value)
    {
        if (data.Length <= 4) return false;

        if (data[3] == ReadCmd)
        {
            if (data[4] != value * 2) return false;
            int expected = data[4] + 7;
            if (data.Length < expected)
                throw new PartialResponseException(data.Length, expected);
        }
        else if (data[3] == WriteCmd || data[3] == WriteMultiCmd)
        {
            if (data.Length < 10) return false;
            ushort respOffset = (ushort)((data[4] << 8) | data[5]);
            if (respOffset != offset) return false;
            short respValue = (short)((data[6] << 8) | data[7]);
            if (respValue != (short)value) return false;
        }

        int checksumOffset = data.Length - 2;
        ushort calcCrc = Checksum(data.AsSpan(2, checksumOffset - 2));
        ushort respCrc = (ushort)((data[checksumOffset + 1] << 8) | data[checksumOffset]);
        if (calcCrc != respCrc) return false;

        if (data[3] != cmd)
        {
            string msg = FailureCodes.TryGetValue(data[4], out var m) ? m : "UNKNOWN";
            throw new RequestRejectedException(msg);
        }

        return true;
    }

    public static bool ValidateTcpResponse(byte[] data, byte cmd, ushort offset, ushort value)
    {
        if (data.Length <= 8) return false;

        if (data[7] == ReadCmd)
        {
            int expected = data[8] + 9;
            if (data.Length < expected)
                throw new PartialResponseException(data.Length, expected);
            if (data[8] != value * 2) return false;
        }
        else if (data[7] == WriteCmd || data[7] == WriteMultiCmd)
        {
            if (data.Length < 12) return false;
            ushort respOffset = (ushort)((data[8] << 8) | data[9]);
            if (respOffset != offset) return false;
            short respValue = (short)((data[10] << 8) | data[11]);
            if (respValue != (short)value) return false;
        }

        if (data[7] != cmd)
        {
            string msg = FailureCodes.TryGetValue(data[8], out var m) ? m : "UNKNOWN";
            throw new RequestRejectedException(msg);
        }

        return true;
    }
}
