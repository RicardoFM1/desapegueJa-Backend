using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendDesapegaJa.Controllers
{
    [ApiController]
    [Route("desapega/ordemProduto")]
    public class OrdemProdutoController : ControllerBase
    {
        private readonly IOrdemProdutoRepository _repo;

        public OrdemProdutoController(IOrdemProdutoRepository repo)
        {
            _repo = repo;
        }

      
        [HttpGet]
        public IActionResult ListarTodos()
        {
            var itens = _repo.ListarTodos();
            return Ok(itens);
        }

   
        [HttpGet("{id}")]
        public IActionResult BuscarPorId(int id)
        {
            var item = _repo.BuscarPorId(id);
            if (item == null)
                return NotFound("Item de ordem_produto não encontrado.");

            return Ok(item);
        }

  
        [HttpPost]
        public IActionResult Criar([FromBody] OrdemProduto item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _repo.Adicionar(item);
            return CreatedAtAction(nameof(BuscarPorId), new { id = item.id }, item);
        }


        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] OrdemProdutoUpdateDTO dto)
        {
            var existente = _repo.BuscarPorId(id);
            if (existente == null)
                return NotFound("Item de ordem_produto não encontrado.");

            _repo.Atualizar(id, dto);
            return Ok(dto);
        }



        [HttpDelete("{id}")]
        public IActionResult Deletar(int id)
        {
            var existente = _repo.BuscarPorId(id);
            if (existente == null)
                return NotFound("Item de ordem_produto não encontrado.");

            _repo.Deletar(id);
            return NoContent();
        }
    }
}
