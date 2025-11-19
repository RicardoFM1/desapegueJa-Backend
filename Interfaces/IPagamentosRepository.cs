using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IPagamentosRepository
    {
        IEnumerable<Pagamentos> ListarTodos(string? status = null);
        Pagamentos BuscarPorId(int id, string? status = null);
        void Adicionar(Pagamentos pagamento);
        Pagamentos Atualizar(int id, PagamentosUpdateDTO pagamento, string? status = null);
    }
}
