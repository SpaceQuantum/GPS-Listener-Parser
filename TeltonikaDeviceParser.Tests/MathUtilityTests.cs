using TeltonikaDeviceParser.Utilities;
using Xunit;
using Assert = Xunit.Assert;

namespace TeltonikaDeviceParser.Tests;

public class MathUtilityTests
{
    [Fact]
    public void CalculateCRC()
    {
        // Arrange
        var hex =
            "080400000113fc208dff000f14f650209cca80006f00d60400040004030101150316030001460000015d0000000113fc17610b000f14ffe0209cc580006e00c00500010004030101150316010001460000015e0000000113fc284945000f150f00209cd200009501080400000004030101150016030001460000015d0000000113fc267c5b000f150a50209cccc0009300680400000004030101150016030001460000015b0004";
        byte[] bytes = Enumerable.Range(0, hex.Length / 2)
            .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
            .ToArray();
        
        // Act
        double result = MathUtilities.GetCRC16(bytes);
        
        // Assert
        Assert.Equal(47688, result);
    }
}