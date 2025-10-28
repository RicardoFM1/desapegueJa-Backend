using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/statusOrdem")]
    public class StatusOrdemController : ControllerBase
    {
        public readonly StatusOrdemService _service;

        public StatusOrdemController(StatusOrdemService service)
        {
            _service = service;
        }

        [HttpGet]

        public IActionResult Get()
        {
            try
            {

            var statusDeOrdemDePagamentos = _service.GetStatusDeOrdemDePagamento();
            return Ok(statusDeOrdemDePagamentos);
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

        public IActionResult GetById(int id)
        {
            try
            {

            var status = _service.GetStatusDeOrdemDePagamentoById(id);
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
        public IActionResult CriarStatusDeOrdemDePagamento([FromBody] StatusOrdem status)
        {
            try
            {
                var isAdmin = false;

                if (User.FindFirst("isAdmin")?.Value.ToLower() == "true")
                {
                    isAdmin = true;
                }
                else
                {
                    isAdmin = false;
                }

                if (isAdmin == false)
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse status de ordem de pagamento" });

                var statusNovo = _service.CriarStatusDeOrdemDePagamento(status);
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
        public IActionResult AtualizarStatusDeOrdemDePagamento(int id, [FromBody] StatusOrdemUpdateDTO status)
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

                if (isAdmin == false && id != loggedUserIdInt)
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse status de ordem de pagamento" });

                var statusAtualizado = _service.AtualizarStatusDeOrdemDePagamento(id, status);
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
