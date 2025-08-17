using MinimalApi;


IHostBuilder  CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

}

CreateHostBuilder(args).Build().Run();

// ✅ [ADICIONAR AQUI] — Esta linha torna a classe Program acessível ao projeto de testes
public partial class Program { }
