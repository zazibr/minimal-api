using MinimalApi.Dominio.Emums;

namespace MinimalApi.Dominio.ModelViews;

public record AdministradoLogado
{
    public string Email { get; set; } = default!;
    public string Perfil { get; set; } = default!;
    public string Token { get; set; } = default!;
}