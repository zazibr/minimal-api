using MinimalApi.Dominio.Entidades;
using MinimalApi.DOTs;

namespace MinimalApi.Dominio.Interfaces;

public interface IAdministradorServico
{
    Administrador? Login(LoginDTO loginDTO);

    Administrador Incluir(Administrador administrador);

    List<Administrador> Todos(int? id);
   
   Administrador? BuscarPorId(int id);

}