# ControllerGenerator
ControllerGenerator is a tool for dynamically creating an API Controller from a domain service.

# HOW TO USE
1. Flag the methods of the service you want to exposes an API Controller with HttpMethodAttribute class (HttpGet, HttpPost,...)
   ```
   [HttpGet]
   public IEnumerable<WeatherForecast> GetWeatherForecastGet1()
   ```
2. Create the controller before using the ApplicationBuilder by calling the ```CreateController<TService>()``` of the ControllerGenerator class.
   ```
   public static Type CreateController<TService>();
   ```

   You can implement your own routing convention by implementing ```IRoutingConvention``` and call ```CreateController<TService>(IRoutingConvention routingConvention)```.

3. Add the dynamic assembly to the ApplicationBuilder
   ```
   var applicationBuilder = WebApplication.CreateBuilder(args);
   applicationBuilder.Services.AddMvc().AddApplicationPart(ControllerGenerator.ControllerGenerator.DynamicAssembly);
   ```
4. Enjoy !
