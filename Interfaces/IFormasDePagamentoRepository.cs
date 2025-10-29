using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IFormasDePagamentoRepository
    {
        IEnumerable<FormasDePagamento> ListarTodos(string? status = null);

        void Adicionar(FormasDePagamento forma);

        FormasDePagamento BuscarPorForma(string forma, string? status = null);

        FormasDePagamento BuscarPorId(int? id, string? status = null);

        void Atualizar(int id, FormasDePagamentoUpdateDTO forma, string? status = null);
    }
}
