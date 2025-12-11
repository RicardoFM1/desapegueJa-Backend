using BackendDesapegaJa.Entities;
using System.Collections.Generic;

namespace BackendDesapegaJa.Interfaces
{
    public interface IUsuarioRepository
    {
        IEnumerable<Usuario> ListarTodos(string? status = null);
        Usuario? BuscarPorEmail(string email, string? status = null);
        void Adicionar(Usuario usuario);

        void Atualizar(int id, UsuarioUpdateDTO usuario, string? status = null);

        Usuario? BuscarPorId(int? id);

        Usuario? BuscarPorCpf(string? cpf, string? status = null);

        Usuario? BuscarPorNome(string? nome, string? status = null);

        Task<Usuario?> BuscarPorEmailAsync(string email, string? status = null);

       
        Task<Usuario> AdicionarAsync(Usuario usuario);

        Task<Dictionary<int, string>> BuscarCepsPorIdsAsync(IEnumerable<int> usuariosIds);

    }
}
