using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IPagamentosRepository
    {
        IEnumerable<Pagamentos> ListarTodos();
        Pagamentos BuscarPorId(int id);

        Pagamentos? BuscarPorUsuarioId(int usuarioId);
        void Adicionar(Pagamentos pagamento);
        Pagamentos Atualizar(int usuarioId, PagamentosUpdateDTO pagamento);

        void DeletarPorUsuarioId(int usuarioId);
    }
}
