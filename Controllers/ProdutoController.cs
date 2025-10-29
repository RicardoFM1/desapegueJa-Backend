using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/produtos")]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoService _service;
        private readonly IProdutoRepository _repoProduto;

        public ProdutoController(ProdutoService service, IProdutoRepository repoProduto)
        {
            _service = service;
            _repoProduto = repoProduto;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string? status)
        {
            try
            {

            var produtos = _service.ObterProdutos(status);
            return Ok(produtos);
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
        public IActionResult GetById(int id, [FromQuery] string? status)
        {
            try
            {

            var produto = _service.GetProdutoById(id, status);
            return Ok(produto);
            }
            catch(InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [Authorize] 
        [HttpPost]
        public IActionResult CriarProduto([FromBody] Produto produto)
        {
            try
            {
                
                var loggedUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if(loggedUserIdStr == null)
                {
                    return StatusCode(403, new { message = "Sem autorização para criar esse produto" });
                }
               

                var novoProduto = _service.CriarProduto(produto);
                return StatusCode(201, novoProduto);
            }
            catch(InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }
        [Authorize]
        [HttpPatch("{id}")]

        public IActionResult atualizarProduto(int id, [FromBody] ProdutoUpdateDTO produtoAtualizado, [FromQuery] string? status)
        {
            try
            {
                var produtoExistente = _repoProduto.BuscarPorId(id, status);
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
                if (produtoExistente != null)
                {
                    produtoAtualizado.usuario_id = produtoExistente.usuario_id;

                }
               
                if (!int.TryParse(loggedUserIdStr, out int loggedUserIdInt))
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse produto" });

                if (isAdmin == false && produtoAtualizado.usuario_id != loggedUserIdInt)
                {
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse produto" });
                }
                if( produtoExistente.status.ToLower() == "inativo" && isAdmin == false)
                {
                    return StatusCode(403, new { message = "Sem autorização para atualizar esse produto" });
                }

                var atualizacao = _service.AtualizarProduto(id, produtoAtualizado, status);
                return StatusCode(200, atualizacao);

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
    } 
}
