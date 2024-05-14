using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using WeatherCRUD.Model;
using WeatherCRUD.repository;
using WeatherCRUD.Service;

SqlMapper.SetTypeMap(
    typeof(City),
    new CustomPropertyTypeMap(
        typeof(City),
        (type, columnName) =>
            type.GetProperties().FirstOrDefault(prop =>
                prop.GetCustomAttributes(false)
                    .OfType<ColumnAttribute>()
                    .Any(attr => attr.Name == columnName))));

SqlMapper.SetTypeMap(
    typeof(Temperature),
    new CustomPropertyTypeMap(
        typeof(Temperature),
        (type, columnName) =>
            type.GetProperties().FirstOrDefault(prop =>
                prop.GetCustomAttributes(false)
                    .OfType<ColumnAttribute>()
                    .Any(attr => attr.Name == columnName))));
//Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ITemperatureRepository, TemperatureRepository>();

builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

app.MapGet("/get-cities", (IWeatherService service) =>
{
    return Results.Ok(service.GetCities());
});

app.MapDelete("/delete-city/{cityName}", (string cityName, IWeatherService service) =>
{
    service.DeleteCity(cityName);
    return Results.NoContent();
});

app.MapGet("/get-temps-by-city-and-range/{cityName}/{start}/{end}", (string cityName, DateTime start, DateTime end, IWeatherService service) =>
{
    return Results.Ok(service.GetTempsByCityAndRange(cityName, start, end));
});

app.Run();