using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherCRUD.Model
{
    public class City
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("city_name")]
        public string CityName { get; set; }
    }
}
