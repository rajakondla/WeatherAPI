using Collette.Data;
using Microsoft.Extensions.Configuration;
using WeatherAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<WeatherForcastService>();
builder.Services.Configure<DbConnectionOption>(options =>
{
    options.ConnectionString = builder.Configuration["ConnectionStrings:Db"]!;
});
builder.Services.AddScoped<IDbConnectionWrapper, DbConnectionWrapper>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();

app.Run();
