using System.Buffers.Binary;
using TeltonikaDeviceParser.Utilities;

namespace TeltonikaDeviceParser.Teltonika
{
    public class FMXXXX_Parser : ParserBase
    {
        public FMXXXX_Parser(bool showDiagnosticInfo)
        {
            base.showDiagnosticInfo = showDiagnosticInfo;
        }
        
        public override (int numberOfData, List<DeviceData>? deviceDataList) DecodeAVL(byte[] receiveBytes,
            string IMEI)
        {
            ReadOnlySpan<byte> buffer = receiveBytes.AsSpan();
            int dataLength = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(4, 4));
            ShowDiagnosticInfo("Data Length:".PadRight(40, '-') + " " + dataLength);

            int codecId = buffer[8];
            ShowDiagnosticInfo("Codec ID:".PadRight(40, '-') + " " + codecId);

            int numberOfData = buffer[9];
            ShowDiagnosticInfo("Number of data:".PadRight(40, '-') + " " + numberOfData);

            List<DeviceData> deviceDataList = [];
            int tokenAddress = 10;

            for (int n = 0; n < numberOfData; n++)
            {
                DeviceData deviceData = new DeviceData();
                
                long timestampUnix = BinaryPrimitives.ReadInt64BigEndian(buffer.Slice(tokenAddress, 8));
                DateTime timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampUnix).DateTime;
                byte priority = buffer[tokenAddress + 8];
                int longitudeInt = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(tokenAddress + 9, 4));
                double longtitude = (double)longitudeInt / 10000000;
                int latitudeInt = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(tokenAddress + 13, 4));
                double latitude = (double)latitudeInt / 10000000;
                int altitude = BinaryPrimitives.ReadInt16BigEndian(buffer.Slice(tokenAddress + 17, 2));
                int angle = BinaryPrimitives.ReadInt16BigEndian(buffer.Slice(tokenAddress + 19, 2));
                byte satellites = buffer[tokenAddress + 21];
                int speed = BinaryPrimitives.ReadInt16BigEndian(buffer.Slice(tokenAddress + 22, 2));
                byte event_IO_element_ID = buffer[tokenAddress + 24];
                byte IO_element_in_record = buffer[tokenAddress + 25];
                

                ShowDiagnosticInfo("Timestamp: -----".PadRight(40, '-') + " "  + timestamp.ToLongDateString() + " " + timestamp.ToLongTimeString());
                ShowDiagnosticInfo("Priority: ------------".PadRight(40, '-') + " "  + priority);
                ShowDiagnosticInfo("Longtitude: -----".PadRight(40, '-') + " "  + longtitude);
                ShowDiagnosticInfo("Latitude: -----".PadRight(40, '-') + " "  + latitude);
                ShowDiagnosticInfo("Altitude: -----".PadRight(40, '-') + " "  + altitude);
                ShowDiagnosticInfo("Angle: -----".PadRight(40, '-') + " "  + angle);
                ShowDiagnosticInfo("Satellites: -----".PadRight(40, '-') + " "  + satellites);
                ShowDiagnosticInfo("Speed: -----".PadRight(40, '-') + " "  + speed);
                ShowDiagnosticInfo("IO element ID of Event generated: ------".PadRight(40, '-') + " "  + event_IO_element_ID);
                ShowDiagnosticInfo("IO_element_in_record: --------".PadRight(40, '-') + " "  + IO_element_in_record);

                if (IO_element_in_record > 0)
                {
                    int currentCursor = tokenAddress + 26;

                    int IO_Elements_1B_Quantity = buffer[currentCursor];
                    ShowDiagnosticInfo("1 byte IO element in record: --------".PadRight(40, '-') + " "  + IO_Elements_1B_Quantity);

                    for (int IO_1 = 0; IO_1 < IO_Elements_1B_Quantity; IO_1++)
                    {
                        var parameterID = buffer[currentCursor + 1 + IO_1 * 2];
                        var IO_Element_1B = buffer[currentCursor + 2 + IO_1 * 2];
                        deviceData.IO_Elements_1B.Add(parameterID, IO_Element_1B);
                        ShowDiagnosticInfo("IO element 1B ID: --------".PadRight(40, '-') + " "  + parameterID);
                        ShowDiagnosticInfo(IO_1 + "'st 1B IO element value: --------".PadRight(40 - IO_1.ToString().Length, '-') + " "  + IO_Element_1B);
                    }
                    currentCursor += IO_Elements_1B_Quantity * 2 + 1;

                    int IO_Elements_2B_Quantity = buffer[currentCursor];
                    ShowDiagnosticInfo("2 byte IO element in record: --------".PadRight(40, '-') + " " + IO_Elements_2B_Quantity);

                    for (int IO_2 = 0; IO_2 < IO_Elements_2B_Quantity; IO_2++)
                    {
                        var parameterID = buffer[currentCursor + 1 + IO_2 * 3];
                        short IO_Element_2B = BinaryPrimitives.ReadInt16BigEndian(buffer.Slice(currentCursor + 2 + IO_2 * 3, 2));
                        deviceData.IO_Elements_2B.Add(parameterID, IO_Element_2B);

                        ShowDiagnosticInfo("IO element 2B ID: --------".PadRight(40, '-') + " "  + parameterID);
                        ShowDiagnosticInfo(IO_2 + "'st 2B IO element value: --------".PadRight(40 - IO_2.ToString().Length, '-') + " "  + IO_Element_2B);
                    }
                    currentCursor += IO_Elements_2B_Quantity * 3 + 1;

                    int IO_Elements_4B_Quantity = buffer[currentCursor];
                    ShowDiagnosticInfo("4 byte IO element in record: --------".PadRight(40, '-') + " " + IO_Elements_4B_Quantity);

                    for (int IO_4 = 0; IO_4 < IO_Elements_4B_Quantity; IO_4++)
                    {
                        var parameterID =  buffer[currentCursor + 1 + IO_4 * 5];
                        int IO_Element_4B = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(currentCursor + 2 + IO_4 * 5, 4));
                        deviceData.IO_Elements_4B.Add(parameterID, IO_Element_4B);

                        ShowDiagnosticInfo("IO element 4B ID: --------".PadRight(40, '-') + " "  + parameterID);
                        ShowDiagnosticInfo(IO_4 + "'st 4B IO element value: --------".PadRight(40 - IO_4.ToString().Length, '-') + " "  + IO_Element_4B);
                    }
                    currentCursor += IO_Elements_4B_Quantity * 5 + 1;

                    int IO_Elements_8B_Quantity = buffer[currentCursor];
                    ShowDiagnosticInfo("8 byte IO element in record: --------".PadRight(40, '-') + " " + IO_Elements_8B_Quantity);

                    for (int IO_8 = 0; IO_8 < IO_Elements_8B_Quantity; IO_8++)
                    {
                        var parameterID =  buffer[currentCursor + 1 + IO_8 * 9];
                        long IO_Element_8B = BinaryPrimitives.ReadInt64BigEndian(buffer.Slice(currentCursor + 2 + IO_8 * 9, 8));
                        deviceData.IO_Elements_8B.Add(parameterID, IO_Element_8B);

                        ShowDiagnosticInfo("IO element 8B ID: --------".PadRight(40, '-') + " "  + parameterID);
                        ShowDiagnosticInfo(IO_8 + "'st 8B IO element value: --------".PadRight(40 - IO_8.ToString().Length, '-') + " "  + IO_Element_8B);
                    }

                    tokenAddress += 30 + IO_Elements_1B_Quantity * 2 +
                        IO_Elements_2B_Quantity * 3 + IO_Elements_4B_Quantity * 5
                        + IO_Elements_8B_Quantity * 9;
                }
                else
                {                   
                    tokenAddress += 30;
                }

                deviceData.Event_IO_element_ID = event_IO_element_ID;
                deviceData.Altitude = (short)altitude;
                deviceData.Direction = (short)angle;
                deviceData.Lat = latitude;
                deviceData.Long = longtitude;
                deviceData.Priority = priority;
                deviceData.Satellites = satellites;
                deviceData.Speed = (short)speed;
                deviceData.Timestamp = timestamp;
                deviceData.IMEI = IMEI.Substring(0, 15);
                
                deviceDataList.Add(deviceData);
            }
            
            //CRC for check of data correction and request again data from a device if it not correct
            int CRC = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(dataLength + 8, 4));
            ShowDiagnosticInfo("CRC: -----".PadRight(40, '-') + " "  + CRC);
            //We must skeep first 8 bytes and last 4 bytes with CRC value.
            int calculatedCRC = MathUtilities.GetCRC16(buffer.Slice(8, receiveBytes.Length - 12));
            ShowDiagnosticInfo("Calculated CRC: -------".PadRight(40, '-') + " "  + calculatedCRC);
            ShowDiagnosticInfo("||||||||||||||||||||||||||||||||||||||||||||||||");
            if (calculatedCRC == CRC)
            {
                return (numberOfData, deviceDataList);
            }

            ShowDiagnosticInfo("Incorect CRC ");
            return (0, null);
        }
    }  
}
