using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class EnderecosService
    {
        public readonly IEnderecoRepository _repo;

        public EnderecosService(IEnderecoRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<Enderecos> ObterEnderecos(string? status = null)
        {
            return _repo.ListarTodos(status);
        }

        public Enderecos GetEnderecosById(int id, string? status = null)
        {
            var enderecos =  _repo.BuscarPorId(id, status);
            if(enderecos == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar esse endereço");
            }
            return enderecos;
        }

        public Enderecos CriarEndereco(Enderecos enderecos, string? status = null)
        {
            _repo.Adicionar(enderecos, status);
            return enderecos;
        }

        public Enderecos AtualizarEnderecos(int id, EnderecosUpdateDTO enderecos, string? status = null)
        {
            var enderecoExistente = _repo.BuscarPorId(id, status);
            if (enderecoExistente == null)
            {
                throw new InvalidOperationException("Nenhum endereço encontrado");
            }
            enderecoExistente.id = id;
            _repo.Atualizar(id, enderecos);
            return enderecoExistente;
        }
    }
}
