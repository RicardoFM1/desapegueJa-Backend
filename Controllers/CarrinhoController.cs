using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/carrinho")]
    public class CarrinhoController : ControllerBase
    {
        public readonly CarrinhoService _service;

        public CarrinhoController(CarrinhoService service)
        {
            _service = service;
        }
        [HttpGet]
        public IActionResult Get()
        {
            try
            {

                var carrinho = _service.BuscarCarrinho();
                return Ok(carrinho);
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

                var carrinho = _service.BuscarPorCarrinhoId(id);
                return Ok(carrinho);
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
        [HttpPost]
        public IActionResult CriarCarrinho([FromBody] Carrinho carrinho)
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
                    return StatusCode(403, new { message = "Sem autorização para criar esse item no carrinho" });
                }

                var novoCarrinho = _service.CriarCarrinho(carrinho);
                return StatusCode(201, novoCarrinho);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }
        [Authorize]
        [HttpPatch("{usuarioId}/{produtoId}")]
        public IActionResult AtualizarCarrinho(int usuarioId, int produtoId, [FromBody] CarrinhoUpdateDTO carrinho)
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
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse carrinho" });

                if (isAdmin == false && usuarioId != loggedUserIdInt)
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse carrinho" });

                var carrinhoAtualizado = _service.AtualizarCarrinho(usuarioId, produtoId, carrinho);
                return StatusCode(200, carrinhoAtualizado);
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
        [Authorize]
        [HttpDelete("{usuarioId}/{produtoId}")]

        public IActionResult DeletarItemCarrinho(int usuarioId, int produtoId)
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
                    return StatusCode(403, new { message = "Sem autorização para deletar esse carrinho" });

                if (isAdmin == false && usuarioId != loggedUserIdInt)
                    return StatusCode(403, new { message = "Sem autorização para deletar esse carrinho" });

                _service.DeletarCarrinho(usuarioId, produtoId);
                return StatusCode(200, new { message = "item do carrinho deletado com sucesso!" });
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
