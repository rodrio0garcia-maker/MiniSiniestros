using Microsoft.EntityFrameworkCore;
using MiniSiniestros.Data;
using MiniSiniestros.Data.Repositories;
using MiniSiniestros.Services;
using MiniSiniestros.Services.Mapping;
using Serilog;
using Serilog.Formatting.Json;
using System.Text.Json.Serialization;

// Serilog ANTES de crear el builder, así captura también los logs que pasan durante el arranque de la app
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilog en vez del logger default
builder.Host.UseSerilog();

builder.Services.AddDbContext<MiniSiniestrosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ISiniestroService, SiniestroService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddAutoMapper(typeof(MappingProfile));

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
