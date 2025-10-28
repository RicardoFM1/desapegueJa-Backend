using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace BackendDesapegaJa.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<Usuario> ObterUsuarios()
        {
            return _repo.ListarTodos();
        }

        public Usuario CriarUsuario(Usuario usuario)
        {
            var existenteEmail = _repo.BuscarPorEmail(usuario.Email);
            if (existenteEmail != null && existenteEmail.status?.ToLower() == "ativo")
                throw new InvalidOperationException("Usuário com este email já existe.");

            var existenteCpf = _repo.BuscarPorCpf(usuario.Cpf);
            if (existenteCpf != null)
                throw new InvalidOperationException("Usuário com este CPF já existe.");

            if (string.IsNullOrWhiteSpace(usuario.Cpf) || !CpfValido(usuario.Cpf))
                throw new InvalidOperationException("CPF inválido.");

            if (string.IsNullOrWhiteSpace(usuario.Senha) || !SenhaValida(usuario.Senha))
                throw new InvalidOperationException("A senha deve ter no mínimo 8 caracteres, incluir letras maiúsculas, números e caracteres especiais.");

            if (!string.IsNullOrWhiteSpace(usuario.Telefone) && !TelefoneValido(usuario.Telefone))
                throw new InvalidOperationException("Telefone inválido. Deve conter apenas números e ter 10 ou 11 dígitos.");

            if (!string.IsNullOrWhiteSpace(usuario.Cep) && !CepValido(usuario.Cep))
                throw new InvalidOperationException("CEP inválido. Deve conter exatamente 8 números.");

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
            _repo.Adicionar(usuario);

            return usuario;
        }

        public LoginResponse Login(Usuario usuario)
        {
            var existente = _repo.BuscarPorEmail(usuario.Email);
            if (existente == null || !BCrypt.Net.BCrypt.Verify(usuario.Senha, existente.Senha))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var chave = config["TokenKEY:SECRET_KEY"];
            var key = Encoding.ASCII.GetBytes(chave);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, existente.Id.ToString()),
                    new Claim(ClaimTypes.Email, existente.Email),
                    new Claim("isAdmin", existente.Admin.ToString().ToLower())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(tokenObj);

            return new LoginResponse
            {
                Id = existente.Id,
                Email = existente.Email,
                Admin = existente.Admin.ToString().ToLower(),
                Token = tokenString
            };
        }

        public Usuario? BuscarUsuarioPorId(int id)
        {
            var usuario = _repo.BuscarPorId(id);
            if (usuario == null)
                throw new InvalidOperationException("Não foi possível encontrar esse usuário.");

            return usuario;
        }

        public Usuario AtualizarUsuario(int id, UsuarioUpdateDTO usuarioDto)
        {
            var existente = _repo.BuscarPorId(id);
            if (existente == null)
                throw new InvalidOperationException("Nenhum usuário encontrado.");

            var existenteEmail = _repo.BuscarPorEmail(usuarioDto.Email);
            if (existenteEmail != null && existenteEmail.Id != id && existenteEmail.status?.ToLower() == "ativo")
                throw new InvalidOperationException("Este email já está em uso por outro usuário.");

            if (!string.IsNullOrWhiteSpace(usuarioDto.Cpf))
            {
                if (!CpfValido(usuarioDto.Cpf))
                    throw new InvalidOperationException("CPF inválido.");

                var existenteCpf = _repo.BuscarPorCpf(usuarioDto.Cpf);
                if (existenteCpf != null && existenteCpf.Id != id)
                    throw new InvalidOperationException("Este CPF já está em uso por outro usuário.");

                existente.Cpf = usuarioDto.Cpf;
            }

            if (!string.IsNullOrWhiteSpace(usuarioDto.Email))
                existente.Email = usuarioDto.Email;

            if (!string.IsNullOrWhiteSpace(usuarioDto.Senha))
            {
                if (!SenhaValida(usuarioDto.Senha))
                    throw new InvalidOperationException("Senha inválida. Deve ter no mínimo 8 caracteres, incluir letras maiúsculas, números e caracteres especiais.");

                existente.Senha = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Senha);
            }

            if (!string.IsNullOrWhiteSpace(usuarioDto.Telefone))
            {
                if (!TelefoneValido(usuarioDto.Telefone))
                    throw new InvalidOperationException("Telefone inválido. Deve conter 10 ou 11 dígitos.");
                existente.Telefone = usuarioDto.Telefone;
            }

            if (!string.IsNullOrWhiteSpace(usuarioDto.Cep))
            {
                if (!CepValido(usuarioDto.Cep))
                    throw new InvalidOperationException("CEP inválido. Deve conter exatamente 8 números.");
                existente.Cep = usuarioDto.Cep;
            }

            if (usuarioDto.Rg.HasValue)
                existente.Rg = usuarioDto.Rg;

            if (!string.IsNullOrWhiteSpace(usuarioDto.Foto_De_Perfil))
                existente.Foto_De_Perfil = usuarioDto.Foto_De_Perfil;

            if (!string.IsNullOrWhiteSpace(usuarioDto.data_de_nascimento))
                existente.data_de_nascimento = usuarioDto.data_de_nascimento;

            if (!string.IsNullOrWhiteSpace(usuarioDto.status))
                existente.status = usuarioDto.status;

            _repo.Atualizar(id, existente);
            return existente;
        }

        private bool SenhaValida(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                return false;

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            return regex.IsMatch(senha);
        }

        private bool TelefoneValido(string telefone)
        {
            telefone = new string(telefone.Where(char.IsDigit).ToArray());
            return telefone.Length >= 10 && telefone.Length <= 11;
        }

        private bool CepValido(string cep)
        {
            cep = new string(cep.Where(char.IsDigit).ToArray());
            return cep.Length == 8;
        }

        private bool CpfValido(string cpf)
        {
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
                return false;

            int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string temp = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(temp[i].ToString()) * mult1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            string digito = resto.ToString();
            temp += digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(temp[i].ToString()) * mult2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }
    }
}
