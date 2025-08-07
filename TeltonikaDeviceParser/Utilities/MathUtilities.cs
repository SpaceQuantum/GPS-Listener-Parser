namespace TeltonikaDeviceParser.Utilities;

public class MathUtilities
{
    public static float GetFloatIEE754(byte[] array)
    {
        Array.Reverse(array);
        return BitConverter.ToSingle(array, 0);
    }
        
    public static int GetCRC16(ReadOnlySpan<byte> buffer)
    {
        return GetCRC16(buffer, 0xA001);
    }

    private static int GetCRC16(ReadOnlySpan<byte> buffer, int polynom)
    {
        polynom &= 0xFFFF;
        int crc = 0;
        foreach (var b in buffer)
        {
            int data = b & 0xFF;
            crc ^= data;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (crc >> 1) ^ polynom;
                else
                    crc >>= 1;
            }
        }
        return crc & 0xFFFF;
    }
}