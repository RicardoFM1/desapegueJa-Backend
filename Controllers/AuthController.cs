using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/usuarios")]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _service;

        public AuthController(UsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string? status)
        {
            try
            {

            var usuarios = _service.ObterUsuarios(status);
                foreach (var u in usuarios)
                {
                    u.Senha = null;
                }
                return Ok(usuarios);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]

        public IActionResult GetById(int id, [FromQuery] string? status)
        {
            try
            {

            var usuario = _service.BuscarUsuarioPorId(id, status);
                usuario.Senha = null;
            return Ok(usuario);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CriarUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var novoUsuario = _service.CriarUsuario(usuario);
                return StatusCode(201, new { novoUsuario });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var usuario = new Usuario { Email = loginDto.Email, Senha = loginDto.Senha };
                var loginResponse = _service.Login(usuario);
                if (loginResponse == null || usuario.status == "inativo")
                {
                   return Unauthorized(new { message = "Usuário ou senha inválidos." });
                }
                if (loginResponse.Status.ToLower() == "inativo")
                    return Unauthorized(new { message = "Usuário ou senha inválidos." });
                return Ok(loginResponse);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("retrieve")]
        public IActionResult GetUsuarioLogado()
        {
            try
            {
               
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized(new { message = "Token inválido ou ausente." });

                if (!int.TryParse(userIdClaim, out int userId))
                    return BadRequest(new { message = "ID de usuário inválido no token." });
                var usuario = _service.BuscarUsuarioPorId(userId);
                if (usuario == null)
                    return NotFound(new { message = "Usuário não encontrado." });

                var response = new DTOresponse
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    Admin = (bool)usuario.Admin,
                    Nome = usuario.Nome,
                    Cpf = usuario.Cpf,
                    Telefone = usuario.Telefone,
                    Data_Nascimento = usuario.data_de_nascimento,
                    Foto_Perfil = usuario.Foto_De_Perfil

                };


                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao recuperar usuário.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch("{id}")]

        public IActionResult AtualizarUsuario(int id, [FromBody] UsuarioUpdateDTO usuario, [FromQuery] string? status)
        {
            try
            {
                var usuarioExistente = _service.BuscarUsuarioPorId(id, status);
                var loggedId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = false;
                var admin = User.FindFirst("isAdmin")?.Value;

                if(User.FindFirst("isAdmin")?.Value.ToLower() == "true")
                {
                    isAdmin = true;
                }
                else
                {
                    isAdmin = false;
                }
                if(!int.TryParse(loggedId, out int loggedidInt))
                {
                    return StatusCode(403, new { message = "Sem autorização para atualizar o usuário" });
                }
                if(isAdmin == false && id != loggedidInt)
                {
                    return StatusCode(403, new { message = "Sem autorização para atualizar o usuário" });
                }
                if(usuarioExistente.status.ToLower() == "inativo" && isAdmin == false)
                {
                    return StatusCode(403, new { message = "Usuário sem permissão" });
                }
                    var atualizacao = _service.AtualizarUsuario(id, usuario, status);
                return StatusCode(200, atualizacao);

            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }


        }
        [HttpGet("login-google")]
        public IActionResult ExternalLogin()
        {
            const string provider = "Google";

            
            var redirectUrl = "/desapega/usuarios/login-google/callback";

            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                
                Items = { { "ReturnUrl", "http://localhost:5173" } }, 
                RedirectUri = redirectUrl
            };

            return Challenge(properties, provider);
        }

        [HttpGet("login-google/callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                return RedirectToAction(nameof(Login), new { error = $"Erro do provedor: {remoteError}" });
            }


            var result = await HttpContext.AuthenticateAsync("Google");

            if (result?.Succeeded != true)
            {
               
                return Redirect("http://localhost:5173/login?error=auth_failed");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var nome = result.Principal.FindFirstValue(ClaimTypes.Name);
            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var usuario = await _service.BuscarOuCriarUsuarioGoogleAsync(
                email, nome, googleId);

            if (usuario == null)
            {
                return Redirect("http://localhost:5173/login?error=user_creation_failed");
            }

       
            try
            {
                var tokenResponse = _service.GerarLoginResponse(usuario);


                bool needsCompletion = (usuario.Cpf == usuario.GoogleId || string.IsNullOrEmpty(usuario.Cpf)) ||
                            (usuario.Telefone == "0000000000000" || string.IsNullOrEmpty(usuario.Telefone)) ||
                            (string.IsNullOrEmpty(usuario.data_de_nascimento));

                var frontendUrl = $"http://localhost:5173/login?token={tokenResponse.Token}&nome={nome}&needs_completion={needsCompletion.ToString().ToLower()}";


                

               
                return Redirect(frontendUrl);
            }
            catch (Exception ex)
            {
               
                return Redirect($"http://localhost:5173/login?error=token_failed&message={Uri.EscapeDataString(ex.Message)}");
            }
        }
        [Authorize] 
[HttpPost("completar-cadastro")]
        public async Task<IActionResult> CompletarCadastro([FromBody] CompletarCadastroDTO dto)
        {
            
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Token inválido ou ID de usuário ausente.");
            }

            try
            {
                
                await _service.CompletarCadastroAsync(userId, dto);

                return Ok(new { message = "Cadastro completado com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ocorreu um erro interno ao completar o cadastro." });
            }
        }
    }

}
