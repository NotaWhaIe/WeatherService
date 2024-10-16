using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WeatherService.Models;

namespace WeatherService.Services
{
    public class WeatherDataService : IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Timer _timer;
        private readonly string _apiKey = "65950ca603e8de3caeb6547119af078f";
        private readonly string _baseUrl = "http://api.openweathermap.org/data/2.5/weather";
        private readonly ConcurrentDictionary<string, ConcurrentBag<TemperatureRecord>> _cityTemperatures;
        private readonly List<string> _cityList; // Список городов
        private int _currentCityIndex; // Индекс текущего города
        private bool _disposed;

        public ConcurrentDictionary<string, ConcurrentBag<TemperatureRecord>> CityTemperatures => _cityTemperatures;

        public WeatherDataService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _cityTemperatures = new ConcurrentDictionary<string, ConcurrentBag<TemperatureRecord>>();
            _cityList = new List<string>();
            _currentCityIndex = 0;

            // Запускаем таймер, который будет вызываться каждые 60 000 миллисекунд (1 минута)
            _timer = new Timer(UpdateTemperature, null, 0, 60000);
        }

        private async void UpdateTemperature(object state)
        {
            if (_cityList.Count == 0)
            {
                return; // Нет городов для обновления
            }

            string city;

            // Получаем текущий город для обновления
            lock (_cityList)
            {
                if (_currentCityIndex >= _cityList.Count)
                {
                    _currentCityIndex = 0; // Сброс индекса, если достигли конца списка
                }

                city = _cityList[_currentCityIndex];
                _currentCityIndex++;
            }

            var temperature = await GetTemperatureForCity(city);
            if (temperature.HasValue)
            {
                var record = new TemperatureRecord
                {
                    Time = DateTime.UtcNow,
                    Temperature = temperature.Value
                };
                _cityTemperatures[city].Add(record);

                // Логирование успешного обновления
                Console.WriteLine($"[{DateTime.UtcNow}] Обновлена температура для {city}: {temperature.Value}°C");
            }
            else
            {
                // Логирование ошибки получения температуры
                Console.WriteLine($"[{DateTime.UtcNow}] Не удалось получить температуру для {city}");
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
                    // Логируем ошибку или обрабатываем её соответствующим образом
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Логируем исключение
                Console.WriteLine($"Ошибка при получении температуры для {city}: {ex.Message}");
                return null;
            }
        }

        // Метод для добавления города
        public bool AddCity(string city)
        {
            lock (_cityList)
            {
                if (_cityTemperatures.ContainsKey(city))
                {
                    return false; // Город уже существует
                }

                _cityTemperatures[city] = new ConcurrentBag<TemperatureRecord>();
                _cityList.Add(city);
                return true;
            }
        }

        // Метод для удаления города
        public bool RemoveCity(string city)
        {
            lock (_cityList)
            {
                if (_cityTemperatures.TryRemove(city, out _))
                {
                    _cityList.Remove(city);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer?.Dispose();
                _disposed = true;
            }
        }
    }
}
