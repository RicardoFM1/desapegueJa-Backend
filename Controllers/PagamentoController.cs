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
        private readonly PagSeguroIntegration _pagSeguro;

        public PagamentoController(PagamentoService service, PagSeguroIntegration pagSeguro)
        {
            _service = service;
            _pagSeguro = pagSeguro; 
        }

        // GET /desapega/pagamentos
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_service.GetPagamentos());
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
        [HttpPost("webhook")]
        [AllowAnonymous]
        public IActionResult HandlePagSeguroWebhook([FromHeader(Name = "X-Webhook-Token")] string? token, [FromBody] PagSeguroWebhookNotification data)
        {
            try
            {
                // 1. Validação de Segurança (Obrigatória!)
                if (string.IsNullOrWhiteSpace(token) || !_pagSeguro.ValidateWebhookToken(token))
                {
                    return Unauthorized(new { message = "Token de Webhook inválido." });
                }

                // ... (Verificação de dados e mapeamento de status) ...
                int novoStatusId;
                switch (data.status?.ToUpper())
                {
                    case "PAID":
                    case "COMPLETED":
                        novoStatusId = 2;
                        break;
                    case "CANCELED":
                    case "EXPIRED":
                        novoStatusId = 4;
                        break;
                    default:
                        return Ok();
                }

                // 2. Atualiza o status no seu sistema
                _service.AtualizarStatusPagamentoPorReferencia(
                    data.reference_id!,
                    novoStatusId,
                    data.amount_paid
                );

                // 3. Resposta obrigatória ao PagSeguro
                return Ok();
            }
            // ... (Tratamento de exceções) ...
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar Webhook: " + ex.Message });
            }
        }

        // GET /desapega/pagamentos/usuario/{usuarioId}
        [HttpGet("usuario/{usuarioId}")]
        public IActionResult GetByUsuarioId(int usuarioId)
        {
            try
            {
                return Ok(_service.GetPagamentoByUsuarioId(usuarioId));
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

        // POST /desapega/pagamentos
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

        // PATCH /desapega/pagamentos/usuario/{usuarioId}
        [HttpPatch("usuario/{usuarioId}")]
        public IActionResult AtualizarPagamento(int usuarioId, [FromBody] PagamentosUpdateDTO pagamento)
        {
            try
            {
                var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (loggedUserId == null)
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse pagamento" });

                var pagamentoAtualizado = _service.AtualizarPagamento(usuarioId, pagamento);
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

        // DELETE /desapega/pagamentos/usuario/{usuarioId}
        [HttpDelete("usuario/{usuarioId}")]
        public IActionResult DeletarPagamento(int usuarioId)
        {
            try
            {
                var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (loggedUserId == null)
                    return StatusCode(403, new { message = "Sem autorização para deletar esse pagamento" });

                _service.DeletarPagamentoPorUsuarioId(usuarioId);
                return NoContent();
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
