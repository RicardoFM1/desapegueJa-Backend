using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IStatusDePagamentoRepository
    {
        IEnumerable<StatusDePagamento> ListarTodos(string? status = null);

        void Adicionar(StatusDePagamento status);

        StatusDePagamento BuscarPorDescricao(string descricao, string? status = null);

        StatusDePagamento BuscarPorId(int? id, string? status = null);

        StatusDePagamento Atualizar(int id, StatusDePagamentoUpdateDTO status, string? statusquery = null);
    }
}
