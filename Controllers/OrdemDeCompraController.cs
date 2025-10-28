using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/ordemCompra")]
    public class OrdemDeCompraController : ControllerBase
    {
        public readonly OrdemDeCompraService _service;

        public OrdemDeCompraController(OrdemDeCompraService service)
        {
            _service = service;
        }

        [HttpGet]

        public IActionResult Get()
        {
            try
            {

            var formas = _service.GetOrdensDeCompras();
            return Ok(formas);
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

            var ordem = _service.GetById(id);
            return Ok(ordem);
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
        public IActionResult CriarOrdemDeCompra([FromBody] OrdemDeCompra ordem)
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
                if (loggedUserIdStr == null && isAdmin == false)
                {
                    return StatusCode(403, new { message = "Sem autorização para criar essa ordem de compra" });
                }
                

                var ordemNova = _service.CriarOrdemDeCompra(ordem);
                return StatusCode(201, ordemNova);
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
        public IActionResult AtualizarOrdemDeCompra(int id, [FromBody] OrdemDeCompraUpdateDTO ordem)
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
                    return StatusCode(403, new { message = "Sem autorização para atualizar essa ordem de pagamento" });

                if (isAdmin == false && id != loggedUserIdInt)
                    return StatusCode(403, new { message = "Sem autorização para atualizar essa ordem de pagamento" });
                var ordemAtualizada = _service.AtualizarOrdemDeCompra(id, ordem);
                return StatusCode(200, ordemAtualizada);
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
