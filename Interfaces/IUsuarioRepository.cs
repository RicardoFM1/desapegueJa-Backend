using BackendDesapegaJa.Entities;
using System.Collections.Generic;

namespace BackendDesapegaJa.Interfaces
{
    public interface IUsuarioRepository
    {
        IEnumerable<Usuario> ListarTodos(string? status = null);
        Usuario? BuscarPorEmail(string email, string? status = null);
        void Adicionar(Usuario usuario);

        void Atualizar(int id, Usuario usuario, string? status = null);

        Usuario? BuscarPorId(int? id, string? status = null);

        Usuario? BuscarPorCpf(string? cpf, string? status = null);

    }
}
