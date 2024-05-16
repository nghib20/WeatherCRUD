using NSubstitute;
using WeatherCRUD.Model;
using WeatherCRUD.repository;
using WeatherCRUD.Service;

namespace WeatherCRUDTest.Service
{
    public class WeatherServiceTest
    {
        private ICityRepository mockCityRepository;
        private ITemperatureRepository mockTemperatureRepository;
        private IWeatherApiClient mockWeatherApiClient;
        private WeatherService weatherService;

        [SetUp]
        public void Setup()
        {
            mockCityRepository = Substitute.For<ICityRepository>();
            mockTemperatureRepository = Substitute.For<ITemperatureRepository>();
            mockWeatherApiClient = Substitute.For<IWeatherApiClient>();

            weatherService = new WeatherService(mockCityRepository, mockTemperatureRepository, mockWeatherApiClient);
        }

        [Test]
        public void GetCities_ReturnsListOfCities()
        {
            // Arrange
            var expectedCities = new List<City>
        {
            new City { Id = 1, CityName = "City1" },
            new City { Id = 2, CityName = "City2" },
        };
            mockCityRepository.GetCities().Returns(expectedCities);

            // Act
            var actualCities = weatherService.GetCities();

            // Assert
            Assert.IsNotNull(actualCities);
            Assert.That(actualCities.Count, Is.EqualTo(expectedCities.Count));

            Assert.That(actualCities[0].Id, Is.EqualTo(expectedCities[0].Id));
            Assert.That(actualCities[0].CityName, Is.EqualTo(expectedCities[0].CityName));
        }

        [Test]
        public void DeleteCity_WhenCityDoesNotExist_DoesNotDeleteAnything()
        {
            // Arrange
            var nonExistentCity = "Non Existent";
            City returnedCity = null;

            mockCityRepository.GetCityByName(nonExistentCity).Returns(returnedCity);

            // Act
            weatherService.DeleteCity(nonExistentCity);

            // Assert that repository delete methods were not called
            mockTemperatureRepository.DidNotReceive().DeleteByCityId(Arg.Any<long>());
            mockCityRepository.DidNotReceive().DeleteCity(Arg.Any<string>());
        }

        [Test]
        public void DeleteCity_WhenCityExists_DeletesCityAndTemperatures()
        {
            // Arrange
            var city = new City
            {
                Id = 1,
                CityName = "City Exists"
            };

            mockCityRepository.GetCityByName("City Exists").Returns(city);

            // Act
            weatherService.DeleteCity(city.CityName);

            // Assert that repository delete methods were called with correct Id and CityName
            mockTemperatureRepository.Received().DeleteByCityId(city.Id);
            mockCityRepository.Received().DeleteCity(city.CityName);
        }

        [Test]
        public void GetTempsByCity_WhenCityExists_ReturnsTemperatures()
        {
            // Arrange
            var city = new City
            {
                Id = 1,
                CityName = "City Exists"
            };

            DateTime dateTime = new DateTime();

            var expectedTemperatures = new List<Temperature>
        {
            new Temperature { Id = 1, CityId = 1, TemperatureValue = 10.0, Time = dateTime},
        };

            mockCityRepository.GetCityByName("City Exists").Returns(city);
            mockTemperatureRepository.GetByCityIdAndDateRange(city.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(expectedTemperatures);

            // Act
            var actualTemperatures = weatherService.GetTempsByCityAndRange(city.CityName, dateTime, dateTime);

            // Assert
            Assert.IsNotNull(actualTemperatures);
            Assert.That(actualTemperatures.Count, Is.EqualTo(expectedTemperatures.Count));
            Assert.That(actualTemperatures[0].Id, Is.EqualTo(expectedTemperatures[0].Id));

            mockTemperatureRepository.Received().GetByCityIdAndDateRange(city.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>());
        }

        [Test]
        public void GetTempsByCity_WhenCityDoesNotExist_ReturnsEmptyList()
        {
            // Arrange
            var cityName = "Non Existent";
            City city = null;

            DateTime dateTime = new DateTime();

            var expectedTemperatures = new List<Temperature>();

            mockCityRepository.GetCityByName(cityName).Returns(city);

            // Act
            var actualTemperatures = weatherService.GetTempsByCityAndRange(cityName, dateTime, dateTime);

            // Assert that temperature repository method is not called and an empty list is returned
            mockTemperatureRepository.DidNotReceive().GetByCityIdAndDateRange(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<DateTime>());

            Assert.That(actualTemperatures.Count, Is.EqualTo(expectedTemperatures.Count));
        }

        [Test]
        public async Task GetWeather_WhenCityNotInApi_ReturnsCityNotFound()
        {
            // Arrange
            var request = new WeatherApiRequest("Non Existent");
            WeatherApiResponse apiResponse = null;
            var expectedResponse = new WeatherResponse(apiResponse, "City not found");

            mockWeatherApiClient.GetWeatherFromApi(request).Returns(apiResponse);

            // Act
            var actualResponse = await weatherService.GetWeather(request);

            // Assert that "City not found" message is returned in response
            Assert.That(actualResponse.Message, Is.EqualTo(expectedResponse.Message));
        }

        [Test]
        public async Task GetWeather_WhenNewCity_ReturnsNewCityResponse()
        {
            // 
            var city = new City { Id = 1, CityName = "New city" };

            var request = new WeatherApiRequest(city.CityName);
            var apiResponse = new WeatherApiResponse(CityName: city.CityName, Temperature: "10.0");
            var expectedResponse = new WeatherResponse(apiResponse, "New city detected. Adding to city repository.");
            City returnedCity = null;

            mockWeatherApiClient.GetWeatherFromApi(request).Returns(apiResponse);

            mockCityRepository.GetCityByName(apiResponse.CityName).Returns(returnedCity);
            mockCityRepository.AddCity(Arg.Any<City>()).Returns(city);

            // Act
            var actualResponse = await weatherService.GetWeather(request);

            // Assert that "New city detected" message is returned in response
            Assert.That(actualResponse.Message, Is.EqualTo(expectedResponse.Message));
        }

        [Test]
        public async Task GetWeather_WhenOldCity_ReturnsOldCityResponse()
        {
            // Arrange
            var city = new City { Id = 1, CityName = "Old city" };

            var request = new WeatherApiRequest(city.CityName);
            var apiResponse = new WeatherApiResponse(CityName: city.CityName, Temperature: "10.0");
            var expectedResponse = new WeatherResponse(apiResponse, "Old city detected. Not adding to city repository.");

            mockWeatherApiClient.GetWeatherFromApi(request).Returns(apiResponse);
            mockCityRepository.GetCityByName(apiResponse.CityName).Returns(city);

            // Act
            var actualResponse = await weatherService.GetWeather(request);

            // Assert that "Old city detected" message is returned in response
            Assert.That(actualResponse.Message, Is.EqualTo(expectedResponse.Message));
            mockTemperatureRepository.Received().Add(Arg.Any<Temperature>());
        }
    }
}