using WeatherCRUD.Model;
using WeatherCRUD.repository;

namespace WeatherCRUD.Service
{
    interface IWeatherService
    {
        List<City> GetCities();

        void DeleteCity(string cityName);

        List<Temperature> GetTempsByCityAndRange(string cityName, DateTime start, DateTime end);
    }

    public class WeatherService : IWeatherService
    {
        private readonly ICityRepository cityRepository;
        private readonly ITemperatureRepository temperatureRepository;

        public WeatherService(ICityRepository cityRepository, ITemperatureRepository temperatureRepository)
        {
            this.cityRepository = cityRepository;
            this.temperatureRepository = temperatureRepository;
        }

        public void DeleteCity(string cityName)
        {
            City city = cityRepository.GetCityByName(cityName);
            if(city != null)
            {
                cityRepository.DeleteCity(cityName);
                temperatureRepository.DeleteByCityId(city.Id);
            }
        }

        public List<City> GetCities()
        {
            return cityRepository.GetCities();
        }

        public List<Temperature> GetTempsByCityAndRange(string cityName, DateTime start, DateTime end)
        {
            City city = cityRepository.GetCityByName(cityName);
            if( city != null )
            {
                return temperatureRepository.GetByCityIdAndDateRange(city.Id, start, end);
            } else
            {
                return new List<Temperature>();
            }
        }
    }
}
