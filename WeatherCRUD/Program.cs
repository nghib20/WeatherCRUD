using Dapper;
using MySqlConnector;
using System.ComponentModel.DataAnnotations.Schema;
using WeatherCRUD.Model;

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
var app = builder.Build();

using var connection = new MySqlConnection("Server=localhost;Port=3306;User ID=root;Password=rootroot;Database=weather_db");
connection.Open();

var sql = "SELECT * FROM city";
var cities = connection.Query<City>(sql);

foreach (var city in cities)
{
    Console.WriteLine($"{city.Id} {city.CityName}");
}

sql = "SELECT * FROM temperature";
var temps = connection.Query<Temperature>(sql);

foreach (var temp in temps)
{
    Console.WriteLine($"{temp.Id} {temp.CityId} {temp.TemperatureValue} {temp.Time}");
}

app.MapGet("/", () => "Hello World!");

app.Run();