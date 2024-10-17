using Microsoft.AspNetCore.Mvc;
using WeatherService.Services;

namespace WeatherService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly WeatherDataService _weatherDataService;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            WeatherDataService weatherDataService)
        {
            _logger = logger;
            _weatherDataService = weatherDataService;
        }

        // Метод для добавления города в список отслеживаемых
        [HttpPost("temperature/{city}")]
        public IActionResult AddCity(string city)
        {
            if (_weatherDataService.AddCity(city))
            {
                return Ok($"Город {city} добавлен для отслеживания.");
            }
            else
            {
                return BadRequest("Город уже отслеживается.");
            }
        }

        // Метод для получения истории температур по всем городам
        [HttpGet("temperature")]
        public IActionResult GetAllTemperatures()
        {
            var data = _weatherDataService.GetAllTemperatures();
            return Ok(data);
        }

        // Метод для удаления города из списка отслеживаемых
        [HttpDelete("temperature/{city}")]
        public IActionResult RemoveCity(string city)
        {
            if (_weatherDataService.RemoveCity(city))
            {
                return Ok($"Город {city} удалён из отслеживания.");
            }
            else
            {
                return NotFound("Город не найден в списке отслеживаемых.");
            }
        }
    }
}
