namespace TeltonikaDeviceParser.Teltonika
{
    public class DeviceData
    {
        public string IMEI { get; set; }

        public DateTime? Timestamp { get; set; }

        public byte? Priority { get; set; }

        public double? Long { get; set; }

        public double? Lat { get; set; }

        public short? Altitude { get; set; }

        public short? Direction { get; set; }

        public byte? Satellites { get; set; }

        public short? Speed { get; set; }

        public short? LocalAreaCode { get; set; }

        public short? CellID { get; set; }

        public byte? GsmSignalQuality { get; set; }

        public int? OperatorCode { get; set; }

        public byte Event_IO_element_ID { get; set; }

        public Dictionary<byte, byte> IO_Elements_1B { get; set; } = new();

        public Dictionary<byte, short> IO_Elements_2B { get; set; } = new();

        public Dictionary<byte, int> IO_Elements_4B { get; set; } = new();

        public Dictionary<byte, long> IO_Elements_8B { get; set; } = new();
    }
}
