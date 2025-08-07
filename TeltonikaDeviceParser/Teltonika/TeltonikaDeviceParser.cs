namespace TeltonikaDeviceParser.Teltonika
{
    public class TeltonikaDeviceParser
    {
        public event Action<string>? OnDataReceive;
        private bool showDiagnosticInfo;
        public TeltonikaDeviceParser(bool showDiagnosticInfo)
        {
            this.showDiagnosticInfo = showDiagnosticInfo;
        }
        
        public void ShowDiagnosticInfo(string message)
        {
            if (showDiagnosticInfo)
            {
                OnDataReceive?.Invoke(message);
            }
        }
        
        public (int numberOfData, List<DeviceData>? deviceDataList) Decode(byte[] receiveBytes, string IMEI)
        {
            ParserBase parser;
            //Get codec ID and initialize appropriate parser
            var codecID = Convert.ToInt32(receiveBytes.Skip(8).Take(1).ToList()[0]);
            switch(codecID)
            {
                case 8:
                    parser = new FMXXXX_Parser(showDiagnosticInfo);
                    break;
                case 7:
                    parser = new GH3000Parser(showDiagnosticInfo);
                    break;
                default:
                    throw new Exception("Unsupported device type code: " + codecID);
            }
            parser.OnDataReceive += ShowDiagnosticInfo;
            (int numberOfData, List<DeviceData>? deviceDataList) result = parser.DecodeAVL(receiveBytes, IMEI);
            parser.OnDataReceive -= ShowDiagnosticInfo;
            return result;
        }
    }
}
