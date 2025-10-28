using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface ICarrinhoRepository
    {
        public IEnumerable<Carrinho> ListarTodos();

        public Carrinho BuscarPorId(int? id);

        public Carrinho BuscarPorUsuarioEProduto(int usuarioId, int produtoId);

        public void Adicionar(Carrinho carrinho);

        public void Atualizar(int usuarioId, int produtoId, CarrinhoUpdateDTO carrinho);

        public void Deletar(int usuarioId, int produtoId);

    }
}
