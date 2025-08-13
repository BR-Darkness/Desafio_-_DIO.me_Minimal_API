using System.Net;
using System.Text;
using System.Text.Json;
using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.ModelViews;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public class AdministradorRequestTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext) => Setup.ClassInit(testContext);

        [ClassCleanup]
        public static void ClassCleanup() => Setup.ClassCleanup();

        [TestMethod]
        public async Task TestMethod1()
        {
            // Arrange
            LoginDTO loginDTO = new() { 
                Email = "adm@teste.com", 
                Senha = "123456" 
            };

            StringContent content = new(
                JsonSerializer.Serialize(loginDTO),
                Encoding.UTF8,
                "Application/json"
            );

            // Act
            HttpResponseMessage? response = await Setup.client.PostAsync("/administradores/login", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string? result = await response.Content.ReadAsStringAsync();
            
            AdministradorLogado? admLogado = JsonSerializer.Deserialize<AdministradorLogado>(
                result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            Assert.IsNotNull(admLogado?.Email ?? "");
            Assert.IsNotNull(admLogado?.Perfil ?? "");
            Assert.IsNotNull(admLogado?.Token ?? "");
        }
    }
}