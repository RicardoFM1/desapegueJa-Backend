using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/statusPagamento")]
    public class StatusDePagamentoController : ControllerBase
    {
        public readonly StatusDePagamentoService _service;

        public StatusDePagamentoController(StatusDePagamentoService service)
        {
            _service = service;
        }

        [HttpGet]

        public IActionResult Get([FromQuery] string? statusquery)
        {
            var statusDePagamentos = _service.GetStatusDePagamento(statusquery);
            return Ok(statusDePagamentos);
        }

        [HttpGet("{id}")]

        public IActionResult GetById(int id, [FromQuery] string? statusquery)
        {
            try
            {

            var status = _service.GetStatusDePagamentoById(id, statusquery);
            return Ok(status);
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
        public IActionResult CriarStatusDePagamento([FromBody] StatusDePagamento status)
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
                if (loggedUserIdStr == null)
                {
                    return StatusCode(403, new { message = "Sem autorização para criar esse status de pagamento" });
                }
                if (isAdmin == false)
                {
                    return StatusCode(403, new { message = "Sem autorização para criar esse status de pagamento" });
                }


                var statusNovo = _service.CriarStatusDePagamento(status);
                return StatusCode(201, statusNovo);
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
        public IActionResult AtualizarStatusDePagamento(int id, [FromBody] StatusDePagamentoUpdateDTO status, [FromQuery] string? statusquery)
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
                if (!int.TryParse(loggedUserIdStr, out int loggedUserIdInt))
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse status de ordem de pagamento" });

                if (isAdmin == false)
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse status de ordem de pagamento" });

                var statusAtualizado = _service.AtualizarStatusDePagamento(id, status, statusquery);
                return StatusCode(200, statusAtualizado);
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
    }
}
