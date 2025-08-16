using MinimalApi.Dominio.Entidades;

namespace Test.Domain.Entidades;

[TestClass]
public sealed class VeiculosTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Arrange
        var veiculo = new Veiculo();

        // Act
        veiculo.Id = 1;
        veiculo.Nome = "HAN - Daniel";
        veiculo.Marca = "BYD";
        veiculo.Ano = 2026;

        // Assert
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("HAN - Daniel", veiculo.Nome);
        Assert.AreEqual("BYD", veiculo.Marca);
        Assert.AreEqual(2026, veiculo.Ano);
    }
}
