using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Server.BackgroundServices;
using Server.Data;
using Server.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ServerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ServerContext") ?? throw new InvalidOperationException("Connection string 'ServerContext' not found.")));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHostedService<SendRowBackgroundService>();
builder.Services.AddSingleton<SendRowBackgroundService>();
builder.Services.AddTransient<IGenerator<Product>, ProductGenerator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await using var dbContext = scope.ServiceProvider.GetService<ServerContext>();
    await dbContext!.Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapHub<SendRowHub>("/send_row");

app.Run();
