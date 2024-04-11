using AdamTibi.OpenWeather;
using Microsoft.AspNetCore.Mvc;

namespace Uqs.Weather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const int FORECAST_DAYS = 5;
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _config;
    
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private string MapFeelToTemp(int temperatureC)
    {
        if (temperatureC <= 0) return Summaries.First();
        int summariesIndex = (temperatureC / 5) + 1;
        if (summariesIndex >= Summaries.Length) return Summaries.Last();
        return Summaries[summariesIndex];
    }

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [HttpGet("ConvertCToF")]
    public double ConvertCToF(double c)
    {
        double f = c * (9d / 5d) + 32;
        _logger.LogInformation("conversion requested");
        return f;
    }

    [HttpGet("GetRandomWeatherForecast")]
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
    
    
    [HttpGet("GetRealWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> GetReal()
    {
        const decimal GREENWICH_LAT = 51.4810m;
        const decimal GREENWICH_LON = 0.0052m;
        string apiKey = _config["OpenWeather:Key"];
        HttpClient httpClient = new HttpClient();
        Client openWeatherClient = new Client(apiKey, httpClient);
        OneCallResponse res = await openWeatherClient.OneCallAsync(GREENWICH_LAT, GREENWICH_LON, new []
        {
            Excludes.Current, Excludes.Minutely, Excludes.Hourly, Excludes.Alerts
        }, Units.Metric);
        return Enumerable.Range(1, FORECAST_DAYS).Select(index =>
            {
                double forecastedTemp = res.Daily[index].Temp.Day;
                int tempC = (int)Math.Round(forecastedTemp);
                return new WeatherForecast
                {
                    Date = res.Daily[index].Dt,
                    TemperatureC = tempC,
                    Summary = MapFeelToTemp(tempC)
                };
            })
            .ToArray();
    }
}
