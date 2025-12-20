using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface ICategoriasRepository
    {
        Task<IEnumerable<Categorias>> ListarTodosAsync(string? status = null);
        Task<Categorias?> BuscarPorNomeAsync(string nome);
        Task<Categorias?> BuscarPorIdAsync(int id, string? status = null);
        Task AdicionarAsync(Categorias categoria);
        Task AtualizarAsync(int id, CategoriasUpdateDTO categorias);
    }
}
