using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IEnderecoRepository
    {
         Task<IEnumerable<Enderecos>> ListarTodosAsync(string? status = null);

         Task AdicionarAsync(Enderecos enderecos, string? status = null);

      

        Task AtualizarPorIdAsync(int id, EnderecosUpdateDTO enderecos, string? status = null);

        Task<Enderecos?> BuscarPorIdAsync(int? id, string? status = null);

        Task<IEnumerable<Enderecos?>> BuscarPorUsuarioIdAsync(int? id, string? status = null);

        Task<Enderecos?> BuscarPorUsuarioIdAtivoAsync(int? id, string? status = null);
    }
}
