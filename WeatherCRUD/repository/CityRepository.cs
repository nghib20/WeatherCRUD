using Dapper;
using MySqlConnector;
using System.Data;
using WeatherCRUD.Model;

namespace WeatherCRUD.repository
{
    public interface ICityRepository
    {
        List<City> GetCities();

        City? GetCityByName(string cityName);
        
        void AddCity(City city);
        
        void DeleteCity(string cityName);
    }

    public class CityRepository : ICityRepository
    {
        private readonly IDbConnection _dbConnection;

        public CityRepository(IConfiguration configuration)
        {
            _dbConnection = new MySqlConnection(configuration.GetConnectionString("WeatherDB"));
            _dbConnection.Open();
        }

        public void AddCity(City city)
        {
            string query = "INSERT INTO city (id, city_name) VALUES (@Id, @CityName)";

            _dbConnection.Execute(query, city);
        }

        public void DeleteCity(string cityName)
        {
            var sql = "DELETE FROM city WHERE city_name = @CityName";
            _dbConnection.Execute(sql, new { CityName = cityName});
        }

        public List<City> GetCities()
        {
            var sql = "SELECT * FROM city";
            return _dbConnection.Query<City>(sql).ToList();
        }

        public City? GetCityByName(string cityName)
        {
            var sql = "SELECT * FROM city WHERE city_name = @CityName";
            return _dbConnection.QueryFirstOrDefault<City>(sql, new { CityName = cityName });
        }
    }
}
