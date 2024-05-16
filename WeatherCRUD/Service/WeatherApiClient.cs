using Newtonsoft.Json;
using WeatherCRUD.Model;

namespace WeatherCRUD.Service
{
    public interface IWeatherApiClient
    {
        Task<WeatherApiResponse> GetWeatherFromApi(WeatherApiRequest request);
    }

    public class WeatherApiClient : IWeatherApiClient
    {
        private readonly HttpClient _client;
        private readonly string API_KEY;

        public WeatherApiClient(HttpClient httpClient, IConfiguration config)
        {
            _client = httpClient;
            API_KEY = config["API_KEY"];
        }

        public async Task<WeatherApiResponse> GetWeatherFromApi(WeatherApiRequest request)
        {
            string url = "data/2.5/weather?q=" + request.CityName + "&appid=" + API_KEY + "&units=metric";
            HttpResponseMessage response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string responseString = await response.Content.ReadAsStringAsync();

            WeatherObject weatherObject = JsonConvert.DeserializeObject<WeatherObject>(responseString);

            return new WeatherApiResponse(
                CityName: request.CityName,
                Temperature: weatherObject.Main.Temp);
        }
    }
}
