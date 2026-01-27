using System.Text;
using FluentValidation.AspNetCore;
using Hotel.Application.Abstractions;
using Hotel.Application.Services;
using Hotel.Application.Settings;
using Hotel.Infrastructure;
using Hotel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REJESTRACJA USŁUG (Services) ---

builder.Services.AddControllers().AddNewtonsoftJson();

// Konfiguracja JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

// Konfiguracja Swaggera z obsługą Bearer Token
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel API", Version = "v1" });

    // Definicja zabezpieczeń (przycisk Authorize)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Wpisz sam token JWT (bez słowa Bearer)."
    });

    // Wymaganie tokena dla wszystkich endpointów
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddFluentValidationAutoValidation();

// Infrastruktura i DI
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IHotelDbContext>(sp =>
    sp.GetRequiredService<HotelDbContext>());

builder.Services.AddScoped<IRoomsService, RoomsService>();
builder.Services.AddScoped<IGuestsService, GuestsService>();
builder.Services.AddScoped<IReservationsService, ReservationsService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

// --- 2. BUDOWA APLIKACJI (Build) ---

var app = builder.Build();

// --- 3. POTOK MIDDLEWARE (Pipeline) ---

app.UseMiddleware<Hotel.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<Hotel.Api.Middleware.RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel API v1");
    });

    // Automatyczna migracja
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

    // UWAGA: Jeśli Azure nadal wyrzuca błąd, zakomentuj poniższą linię, aby chociaż Swagger ruszył
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }