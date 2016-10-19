using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GPSParser.DBLogic;

namespace GPSParser.Teltonika
{
    public class TeltonikaDevicesParser
    {
        public event Action<string> OnDataReceive;
        private bool _showDiagnosticInfo;
        public TeltonikaDevicesParser(bool showDiagnosticInfo)
        {
            _showDiagnosticInfo = showDiagnosticInfo;
        }
        public void ShowDiagnosticInfo(string message)
        {
            if (_showDiagnosticInfo)
                OnDataReceive.Invoke(message);
        }
        public int Decode(List<byte> receiveBytes, string IMEI)
        {
            ParserBase parser = null;
            //Get codec ID and initialize appropriate parser
            var codecID = Convert.ToInt32(receiveBytes.Skip(8).Take(1).ToList()[0]);
            switch(codecID)
            {
                case 8:
                    parser = new FMXXXX_Parser(_showDiagnosticInfo);
                    break;
                case 7:
                    parser = new GH3000Parser(_showDiagnosticInfo);
                    break;
                default:
                    throw new Exception("Unsupported device type code: " + codecID);
            }
            parser.OnDataReceive += ShowDiagnosticInfo;
            int result = parser.DecodeAVL(receiveBytes, IMEI);
            parser.OnDataReceive -= ShowDiagnosticInfo;
            return result;
        }
    }
}
