using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IOrdemDeCompraRepository
    {
        IEnumerable<OrdemDeCompra> ListarTodos();

        void Adicionar(OrdemDeCompra ordem);
        OrdemDeCompra BuscarPorId(int? id);
        OrdemDeCompra BuscarPorProdutoId(int? id);

        OrdemDeCompra BuscarPorUsuarioId(int? id);

        OrdemDeCompra BuscarPorStatusDeCompraId(int? id);

        OrdemDeCompra Atualizar(int id, OrdemDeCompraUpdateDTO ordem);
    }
}
