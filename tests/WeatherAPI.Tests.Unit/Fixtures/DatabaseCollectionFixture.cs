using Xunit;

namespace WeatherAPI.Tests.Unit
{
    [CollectionDefinition("Database")]
    public class DatabaseCollectionFixture : ICollectionFixture<DatabaseFixture>
    {
    }
}
