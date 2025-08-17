# TESTE - minimal-api
TESTE do Desafio de Projeto - Trabalhando com ASP.NET Minimals  APIs

- criar o banco de dados no MySQL
--- create database minimal_apitest;

- restaurar o backup para a base de dados de teste

--- usar o cmd pois o poweshell não deixa executar o comando abaixo

---  mysql --binary-mode=1 -uroot -p_SUA_SENHA_ minimal_apitest < minimal_api.dump.sql


use o seguinte comando para informar os dados do banco de dados

*** inicialmente rodar antes de executar **
dotnet user-secrets --id minimal-api-123456 set "ConnectionStrings:MySql" "Server=localhost;Database=minimal_apitest;Uid=root;Pwd=_SUA_SENHA_;"
dotnet run

substituindo _SUA_SENHA_ pela senha do banco MySql

-- para listar  dotnet user-secrets list


  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <UserSecretsId>minimal-api-123456</UserSecretsId> --- deve possuir essa linha ---
    <Nullable>enable</Nullable>


- Testes Unitários das Entidades
--- AdministradorTest
--- VeiculosTest (solicitado para para ser criado como desafio)

-