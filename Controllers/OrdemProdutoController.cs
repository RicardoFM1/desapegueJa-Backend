using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/ordemProduto")]
    public class OrdemProdutoController : ControllerBase
    {
        private readonly OrdemProdutoService _service;

        public OrdemProdutoController(OrdemProdutoService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult ListarTodos()
        {
            return Ok(_service.ListarTodos());
        }

        [HttpGet("usuario/{usuarioId}")]
        public IActionResult BuscarPorUsuarioId(int usuarioId)
        {
            try
            {
                var item = _service.BuscarPorUsuarioId(usuarioId);
                return Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("usuario/{usuarioId}")]
        public IActionResult Criar(int usuarioId, [FromBody] OrdemProduto dto)
        {
            try
            {
                var item = _service.CriarOrdemProduto(usuarioId, dto);
                return CreatedAtAction(nameof(BuscarPorUsuarioId), new { usuarioId = usuarioId }, item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("usuario/{usuarioId}")]
        public IActionResult Atualizar(int usuarioId, [FromBody] OrdemProdutoUpdateDTO dto)
        {
            try
            {
                var item = _service.AtualizarOrdemProduto(usuarioId, dto);
                return Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
    }
}
