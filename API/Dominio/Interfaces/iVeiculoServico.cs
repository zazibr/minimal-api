using MinimalApi.Dominio.Entidades;
using MinimalApi.DOTs;

namespace MinimalApi.Dominio.Interfaces;

public interface IVeiculoServico
{
    List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);
    Veiculo? BuscarPorId(int id);
    Veiculo Incluir(Veiculo veiculo);
    void Atualizar(Veiculo veiculo);
    void ApagarPorId(Veiculo veiculo);
}