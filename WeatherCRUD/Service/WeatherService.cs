using WeatherCRUD.Model;
using WeatherCRUD.repository;

namespace WeatherCRUD.Service
{
    interface IWeatherService
    {
        List<City> GetCities();

        void DeleteCity(string cityName);

        List<Temperature> GetTempsByCityAndRange(string cityName, DateTime start, DateTime end);

        Task<WeatherResponse> GetWeather(WeatherApiRequest request);
    }

    public class WeatherService : IWeatherService
    {
        private readonly ICityRepository cityRepository;
        private readonly ITemperatureRepository temperatureRepository;
        private readonly IWeatherApiClient weatherApiClient;

        public WeatherService(ICityRepository cityRepository, ITemperatureRepository temperatureRepository, IWeatherApiClient weatherApiClient)
        {
            this.cityRepository = cityRepository;
            this.temperatureRepository = temperatureRepository;
            this.weatherApiClient = weatherApiClient;
        }

        public void DeleteCity(string cityName)
        {
            City city = cityRepository.GetCityByName(cityName);
            if(city != null)
            {
                temperatureRepository.DeleteByCityId(city.Id);
                cityRepository.DeleteCity(cityName);
            }
        }

        public List<City> GetCities()
        {
            return cityRepository.GetCities();
        }

        public List<Temperature> GetTempsByCityAndRange(string cityName, DateTime start, DateTime end)
        {
            City city = cityRepository.GetCityByName(cityName);
            if(city != null)
            {
                return temperatureRepository.GetByCityIdAndDateRange(city.Id, start, end);
            } else
            {
                return new List<Temperature>();
            }
        }

        public async Task<WeatherResponse> GetWeather(WeatherApiRequest request)
        {
            string responseString = "Old city detected. Not adding to city repository.";
            WeatherApiResponse apiResponse = await weatherApiClient.GetWeather(request);

            if( apiResponse == null )
            {
                return new WeatherResponse(apiResponse, "City not found");
            }

            City city = cityRepository.GetCityByName(apiResponse.CityName);

            if (city == null)
            {
                city = new City
                {
                    CityName = apiResponse.CityName,
                };
                city = cityRepository.AddCity(city);
                responseString = "New city detected. Adding to city repository.";
            }

            Temperature temperature = new Temperature
            {
                CityId = city.Id,
                TemperatureValue = double.Parse(apiResponse.Temperature),
                Time = DateTime.Now
            };
            temperatureRepository.Add(temperature);

            return new WeatherResponse(apiResponse, responseString);
        }
    }
}
