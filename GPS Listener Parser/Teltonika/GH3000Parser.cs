using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GPSParser.DBLogic;
using GPSParser.Utils;
using System.Globalization;

namespace GPSParser.Teltonika
{
    public class GH3000Parser : ParserBase
    {
        [Flags]
        public enum GlobalMask
        {
            GPSelement = 0x01,
            IO_Element_1B = 0x02,
            IO_Element_2B = 0x04,
            IO_Element_4B = 0x08
        }
        [Flags]
        public enum GPSmask
        {
            LatAndLong = 0x01,
            Altitude = 0x02,
            Angle = 0x04,
            Speed = 0x08,
            Sattelites = 0x10,
            LocalAreaCodeAndCellID = 0x20,
            SignalQuality = 0x40,
            OperatorCode = 0x80
        }

        public GH3000Parser(bool showDiagnosticInfo)
        {
            _showDiagnosticInfo = showDiagnosticInfo;
        }
        public override int DecodeAVL(List<byte> receiveBytes, string IMEI)
        {
            string hexDataLength = string.Empty;
            receiveBytes.Skip(4).Take(4).ToList().ForEach(delegate(byte b) { hexDataLength += String.Format("{0:X2}", b); });
            int dataLength = Convert.ToInt32(hexDataLength, 16);
            ShowDiagnosticInfo("Data Length: ----- " + dataLength);
            int codecId = Convert.ToInt32(receiveBytes.Skip(8).Take(1).ToList()[0]);
            ShowDiagnosticInfo("Codec ID: ----- " + codecId);
            int numberOfData = Convert.ToInt32(receiveBytes.Skip(9).Take(1).ToList()[0]); ;
            ShowDiagnosticInfo("Number of data: ---- " + numberOfData);

            int nextPacketStartAddress = 10;
            Data dt = new Data();

            for (int n = 0; n < numberOfData; n++)
            {
                string hexTimeStamp = string.Empty;
                receiveBytes.Skip(nextPacketStartAddress).Take(4).ToList().ForEach(delegate(byte b) { hexTimeStamp += String.Format("{0:X2}", b); });

                //ShowDiagnosticInfo(bit_30_timestamp);
                var result = Convert.ToInt64(hexTimeStamp, 16) & 0x3FFFFFFF;
                long timeSt = Convert.ToInt64(result);
                // long timeSt = Convert.ToInt64(Convert.ToString(Convert.ToInt32(hexTimeStamp, 16), 2).Substring(2, 30), 2);
                //long timeSt = Convert.ToInt64(hexTimeStamp.Substring(2, 30), 16);

                // For GH3000 time is seconds from 2007.01.01 00:00:00
                DateTime origin = new DateTime(2007, 1, 1, 0, 0, 0, 0);
                DateTime timestamp = origin.AddSeconds(Convert.ToDouble(timeSt));

                //DateTime timestamp = DateTime.FromBinary(timeSt);
                ShowDiagnosticInfo("Timestamp: ----- " + timestamp.ToLongDateString() + " " + timestamp.ToLongTimeString());

                int priority = (Convert.ToByte(hexTimeStamp.Substring(0, 2), 16) & 0xC0) / 64;//Convert.ToInt32(receiveBytes.Skip(nextPacketStartAddress + 8).Take(1));
                ShowDiagnosticInfo("Priority: ------------ " + priority);

                // If ALARM send SMS
                // if (priority == 2)
                //     SMSsender.SendSms(dt.GetAlarmNumberFromModemId(IMEI), "5555555", "Alarm button pressed", 3, true);

                GlobalMask globalMask = (GlobalMask)receiveBytes.Skip(nextPacketStartAddress + 4).Take(1).First();
                GPSmask gpsMask = (GPSmask)receiveBytes.Skip(nextPacketStartAddress + 5).Take(1).First();

                GPSdata gpsData = new GPSdata();
                gpsData.Priority = (byte)priority;
                gpsData.Timestamp = timestamp;
                int gpsElementDataAddress = 0;
                if ((globalMask & GH3000Parser.GlobalMask.GPSelement) != 0)
                {
                    if ((gpsMask & GH3000Parser.GPSmask.LatAndLong) != 0)
                    {
                        gpsElementDataAddress = 6;
                        string longt = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress + 4).Take(4).ToList().ForEach(delegate(byte b) { longt += String.Format("{0:X2}", b); });
                        float longtitude = GetFloatIEE754(receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress + 4).Take(4).ToArray());
                        ShowDiagnosticInfo("Longtitude: ----- " + longtitude);

                        string lat = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(4).ToList().ForEach(delegate(byte b) { lat += String.Format("{0:X2}", b); });
                        float latitude = GetFloatIEE754(receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(4).ToArray());
                        ShowDiagnosticInfo("Latitude: ----- " + latitude);
                        gpsElementDataAddress += 8;
                        gpsData.Lat = latitude;
                        gpsData.Long = longtitude;
                    }
                    if ((gpsMask & GH3000Parser.GPSmask.Altitude) != 0)
                    {
                        string alt = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(2).ToList().ForEach(delegate(byte b) { alt += String.Format("{0:X2}", b); });
                        int altitude = Convert.ToInt32(alt, 16);
                        ShowDiagnosticInfo("Altitude: ----- " + altitude);
                        gpsElementDataAddress += 2;
                        gpsData.Altitude = (short)altitude;
                    }

                    if ((gpsMask & GH3000Parser.GPSmask.Angle) != 0)
                    {
                        string ang = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(1).ToList().ForEach(delegate(byte b) { ang += String.Format("{0:X2}", b); });
                        int angle = Convert.ToInt32(ang, 16);
                        angle = Convert.ToInt32(angle * 360.0 / 256.0);
                        ShowDiagnosticInfo("Angle: ----- " + angle);
                        gpsElementDataAddress += 1;
                        gpsData.Direction = (short)angle;
                    }

                    if ((gpsMask & GH3000Parser.GPSmask.Speed) != 0)
                    {
                        string sp = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(1).ToList().ForEach(delegate(byte b) { sp += String.Format("{0:X2}", b); });
                        int speed = Convert.ToInt32(sp, 16);
                        ShowDiagnosticInfo("Speed: ----- " + speed);
                        gpsElementDataAddress += 1;
                        gpsData.Speed = (short)speed;
                    }

                    if ((gpsMask & GH3000Parser.GPSmask.Sattelites) != 0)
                    {
                        int satellites = Convert.ToInt32(receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(1).ToList()[0]);
                        ShowDiagnosticInfo("Satellites: ----- " + satellites);
                        gpsElementDataAddress += 1;
                        gpsData.Satellites = (byte)satellites;
                    }

                    if ((gpsMask & GH3000Parser.GPSmask.LocalAreaCodeAndCellID) != 0)
                    {
                        string localArea = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(2).ToList().ForEach(delegate(byte b) { localArea += String.Format("{0:X2}", b); });
                        int localAreaCode = Convert.ToInt32(localArea, 16);
                        ShowDiagnosticInfo("Local area code: ----- " + localAreaCode);
                        gpsElementDataAddress += 2;
                        gpsData.LocalAreaCode = (short)localAreaCode;

                        string cell_ID = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(2).ToList().ForEach(delegate(byte b) { cell_ID += String.Format("{0:X2}", b); });
                        int cellID = Convert.ToInt32(cell_ID, 16);
                        ShowDiagnosticInfo("Cell ID: ----- " + localAreaCode);
                        gpsElementDataAddress += 2;
                        gpsData.CellID = (short)cellID;
                    }

                    if ((gpsMask & GH3000Parser.GPSmask.SignalQuality) != 0)
                    {
                        string gsmQua = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(1).ToList().ForEach(delegate(byte b) { gsmQua += String.Format("{0:X2}", b); });
                        int gsmSignalQuality = Convert.ToInt32(gsmQua, 16);
                        ShowDiagnosticInfo("GSM signal quality: ----- " + gsmSignalQuality);
                        gpsElementDataAddress += 1;
                        gpsData.GsmSignalQuality = (byte)gsmSignalQuality;
                    }

                    if ((gpsMask & GH3000Parser.GPSmask.OperatorCode) != 0)
                    {
                        string opCode = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + gpsElementDataAddress).Take(4).ToList().ForEach(delegate(byte b) { opCode += String.Format("{0:X2}", b); });
                        int operatorCode = Convert.ToInt32(opCode, 16);
                        ShowDiagnosticInfo("Operator code: ----- " + operatorCode);
                        gpsElementDataAddress += 4;
                        gpsData.OperatorCode = operatorCode;
                    }
                }
                nextPacketStartAddress += gpsElementDataAddress;
                if ((globalMask & GH3000Parser.GlobalMask.IO_Element_1B) != 0)
                {
                    byte quantityOfIOelementData = receiveBytes.Skip(nextPacketStartAddress).Take(1).First();
                    nextPacketStartAddress += 1;
                    for (int i = 0; i < quantityOfIOelementData; i++)
                    {
                        byte parameterID = receiveBytes.Skip(nextPacketStartAddress).Take(1).First();
                        ShowDiagnosticInfo("IO element 1B ID: -----".PadRight(40, '-') + " " + parameterID);
                        byte parameterValue = receiveBytes.Skip(nextPacketStartAddress + 1).Take(1).First();
                        ShowDiagnosticInfo("IO element 1B value: -----".PadRight(40, '-') + " " + parameterValue);
                        gpsData.IO_Elements_1B.Add(parameterID, parameterValue);
                        nextPacketStartAddress += 2;

                        //--------------alarm send by sms
                        if (parameterID == 222)
                        {
                            string message = string.Empty;

                            //switch (parameterValue)
                            //{
                            //    case 5:
                            //        message = "Man-down sensor activated from " + IMEI + " at this place " + "http://maps.google.com/maps?q=" + gpsData.Lat.Value.ToString(CultureInfo.InvariantCulture) + "," + gpsData.Long.Value.ToString(CultureInfo.InvariantCulture);
                            //        SMSsender.SendSms(dt.GetAlarmNumberFromModemId(IMEI), "55555555", message, 3, true);
                            //        break;
                            //    case 1:
                            //        message = "Emergency button activated from " + IMEI + " at this place " + "http://maps.google.com/maps?q=" + gpsData.Lat.Value.ToString(CultureInfo.InvariantCulture) + "," + gpsData.Long.Value.ToString(CultureInfo.InvariantCulture);
                            //        SMSsender.SendSms(dt.GetAlarmNumberFromModemId(IMEI), "55555555", message, 3, true);
                            //        break;
                            //}

                        }

                        //------------------- end
                    }
                }

