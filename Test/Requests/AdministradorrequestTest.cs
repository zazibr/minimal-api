using System.Net;
using System.Text;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.DOTs;
using Newtonsoft.Json;
using Test.Helpers;

namespace Test.Reqrests;

[TestClass]
public sealed class AdministradorRequestTest
{
        [ClassInitialize]
        public static void ClassInit(TestContext testContext) {
            Setup.ClassInit(testContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Setup.ClassCleanup();
        }

    [TestMethod]
    public async Task TestarGetSetPropriedades()
    {
        // Arrange
        var loginDTO = new LoginDTO
        {
            Email = "adm@teste.com",
            Senha = "654321"
        };

        var content = new StringContent(JsonConvert.SerializeObject(loginDTO), Encoding.UTF8, "application/json");

        // Act
        var response = await Setup.client.PostAsync("/administradores/login", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var admLogado = JsonConvert.DeserializeObject<AdministradoLogado>(result);

        Assert.IsFalse(string.IsNullOrEmpty(admLogado?.Email));
        Assert.IsFalse(string.IsNullOrEmpty(admLogado?.Perfil));
        Assert.IsFalse(string.IsNullOrEmpty(admLogado?.Token));
    }
}
