using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IPagamentosRepository
    {
        IEnumerable<Pagamentos> ListarTodos(string? status = null);
        Pagamentos BuscarPorId(int id, string? status = null);

        Pagamentos? BuscarPorUsuarioId(int usuarioId, string? status = null);
        void Adicionar(Pagamentos pagamento);
        Pagamentos Atualizar(int usuarioId, PagamentosUpdateDTO pagamento, string? statusQuery = null);
   
    }
}
