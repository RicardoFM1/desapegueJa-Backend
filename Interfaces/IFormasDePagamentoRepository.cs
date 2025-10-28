using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IFormasDePagamentoRepository
    {
        IEnumerable<FormasDePagamento> ListarTodos();

        void Adicionar(FormasDePagamento forma);

        FormasDePagamento BuscarPorForma(string forma);

        FormasDePagamento BuscarPorId(int? id);

        void Atualizar(int id, FormasDePagamentoUpdateDTO forma);
    }
}
