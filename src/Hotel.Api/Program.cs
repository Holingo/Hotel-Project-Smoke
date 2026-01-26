using System.Text;
using FluentValidation.AspNetCore;
using Hotel.Application.Abstractions;
using Hotel.Application.Services;
using Hotel.Application.Settings;
using Hotel.Infrastructure;
using Hotel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Settings
builder.Services.Configure<ReservationSettings>(
    builder.Configuration.GetSection("Reservation"));

// Controllers
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Validation
builder.Services.AddFluentValidationAutoValidation();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (DbContext, repositories itd.)
builder.Services.AddInfrastructure(builder.Configuration);

// DbContext ~ Do polaczenia sie z Azure - Oskar
builder.Services.AddScoped<IHotelDbContext>(sp =>
    sp.GetRequiredService<HotelDbContext>());

// Application services
builder.Services.AddScoped<IRoomsService, RoomsService>();
builder.Services.AddScoped<IGuestsService, GuestsService>();
builder.Services.AddScoped<IReservationsService, ReservationsService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

var app = builder.Build();

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

// Middleware
app.UseMiddleware<Hotel.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<Hotel.Api.Middleware.RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Automatyczna migracja
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
