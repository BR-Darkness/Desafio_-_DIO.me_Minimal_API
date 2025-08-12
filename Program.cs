using MinimalAPI.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Dominio.ModelViews;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using MinimalAPI.Dominio.Enuns;

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
static ErrosDeValidacao ValidaAdministradorDTO(AdministradorDTO administradorDTO)
{
    ErrosDeValidacao validacao = new();

    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("O campo email não pode ser vazio");

    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("O campo senha não pode ser vazio");

    if (administradorDTO.Perfil == null)
        validacao.Mensagens.Add("O campo perfil não pode ser vazio");

    return validacao;
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => 
{    
    return (administradorServico.Login(loginDTO) != null) 
    ? Results.Ok("Login com sucesso")
    : Results.Unauthorized();
}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => 
{
    List<AdministradorModelView> listaAdministradores = new();

    foreach (var administrador in administradorServico.Todos(pagina))
    {
        listaAdministradores.Add(new AdministradorModelView {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });
    }

    return Results.Ok(listaAdministradores);
}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => 
{
    Administrador? administrador = administradorServico.BuscaPorId(id);
    if (administrador == null) return Results.NotFound();
    return Results.Ok(new AdministradorModelView {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => 
{    
    ErrosDeValidacao validacao = ValidaAdministradorDTO(administradorDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    Administrador administrador = new()
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? TipoPerfil.Editor.ToString()
    };

    administradorServico.Incluir(administrador);

    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).WithTags("Administradores");
#endregion

#region "Veiculos"
static ErrosDeValidacao ValidaVeiculoDTO(VeiculoDTO veiculoDTO)
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
    ErrosDeValidacao validacao = ValidaVeiculoDTO(veiculoDTO);
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

    ErrosDeValidacao validacao = ValidaVeiculoDTO(veiculoDTO);
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