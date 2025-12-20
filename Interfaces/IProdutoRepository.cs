using BackendDesapegaJa.Entities;

public interface IProdutoRepository
{
    Task<(IEnumerable<Produto> produtos, int total)> ListarTodosAsync(string? status, int offset, int limit);
    Task<IEnumerable<Produto>> BuscarPorNomeAsync(string nome, string? status);
    Task<IEnumerable<Produto>> BuscarPorUsuarioIdAsync(int? id, string? status);
    Task<Produto?> BuscarPorIdAsync(int? id, string? status);
    Task AdicionarAsync(Produto produto);
    Task<Produto?> AtualizarAsync(int id, ProdutoUpdateDTO produto, string? status);
}
