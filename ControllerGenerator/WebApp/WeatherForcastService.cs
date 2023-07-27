using AttributeSharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace WebApp
{
    public class WeatherForcastService
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private IWeatherForecast WeatherForcast { get; set; }

        public WeatherForcastService(IWeatherForecast weatherForecast) 
        {
            WeatherForcast = weatherForecast;
        }

        [SignatureVerified]
        [HttpGet]
        public IEnumerable<WeatherForecast> GetWeatherForecastGet1()
        {
            return WeatherForcast.GetWeatherForecastGet10();
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> GetWeatherForecastGet2(string test)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test
            })
            .ToArray();
        }

        [SignatureVerified]
        [HttpGet]
        public IEnumerable<WeatherForecast> GetWeatherForecastGet3(string test, string test2)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test + test2
            })
            .ToArray();
        }

        [SignatureVerified]
        [HttpPost]
        public IEnumerable<WeatherForecast> GetWeatherForecastPost1()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public IEnumerable<WeatherForecast> GetWeatherForecastPost2(string test)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test
            })
            .ToArray();
        }

        [SignatureVerified]
        [HttpPost]
        public IEnumerable<WeatherForecast> GetWeatherForecastPost3(string test, string test2)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test + test2
            })
            .ToArray();
        }

        [HttpPut]
        public IEnumerable<WeatherForecast> GetWeatherForecastPut1()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPut]
        public IEnumerable<WeatherForecast> GetWeatherForecastPut2(string test)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test
            })
            .ToArray();
        }

        [HttpPut]
        public IEnumerable<WeatherForecast> GetWeatherForecastPut3(string test, string test2)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test + test2
            })
            .ToArray();
        }

        [HttpDelete]
        public IEnumerable<WeatherForecast> GetWeatherForecastDelete1()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpDelete]
        public IEnumerable<WeatherForecast> GetWeatherForecastDelete2(string test)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test
            })
            .ToArray();
        }

        [HttpDelete]
        public IEnumerable<WeatherForecast> GetWeatherForecastDelete3(string test, string test2)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test + test2
            })
            .ToArray();
        }

        [HttpPatch]
        public IEnumerable<WeatherForecast> GetWeatherForecastPatch1()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPatch]
        public IEnumerable<WeatherForecast> GetWeatherForecastPatch2(string test)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test
            })
            .ToArray();
        }

        [HttpPatch]
        public IEnumerable<WeatherForecast> GetWeatherForecastPatch3(string test, string test2)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = test + test2
            })
            .ToArray();
        }
    }
}
