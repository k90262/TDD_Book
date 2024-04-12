using AdamTibi.OpenWeather;
using Microsoft.AspNetCore.Mvc;
using Uqs.Weather.Wrappers;

namespace Uqs.Weather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const int FORECAST_DAYS = 5;
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _config;
    private readonly IClient _client;
    private readonly INowWrapper _nowWrapper;
    private readonly IRandomWrapper _randomWrapper;
    
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

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger, 
        IConfiguration config, 
        IClient client,
        INowWrapper nowWrapper,
        IRandomWrapper randomWrapper
        )
    {
        _logger = logger;
        _config = config;
        _client = client;
        _nowWrapper = nowWrapper;
        _randomWrapper = randomWrapper;
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
                var tempC = _randomWrapper.Next(-20, 55);
                return new WeatherForecast
                {
                    Date = _nowWrapper.Now.AddDays(index),
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
        OneCallResponse res = await _client.OneCallAsync(GREENWICH_LAT, GREENWICH_LON, new []
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
