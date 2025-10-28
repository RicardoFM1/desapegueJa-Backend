using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IEnderecoRepository
    {
        IEnumerable<Enderecos> ListarTodos(string? status = null);

        void Adicionar(Enderecos enderecos, string? status = null);

        void Atualizar(int id, EnderecosUpdateDTO enderecos, string? status = null);

        Enderecos? BuscarPorId(int? id, string? status = null);
    }
}
