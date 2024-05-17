using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;
using WeatherCRUD.Model;
using WeatherCRUD.repository;
using WeatherCRUD.Service;

setUpMappings();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddSingleton<ICityRepository, CityRepository>();
builder.Services.AddSingleton<ITemperatureRepository, TemperatureRepository>();

// API client for OpenWeatherMap
builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
{
    string url = "https://api.openweathermap.org/";
    client.BaseAddress = new Uri(url);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddSingleton<IWeatherService, WeatherService>();

var app = builder.Build();

app.MapGet("/get-cities", (IWeatherService service) =>
{
    return Results.Ok(service.GetCities());
});

app.MapDelete("/delete-city/{cityName}", (string cityName, IWeatherService service) =>
{
    service.DeleteCity(cityName);
    return TypedResults.NoContent();
});

app.MapGet("/get-temps-by-city-and-range/{cityName}/{start}/{end}", (string cityName, DateTime start, DateTime end, IWeatherService service) =>
{
    return Results.Ok(service.GetTempsByCityAndRange(cityName, start, end));
});

app.MapPost("/get-temp-for-city/{cityName}", async (string cityName, IWeatherService service) =>
{
    var response = await service.GetWeather(new WeatherApiRequest(cityName));
    if (response.WeatherApiResponse == null)
    {
        return Results.NotFound(response);
    }
    return Results.Ok(response);
});

app.Run();

/*
 * maps column names to class fields
 */
void setUpMappings()
{
    // city mapping
    SqlMapper.SetTypeMap(
    typeof(City),
    new CustomPropertyTypeMap(
        typeof(City),
        (type, columnName) =>
            type.GetProperties().FirstOrDefault(prop =>
                prop.GetCustomAttributes(false)
                    .OfType<ColumnAttribute>()
                    .Any(attr => attr.Name == columnName))));

    // temperature mapping
    SqlMapper.SetTypeMap(
        typeof(Temperature),
        new CustomPropertyTypeMap(
            typeof(Temperature),
            (type, columnName) =>
                type.GetProperties().FirstOrDefault(prop =>
                    prop.GetCustomAttributes(false)
                        .OfType<ColumnAttribute>()
                        .Any(attr => attr.Name == columnName))));
}

public partial class WeatherController { }