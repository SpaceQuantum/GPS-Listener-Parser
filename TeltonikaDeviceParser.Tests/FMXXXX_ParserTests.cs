using TeltonikaDeviceParser.Teltonika;
using Xunit;
using Assert = Xunit.Assert;

namespace TeltonikaDeviceParser.Tests
{
    public class FMXXXX_ParserTests
    {
        const string DeviceDataHex = "00000000000000A7080400000113fc208dff000f14f650209cca80006f00d60400040004030101150316030001460000015d0000000113fc17610b000f14ffe0209cc580006e00c00500010004030101150316010001460000015e0000000113fc284945000f150f00209cd200009501080400000004030101150016030001460000015d0000000113fc267c5b000f150a50209cccc0009300680400000004030101150016030001460000015b00040000BA48";
        
        [Fact]
        public void DecodeAVL_ParsesAllDataScenario()
        {
            // Use the same raw data as in DecodeAVL_ParsesRealRawDataExample for consistency
            byte[] bytes = Enumerable.Range(0, DeviceDataHex.Length / 2)
                .Select(i => Convert.ToByte(DeviceDataHex.Substring(i * 2, 2), 16))
                .ToArray();
            var parser = new FMXXXX_Parser(false);
            var result = parser.DecodeAVL(bytes, "123456789012345");
            Assert.True(result.numberOfData > 0);
            Assert.NotNull(result.deviceDataList);
            Assert.Equal(result.numberOfData, result.deviceDataList.Count);

            foreach (var data in result.deviceDataList)
            {
                Assert.True(data.Timestamp > new DateTime(2000, 1, 1));
                Assert.True(data.Long is > -180 and < 180);
                Assert.True(data.Lat is > -90 and < 90);
                Assert.True(data.Speed >= 0);
                Assert.True(data.Altitude is >= -500 and < 10000);
                Assert.True(data.Direction is >= 0 and < 360);
                Assert.True(data.Satellites is >= 0 and < 100);
                Assert.True(!string.IsNullOrEmpty(data.IMEI));
                Assert.True(data.Event_IO_element_ID >= 0);
                Assert.True(data.Priority >= 0);
                // IO elements
                Assert.NotNull(data.IO_Elements_1B);
                Assert.NotNull(data.IO_Elements_2B);
                Assert.NotNull(data.IO_Elements_4B);
                Assert.NotNull(data.IO_Elements_8B);
            }
        }
    }
}
