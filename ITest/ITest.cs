using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using WeatherCRUD.Model;
using WeatherCRUD.repository;
using WeatherCRUD.Service;

namespace ITest
{
    [TestFixture]
    public class WeatherApiTests
    {
        private CustomWebApplicationFactory<WeatherController> _factory;
        private HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory<WeatherController>();
            _client = _factory.CreateClient(); 
        }

        [Test]
        public async Task GetTempsAndCheckNewCities()
        {
            // Arrange
            var city1 = "Tbilisi";
            var city2 = "Warsaw";

            // Act
            var addCityResponse1 = await _client.PostAsync($"/get-temp-for-city/{city1}", null);
            var addCityResponse2 = await _client.PostAsync($"/get-temp-for-city/{city2}", null);
            var getCitiesResponse = await _client.GetAsync("/get-cities");

            Assert.That(addCityResponse1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            string responseString1 = await addCityResponse1.Content.ReadAsStringAsync();
            var weatherResponse1 = JsonConvert.DeserializeObject<WeatherResponse>(responseString1);
            Assert.That(weatherResponse1.Message, Is.EqualTo("New city detected. Adding to city repository."));

            Assert.That(addCityResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            string responseString2 = await addCityResponse2.Content.ReadAsStringAsync();
            var weatherResponse2 = JsonConvert.DeserializeObject<WeatherResponse>(responseString2);
            Assert.That(weatherResponse2.Message, Is.EqualTo("New city detected. Adding to city repository."));

            Assert.That(getCitiesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            string citiesString = await getCitiesResponse.Content.ReadAsStringAsync();
            var cities = JsonConvert.DeserializeObject<List<City>>(citiesString);

            Assert.IsNotNull(cities);
            Assert.That(cities.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetTempsForCity_WhenCityIsNotValid_ReturnsNotFound()
        {
            // Arrange
            var cityName = "asdasdasd";

            // Act
            var response = await _client.PostAsync($"/get-temp-for-city/{cityName}", null);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            string responseString = await response.Content.ReadAsStringAsync();
            var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(responseString);
            Assert.That(weatherResponse.Message, Is.EqualTo("City not found"));
        }

        [Test]
        public async Task GetTempsByCityAndRange_ReturnsTemps()
        {
            // Arrange
            var cityName = "Tbilisi";
            var startTime = "2024-05-15T00:00:00";
            var endTime = "2024-05-18T16:44:10";
            
            // Act
            var response = await _client.GetAsync($"/get-temps-by-city-and-range/{cityName}/{startTime}/{endTime}");
            var emptyResponse = await _client.GetAsync($"/get-temps-by-city-and-range/{cityName}/{startTime}/{startTime}");
            
            // Assert
            string responseString = await response.Content.ReadAsStringAsync();
            var temperatures = JsonConvert.DeserializeObject<List<Temperature>>(responseString);
            Assert.That(temperatures.Count, Is.EqualTo(1));

            string emptyResponseString = await emptyResponse.Content.ReadAsStringAsync();
            var emptyTemperatures = JsonConvert.DeserializeObject<List<Temperature>>(emptyResponseString);
            Assert.That(emptyTemperatures.Count, Is.EqualTo(0));
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ICityRepository, CityRepository>();
                services.AddSingleton<ITemperatureRepository, TemperatureRepository>();
                services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
                {
                    string url = "https://api.openweathermap.org/";
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                });
            });

            return base.CreateHost(builder);
        }
    }
}