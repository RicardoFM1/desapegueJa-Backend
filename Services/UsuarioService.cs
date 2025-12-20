using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace BackendDesapegaJa.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repo;
        private readonly IConfiguration _configuration;
        private readonly IEnderecoRepository _repoEndereco;

        public UsuarioService(
            IUsuarioRepository repo,
            IConfiguration configuration,
            IEnderecoRepository repoEndereco)
        {
            _repo = repo;
            _configuration = configuration;
            _repoEndereco = repoEndereco;
        }

        public async Task<IEnumerable<Usuario>> ObterUsuariosAsync(string? status = null)
        {
            return await _repo.ListarTodosAsync(status);
        }

        public async Task<Usuario> CriarUsuarioAsync(Usuario usuario)
        {
            var existenteEmail = await _repo.BuscarPorEmailAsync(usuario.Email);
            if (existenteEmail != null && existenteEmail.status?.ToLower() == "ativo")
                throw new InvalidOperationException("Usuário com este email já existe.");

            var existenteCpf = await _repo.BuscarPorCpfAsync(usuario.Cpf);
            if (existenteCpf != null && existenteCpf.status?.ToLower() == "ativo")
                throw new InvalidOperationException("Usuário com este CPF já existe.");

            if (string.IsNullOrWhiteSpace(usuario.Cpf) || !CpfValido(usuario.Cpf))
                throw new InvalidOperationException("CPF inválido.");

            if (string.IsNullOrWhiteSpace(usuario.Senha) || !SenhaValida(usuario.Senha))
                throw new InvalidOperationException("Senha inválida.");

            if (string.IsNullOrWhiteSpace(usuario.Telefone) || !TelefoneValido(usuario.Telefone))
                throw new InvalidOperationException("Telefone inválido.");

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha, 8);

            return await _repo.AdicionarAsync(usuario);
        }

        public async Task<LoginResponse> LoginAsync(Usuario usuario)
        {
            var existente = await _repo.BuscarPorEmailAsync(usuario.Email);

            if (existente == null ||
                string.IsNullOrWhiteSpace(existente.Senha) ||
                !BCrypt.Net.BCrypt.Verify(usuario.Senha, existente.Senha))
            {
                throw new InvalidOperationException("Senha e/ou email inválidos");
            }

            return GerarLoginResponse(existente);
        }

        public async Task<Usuario> BuscarUsuarioPorIdAsync(int id)
        {
            var usuario = await _repo.BuscarPorIdAsync(id);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            return usuario;
        }

        public async Task<Usuario> AtualizarUsuarioAsync(int id, UsuarioUpdateDTO dto, string? status = null)
        {
            var existente = await _repo.BuscarPorIdAsync(id);
            if (existente == null)
                throw new InvalidOperationException("Nenhum usuário encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExistente = await _repo.BuscarPorEmailAsync(dto.Email);
                if (emailExistente != null && emailExistente.Id != id)
                    throw new InvalidOperationException("Email já está em uso.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Cpf))
            {
                if (!CpfValido(dto.Cpf))
                    throw new InvalidOperationException("CPF inválido.");

                var cpfExistente = await _repo.BuscarPorCpfAsync(dto.Cpf);
                if (cpfExistente != null && cpfExistente.Id != id)
                    throw new InvalidOperationException("CPF já está em uso.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Senha))
            {
                if (!SenhaValida(dto.Senha))
                    throw new InvalidOperationException("Senha inválida.");

                dto.Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
            }

            if (!string.IsNullOrWhiteSpace(dto.Telefone) && !TelefoneValido(dto.Telefone))
                throw new InvalidOperationException("Telefone inválido.");

            await _repo.AtualizarAsync(id, dto, status);
            return await _repo.BuscarPorIdAsync(id) ?? existente;
        }

        public LoginResponse GerarLoginResponse(Usuario usuario)
        {
            var chave = _configuration["TokenKEY:SECRET_KEY"];
            if (string.IsNullOrWhiteSpace(chave))
                throw new InvalidOperationException("Chave JWT ausente.");

            var key = Encoding.ASCII.GetBytes(chave);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim("isAdmin", usuario.Admin.ToString().ToLower()),
                    new Claim(ClaimTypes.Name, usuario.Nome ?? "Usuário")
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginResponse
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Admin = usuario.Admin.ToString().ToLower(),
                Token = tokenHandler.WriteToken(token),
                Status = usuario.status
            };
        }

        public async Task<Usuario> BuscarOuCriarUsuarioGoogleAsync(string email, string nome, string googleId)
        {
            var existente = await _repo.BuscarPorEmailAsync(email);
            if (existente != null)
                return existente;

            var novo = new Usuario
            {
                Email = email,
                Nome = nome,
                GoogleId = googleId,
                Senha = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                Cpf = $"T{Guid.NewGuid():N}".Substring(0, 11),
                Telefone = "0000000000000",
                status = "ativo"
            };

            return await _repo.AdicionarAsync(novo);
        }

        public async Task CompletarCadastroAsync(int id, CompletarCadastroDTO dto)
        {
            if (!CpfValido(dto.Cpf)) throw new InvalidOperationException("CPF inválido.");
            if (!TelefoneValido(dto.Telefone)) throw new InvalidOperationException("Telefone inválido.");
            if (!CepValido(dto.Cep)) throw new InvalidOperationException("CEP inválido.");

            await _repo.AtualizarAsync(id, new UsuarioUpdateDTO
            {
                Cpf = dto.Cpf,
                Telefone = dto.Telefone,
                data_de_nascimento = dto.DataDeNascimento
            });

            await _repoEndereco.AdicionarAsync(new Enderecos
            {
                usuario_id = id,
                Cep = new string(dto.Cep.Where(char.IsDigit).ToArray()),
                numero = dto.Numero,
                rua = dto.Rua,
                bairro = dto.Bairro,
                cidade = dto.Cidade,
                estado = dto.Estado,
                tipo_de_endereco = "residencial",
                tipo_de_logradouro = "Não Informado",
                status = "ativo"
            });
        }

        private bool SenhaValida(string senha) =>
            Regex.IsMatch(senha, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$");

        private bool TelefoneValido(string telefone) =>
            !string.IsNullOrWhiteSpace(new string(telefone.Where(char.IsDigit).ToArray()));

        private bool CepValido(string cep) =>
            new string(cep.Where(char.IsDigit).ToArray()).Length == 8;

        private bool CpfValido(string cpf)
        {
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;

            int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string temp = cpf[..9];
            int soma = temp.Select((t, i) => int.Parse(t.ToString()) * mult1[i]).Sum();
            int resto = soma % 11 < 2 ? 0 : 11 - soma % 11;
            temp += resto;

            soma = temp.Select((t, i) => int.Parse(t.ToString()) * mult2[i]).Sum();
            resto = soma % 11 < 2 ? 0 : 11 - soma % 11;

            return cpf.EndsWith(temp[^1] + resto.ToString());
        }
    }
}
