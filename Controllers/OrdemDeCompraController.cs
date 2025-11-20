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

        [HttpGet("/usuario/{id}")]

        public IActionResult GetByUsuarioId(int id)
        {
            try
            {

            var ordem = _service.GetByUsuarioId(id);
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
        public IActionResult CriarOrdemDeCompra([FromBody] OrdemDeCompraCreateDTO dto)
        {
            try
            {
                var loggedUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.FindFirst("isAdmin")?.Value.ToLower() == "true";

                if (loggedUserIdStr == null && !isAdmin)
                    return StatusCode(403, new { message = "Sem autorização para criar essa ordem de compra" });

            
                var ordem = new OrdemDeCompraCreateDTO
                {
                    usuario_id = dto.usuario_id,
                    status_ordem_id = dto.status_ordem_id,
                    valor_total = dto.valor_total,
                    metodo_entrega = dto.metodo_entrega
                };

                var itens = dto.itens.Select(i => new OrdemProduto
                {
                    produto_id = i.produto_id,
                    quantidade = i.quantidade
                }).ToList();

                var ordemNova = _service.CriarOrdemDeCompra(ordem, itens);
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
        [HttpDelete("{usuarioId}")]
        public IActionResult DeletarOrdemDeCompra(int usuarioId)
        {
            try
            {
                var loggedUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.FindFirst("isAdmin")?.Value.ToLower() == "true";

                if (!int.TryParse(loggedUserIdStr, out int loggedUserIdInt) && !isAdmin)
                    return StatusCode(403, new { message = "Sem autorização para deletar essa ordem de compra" });

                if (!isAdmin && usuarioId != loggedUserIdInt)
                    return StatusCode(403, new { message = "Sem autorização para deletar essa ordem de compra" });

                _service.DeletarOrdemDeCompra(usuarioId);
                return Ok(new { message = "Ordem de compra deletada com sucesso." });
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
