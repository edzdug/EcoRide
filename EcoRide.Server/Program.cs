using EcoRide.Server.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EcoRide.Server.Model;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddTransient<EmailService>();
builder.Services.AddScoped<CovoiturageService>();
builder.Services.AddScoped<UtilisateurService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<ParticipationService>();
builder.Services.AddScoped<VoitureService>();

builder.Services.AddSingleton<JwtService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
