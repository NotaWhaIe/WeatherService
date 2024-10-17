using System.Collections.Concurrent;
using WeatherService.Models;

namespace WeatherService.Services
{
    public class WeatherDataService
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<TemperatureRecord>> _cityTemperatures;
        private readonly List<string> _cityList;
        private readonly object _cityListLock = new object();

        public WeatherDataService()
        {
            _cityTemperatures = new ConcurrentDictionary<string, ConcurrentBag<TemperatureRecord>>();
            _cityList = new List<string>();
        }

        public bool AddCity(string city)
        {
            lock (_cityListLock)
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

        public bool RemoveCity(string city)
        {
            lock (_cityListLock)
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

        public List<string> GetCityList()
        {
            lock (_cityListLock)
            {
                return new List<string>(_cityList);
            }
        }

        public ConcurrentDictionary<string, ConcurrentBag<TemperatureRecord>> GetAllTemperatures()
        {
            return _cityTemperatures;
        }

        public void AddTemperatureRecord(string city, TemperatureRecord record)
        {
            if (_cityTemperatures.ContainsKey(city))
            {
                _cityTemperatures[city].Add(record);
            }
        }
    }
}
