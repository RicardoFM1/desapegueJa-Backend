using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> ListarTodosAsync(string? status = null);
        Task<Usuario?> BuscarPorEmailAsync(string email, string? status = null);
        Task<Usuario?> BuscarPorNomeAsync(string nome, string? status = null);
        Task<Usuario?> BuscarPorCpfAsync(string cpf, string? status = null);
        Task<Usuario?> BuscarPorIdAsync(int? id);
        Task<Usuario> AdicionarAsync(Usuario usuario);
        Task AtualizarAsync(int id, UsuarioUpdateDTO usuario, string? status = null);
        Task<Dictionary<int, string>> BuscarCepsPorIdsAsync(IEnumerable<int> usuariosIds);
    }
}
