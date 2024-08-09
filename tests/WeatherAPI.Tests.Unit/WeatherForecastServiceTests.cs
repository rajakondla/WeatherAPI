using System.Threading.Tasks;
using WeatherAPI.Services;
using Xunit;

namespace WeatherAPI.Tests.Unit
{
    [Collection("Database")]
    public class WeatherForecastServiceTests : IngestionBase
    {
        public WeatherForecastServiceTests(DatabaseFixture fixture) : base(fixture)
        {
            
        }

        [Fact]
        public async Task Shoud_Get_Different_Climate_Temperature_Reading()
        {
            var service = new WeatherForcastService(_dbConnectionWrapper);

            var result = await service.Get();

            Assert.NotEmpty(result);
        }
    }
}
