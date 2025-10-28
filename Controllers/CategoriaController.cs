using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/categorias")]
    public class CategoriaController : ControllerBase
    {
        public readonly CategoriasService _service;

        public CategoriaController(CategoriasService service)
        {
            _service = service;
        }
        [HttpGet]
        public IActionResult Get([FromQuery] string? status)
        {
            try
            {

            var categorias = _service.ObterCategorias(status);
            return Ok(categorias);
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

            var categoria = _service.BuscarCategoriaPorId(id, status);
            return Ok(categoria);
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
        public IActionResult CriarCategoria([FromBody] Categorias categorias)
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
                if (loggedUserIdStr == null || isAdmin == false)
                {
                    return StatusCode(403, new { message = "Sem autorização para criar essa ordem de compra" });
                }
                
                var novaCategoria = _service.CriarCategoria(categorias);
                return StatusCode(201, novaCategoria);
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
        [HttpPatch("{id}")]
        public IActionResult AtualizarCategoria(int id, [FromBody] CategoriasUpdateDTO categorias)
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
                    return StatusCode(403, new { message = "Sem autorização para atualizar essa categoria" });

                if (isAdmin == false)
                    return StatusCode(403, new { message = "Sem autorização para atualizar essa categoria" });

                var categoria = _service.AtualizarCategoria(id, categorias);
                return StatusCode(200, categoria);
            }
            catch(InvalidOperationException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno" + ex.Message });
            }
        }
    }
}
