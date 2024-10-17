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

        // ����� ��� ���������� ������ � ������ �������������
        [HttpPost("temperature/{city}")]
        public IActionResult AddCity(string city)
        {
            if (_weatherDataService.AddCity(city))
            {
                return Ok($"����� {city} �������� ��� ������������.");
            }
            else
            {
                return BadRequest("����� ��� �������������.");
            }
        }

        // ����� ��� ��������� ������� ���������� �� ���� �������
        [HttpGet("temperature")]
        public IActionResult GetAllTemperatures()
        {
            var data = _weatherDataService.GetAllTemperatures();
            return Ok(data);
        }

        // ����� ��� �������� ������ �� ������ �������������
        [HttpDelete("temperature/{city}")]
        public IActionResult RemoveCity(string city)
        {
            if (_weatherDataService.RemoveCity(city))
            {
                return Ok($"����� {city} ����� �� ������������.");
            }
            else
            {
                return NotFound("����� �� ������ � ������ �������������.");
            }
        }
    }
}
