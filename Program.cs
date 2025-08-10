using MinimalAPI.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using MinimalAPI.DTOs;

#region "Builder"
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});
#endregion

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/Teste", () => "Olá Mundo!");

app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
    if (administradorServico.Login(loginDTO) != null)
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
});

app.Run();