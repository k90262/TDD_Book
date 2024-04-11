using Microsoft.AspNetCore.Mvc;

namespace Uqs.Weather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    private static readonly int FORECAST_DAYS = 5;
    private readonly ILogger<WeatherForecastController> _logger;

    private string MapFeelToTemp(int temperatureC)
    {
        if (temperatureC <= 0) return Summaries.First();
        int summariesIndex = (temperatureC / 5) + 1;
        if (summariesIndex >= Summaries.Length) return Summaries.Last();
        return Summaries[summariesIndex];
    }

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetRandomWeatherForecast")]
    public IEnumerable<WeatherForecast> GetRandom()
    {
        return Enumerable.Range(1, FORECAST_DAYS).Select(index =>
            {
                var tempC = Random.Shared.Next(-20, 55);
                return new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = tempC,
                    Summary = MapFeelToTemp(tempC)
                };
            })
        .ToArray();
    }
}