                if ((globalMask & GH3000Parser.GlobalMask.IO_Element_2B) != 0)
                {
                    byte quantityOfIOelementData = receiveBytes.Skip(nextPacketStartAddress).Take(1).First();
                    nextPacketStartAddress += 1;
                    for (int i = 0; i < quantityOfIOelementData; i++)
                    {
                        byte parameterID = receiveBytes.Skip(nextPacketStartAddress).Take(1).First();
                        ShowDiagnosticInfo("IO element 2B ID: -----".PadRight(40, '-') + " " + parameterID);
                        string value = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + 1).Take(2).ToList().ForEach(delegate(byte b) { value += String.Format("{0:X2}", b); });
                        short parameterValue = (short)Convert.ToInt32(value, 16);
                        ShowDiagnosticInfo("IO element 2B value: -----".PadRight(40, '-') + " " + parameterValue);
                        gpsData.IO_Elements_2B.Add(parameterID, parameterValue);
                        nextPacketStartAddress += 3;
                    }
                }
                if ((globalMask & GH3000Parser.GlobalMask.IO_Element_4B) != 0)
                {
                    byte quantityOfIOelementData = receiveBytes.Skip(nextPacketStartAddress).Take(1).First();
                    nextPacketStartAddress += 1;
                    for (int i = 0; i < quantityOfIOelementData; i++)
                    {
                        byte parameterID = receiveBytes.Skip(nextPacketStartAddress).Take(1).First();
                        ShowDiagnosticInfo("IO element 4B ID: -----".PadRight(40, '-') + " " + parameterID);
                        string value = string.Empty;
                        receiveBytes.Skip(nextPacketStartAddress + 1).Take(4).ToList().ForEach(delegate(byte b) { value += String.Format("{0:X2}", b); });
                        int parameterValue = Convert.ToInt32(value, 16);
                        ShowDiagnosticInfo("IO element 4B value: -----".PadRight(40, '-') + " " + parameterValue);
                        gpsData.IO_Elements_4B.Add(parameterID, parameterValue);
                        nextPacketStartAddress += 5;
                    }
                }

                gpsData.IMEI = IMEI.Substring(0, 15);
                dt.SaveGPSPositionGH3000(gpsData);

            }
            int numberOfData1 = Convert.ToInt32(receiveBytes.Skip(nextPacketStartAddress).Take(1).ToList()[0]);

            //CRC for check of data correction and request again data from device if it not correct
            string crcString = string.Empty;
            receiveBytes.Skip(dataLength + 8).Take(4).ToList().ForEach(delegate(byte b) { crcString += String.Format("{0:X2}", b); });
            int CRC = Convert.ToInt32(crcString, 16);
            ShowDiagnosticInfo("CRC: -----".PadRight(40, '-') + " " + CRC);
            //We must skeep first 8 bytes and last 4 bytes with CRC value.
            int calculatedCRC = GetCRC16(receiveBytes.Skip(8).Take(receiveBytes.Count - 12).ToArray());
            ShowDiagnosticInfo("Calculated CRC: -------".PadRight(40, '-') + " " + calculatedCRC);
            ShowDiagnosticInfo("||||||||||||||||||||||||||||||||||||||||||||||||");
            if (calculatedCRC == CRC)
                return numberOfData;
            else
            {
                ShowDiagnosticInfo("Incorect CRC ");
                return 0;
            }
        }
        public float GetFloatIEE754(byte[] array)
        {
            Array.Reverse(array);
            return BitConverter.ToSingle(array, 0);
        }
        private int GetCRC16(byte[] buffer)
        {
            return GetCRC16(buffer, buffer.Length, 0xA001);
        }
        private int GetCRC16(byte[] buffer, int bufLen, int polynom)
        {
            polynom &= 0xFFFF;
            int crc = 0;
            for (int i = 0; i < bufLen; i++)
            {
                int data = buffer[i] & 0xFF;
                crc ^= data;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc = (crc >> 1) ^ polynom;
                    }
                    else
                    {
                        crc = crc >> 1;
                    }
                }
            }
            return crc & 0xFFFF;
        }
    }
}
