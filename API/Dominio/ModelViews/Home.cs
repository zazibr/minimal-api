namespace MinimalApi.Dominio.ModelViews;

public struct Home
{
    public Home(bool swaggerAtivo)
    { 
        Doc = swaggerAtivo ? "/swagger" : null;
    }
    public string Mensagem { get => "Bem-vindo a  API de veículos - Minimal API"; }
    public string? Doc { get; }
}