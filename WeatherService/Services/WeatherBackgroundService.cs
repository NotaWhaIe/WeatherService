using System.Text.Json;
using WeatherService.Models;

namespace WeatherService.Services
{
    public class WeatherBackgroundService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WeatherDataService _weatherDataService;
        private readonly string _apiKey = "65950ca603e8de3caeb6547119af078f";
        private readonly string _baseUrl = "http://api.openweathermap.org/data/2.5/weather";

        public WeatherBackgroundService(
            IHttpClientFactory httpClientFactory,
            WeatherDataService weatherDataService)
        {
            _httpClientFactory = httpClientFactory;
            _weatherDataService = weatherDataService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int currentCityIndex = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                var cityList = _weatherDataService.GetCityList();

                if (cityList.Count > 0)
                {
                    if (currentCityIndex >= cityList.Count)
                    {
                        currentCityIndex = 0;
                    }

                    var city = cityList[currentCityIndex];
                    currentCityIndex++;

                    var temperature = await GetTemperatureForCity(city);

                    if (temperature.HasValue)
                    {
                        var record = new TemperatureRecord
                        {
                            Time = DateTime.UtcNow,
                            Temperature = temperature.Value
                        };
                        _weatherDataService.AddTemperatureRecord(city, record);

                        // Логируем успешное обновление
                        Console.WriteLine($"[{DateTime.UtcNow}] Обновлена температура для {city}: {temperature.Value}°C");
                    }
                    else
                    {
                        // Логируем неудачу
                        Console.WriteLine($"[{DateTime.UtcNow}] Не удалось получить температуру для {city}");
                    }
                }

                // Ждём 1 минуту перед следующим запросом
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task<double?> GetTemperatureForCity(string city)
        {
            var client = _httpClientFactory.CreateClient();
            var requestUrl = $"{_baseUrl}?q={city}&appid={_apiKey}&units=metric";

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

                    return temp;
                }
                else
                {
                    // Логируем ошибку
                    Console.WriteLine($"Ошибка при получении температуры для {city}: {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Логируем исключение
                Console.WriteLine($"Исключение при получении температуры для {city}: {ex.Message}");
                return null;
            }
        }
    }
}
