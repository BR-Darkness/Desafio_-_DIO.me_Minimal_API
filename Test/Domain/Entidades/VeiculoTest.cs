using MinimalAPI.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class VeiculoTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            // Arrange
            Veiculo veiculo = new();

            // Act
            veiculo.Id = 1;
            veiculo.Nome = "teste";
            veiculo.Marca = "marca teste";
            veiculo.Ano = 1995;

            // Assert
            Assert.AreEqual(1, veiculo.Id);
            Assert.AreEqual("teste", veiculo.Nome);
            Assert.AreEqual("marca teste", veiculo.Marca);
            Assert.AreEqual(1995, veiculo.Ano);
        }
    }
}