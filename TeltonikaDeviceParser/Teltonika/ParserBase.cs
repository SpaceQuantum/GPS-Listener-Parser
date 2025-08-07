namespace TeltonikaDeviceParser.Teltonika
{
    public abstract class ParserBase
    {     
        public event Action<string> OnDataReceive;
        protected bool showDiagnosticInfo;

        protected void ShowDiagnosticInfo(string message)
        {
            if (showDiagnosticInfo)
                OnDataReceive.Invoke(message);
        }

        public abstract (int numberOfData, List<DeviceData>? deviceDataList) DecodeAVL(byte[] receiveBytes,
            string IMEI);
    }
}
