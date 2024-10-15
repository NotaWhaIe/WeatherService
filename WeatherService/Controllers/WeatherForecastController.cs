using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WeatherService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public WeatherForecastController(
                    ILogger<WeatherForecastController> logger,
                    IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // Новый метод для получения температуры по городу
        [HttpGet("temperature/{city}")]
        public async Task<IActionResult> GetTemperature(string city)
        {
            var client = _httpClientFactory.CreateClient();

            // Используем ваш API-ключ
            string apiKey = "65950ca603e8de3caeb6547119af078f";
            string requestUrl = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            try
            {
                var response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Парсим JSON-ответ
                    using var jsonDoc = JsonDocument.Parse(content);
                    var root = jsonDoc.RootElement;

                    double temp = root.GetProperty("main").GetProperty("temp").GetDouble();

                    return Ok(new { city = city, temperature = temp });
                }
                else
                {
                    return BadRequest("Не удалось получить данные о погоде.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных о погоде.");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }
    }
}