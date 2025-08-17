using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

namespace Test.Domain.Servicos;


[TestClass]
public class VeiculosServicoTest
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

    [TestMethod]     public void TestandoSalvarVeiculo()
    {
        // Arrange

        // Criar o contexto de teste e zerando a tabela de administradores
        var contexto = CriarContextoDeTeste();
        contexto.Veiculos.RemoveRange(contexto.Veiculos);
        contexto.SaveChanges();

        // Criar um administrador para teste
        var veiculo = new Veiculo();
        veiculo.Nome = "HAN - Daniel - TESTE";
        veiculo.Marca = "BYD TESTE";
        veiculo.Ano = 2026;

        var veiculoServico = new VeiculoServico(contexto);

        // Act
        var resultado = veiculoServico.Incluir(veiculo);
        var idGerado = resultado.Id;
        TestContext.WriteLine($"ID gerado: {idGerado}");
        

        // Assert
        Assert.AreEqual(idGerado, veiculo.Id);
        Assert.AreEqual("HAN - Daniel - TESTE", veiculo.Nome);
        Assert.AreEqual("BYD TESTE", veiculo.Marca);
        Assert.AreEqual(2026, veiculo.Ano);
    }
}
