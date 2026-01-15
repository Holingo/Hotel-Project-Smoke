using FluentValidation.AspNetCore;
using Hotel.Application.Services;
using Hotel.Application.Settings;
using Hotel.Application.Abstractions;
using Hotel.Infrastructure;
using Hotel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ReservationSettings>(builder.Configuration.GetSection("Reservation"));

builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

// Bridge DbContext -> abstraction (clean architecture)
builder.Services.AddScoped<IHotelDbContext>(sp => sp.GetRequiredService<HotelDbContext>());

// Application services
builder.Services.AddScoped<IRoomsService, RoomsService>();
builder.Services.AddScoped<IGuestsService, GuestsService>();
builder.Services.AddScoped<IReservationsService, ReservationsService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

var app = builder.Build();

app.UseMiddleware<Hotel.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<Hotel.Api.Middleware.RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Auto-migrate in Development (optional but convenient)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();

app.Run();

public partial class Program { } // for tests
