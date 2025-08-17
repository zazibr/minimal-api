using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi;
using MinimalApi.Dominio.Emums;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DOTs;
using MinimalApi.Infraestrutura.Db;
using Newtonsoft.Json;

public class Startup
{

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        keyJwt = Configuration.GetValue<string>("Jwt:Key") ?? string.Empty;;
        if (string.IsNullOrWhiteSpace(keyJwt))
        {
            throw new ArgumentException("A chave JWT não foi configurada no appsettings.json");
        }

        
        // Configuração do DbContext para usar MySQL
        var conexaoMySql = Configuration.GetConnectionString("MySql");
        if (string.IsNullOrWhiteSpace(conexaoMySql))
        {
            throw new ArgumentException("A string de conexão com o MySQL não foi configurada leia o README.md");
        }

        Console.WriteLine($"string de conexão com o MySQL...: {conexaoMySql}");
        Console.WriteLine(new string('*', 100));

    }

    private readonly string keyJwt = string.Empty;

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {

        
        var tamanhoChave = System.Text.Encoding.UTF8.GetByteCount(keyJwt);

        Console.WriteLine(new string('*', 100));
        Console.WriteLine($"Chave JWT.......................: {keyJwt}");
        Console.WriteLine($"................................: {tamanhoChave}");
        Console.WriteLine(new string('*', 100));



        if (tamanhoChave < 32)
        {
            Console.WriteLine("⚠️ Aviso: A chave JWT deve ter pelo menos 32 caracteres para garantir a segurança.");
        }

        // Configuração do serviço de autenticação JWT
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyJwt)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero, // Reduz o tempo de tolerância para expiração do token
                RequireExpirationTime = true // Exige que o token tenha um tempo de expiração
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT desta forma: Bearer {seu token}"

            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement{
                {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference{
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
                }

            });
        });

        // Configuração do DbContext para usar MySQL
        var conexaoMySql = Configuration.GetConnectionString("MySql");
        if (string.IsNullOrWhiteSpace(conexaoMySql))
        {
            throw new ArgumentException("A string de conexão com o MySQL não foi configurada leia o README.md");
        }

        Console.WriteLine($"string de conexão com o MySQL...: {conexaoMySql}");
        Console.WriteLine(new string('*', 100));
        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                conexaoMySql,
                ServerVersion.AutoDetect(conexaoMySql)
            );
        });

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var swaggerAtivo = Configuration.GetValue<bool>("Swagger:Enabled");

        if (swaggerAtivo)
        {
            app.UseSwagger(); // 📝 Habilita o Swagger para documentação da API
            app.UseSwaggerUI(); // 🖥️ Interface do Swagger para testes interativos
        }

        app.UseRouting();
        app.UseAuthentication(); // 🔐 Habilita a autenticação JWT
        app.UseAuthorization(); // ✅ Habilita a autorização

        app.UseEndpoints(endpoints =>
        {
            #region Home
            // 🌐 Rota GET simples para testar se a API está funcionando
            endpoints.MapGet("/", () =>
            {
                return Results.Json(new Home(swaggerAtivo));
            }).AllowAnonymous().WithTags("Home");
            #endregion

            #region Administradores

            string GerarTokenJwt(Administrador administrador, string keyJwt)
            {
                var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyJwt));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    new Claim(ClaimTypes.Role, administrador.Perfil)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);


            }

            // 🔐 Rota POST para login, usando DTO para receber os dados
            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
            {
                var adm = administradorServico.Login(loginDTO);
                if (adm != null)
                {
                    string token = GerarTokenJwt(adm, keyJwt);
                    var admLogado = new AdministradoLogado
                    {
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    };
                    var json = JsonConvert.SerializeObject(admLogado);
                    return Results.Text(json, "application/json");
                    
                }
                else
                {
                    return Results.Unauthorized();
                }

            }).AllowAnonymous().WithTags("Administradores");


            // 🛠️ Rota GET para listar todos os administradores, com paginação opcional
            endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
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
                    });
                }

                return Results.Ok(adms);


            }).RequireAuthorization()
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador" })
            .WithTags("Administradores")
            .WithDescription("Listar todos os Administradores por Paginação.");



            // 🛠️ Rota POST para criar um novo administrador, usando DTO para receber os dados
            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
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

            }).RequireAuthorization()
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador" })
            .WithTags("Administradores").WithDescription("Incluir novo administrador.");

            // 🛠️ Rota GET para buscar um administrador por ID
            endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
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
                
            }).RequireAuthorization()
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador" })
            .WithTags("Administradores").WithDescription("Busca um administrador pelo ID.");


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
            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
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
            }).RequireAuthorization()
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador,Editor"})  
            .WithTags("Veículos").WithDescription("Incluir Novo Veículo.");

            // 🛠️ Rota GET para listar todos os veículos, com paginação opcional
            endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
            {
                var veiculos = veiculoServico.Todos(pagina ?? 1);
                return Results.Ok(veiculos);
            }).RequireAuthorization()
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador,Editor" })  
            .WithTags("Veículos")
            .WithDescription("Listar Todos os Veículos por Paginação.");


            // 🛠️ Rota GET para buscar um veículo por ID
            endpoints.MapGet("/veiculo/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(id);
                if (veiculo == null)
                {
                    return Results.NotFound();
                } else
                {
                    return Results.Ok(veiculo);
                }
                
            }).RequireAuthorization()
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador,Editor" })  
            .WithTags("Veículos").WithDescription("Busca um Veículo pelo ID.");



            // 🛠️ Rota PUT para atualizar um veículo existente, usando DTO para receber os dados
            endpoints.MapPut("/veiculo/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
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
                }
                else
                {
                    veiculo.Nome = veiculoDTO.Nome;
                    veiculo.Marca = veiculoDTO.Marca;
                    veiculo.Ano = veiculoDTO.Ano;

                    veiculoServico.Atualizar(veiculo);

                    return Results.Ok(veiculo);
                }

            }).RequireAuthorization()
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador" })  
            .WithTags("Veículos")
            .WithDescription("Atualiza um Veículo pelo ID.");

            // 🛠️ Rota DELETE para excluir um veículo pelo ID
            endpoints.MapDelete("/veiculo/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(id);
                if (veiculo == null)
                {
                    return Results.NotFound();
                } else {        
                    veiculoServico.ApagarPorId(veiculo);
                    
                    return Results.NoContent();
                }
                
            }).RequireAuthorization()
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador" })  
            .WithTags("Veículos").WithDescription("Excluir um Veículo pelo ID.");


            #endregion
            
        });


    }

}