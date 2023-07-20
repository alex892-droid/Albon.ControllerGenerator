using ControllerGenerator;

var builder = WebApplication.CreateBuilder(args);

ControllerGenerator.ControllerGenerator.CreateController<WeatherForcastService>(new DefaultRoutingConvention());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMvc().AddApplicationPart(ControllerGenerator.ControllerGenerator.DynamicAssembly);
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
