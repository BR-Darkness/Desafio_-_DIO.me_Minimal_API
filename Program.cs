using MinimalAPI.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using MinimalAPI.DTOs;
using MinimalAPI.Dominio.ModelViews;

#region "Builders"
var builder = WebApplication.CreateBuilder(args);

// Escopos:
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();

// Swagger:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext:
builder.Services.AddDbContext<DbContexto>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});
#endregion

// Realiza o build da aplicação:
WebApplication app = builder.Build();

#region "Mapeando Endpoints"
app.MapGet("/", () => Results.Json(new Home()));

app.MapGet("/Teste", () => "Olá Mundo!");

app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => 
{    
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login com sucesso");
    }
    else
    {
        return Results.Unauthorized();
    }
});
#endregion

// Iniciando Swagger e Swagger UI:
app.UseSwagger();
app.UseSwaggerUI();

// Iniciando a API:
app.Run();