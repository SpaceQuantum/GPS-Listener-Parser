using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPSParser.DBLogic
{
    public class GPSdata
    {

        private int _ID;
        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        private string _IMEI;
        public string IMEI
        {
            get { return _IMEI; }
            set { _IMEI = value; }
        }

        private System.Nullable<System.DateTime> _timestamp;
        public System.Nullable<System.DateTime> Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        private System.Nullable<byte> _priority;
        public System.Nullable<byte> Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        private System.Nullable<double> _long;
        public System.Nullable<double> Long
        {
            get { return _long; }
            set { _long = value; }
        }

        private System.Nullable<double> _lat;
        public System.Nullable<double> Lat
        {
            get { return _lat; }
            set { _lat = value; }
        }

        private System.Nullable<short> _altitude;
        public System.Nullable<short> Altitude
        {
            get { return _altitude; }
            set { _altitude = value; }
        }

        private System.Nullable<short> _direction;
        public System.Nullable<short> Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        private System.Nullable<byte> _satellites;
        public System.Nullable<byte> Satellites
        {
            get { return _satellites; }
            set { _satellites = value; }
        }

        private System.Nullable<short> _speed;
        public System.Nullable<short> Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        private System.Nullable<short> _localAreaCode;
        public System.Nullable<short> LocalAreaCode
        {
            get { return _localAreaCode; }
            set { _localAreaCode = value; }
        }

        private System.Nullable<short> _cellID;
        public System.Nullable<short> CellID
        {
            get { return _cellID; }
            set { _cellID = value; }
        }

        private System.Nullable<byte> _gsmSignalQuality;
        public System.Nullable<byte> GsmSignalQuality
        {
            get { return _gsmSignalQuality; }
            set { _gsmSignalQuality = value; }
        }

        private System.Nullable<int> _operatorCode;
        public System.Nullable<int> OperatorCode
        {
            get { return _operatorCode; }
            set { _operatorCode = value; }
        }

        private byte event_IO_element_ID;
        public byte Event_IO_element_ID
        {
            get { return event_IO_element_ID; }
            set { event_IO_element_ID = value; }
        }

        private Dictionary<byte, byte> _IO_Elements_1B = new Dictionary<byte,byte>();
        public Dictionary<byte, byte> IO_Elements_1B
        {
            get { return _IO_Elements_1B; }
            set { _IO_Elements_1B = value; }
        }

        private Dictionary<byte, short> _IO_Elements_2B = new Dictionary<byte,short>();
        public Dictionary<byte, short> IO_Elements_2B
        {
            get { return _IO_Elements_2B; }
            set { _IO_Elements_2B = value; }
        }

        private Dictionary<byte, int> _IO_Elements_4B = new Dictionary<byte,int>();
        public Dictionary<byte, int> IO_Elements_4B
        {
            get { return _IO_Elements_4B; }
            set { _IO_Elements_4B = value; }
        }

        private Dictionary<byte, long> _IO_Elements_8B = new Dictionary<byte,long>();
        public Dictionary<byte, long> IO_Elements_8B
        {
            get { return _IO_Elements_8B; }
            set { _IO_Elements_8B = value; }
        }
    }
}
