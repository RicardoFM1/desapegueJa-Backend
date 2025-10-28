using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IProdutoRepository
    {
        IEnumerable<Produto> ListarTodos(string? status);

        void Adicionar(Produto produto);

        Produto? BuscarPorNome(string nome);

        Produto? BuscarPorId(int? id, string? status = null);

        Produto? Atualizar(int id, string? status, ProdutoUpdateDTO produto);
    }
}
