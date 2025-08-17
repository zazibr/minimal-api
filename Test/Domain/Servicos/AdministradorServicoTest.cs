using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

namespace Test.Domain.Servicos;


[TestClass]
public class AdministradorServicoTest
{
    public TestContext TestContext { get; set; }
    private DbContexto CriarContextoDeTeste()
    {
        var options = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets("minimal-api-123456") // ← carrega os segredos
            .AddEnvironmentVariables();

        var configuration = options.Build();

        return new DbContexto(configuration);

    }

    [TestMethod]     public void TestandoSalvarAdministrador()
    {
        // Arrange

        // Criar o contexto de teste e zerando a tabela de administradores
        var contexto = CriarContextoDeTeste();
        contexto.Administradores.RemoveRange(contexto.Administradores);
        contexto.SaveChanges();

        // Criar um administrador para teste
        var administrador = new Administrador();
        administrador.Email = "teste@teste.com";
        administrador.Senha = "Teste";
        administrador.Perfil = "Administrador";

        var administradorServico = new AdministradorServico(contexto);

        // Act
        var resultado = administradorServico.Incluir(administrador);
        var idGerado = resultado.Id;
        TestContext.WriteLine($"ID gerado: {idGerado}");
        

        // Assert
        Assert.AreEqual(idGerado, administrador.Id);
        Assert.AreEqual("teste@teste.com", administrador.Email);
        Assert.AreEqual("Teste", administrador.Senha);
        Assert.AreEqual("Administrador", administrador.Perfil);
    }
}
