using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Helpers;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/pagamentos")]
    public class PagamentoController : ControllerBase
    {
        private readonly PagamentoService _service;
        private readonly MercadoPagoIntegration _mp;
        private readonly IConfiguration _config;

        public PagamentoController(PagamentoService service, MercadoPagoIntegration mp, IConfiguration config)
        {
            _service = service;
            _mp = mp;
            _config = config;
        }

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

        [HttpGet("status/{transacaoIdExterno}")]
        public IActionResult GetStatusPagamento(string transacaoIdExterno)
        {
            try
            {
                var pagamento = _service.GetPagamentoByTransacaoIdExterno(transacaoIdExterno);

                if (pagamento == null)
                    return NotFound(new { message = "Pagamento não encontrado" });

                return Ok(new { statusId = pagamento.status_pagamento_id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("/desapega/pagamentos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleMercadoPagoWebhook([FromBody] JsonElement debugData)
        {
            try
            {
                string jsonString = debugData.GetRawText();
                Console.WriteLine("--- WEBHOOK PAYLOAD (CORRIGIDO) ---");
                Console.WriteLine(jsonString);

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<MercadoPagoWebhook>(jsonString);

                if (data == null || data.data == null || string.IsNullOrWhiteSpace(data.data.id))
                {
                    Console.WriteLine("AVISO: Payload incompleto ou ID nulo.");
                    return Ok();
                }

                string paymentId = data.data.id;
                Console.WriteLine($"Buscando Pagamento ID no MP: {paymentId}");

                var pagamentoMP = await _mp.ObterPagamentoPorId(paymentId);

                if (pagamentoMP == null)
                {
                    Console.WriteLine($"Pagamento {paymentId} não encontrado na API do MP.");
                    return Ok();
                }

                int novoStatusId = pagamentoMP.Status switch
                {
                    "approved" => 2,
                    "rejected" or "cancelled" => 4,
                    _ => 0
                };

                if (novoStatusId == 0) return Ok();

                int? valorPago = pagamentoMP.Status == "approved"
                    ? (int)(pagamentoMP.TransactionAmount * 100)
                    : null;

                Console.WriteLine($"[DEBUG] Valor vindo do MP: {pagamentoMP.TransactionAmount}");
                Console.WriteLine($"[DEBUG] Valor calculado (centavos): {valorPago}");



                const int maxRetries = 3;
const int delayMs = 500; 

string externalReference = pagamentoMP.ExternalReference;

if (string.IsNullOrEmpty(externalReference))
{
    Console.WriteLine("AVISO: Pagamento sem ExternalReference no objeto retornado do MP.");
    return Ok();
}

for (int attempt = 0; attempt < maxRetries; attempt++)
{
    try
    {
        _service.AtualizarStatusPagamentoPorReferencia(
            externalReference,
            novoStatusId,
            valorPago
        );
        Console.WriteLine($"SUCESSO: Banco de dados atualizado na tentativa {attempt + 1} com UUID MP: {externalReference}!");
        return Ok();
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Pagamento não encontrado"))
    {
        
        if (attempt < maxRetries - 1)
        {
            Console.WriteLine($"AVISO: Pagamento {externalReference} não encontrado no DB. Tentando novamente em {delayMs}ms (Tentativa {attempt + 2})...");
            await Task.Delay(delayMs);
        }
        else
        {
            Console.WriteLine($"ERRO CRÍTICO: Pagamento {externalReference} não encontrado após {maxRetries} tentativas. {ex.Message}");
            return StatusCode(500, new { message = "Erro interno: Pagamento não encontrado no DB após tentativas." });
        }
    }
}
return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO GERAL NO WEBHOOK: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, new { message = "Erro interno no webhook: " + ex.Message });
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
            catch (InvalidOperationException ex) 
            {
               
                Console.WriteLine($"ERRO DE OPERAÇÃO: {ex.Message}");
                return BadRequest(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO GERAL: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
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