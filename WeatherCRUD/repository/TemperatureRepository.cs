using Dapper;
using MySqlConnector;
using System.Data;
using WeatherCRUD.Model;

namespace WeatherCRUD.repository
{
    public interface ITemperatureRepository
    {
        List<Temperature> GetByCityIdAndDateRange(long cityId, DateTime start, DateTime end);

        void DeleteByCityId(long cityId);

        void Add(Temperature temperature);
    }

    public class TemperatureRepository : ITemperatureRepository
    {
        private readonly IDbConnection _dbConnection;

        public TemperatureRepository(IConfiguration configuration)
        {
            _dbConnection = new MySqlConnection(configuration.GetConnectionString("WeatherDB"));
            _dbConnection.Open();
        }

        public void Add(Temperature temperature)
        {
            string query = "INSERT INTO temperature (id, city_id, temperature, time) VALUES (@Id, @CityId, @TemperatureValue, @Time)";

            _dbConnection.Execute(query, temperature);
        }

        public void DeleteByCityId(long cityId)
        {
            var sql = "DELETE FROM temperature WHERE city_id = @CityId";
            _dbConnection.Execute(sql, new { CityId = cityId });
        }

        public List<Temperature> GetByCityIdAndDateRange(long cityId, DateTime start, DateTime end)
        {
            var sql = @"
                SELECT *
                FROM temperature
                WHERE city_id = @CityId 
                AND time >= @StartDate
                AND time <= @EndDate
            ";

            return _dbConnection.Query<Temperature>(sql, new { CityId = cityId, StartDate = start, EndDate = end }).ToList();
        }
    }
}
