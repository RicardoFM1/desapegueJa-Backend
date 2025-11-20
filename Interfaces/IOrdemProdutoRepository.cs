using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IOrdemProdutoRepository
    {
        IEnumerable<OrdemProduto> ListarTodos();
        OrdemProduto? BuscarPorUsuarioId(int usuarioId);
        void Adicionar(OrdemProduto ordemProduto);
        OrdemProduto AtualizarPorUsuarioId(int usuarioId, OrdemProdutoUpdateDTO dto);
        void DeletarPorUsuarioId(int usuarioId);
    }
}
