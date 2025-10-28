using BackendDesapegaJa.Entities;
using System.Collections.Generic;

namespace BackendDesapegaJa.Interfaces
{
    public interface IUsuarioRepository
    {
        IEnumerable<Usuario> ListarTodos();
        Usuario? BuscarPorEmail(string email);
        void Adicionar(Usuario usuario);

        void Atualizar(int id, Usuario usuario);

        Usuario? BuscarPorId(int? id);

        Usuario? BuscarPorCpf(string? cpf);

    }
}
