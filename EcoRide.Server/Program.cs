using EcoRide.Server.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EcoRide.Server.Model;
/*
using (var context = new AppDbContext())
{
    // Assure la création de la base de données si elle n'existe pas
    context.Database.EnsureCreated();

    // Ajouter un utilisateur
    context.covoiturage.Add(new Covoiturage { DateDepart = DateTime.Now});
    context.SaveChanges();

    // Lire les utilisateurs
    var covoiturages = context.covoiturage.ToList();
    foreach (var covoiturage in covoiturages)
    {
        Console.WriteLine($"ID: {covoiturage.Id}, Nom: {covoiturage.Nom}");
    }
}
*/

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
builder.Services.AddScoped<CovoiturageService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder => builder
            .WithOrigins("http://localhost:4200") // URL de ton app Angular
            .AllowAnyHeader()
            .AllowAnyMethod());
});




var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAngular");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
