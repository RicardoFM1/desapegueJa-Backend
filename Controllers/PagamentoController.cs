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
        public readonly PagamentoService _service;

        public PagamentoController(PagamentoService service)
        {
            _service = service;
        }

        [HttpGet]

        public IActionResult Get()
        {
            try
            {

            var pagamentos = _service.GetPagamentos();
            return Ok(pagamentos);
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

            var pagamento = _service.GetPagamentosById(id);
            return Ok(pagamento);
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
        public IActionResult CriarPagamento([FromBody] Pagamentos pagamento)
        {
            try
            {
                var loggedUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
               
                if (loggedUserIdStr == null)
                {
                    return StatusCode(403, new { message = "Sem autorização para efetuar esse pagamento" });
                }
                

                var pagamentoNovo = _service.CriarPagamento(pagamento);
                return StatusCode(201, pagamentoNovo);
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
        [HttpPatch("{id}")]
        public IActionResult AtualizarPagamento(int id, [FromBody] PagamentosUpdateDTO pagamento)
        {
            try
            {
                var pagamentoExistente = _service.GetPagamentosById(id);
                var isAdmin = false;
                var admin = User.FindFirst("isAdmin").Value;

                if (User.FindFirst("isAdmin")?.Value == "true")
                {
                    isAdmin = true;
                }
                else
                {
                    isAdmin = false;
                }

                var loggedUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                

                if (loggedUserIdStr == null)
                {
                    return StatusCode(403, new { message = "Sem autorização para efetuar esse pagamento" });
                }
                if( pagamentoExistente.status.ToLower() == "inativo" && isAdmin == false)
                {
                    return StatusCode(403, new { message = "Sem autorização para efetuar esse pagamento" });
                }

                var pagamentoAtualizado = _service.AtualizarPagamentos(id, pagamento);
                return StatusCode(200, pagamentoAtualizado);
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
