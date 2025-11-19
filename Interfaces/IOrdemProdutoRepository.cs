using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IOrdemProdutoRepository
    {
        IEnumerable<OrdemProduto> ListarTodos();
        IEnumerable<OrdemProduto> ListarPorOrdemId(int ordem_id);
        OrdemProduto? BuscarPorId(int id);
        void Adicionar(OrdemProduto ordemProduto);
        OrdemProduto Atualizar(int id, OrdemProdutoUpdateDTO ordemProduto);
        void Deletar(int id);
        void DeletarPorOrdemId(int ordem_id); 
    }
}
