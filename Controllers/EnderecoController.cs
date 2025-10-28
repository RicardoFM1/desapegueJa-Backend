using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/enderecos")]
    public class EnderecoController : ControllerBase
    {
        public readonly EnderecosService _service;

        public EnderecoController(EnderecosService service)
        {
            _service = service;
        }
        [HttpGet]
        public IActionResult Get([FromQuery] string? status)
        {
            try
            {

            var enderecos = _service.ObterEnderecos(status);
            return Ok(enderecos);
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

            var endereco = _service.GetEnderecosById(id, status);
            return Ok(endereco);
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
        public IActionResult CriarEndereco([FromBody] Enderecos enderecos, [FromQuery] string? status)
        {
            try
            {
                var loggedUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = false;

                if (User.FindFirst("isAdmin")?.Value.ToLower() == "true")
                {
                    isAdmin = true;
                }
                else
                {
                    isAdmin = false;
                }
                if (loggedUserIdStr == null || isAdmin == false)
                {
                    return StatusCode(403, new { message = "Sem autorização para criar essa ordem de compra" });
                }
                
                var novoEndereco = _service.CriarEndereco(enderecos, status);
                return StatusCode(201, novoEndereco);

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

        [Authorize]
        [HttpPatch("{id}")]
        public IActionResult AtualizarEnderecos(int id, [FromBody] EnderecosUpdateDTO enderecos, [FromQuery] string? status)
        {
            try
            {
                var enderecoExistente = _service.GetEnderecosById(id, status);
                var loggedUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = false;

                if (User.FindFirst("isAdmin")?.Value.ToLower() == "true")
                {
                    isAdmin = true;
                }
                else
                {
                    isAdmin = false;
                }
                if (!int.TryParse(loggedUserIdStr, out int loggedUserIdInt))
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse endereço" });

                if (isAdmin == false && enderecos.usuario_id != loggedUserIdInt)
                    return StatusCode(403, new { message = "Sem autorização para atualizar essa endereço" });

                if(enderecoExistente.status == "inativo" && isAdmin == false)
                {
                    return StatusCode(403, new { message = "Sem autorização para atualizar essa endereço" });
                }
                var enderecosAtualizado = _service.AtualizarEnderecos(id, enderecos, status);
                return StatusCode(200, enderecos);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno" + ex.Message });
            }
        }
    }
}
