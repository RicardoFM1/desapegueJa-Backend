using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;
using System.Security.Claims;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/formasPagamento")]
    public class FormasDePagamentoController : ControllerBase
    {
        public readonly FormasDePagamentoService _service;

        public FormasDePagamentoController(FormasDePagamentoService service)
        {
            _service = service;
        }

        [HttpGet]

        public IActionResult Get()
        {
            try
            {

            var formas = _service.GetFormasDePagamentos();
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

            var forma = _service.getFormasById(id);
            return Ok(forma);
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
        public IActionResult CriarFormaDePagamento([FromBody] FormasDePagamento forma)
        {
            try
            {
            var formaNova = _service.CriarFormaDePagamento(forma);
            return StatusCode(201, formaNova);
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
        [HttpPatch("{id}")]
        public IActionResult AtualizarFormaDePagamento(int id, [FromBody] FormasDePagamentoUpdateDTO forma)
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
             

                var formaAtualizada = _service.AtualizarFormaDePagamento(id, forma);
                return StatusCode(200, formaAtualizada);
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
    }
}
