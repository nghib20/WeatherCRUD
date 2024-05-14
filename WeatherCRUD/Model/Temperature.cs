using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherCRUD.Model
{
    public class Temperature
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("city_id")]
        public long CityId { get; set; }

        [Column("temperature")]
        public double TemperatureValue { get; set; }
        
        [Column("time")]
        public DateTime Time { get; set; }
    }
}
