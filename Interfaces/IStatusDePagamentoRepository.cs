using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IStatusDePagamentoRepository
    {
        IEnumerable<StatusDePagamento> ListarTodos();

        void Adicionar(StatusDePagamento status);

        StatusDePagamento BuscarPorDescricao(string descricao);

        StatusDePagamento BuscarPorId(int? id);

        StatusDePagamento Atualizar(int id, StatusDePagamentoUpdateDTO status);
    }
}
