using System.Runtime.InteropServices;
using Application.Activities;
using Application.Activities.Commands;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt=>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors();  //SINCE IT ADDS AN HEADer we need to add middleware
builder.Services.AddMediatR(x =>
x.RegisterServicesFromAssemblyContaining<CreateActivity.Handler>());

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var app = builder.Build();

// Configure the HTTP request pipeline. add middleware that can access the http request as it comes in, manipulate request, authhenticate the request, check and on itw way out

app.UseCors(x =>x.AllowAnyHeader().AllowAnyMethod()
.WithOrigins("http://localhost:3000", "https://localhost:3000"));

app.MapControllers();

using var scope = app.Services.CreateScope();  //anything we use in this scope will be cleaned up once we leave this scope
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedData(context);
}
catch (Exception ex)
{
    
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error ocurred during migration.");
}

app.Run();
