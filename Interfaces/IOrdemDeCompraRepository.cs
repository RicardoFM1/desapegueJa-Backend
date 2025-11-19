using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IOrdemDeCompraRepository
    {
        IEnumerable<OrdemDeCompra> ListarTodos();
        OrdemDeCompra? BuscarPorId(int id);
        void Adicionar(OrdemDeCompra ordem);
        OrdemDeCompra Atualizar(int id, OrdemDeCompraUpdateDTO ordem);
        void Deletar(int id);
    }
}
