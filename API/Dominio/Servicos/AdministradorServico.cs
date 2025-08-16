using System.Data.Common;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DOTs;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;
    public AdministradorServico(DbContexto contexto)
    {
        _contexto = contexto;
    }
    public Administrador? Login(LoginDTO loginDTO)
    {
        var adm = _contexto.Administradores.Where(
            a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha
            ).FirstOrDefault();

        return adm;
    }

    public Administrador Incluir(Administrador administrador)
    {
        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();
        return administrador;
    }

    public Administrador? BuscarPorId(int id)
    {
        return _contexto.Administradores.Where(x => x.Id == id).FirstOrDefault();
    }

    public List<Administrador> Todos(int? pagina)
    {

        var query = _contexto.Administradores.AsQueryable();

        int itensPorPagina = 10;

        if (pagina != null && pagina > 0)
        {
            query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
        }

        return query.ToList();
    }

}