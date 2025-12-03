using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IOrdemProdutoRepository
    {
        IEnumerable<OrdemProduto> ListarTodos();
        OrdemProduto? BuscarPorUsuarioId(int usuarioId);

        IEnumerable<OrdemProduto> BuscarProdutosPorOrdemId(int ordemId);
        void Adicionar(OrdemProduto ordemProduto);
        OrdemProduto AtualizarPorUsuarioId(int usuarioId, OrdemProdutoUpdateDTO dto);
        
    }
}
