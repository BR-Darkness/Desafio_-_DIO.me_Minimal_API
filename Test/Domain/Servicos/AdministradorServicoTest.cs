using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Infraestrutura.Db;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdministradorServicoTest
    {
        private DbContexto CriarContextoDeTeste()
        {
            string? assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string? path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            return new DbContexto(configuration);
        }

        [TestMethod]
        public void TestandoSalvarAdministrador()
        {
            // Arrange
            DbContexto contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            Administrador administrador = new();
            administrador.Email = "teste@teste.com";
            administrador.Senha = "teste";
            administrador.Perfil = "Administrador";
            AdministradorServico administradorServico = new(contexto);

            // Act
            administradorServico.Incluir(administrador);
            Administrador? administradorTeste = administradorServico.BuscaPorId(administrador.Id);

            // Assert
            Assert.AreEqual(administrador, administradorServico.BuscaPorId(administrador.Id));
            Assert.AreEqual(1, administradorServico.Todos(1).Count());
            Assert.AreEqual(1, administradorTeste.Id);
        }
    }
}