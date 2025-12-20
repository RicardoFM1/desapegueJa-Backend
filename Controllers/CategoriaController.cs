using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/categorias")]
    public class CategoriaController : ControllerBase
    {
        private readonly CategoriasService _service;

        public CategoriaController(CategoriasService service)
        {
            _service = service;
        }

        // GET /desapega/categorias
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? status)
        {
            try
            {
                var categorias = await _service.ObterCategoriasAsync(status);
                return Ok(categorias);
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

        // GET /desapega/categorias/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] string? status)
        {
            try
            {
                var categoria = await _service.BuscarCategoriaPorIdAsync(id, status);
                return Ok(categoria);
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

        // POST /desapega/categorias
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CriarCategoria([FromBody] Categorias categorias)
        {
            try
            {
                var isAdmin = User.FindFirst("isAdmin")?.Value?.ToLower() == "true";
                if (!isAdmin)
                    return StatusCode(403, new { message = "Sem autorização para criar categoria" });

                var novaCategoria = await _service.CriarCategoriaAsync(categorias);
                return StatusCode(201, novaCategoria);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }

        // PATCH /desapega/categorias/{id}
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> AtualizarCategoria(
            int id,
            [FromBody] CategoriasUpdateDTO categorias)
        {
            try
            {
                var isAdmin = User.FindFirst("isAdmin")?.Value?.ToLower() == "true";
                if (!isAdmin)
                    return StatusCode(403, new { message = "Sem autorização para atualizar categoria" });

                var categoriaAtualizada =
                    await _service.AtualizarCategoriaAsync(id, categorias);

                return Ok(categoriaAtualizada);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }
    }
}
