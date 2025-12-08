using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;

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

        public IEnumerable<Enderecos?> GetEnderecosByUsuarioId(int id, string? status = null)
        {
            var enderecos =  _repo.BuscarPorUsuarioId(id, status);
            if(enderecos == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar esse endereço");
            }
            return enderecos;
        }
        public Enderecos GetEnderecoById(int id, string? status = null)
        {
            var enderecos = _repo.BuscarPorId(id, status);
            if (enderecos == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar esse endereço");
            }
            return enderecos;
        }

        private bool CepValido(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep)) return false;
            
            var numeros = new string(cep.Where(char.IsDigit).ToArray());
            return numeros.Length == 8;
        }



        public Enderecos CriarEndereco(Enderecos enderecos, string? status = null)
        {

            
            _repo.Adicionar(enderecos, status);
            return enderecos;
        }

        //public Enderecos AtualizarEnderecos(int id, EnderecosUpdateDTO enderecos, string? status = null)
        //{
        //    var enderecoExistente = _repo.BuscarPorUsuarioId(id, status);
        //    if (enderecoExistente == null)
        //    {
        //        throw new InvalidOperationException("Nenhum endereço encontrado");
        //    }
        //    if (!string.IsNullOrWhiteSpace(enderecos.Cep))
        //    {
                
        //        var cepNumeros = new string(enderecos.Cep.Where(char.IsDigit).ToArray());
        //        if (!CepValido(cepNumeros))
        //            throw new InvalidOperationException("CEP inválido. Deve conter exatamente 8 números.");
        //        enderecos.Cep = cepNumeros;
        //    }
        //    enderecoExistente.id = id;
        //    _repo.Atualizar(id, enderecos);
        //    return enderecoExistente;
        //}
        public Enderecos AtualizarEnderecosPorId(int id, EnderecosUpdateDTO enderecos, string? status = null)
        {
            var enderecoExistente = _repo.BuscarPorId(id, status);
            if (enderecoExistente == null)
            {
                throw new InvalidOperationException("Nenhum endereço encontrado");
            }
            if (!string.IsNullOrWhiteSpace(enderecos.Cep))
            {

                var cepNumeros = new string(enderecos.Cep.Where(char.IsDigit).ToArray());
                if (!CepValido(cepNumeros))
                    throw new InvalidOperationException("CEP inválido. Deve conter exatamente 8 números.");
                enderecos.Cep = cepNumeros;
            }
            enderecoExistente.id = id;
            _repo.AtualizarPorId (id, enderecos);
            return enderecoExistente;
        }
    }
}
