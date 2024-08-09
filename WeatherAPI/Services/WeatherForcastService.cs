using Collette.Data;
using Dapper.Database.Extensions;

namespace WeatherAPI.Services
{
    public class WeatherForcastService
    {
        private readonly IDbConnectionWrapper _dbConnectionWrapper;

        public WeatherForcastService(IDbConnectionWrapper dbConnectionWrapper)
        {
            _dbConnectionWrapper = dbConnectionWrapper;
        }

        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await _dbConnectionWrapper.
                GetConnection().
                GetListAsync<WeatherForecast>("SELECT [Forecast] AS Summary, [Date], [Temperature] AS TemperatureC FROM [WeatherForecast]");
        }
    }
}
