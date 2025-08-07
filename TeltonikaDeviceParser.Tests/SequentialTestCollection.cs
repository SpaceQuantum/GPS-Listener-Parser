using Xunit;

namespace TeltonikaDeviceParser.Tests;

[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialTestCollection : ICollectionFixture<object>
{
}