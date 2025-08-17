using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DOTs;

namespace Test.Mocks;

public class AdministradorServicoMock : IAdministradorServico
{
    private static List<Administrador> admistradores = new List<Administrador>()
    {
        new Administrador{
            Id = 1,
            Email = "adm@teste.com",
            Senha = "654321",
            Perfil = "Administrador"
        },
        new Administrador{
            Id = 2,
            Email = "editor@teste.com",
            Senha = "321",
            Perfil = "editor"
        }        
    };
    
    public Administrador? BuscarPorId(int id)
    {
        bool Criterio(Administrador a) => a.Id == id;
        return admistradores.Find(match: Criterio);
    }

    public Administrador Incluir(Administrador administrador)
    {
        administrador.Id = admistradores.Count() + 1;
        admistradores.Add(administrador);

        return administrador;
    }

    public Administrador? Login(LoginDTO loginDTO)
    {
        bool CredenciaisCorretas(Administrador a) =>
        a.Email == loginDTO.Email && a.Senha == loginDTO.Senha;

        return admistradores.Find(match: CredenciaisCorretas);

        // return admistradores.Find(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
    }

    public List<Administrador> Todos(int? id)
    {
        return admistradores;
    }
}