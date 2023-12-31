using Albon.ControllerGenerator;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

ControllerGenerator controllerGenerator = new ControllerGenerator();

controllerGenerator.CreateController<WeatherForcastService>();
controllerGenerator.CreateController<WeatherForcastService2>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMvc().AddApplicationPart(controllerGenerator.DynamicAssembly);
builder.Services.AddScoped<IWeatherForecast, WeatherForcastService2>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
