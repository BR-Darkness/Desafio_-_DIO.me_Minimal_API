using MinimalAPI.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using MinimalAPI.DTOs;
using MinimalAPI.Dominio.ModelViews;
using MinimalAPI.Dominio.Entidades;

#region "Builders"
var builder = WebApplication.CreateBuilder(args);

// Escopos:
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

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

#region "Home"
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region "Administradores"
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => 
{    
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login com sucesso");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administradores");
#endregion

#region "Veiculos"

ErrosDeValidacao ValidaDTO(VeiculoDTO veiculoDTO)
{
    ErrosDeValidacao validacao = new();

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ser um campo vazio");

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A marca não pode ser um campo vazio");

    if (veiculoDTO.Ano < 1900)
        validacao.Mensagens.Add("O ano de fabricação do veiculo é muito antigo, somente é aceito veiculos com anos de fabricação superiores a 1900");

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => 
{
    ErrosDeValidacao validacao = ValidaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    Veiculo veiculo = new()
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => 
{
    List<Veiculo> veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => 
{
    Veiculo? veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => 
{
    Veiculo? veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    ErrosDeValidacao validacao = ValidaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0) 
        return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => 
{
    Veiculo? veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(veiculo);

    return Results.NoContent();
}).WithTags("Veiculos");
#endregion

#region "App"
// Iniciando Swagger e Swagger UI:
app.UseSwagger();
app.UseSwaggerUI();

// Iniciando a API:
app.Run();
#endregion