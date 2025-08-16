namespace  MinimalApi.DOTs;
using MinimalApi.Dominio.Emums;
public class AdministradorDTO()
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
    public Perfil? Perfil { get; set; } = default!;    
}
