using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniSiniestros.Api.Configuration;
using MiniSiniestros.Data;
using MiniSiniestros.Data.Repositories;
using MiniSiniestros.Services;
using MiniSiniestros.Services.Mapping;
using Serilog;
using Serilog.Formatting.Json;
using System.Text;
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

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta configuración de Jwt");

builder.Services.AddSingleton<JwtSettingsBase>(jwtSettings);
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Aplica migraciones pendientes automáticamente al arrancar -
// necesario para que "docker compose up" funcione sin pasos manuales
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MiniSiniestrosDbContext>();
    db.Database.Migrate();

    // Seed de datos de prueba, solo si la base está vacía 
    if (!db.Siniestros.Any())
    {
        var siniestro1 = new MiniSiniestros.Entities.Siniestro { CuitEmpleador = "20304050607", CuilTrabajador = "27111222339", Estado = MiniSiniestros.Entities.Enums.EstadoSiniestro.EnAnalisis, FechaOcurrencia = new DateTime(2026, 8, 1), FechaAlta = new DateTime(2026, 8, 1) };
        var siniestro2 = new MiniSiniestros.Entities.Siniestro { CuitEmpleador = "20111222338", CuilTrabajador = "20222333449", Estado = MiniSiniestros.Entities.Enums.EstadoSiniestro.Recibido, FechaOcurrencia = new DateTime(2026, 7, 28), FechaAlta = new DateTime(2026, 7, 28) };
        var siniestro3 = new MiniSiniestros.Entities.Siniestro { CuitEmpleador = "27333444559", CuilTrabajador = "23444555669", Estado = MiniSiniestros.Entities.Enums.EstadoSiniestro.Aprobado, FechaOcurrencia = new DateTime(2026, 7, 15), FechaAlta = new DateTime(2026, 7, 16) };
        var siniestro4 = new MiniSiniestros.Entities.Siniestro { CuitEmpleador = "20555666779", CuilTrabajador = "27666777889", Estado = MiniSiniestros.Entities.Enums.EstadoSiniestro.Rechazado, FechaOcurrencia = new DateTime(2026, 7, 10), FechaAlta = new DateTime(2026, 7, 11) };
        var siniestro5 = new MiniSiniestros.Entities.Siniestro { CuitEmpleador = "23777888990", CuilTrabajador = "20888999009", Estado = MiniSiniestros.Entities.Enums.EstadoSiniestro.Cerrado, FechaOcurrencia = new DateTime(2026, 6, 20), FechaAlta = new DateTime(2026, 6, 22) };

        var prestador = new MiniSiniestros.Entities.Prestador { Nombre = "Clínica San Martín", Especialidad = "Traumatología" };

        // El siniestro1 arranca con historial simulado (Recibido -> EnAnalisis)
        // y con el prestador ya asignado, para que el detalle se vea completo
        // apenas se levanta el contenedor, sin pasos manuales
        siniestro1.Prestadores.Add(prestador);
        siniestro1.Historial.Add(new MiniSiniestros.Entities.HistorialEstado
        {
            EstadoAnterior = MiniSiniestros.Entities.Enums.EstadoSiniestro.Recibido,
            EstadoNuevo = MiniSiniestros.Entities.Enums.EstadoSiniestro.EnAnalisis,
            FechaCambio = new DateTime(2026, 8, 2)
        });

        db.Siniestros.AddRange(siniestro1, siniestro2, siniestro3, siniestro4, siniestro5);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
