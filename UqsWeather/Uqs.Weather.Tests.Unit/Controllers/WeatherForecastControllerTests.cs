using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Uqs.Weather.Controllers;

namespace Uqs.Weather.Tests.Unit.Controllers;

[TestSubject(typeof(WeatherForecastController))]
public class WeatherForecastControllerTests
{
    private double _actual;

    [Fact]
    public void ConvertCToF_0Celsius_32Fahrenheit()
    {
        const double expected = 32d;
        WhenConvertCToF(0);
        Assert.Equal(expected, _actual);
    }

    private void WhenConvertCToF(double c)
    {
        var logger = NullLogger<WeatherForecastController>.Instance;
        var controller = new WeatherForecastController(logger, null!, null!, null!, null!);
        _actual = controller.ConvertCToF(c);
    }

    [Theory]
    [InlineData(-100, -148)]
    [InlineData(-10.1, 13.8)]
    [InlineData(10, 50)]
    public void ConvertCToF_Cel_CorrectFah(double c, double f)
    {
        double expected = f;
        WhenConvertCToF(c);
        Assert.Equal(expected, _actual, 1);
    }
}