namespace WeatherCRUD.Model
{
    public record WeatherApiRequest(string CityName);
    public record WeatherApiResponse(string CityName, string Temperature);

    public record WeatherResponse(WeatherApiResponse WeatherApiResponse, string message);
}
