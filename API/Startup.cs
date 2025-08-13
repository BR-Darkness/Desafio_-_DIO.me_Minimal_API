using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Enuns;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Dominio.ModelViews;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Infraestrutura.Db;

namespace MinimalAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
        }

        private string key = string.Empty;

        public IConfiguration Configuration { get; set; } = default!;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option => {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization();

            // Escopos:
            services.AddScoped<IAdministradorServico, AdministradorServico>();
            services.AddScoped<IVeiculoServico, VeiculoServico>();

            // Swagger:
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options => {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT aqui"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement 
                {
                    {
                        new OpenApiSecurityScheme 
                        {
                            Reference = new OpenApiReference 
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // DbContext:
            services.AddDbContext<DbContexto>(options => {
                options.UseMySql(
                    Configuration.GetConnectionString("MySql"),
                    ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
                );
            });
        }
    
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Iniciando Swagger e Swagger UI:
            app.UseSwagger();
            app.UseSwaggerUI();

            // Rotas
            app.UseRouting();

            // Jwt
            app.UseAuthentication();
            app.UseAuthorization();

            // Endpoints
            app.UseEndpoints(endpoints =>
            {
                #region "Home"
                endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion

                #region "Administradores"
                string GerarTokenJwt(Administrador administrador)
                {
                    if (string.IsNullOrEmpty(key)) return string.Empty;

                    SymmetricSecurityKey securityKey = new (Encoding.UTF8.GetBytes(key));
                    SigningCredentials credentials = new (securityKey, SecurityAlgorithms.HmacSha256);

                    List<Claim> claims = new()
                    {
                        new Claim("Email", administrador.Email),
                        new Claim("Perfil", administrador.Perfil),
                        new Claim(ClaimTypes.Role, administrador.Perfil)
                    };

                    JwtSecurityToken token = new(
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }

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

                endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => 
                {
                    Administrador? administrador = administradorServico.Login(loginDTO);

                    if (administrador != null)
                    {
                        string token = GerarTokenJwt(administrador);

                        return Results.Ok(new AdministradorLogado {
                            Email = administrador.Email,
                            Perfil = administrador.Perfil,
                            Token = token
                        });
                    }
                    else
                    {
                        return Results.Unauthorized();
                    }
                }).AllowAnonymous().WithTags("Administradores");

                endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => 
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
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
                .WithTags("Administradores");

                endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => 
                {
                    Administrador? administrador = administradorServico.BuscaPorId(id);
                    if (administrador == null) return Results.NotFound();
                    return Results.Ok(new AdministradorModelView {
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
                .WithTags("Administradores");

                endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => 
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
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
                .WithTags("Administradores");
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

                endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => 
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
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador, Editor" })
                .WithTags("Veiculos");

                endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => 
                {
                    List<Veiculo> veiculos = veiculoServico.Todos(pagina);
                    return Results.Ok(veiculos);
                }).RequireAuthorization().WithTags("Veiculos");

                endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => 
                {
                    Veiculo? veiculo = veiculoServico.BuscaPorId(id);
                    if (veiculo == null) return Results.NotFound();
                    return Results.Ok(veiculo);
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador, Editor" })
                .WithTags("Veiculos");

                endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => 
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
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
                .WithTags("Veiculos");

                endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => 
                {
                    Veiculo? veiculo = veiculoServico.BuscaPorId(id);
                    if (veiculo == null) return Results.NotFound();

                    veiculoServico.Apagar(veiculo);

                    return Results.NoContent();
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
                .WithTags("Veiculos");
                #endregion
            });
        }
    }
}