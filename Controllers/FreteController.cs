using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega")]
    public class FreteController : ControllerBase
    {
        public readonly FreteService _service;

       
        public FreteController(FreteService service)
        {
            _service = service;
        }


        [HttpPost("calcularFrete")]
      
        public async Task<IActionResult> CalcularFrete([FromBody] DTOFreteRequest request)
        {
            try
            {
         
                var frete = await _service.CalcularFreteTotalAsync(request);

                return Ok(frete);
            }
            catch (InvalidOperationException ex)
            {
             
                return StatusCode(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, new { message = "Erro interno ao calcular frete: " + ex.Message });
            }
        }
    }
}

