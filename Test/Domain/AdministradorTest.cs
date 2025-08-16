using MinimalApi.Dominio.Entidades;

namespace Test.Domain.Entidades;

[TestClass]
public sealed class AdministradorTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Arrange
        var administrador = new Administrador();

        // Act
        administrador.Id = 1;
        administrador.Email = "teste@teste.com";
        administrador.Senha = "Teste";
        administrador.Perfil = "Administrador";

        // Assert
        Assert.AreEqual(1, administrador.Id);
        Assert.AreEqual("teste@teste.com", administrador.Email);
        Assert.AreEqual("Teste", administrador.Senha);
        Assert.AreEqual("Administrador", administrador.Perfil);
    }
}
