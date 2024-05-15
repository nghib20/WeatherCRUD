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
        
        City AddCity(City city);
        
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

        public City AddCity(City city)
        {
            string query = "INSERT INTO city (city_name) VALUES (@CityName); SELECT LAST_INSERT_ID();";

            long insertedId = _dbConnection.ExecuteScalar<long>(query, city);

            city.Id = insertedId;

            return city;
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
