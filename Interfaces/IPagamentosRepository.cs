using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IPagamentosRepository
    {
        IEnumerable<Pagamentos> ListarTodos();

        void Adicionar(Pagamentos pagamentos);
        Pagamentos BuscarPorId(int? id);

        Pagamentos Atualizar(int id, PagamentosUpdateDTO pagamento);
    }
}
