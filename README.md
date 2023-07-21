# ControllerGenerator
ControllerGenerator is a tool for dynamically creating an API Controller from a domain service at runtime.

## Why use it
The ControllerGenerator is a useful tool for ASP.NET WebApps that enables dynamic creation of ApiController classes at runtime. This approach offers several benefits and use cases:
- Reduced Boilerplate Code: With ControllerGenerator, you can generate ApiController classes on-the-fly without the need to write extensive boilerplate code manually. This leads to cleaner and more concise codebases.

Dynamic API Endpoints: It allows you to create API endpoints dynamically, which can be particularly helpful when you have a large number of similar endpoints with minor variations.

Plugin Architecture: When building extensible applications, you can use ControllerGenerator to dynamically load and integrate external or third-party API controllers without having to recompile your main project.

Customization and Configuration: By generating ApiController classes at runtime, you can customize the behavior of API endpoints based on configuration files, database settings, or user input, providing greater flexibility to your application.

Versioning and Feature Toggling: ControllerGenerator can be leveraged to implement API versioning, allowing you to maintain backward compatibility while introducing new features to the API.

Testing and Mocking: In unit testing scenarios, ControllerGenerator can be beneficial for creating mock ApiController classes with predefined responses, making it easier to isolate and test different parts of your application.

Rapid Prototyping: During the early stages of development or when prototyping new features, using ControllerGenerator can accelerate the process of creating temporary or experimental API endpoints.

Microservices and Multi-Tenancy: In microservices architectures or multi-tenant applications, ControllerGenerator can dynamically generate controllers based on tenant-specific configurations or service requirements.

## Get Started
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
