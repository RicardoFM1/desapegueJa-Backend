using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Helpers;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/pagamentos")]
    public class PagamentoController : ControllerBase
    {
        private readonly PagamentoService _service;
        private readonly MercadoPagoIntegration _mp;

        public PagamentoController(PagamentoService service, MercadoPagoIntegration mp)
        {
            _service = service;
            _mp = mp;
        }

        // =========================================
        // GET geral
        // =========================================
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_service.GetPagamentos());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =========================================
        // WEBHOOK MERCADO PAGO
        // =========================================
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleMercadoPagoWebhook([FromBody] MercadoPagoWebhook data)
        {
            try
            {
                if (data == null || data.data == null || string.IsNullOrWhiteSpace(data.data.id))
                    return BadRequest(new { message = "Payload inválido" });

                string paymentId = data.data.id;

                var pagamentoMP = await _mp.ObterPagamentoPorId(paymentId);


                if (pagamentoMP == null)
                    return NotFound(new { message = "Pagamento não encontrado" });

                int novoStatusId = pagamentoMP.Status switch
                {
                    "approved" => 2,
                    "rejected" or "cancelled" => 4,
                    _ => 0
                };

                if (novoStatusId == 0)
                    return Ok();

                int? valorPago = pagamentoMP.Status == "approved"
                    ? (int)(pagamentoMP.TransactionAmount * 100)
                    : null;

                _service.AtualizarStatusPagamentoPorReferencia(
                    pagamentoMP.ExternalReference!,
                    novoStatusId,
                    valorPago
                );

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar webhook: " + ex.Message });
            }
        }


        [HttpGet("usuario/{usuarioId}")]
        public IActionResult GetByUsuarioId(int usuarioId)
        {
            try
            {
                return Ok(_service.GetPagamentoByUsuarioId(usuarioId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CriarPagamento([FromBody] Pagamentos pagamento)
        {
            try
            {
                var pagamentoNovo = await _service.CriarPagamentoAsync(pagamento);
                return StatusCode(201, pagamentoNovo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    
        [HttpPatch("usuario/{usuarioId}")]
        public IActionResult AtualizarPagamento(int usuarioId, [FromBody] PagamentosUpdateDTO pagamento)
        {
            try
            {
                var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (loggedUserId == null)
                    return StatusCode(403, new { message = "Sem autorização" });

                var resultado = _service.AtualizarPagamento(usuarioId, pagamento);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("usuario/{usuarioId}")]
        public IActionResult DeletarPagamento(int usuarioId)
        {
            try
            {
                var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (loggedUserId == null)
                    return StatusCode(403, new { message = "Sem autorização" });

                _service.DeletarPagamentoPorUsuarioId(usuarioId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
