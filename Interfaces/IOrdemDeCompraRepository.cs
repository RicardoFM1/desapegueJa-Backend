using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IOrdemDeCompraRepository
    {
        IEnumerable<OrdemDeCompra> ListarTodos();
        OrdemDeCompra? BuscarPorUsuarioId(int id);

        OrdemDeCompra? BuscarPorId(int id);
        void Adicionar(OrdemDeCompraCreateDTO ordem);
        OrdemDeCompra Atualizar(int id, OrdemDeCompraUpdateDTO ordem);
        void Deletar(int id);
    }
}
