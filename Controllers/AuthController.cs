using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
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
                if (loginResponse == null)
                {
                   return Unauthorized(new { message = "Usuário ou senha inválidos." });
                }
                 
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
                    Admin = (bool)usuario.Admin

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
    }

}
