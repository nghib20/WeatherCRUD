using Dapper;
using MySqlConnector;
using System.ComponentModel.DataAnnotations.Schema;
using WeatherCRUD.Model;
using WeatherCRUD.repository;

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

var app = builder.Build();

app.MapGet("/get-cities", (ICityRepository cityRepository, ITemperatureRepository temperatureRepository) =>
{
    foreach(City city in cityRepository.GetCities())
    {
        Console.WriteLine($"{city.Id} {city.CityName}");
    }
});

app.Run();