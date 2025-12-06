using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Data;

namespace BackendDesapegaJa.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repo;
        private readonly IConfiguration _configuration;

        public UsuarioService(IUsuarioRepository repo, IConfiguration configuration) 
        {
            _repo = repo;
            _configuration = configuration;
        }

        public IEnumerable<Usuario> ObterUsuarios(string? status = null)
        {
            return _repo.ListarTodos(status);
        }

        public Usuario CriarUsuario(Usuario usuario)
        {
            var existenteEmail = _repo.BuscarPorEmail(usuario.Email);
            if (existenteEmail != null && existenteEmail.status?.ToLower() == "ativo")
                throw new InvalidOperationException("Usuário com este email já existe.");

            var existenteCpf = _repo.BuscarPorCpf(usuario.Cpf);
            if (existenteCpf != null && existenteCpf.status?.ToLower() == "ativo")
                throw new InvalidOperationException("Usuário com este CPF já existe.");

            if (string.IsNullOrWhiteSpace(usuario.Cpf) || !CpfValido(usuario.Cpf))
                throw new InvalidOperationException("CPF inválido.");

            if (string.IsNullOrWhiteSpace(usuario.Senha) || !SenhaValida(usuario.Senha))
                throw new InvalidOperationException("A senha deve ter no mínimo 8 caracteres, incluir letras maiúsculas, números e caracteres especiais.");

            if (string.IsNullOrWhiteSpace(usuario.Telefone) || !TelefoneValido(usuario.Telefone))
                throw new InvalidOperationException("Telefone inválido. Deve conter apenas números e ter 10 ou 13 dígitos.");


            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
            _repo.Adicionar(usuario);

            return usuario;
        }

        public LoginResponse Login(Usuario usuario)
        {
            var existente = _repo.BuscarPorEmail(usuario.Email);
            if (existente == null || string.IsNullOrWhiteSpace(existente.Senha) ||
    !BCrypt.Net.BCrypt.Verify(usuario.Senha, existente.Senha))
            {
                throw new InvalidOperationException("Senha e/ou email inválidos");
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            
            var chave = _configuration["TokenKEY:SECRET_KEY"];

            if (string.IsNullOrWhiteSpace(chave))
            {
                throw new InvalidOperationException("Chave JWT ausente no sistema de configuração.");
            }


            var key = Encoding.ASCII.GetBytes(chave);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, existente.Id.ToString()),
                    new Claim(ClaimTypes.Email, existente.Email),
                    new Claim("isAdmin", existente.Admin.ToString().ToLower()),
                    new Claim(ClaimTypes.Name, existente.Nome ?? "Usuário"),
                    new Claim("Nascimento", existente.data_de_nascimento ?? DateTime.UtcNow.ToString("dd-MM-yyyy")),
                    new Claim("Telefone", existente.Telefone ?? "5551992320421"),
                    new Claim("Cpf", existente.Cpf ?? "000.000.000-00")
                   
                }),
                Expires = DateTime.UtcNow.AddHours(12),
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
                Token = tokenString,
                Status = existente.status
                
            };
        }

        public Usuario? BuscarUsuarioPorId(int id, string? status = null)
        {
            var usuario = _repo.BuscarPorId(id);
            if (usuario == null)
                throw new InvalidOperationException("Não foi possível encontrar esse usuário.");

            return usuario;
        }

        public Usuario AtualizarUsuario(int id, UsuarioUpdateDTO usuarioDto, string? status = null)
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

                
            }

            if (!string.IsNullOrWhiteSpace(usuarioDto.Senha))
            {
                if (!SenhaValida(usuarioDto.Senha))
                    throw new InvalidOperationException("Senha inválida. Deve ter no mínimo 8 caracteres, incluir letras maiúsculas, números e caracteres especiais.");

                
                usuarioDto.Senha = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Senha);
            }

            if (!string.IsNullOrWhiteSpace(usuarioDto.Telefone))
            {
                if (!TelefoneValido(usuarioDto.Telefone))
                    throw new InvalidOperationException("Telefone inválido. Deve conter apenas números."); 

            }


            usuarioDto.Admin ??= existente.Admin;

            
            usuarioDto.Email ??= existente.Email;
            usuarioDto.Telefone ??= existente.Telefone;
            usuarioDto.data_de_nascimento ??= existente.data_de_nascimento;
            usuarioDto.Cpf ??= existente.Cpf;
            usuarioDto.status ??= existente.status;
            usuarioDto.Nome ??= existente.Nome;
            usuarioDto.Foto_De_Perfil ??= existente.Foto_De_Perfil;
            _repo.Atualizar(id, usuarioDto, status);
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
            if (string.IsNullOrEmpty(telefone)) return false;

         
            telefone = new string(telefone.Where(char.IsDigit).ToArray());

      
            if (string.IsNullOrEmpty(telefone)) return false;


            
            return true; 
        }

        public LoginResponse GerarLoginResponse(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario), "Usuário não pode ser nulo para gerar LoginResponse.");
            }

         
            var tokenHandler = new JwtSecurityTokenHandler();

            var chave = _configuration["TokenKEY:SECRET_KEY"];

            if (string.IsNullOrWhiteSpace(chave))
            {
                throw new InvalidOperationException("Chave JWT ausente no sistema de configuração.");
            }

            var key = Encoding.ASCII.GetBytes(chave);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim("isAdmin", usuario.Admin.ToString().ToLower()),
            new Claim(ClaimTypes.Name, usuario.Nome ?? "Usuário"),
            new Claim("Nascimento", usuario.data_de_nascimento ?? DateTime.UtcNow.ToString("dd-MM-yyyy")),
            new Claim("Telefone", usuario.Telefone ?? "5551992320421"),
            new Claim("Cpf", usuario.Cpf ?? "000.000.000-00")
        }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(tokenObj);

            return new LoginResponse
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Admin = usuario.Admin.ToString().ToLower(),
                Token = tokenString,
                Status = usuario.status
            };
        }



        public async Task<Usuario> BuscarOuCriarUsuarioGoogleAsync(string email, string nome, string googleId)
        {
            var usuarioExistente = await _repo.BuscarPorEmailAsync(email);

            if (usuarioExistente != null)
            {
               
                return usuarioExistente;
            }

            
            var senhaAleatoria = Guid.NewGuid().ToString("N");

           
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaAleatoria);

            string uniqueGuidPart = Guid.NewGuid().ToString("N").Substring(0, 11);


            var novoUsuario = new Usuario
            {
                Email = email,
                Nome = nome,
                GoogleId = googleId,
                Senha = senhaHash,
                Cpf = $"T{uniqueGuidPart.Substring(0, 10)}",
                Telefone = "0000000000000" 
            };

           
            if (string.IsNullOrWhiteSpace(novoUsuario.Cpf))
            {
               
                novoUsuario.Cpf = $"T{uniqueGuidPart.Substring(0, 10)}";
            }

            
            if (string.IsNullOrWhiteSpace(novoUsuario.Telefone))
            {
                novoUsuario.Telefone = "0000000000000";
            }

           
            if (string.IsNullOrWhiteSpace(novoUsuario.status))
            {
                novoUsuario.status = "ativo";
            }

           
            var usuarioCriado = await _repo.AdicionarAsync(novoUsuario);

            

            return usuarioCriado;
        }

        public async Task CompletarCadastroAsync(int id, CompletarCadastroDTO dto)
        {
         
            if (!CpfValido(dto.Cpf))
                throw new InvalidOperationException("CPF inválido.");

            if (!TelefoneValido(dto.Telefone))
                throw new InvalidOperationException("Telefone inválido.");

            
            var update = new UsuarioUpdateDTO
            {
                Cpf = dto.Cpf,
                Telefone = dto.Telefone,
                data_de_nascimento = dto.DataDeNascimento,
              
            };

            
            _repo.Atualizar(id, update);

           
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
