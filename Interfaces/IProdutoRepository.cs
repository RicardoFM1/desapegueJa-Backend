using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IProdutoRepository
    {
        (IEnumerable<Produto> produtos, int total) ListarTodos(string? status = null, int offset = 0, int limit = 10);

        void Adicionar(Produto produto);

        Produto? BuscarPorNome(string nome, string? status = null);

        Produto? BuscarPorId(int? id, string? status = null);

        Produto? Atualizar(int id, ProdutoUpdateDTO produto, string? status = null);
    }
}
