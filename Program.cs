using Microsoft.AspNetCore.Mvc;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.DOTs;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Emums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;



#region Builder
var builder = WebApplication.CreateBuilder(args);

var keyJwt = builder.Configuration.GetValue<string>("Jwt:Key");
if (string.IsNullOrWhiteSpace(keyJwt))
{
    throw new ArgumentException("A chave JWT não foi configurada no appsettings.json");
}

var tamanhoChave = System.Text.Encoding.UTF8.GetByteCount(keyJwt);

Console.WriteLine(new string('*', 72));
Console.WriteLine($"Chave JWT: {keyJwt}");
Console.WriteLine($".........: {tamanhoChave}");

Console.WriteLine(new string('*', 72));



if (tamanhoChave < 32)
{
    Console.WriteLine("⚠️ Aviso: A chave JWT deve ter pelo menos 32 caracteres para garantir a segurança.");
}



// Configuração do serviço de autenticação JWT
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme =JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyJwt))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>( options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

var app = builder.Build();
var swaggerAtivo = builder.Configuration.GetValue<bool>("Swagger:Enabled");
#endregion


#region Home
// 🌐 Rota GET simples para testar se a API está funcionando
app.MapGet("/", () =>
{
    return Results.Json(new Home(swaggerAtivo));
}).WithTags("Home");
#endregion

#region Administradores

string GerarTokenJwt(Administrador administrador, string keyJwt)
{
    var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyJwt));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);

        
}   

// 🔐 Rota POST para login, usando DTO para receber os dados
    app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    var adm = administradorServico.Login(loginDTO);
    if (adm != null)
    {
        string token = GerarTokenJwt(adm, keyJwt);
        return Results.Ok(new AdministradoLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
         });
    }
    else
    {
        return Results.Unauthorized();
    }

}).WithTags("Administradores");


// 🛠️ Rota GET para listar todos os administradores, com paginação opcional
app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina ?? 1);
    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        } );
    }
  
        return Results.Ok(adms);
 

}).RequireAuthorization().WithTags("Administradores").WithDescription("Listar todos os Administradores por Paginação.");



// 🛠️ Rota POST para criar um novo administrador, usando DTO para receber os dados
app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{

     var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
     };
  

    if (string.IsNullOrWhiteSpace(administradorDTO.Email))
    {
        validacao.Mensagens.Add("Email é obrigatório.");
    }

    if (string.IsNullOrWhiteSpace(administradorDTO.Senha))
    {
        validacao.Mensagens.Add("O Senha é obrigatório.");
    }

    if (administradorDTO.Perfil == null)
    {
        validacao.Mensagens.Add("O Perfil é obrigatório.");
    }

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };
    administradorServico.Incluir(administrador);
    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });

}).RequireAuthorization().WithTags("Administradores").WithDescription("Incluir novo administrador.");

// 🛠️ Rota GET para buscar um administrador por ID
app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null)
    {
        return Results.NotFound();
    } else
    {
        return Results.Ok(new AdministradorModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });
    }
    
}).RequireAuthorization().WithTags("Administradores").WithDescription("Busca um administrador pelo ID.");


#endregion

#region Veículos

ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
     };

    if (string.IsNullOrWhiteSpace(veiculoDTO.Nome))
    {
        validacao.Mensagens.Add("O nome do veículo é obrigatório.");
    }

    if (string.IsNullOrWhiteSpace(veiculoDTO.Marca))
    {
        validacao.Mensagens.Add("A marca do veículo é obrigatória.");
    }

    if (veiculoDTO.Ano > 0 && veiculoDTO.Ano <= 1949)
    {
        validacao.Mensagens.Add("O ano do veículo é obrigatório. deve ser maior que 1949.");
    }
    else if (veiculoDTO.Ano < 0)
    {
        validacao.Mensagens.Add("O ano do veículo não pode ser negativo.");
    }
    else if (veiculoDTO.Ano == 0)
    {
        validacao.Mensagens.Add("O ano do veículo ser zero.");

    }
    return validacao;
    
}

// 🛠️ Rota POST para criar um novo veículo, usando DTO para receber os dados
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }


    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization().WithTags("Veículos").WithDescription("Incluir Novo Veículo.");

// 🛠️ Rota GET para listar todos os veículos, com paginação opcional
app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina ?? 1);
    return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veículos").WithDescription("Listar Todos os Veículos por Paginação.");


// 🛠️ Rota GET para buscar um veículo por ID
app.MapGet("/veiculo/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    } else
    {
        return Results.Ok(veiculo);
    }
    
}).RequireAuthorization().WithTags("Veículos").WithDescription("Busca um Veículo pelo ID.");



// 🛠️ Rota PUT para atualizar um veículo existente, usando DTO para receber os dados
app.MapPut("/veiculo/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    } else {
        veiculo.Nome = veiculoDTO.Nome;
        veiculo.Marca = veiculoDTO.Marca;
        veiculo.Ano = veiculoDTO.Ano;

        veiculoServico.Atualizar(veiculo);

        return Results.Ok(veiculo);
    }
    
}).RequireAuthorization().WithTags("Veículos").WithDescription("Atualiza um Veículo pelo ID.");

// 🛠️ Rota DELETE para excluir um veículo pelo ID
app.MapDelete("/veiculo/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    } else {        
        veiculoServico.ApagarPorId(veiculo);
        
        return Results.NoContent();
    }
    
}).RequireAuthorization().WithTags("Veículos").WithDescription("Excluir um Veículo pelo ID.");


#endregion

#region App

if (swaggerAtivo)
{
    app.UseSwagger(); // 📝 Habilita o Swagger para documentação da API
    app.UseSwaggerUI(); // 🖥️ Interface do Swagger para testes interativos
}

app.UseAuthentication(); // 🔐 Habilita a autenticação JWT
app.UseAuthorization(); // ✅ Habilita a autorização

app.Run(); // 🚀 Inicia a aplicação
#endregion

