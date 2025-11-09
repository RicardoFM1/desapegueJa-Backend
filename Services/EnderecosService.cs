using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using System.Runtime.ConstrainedExecution;

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

        public Enderecos GetEnderecosByUsuarioId(int id, string? status = null)
        {
            var enderecos =  _repo.BuscarPorUsuarioId(id, status);
            if(enderecos == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar esse endereço");
            }
            return enderecos;
        }

        private bool CepValido(string cep)
        { 

        cep = new string(cep.Where(char.IsDigit).ToArray()); return cep.Length == 8; 
        }

        public Enderecos CriarEndereco(Enderecos enderecos, string? status = null)
        {

            if (!string.IsNullOrWhiteSpace(enderecos.Cep)) { 
             if (!CepValido(enderecos.Cep))
                {
                   throw new InvalidOperationException("CEP inválido. Deve conter exatamente 8 números."); 
                }
            }
            _repo.Adicionar(enderecos, status);
            return enderecos;
        }

        public Enderecos AtualizarEnderecos(int id, EnderecosUpdateDTO enderecos, string? status = null)
        {
            var enderecoExistente = _repo.BuscarPorUsuarioId(id, status);
            if (enderecoExistente == null)
            {
                throw new InvalidOperationException("Nenhum endereço encontrado");
            }
            if (!string.IsNullOrWhiteSpace(enderecos.Cep))
            {
                if (!CepValido(enderecos.Cep))
                {
                    throw new InvalidOperationException("CEP inválido. Deve conter exatamente 8 números.");
                }
            }
            enderecoExistente.id = id;
            _repo.Atualizar(id, enderecos);
            return enderecoExistente;
        }
    }
}
