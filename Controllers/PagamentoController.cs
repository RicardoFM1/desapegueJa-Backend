using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/pagamentos")]
    public class PagamentoController : ControllerBase
    {
        private readonly PagamentoService _service;

        public PagamentoController(PagamentoService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string? status)
        {
            try
            {
                return Ok(_service.GetPagamentos(status));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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
                return Ok(_service.GetPagamentosById(id, status));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CriarPagamento([FromBody] Pagamentos pagamento)
        {
            try
            {
                var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (loggedUserId == null)
                    return StatusCode(403, new { message = "Sem autorização para efetuar esse pagamento" });

                var pagamentoNovo = _service.CriarPagamento(pagamento);
                return StatusCode(201, pagamentoNovo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPatch("{id}")]
        public IActionResult AtualizarPagamento(int id, [FromBody] PagamentosUpdateDTO pagamento, [FromQuery] string? status)
        {
            try
            {
                var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (loggedUserId == null)
                    return StatusCode(403, new { message = "Sem autorização para efetuar esse pagamento" });

                var isAdmin = User.FindFirst("isAdmin")?.Value == "true";
                var pagamentoExistente = _service.GetPagamentosById(id, status);

                if (pagamentoExistente.status?.ToLower() == "inativo" && !isAdmin)
                    return StatusCode(403, new { message = "Sem autorização para atualizar pagamento inativo" });

                var pagamentoAtualizado = _service.AtualizarPagamentos(id, pagamento, status);
                return Ok(pagamentoAtualizado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
