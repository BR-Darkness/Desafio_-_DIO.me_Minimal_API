using MinimalAPI.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class AdministradorTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            // Arrange
            Administrador administrador = new();

            // Act
            administrador.Id = 1;
            administrador.Email = "teste@teste.com";
            administrador.Senha = "teste";
            administrador.Perfil = "Administrador";

            // Assert
            Assert.AreEqual(1, administrador.Id);
            Assert.AreEqual("teste@teste.com", administrador.Email);
            Assert.AreEqual("teste", administrador.Senha);
            Assert.AreEqual("Administrador", administrador.Perfil);
        }
    }
}