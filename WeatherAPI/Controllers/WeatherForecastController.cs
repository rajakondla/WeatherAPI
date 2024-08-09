using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly WeatherForcastService _weatherForcastService;

        public WeatherForecastController(WeatherForcastService weatherForcastService)
        {
            _weatherForcastService = weatherForcastService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await _weatherForcastService.Get();
        }
    }
}
