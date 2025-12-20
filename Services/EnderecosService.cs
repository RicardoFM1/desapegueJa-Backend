using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class EnderecosService
    {
        private readonly IEnderecoRepository _repo;

        public EnderecosService(IEnderecoRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Enderecos>> ObterEnderecosAsync(string? status = null)
        {
            return await _repo.ListarTodosAsync(status);
        }

        public async Task<Enderecos> GetEnderecoAtivoByUsuarioIdAsync(int id, string? status = null)
        {
            var endereco = await _repo.BuscarPorUsuarioIdAtivoAsync(id, status);

            if (endereco == null)
                throw new InvalidOperationException("Nenhum endereço ativo encontrado para este usuário.");

            return endereco;
        }

        public async Task<IEnumerable<Enderecos>> GetEnderecosByUsuarioIdAsync(int id, string? status = null)
        {
            var enderecos = await _repo.BuscarPorUsuarioIdAsync(id, status);

            if (!enderecos.Any())
                throw new InvalidOperationException("Nenhum endereço encontrado para este usuário.");

            return enderecos;
        }

        public async Task<Enderecos> GetEnderecoByIdAsync(int id, string? status = null)
        {
            var endereco = await _repo.BuscarPorIdAsync(id, status);

            if (endereco == null)
                throw new InvalidOperationException("Endereço não encontrado.");

            return endereco;
        }

        public async Task<Enderecos> CriarEnderecoAsync(Enderecos enderecos, string? status = null)
        {
            if (!string.IsNullOrWhiteSpace(enderecos.Cep))
            {
                var cepNumeros = new string(enderecos.Cep.Where(char.IsDigit).ToArray());
                if (!CepValido(cepNumeros))
                    throw new InvalidOperationException("CEP inválido. Deve conter 8 números.");

                enderecos.Cep = cepNumeros;
            }

            await _repo.AdicionarAsync(enderecos, status);
            return enderecos;
        }

        public async Task AtualizarEnderecoPorIdAsync(int id, EnderecosUpdateDTO enderecos, string? status = null)
        {
            var existente = await _repo.BuscarPorIdAsync(id, status);

            if (existente == null)
                throw new InvalidOperationException("Endereço não encontrado.");

            if (!string.IsNullOrWhiteSpace(enderecos.Cep))
            {
                var cepNumeros = new string(enderecos.Cep.Where(char.IsDigit).ToArray());
                if (!CepValido(cepNumeros))
                    throw new InvalidOperationException("CEP inválido. Deve conter exatamente 8 números.");

                enderecos.Cep = cepNumeros;
            }

            await _repo.AtualizarPorIdAsync(id, enderecos, status);
        }

        private bool CepValido(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep)) return false;
            return cep.Length == 8 && cep.All(char.IsDigit);
        }
    }
}
