using TeltonikaDeviceParser.Utilities;

namespace TeltonikaDeviceParser.Teltonika
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
            this.showDiagnosticInfo = showDiagnosticInfo;
        }
        public override (int numberOfData, List<DeviceData>? deviceDataList) DecodeAVL(byte[] receiveBytes, string IMEI)
        {
            string hexDataLength = string.Empty;
            receiveBytes.Skip(4).Take(4).ToList().ForEach(delegate(byte b) { hexDataLength += String.Format("{0:X2}", b); });
            int dataLength = Convert.ToInt32(hexDataLength, 16);
            ShowDiagnosticInfo("Data Length: ----- " + dataLength);
            int codecId = Convert.ToInt32(receiveBytes.Skip(8).Take(1).ToList()[0]);
            ShowDiagnosticInfo("Codec ID: ----- " + codecId);
            int numberOfData = Convert.ToInt32(receiveBytes.Skip(9).Take(1).ToList()[0]);
            ShowDiagnosticInfo("Number of data: ---- " + numberOfData);

            List<DeviceData> deviceDataList = [];
            int tokenAddress = 10;

            for (int n = 0; n < numberOfData; n++)
            {
                string hexTimeStamp = string.Empty;
                receiveBytes.Skip(tokenAddress).Take(4).ToList().ForEach(delegate(byte b) { hexTimeStamp += String.Format("{0:X2}", b); });

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

                GlobalMask globalMask = (GlobalMask)receiveBytes.Skip(tokenAddress + 4).Take(1).First();
                GPSmask gpsMask = (GPSmask)receiveBytes.Skip(tokenAddress + 5).Take(1).First();

                DeviceData deviceData = new DeviceData();
                deviceData.Priority = (byte)priority;
                deviceData.Timestamp = timestamp;
                int gpsElementDataAddress = 0;
                if ((globalMask & GlobalMask.GPSelement) != 0)
                {
                    if ((gpsMask & GPSmask.LatAndLong) != 0)
                    {
                        gpsElementDataAddress = 6;
                        string longt = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress + 4).Take(4).ToList().ForEach(delegate(byte b) { longt += String.Format("{0:X2}", b); });
                        float longtitude = MathUtilities.GetFloatIEE754(receiveBytes.Skip(tokenAddress + gpsElementDataAddress + 4).Take(4).ToArray());
                        ShowDiagnosticInfo("Longtitude: ----- " + longtitude);

                        string lat = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(4).ToList().ForEach(delegate(byte b) { lat += String.Format("{0:X2}", b); });
                        float latitude = MathUtilities.GetFloatIEE754(receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(4).ToArray());
                        ShowDiagnosticInfo("Latitude: ----- " + latitude);
                        gpsElementDataAddress += 8;
                        deviceData.Lat = latitude;
                        deviceData.Long = longtitude;
                    }
                    if ((gpsMask & GPSmask.Altitude) != 0)
                    {
                        string alt = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(2).ToList().ForEach(delegate(byte b) { alt += String.Format("{0:X2}", b); });
                        int altitude = Convert.ToInt32(alt, 16);
                        ShowDiagnosticInfo("Altitude: ----- " + altitude);
                        gpsElementDataAddress += 2;
                        deviceData.Altitude = (short)altitude;
                    }

                    if ((gpsMask & GPSmask.Angle) != 0)
                    {
                        string ang = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(1).ToList().ForEach(delegate(byte b) { ang += String.Format("{0:X2}", b); });
                        int angle = Convert.ToInt32(ang, 16);
                        angle = Convert.ToInt32(angle * 360.0 / 256.0);
                        ShowDiagnosticInfo("Angle: ----- " + angle);
                        gpsElementDataAddress += 1;
                        deviceData.Direction = (short)angle;
                    }

                    if ((gpsMask & GPSmask.Speed) != 0)
                    {
                        string sp = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(1).ToList().ForEach(delegate(byte b) { sp += String.Format("{0:X2}", b); });
                        int speed = Convert.ToInt32(sp, 16);
                        ShowDiagnosticInfo("Speed: ----- " + speed);
                        gpsElementDataAddress += 1;
                        deviceData.Speed = (short)speed;
                    }

                    if ((gpsMask & GPSmask.Sattelites) != 0)
                    {
                        int satellites = Convert.ToInt32(receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(1).ToList()[0]);
                        ShowDiagnosticInfo("Satellites: ----- " + satellites);
                        gpsElementDataAddress += 1;
                        deviceData.Satellites = (byte)satellites;
                    }

                    if ((gpsMask & GPSmask.LocalAreaCodeAndCellID) != 0)
                    {
                        string localArea = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(2).ToList().ForEach(delegate(byte b) { localArea += String.Format("{0:X2}", b); });
                        int localAreaCode = Convert.ToInt32(localArea, 16);
                        ShowDiagnosticInfo("Local area code: ----- " + localAreaCode);
                        gpsElementDataAddress += 2;
                        deviceData.LocalAreaCode = (short)localAreaCode;

                        string cell_ID = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(2).ToList().ForEach(delegate(byte b) { cell_ID += String.Format("{0:X2}", b); });
                        int cellID = Convert.ToInt32(cell_ID, 16);
                        ShowDiagnosticInfo("Cell ID: ----- " + localAreaCode);
                        gpsElementDataAddress += 2;
                        deviceData.CellID = (short)cellID;
                    }

                    if ((gpsMask & GPSmask.SignalQuality) != 0)
                    {
                        string gsmQua = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(1).ToList().ForEach(delegate(byte b) { gsmQua += String.Format("{0:X2}", b); });
                        int gsmSignalQuality = Convert.ToInt32(gsmQua, 16);
                        ShowDiagnosticInfo("GSM signal quality: ----- " + gsmSignalQuality);
                        gpsElementDataAddress += 1;
                        deviceData.GsmSignalQuality = (byte)gsmSignalQuality;
                    }

                    if ((gpsMask & GPSmask.OperatorCode) != 0)
                    {
                        string opCode = string.Empty;
                        receiveBytes.Skip(tokenAddress + gpsElementDataAddress).Take(4).ToList().ForEach(delegate(byte b) { opCode += String.Format("{0:X2}", b); });
                        int operatorCode = Convert.ToInt32(opCode, 16);
                        ShowDiagnosticInfo("Operator code: ----- " + operatorCode);
                        gpsElementDataAddress += 4;
                        deviceData.OperatorCode = operatorCode;
                    }
                }
                tokenAddress += gpsElementDataAddress;
                if ((globalMask & GlobalMask.IO_Element_1B) != 0)
                {
                    byte quantityOfIOelementData = receiveBytes.Skip(tokenAddress).Take(1).First();
                    tokenAddress += 1;
                    for (int i = 0; i < quantityOfIOelementData; i++)
                    {
                        byte parameterID = receiveBytes.Skip(tokenAddress).Take(1).First();
                        ShowDiagnosticInfo("IO element 1B ID: -----".PadRight(40, '-') + " " + parameterID);
                        byte parameterValue = receiveBytes.Skip(tokenAddress + 1).Take(1).First();
                        ShowDiagnosticInfo("IO element 1B value: -----".PadRight(40, '-') + " " + parameterValue);
                        deviceData.IO_Elements_1B.Add(parameterID, parameterValue);
                        tokenAddress += 2;

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

                if ((globalMask & GlobalMask.IO_Element_2B) != 0)
                {
                    byte quantityOfIOelementData = receiveBytes.Skip(tokenAddress).Take(1).First();
                    tokenAddress += 1;
                    for (int i = 0; i < quantityOfIOelementData; i++)
                    {
                        byte parameterID = receiveBytes.Skip(tokenAddress).Take(1).First();
                        ShowDiagnosticInfo("IO element 2B ID: -----".PadRight(40, '-') + " " + parameterID);
                        string value = string.Empty;
                        receiveBytes.Skip(tokenAddress + 1).Take(2).ToList().ForEach(delegate(byte b) { value += String.Format("{0:X2}", b); });
                        short parameterValue = (short)Convert.ToInt32(value, 16);
                        ShowDiagnosticInfo("IO element 2B value: -----".PadRight(40, '-') + " " + parameterValue);
                        deviceData.IO_Elements_2B.Add(parameterID, parameterValue);
                        tokenAddress += 3;
                    }
                }
                if ((globalMask & GlobalMask.IO_Element_4B) != 0)
                {
                    byte quantityOfIOelementData = receiveBytes.Skip(tokenAddress).Take(1).First();
                    tokenAddress += 1;
                    for (int i = 0; i < quantityOfIOelementData; i++)
                    {
                        byte parameterID = receiveBytes.Skip(tokenAddress).Take(1).First();
                        ShowDiagnosticInfo("IO element 4B ID: -----".PadRight(40, '-') + " " + parameterID);
                        string value = string.Empty;
                        receiveBytes.Skip(tokenAddress + 1).Take(4).ToList().ForEach(delegate(byte b) { value += String.Format("{0:X2}", b); });
                        int parameterValue = Convert.ToInt32(value, 16);
                        ShowDiagnosticInfo("IO element 4B value: -----".PadRight(40, '-') + " " + parameterValue);
                        deviceData.IO_Elements_4B.Add(parameterID, parameterValue);
                        tokenAddress += 5;
                    }
                }

                deviceData.IMEI = IMEI.Substring(0, 15);
                deviceDataList.Add(deviceData);
            }

            //CRC for check of data correction and request again data from device if it not correct
            string crcString = string.Empty;
            receiveBytes.Skip(dataLength + 8).Take(4).ToList().ForEach(delegate(byte b) { crcString += String.Format("{0:X2}", b); });
            int CRC = Convert.ToInt32(crcString, 16);
            ShowDiagnosticInfo("CRC: -----".PadRight(40, '-') + " " + CRC);
            //We must skeep first 8 bytes and last 4 bytes with CRC value.
            int calculatedCRC = MathUtilities.GetCRC16(receiveBytes.Skip(8).Take(receiveBytes.Length - 12).ToArray());
            ShowDiagnosticInfo("Calculated CRC: -------".PadRight(40, '-') + " " + calculatedCRC);
            ShowDiagnosticInfo("||||||||||||||||||||||||||||||||||||||||||||||||");
            if (calculatedCRC == CRC)
            {
                return (numberOfData, deviceDataList);
            }
            else
            {
                ShowDiagnosticInfo("Incorect CRC ");
                return (0, null);
            }
        }
    }
}
