using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var server = "sleedy.postgres.database.azure.com";
var name = "SleedyDb";
var port = "5432";
var userId = "aziiadmin";
var password = "EduLab@22";
var connectionString = $"Server={server};Database={name};Port={port};User Id={userId};Password={password};";

builder.Services.AddDbContext<ServerContext>(
    options=>options.UseNpgsql(connectionString)
    );

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5284/").AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();
