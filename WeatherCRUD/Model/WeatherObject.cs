namespace WeatherCRUD.Model
{
    public class WeatherObject
    {
        public Weather[] Weather { get; set; }
        public Main Main { get; set; }
    }

    public class Main
    {
        public string Temp { get; set; }
    }

    public class Weather
    {
        public string Main { get; set; }
    }
}
