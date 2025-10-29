using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BackendDesapegaJa.Services
{
    public class FormasDePagamentoService
    {
        public readonly IFormasDePagamentoRepository _repo;

        public FormasDePagamentoService(IFormasDePagamentoRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<FormasDePagamento> GetFormasDePagamentos(string? status = null)
        {
            return _repo.ListarTodos(status);
        }
        
        public FormasDePagamento getFormasById(int id, string? status = null)
        {
            var formas = _repo.BuscarPorId(id, status);
            if(formas == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar essa forma de pagamento");
            }
            return formas;
        }

        public FormasDePagamento CriarFormaDePagamento(FormasDePagamento forma)
        {
            var formaExistente = _repo.BuscarPorForma(forma.forma);
            if(formaExistente != null && formaExistente.status.ToLower() == "ativo")
            {
                throw new InvalidOperationException("Forma de pagamento já existente.");
                
            }
            _repo.Adicionar(forma);
            return forma;
        }
        public FormasDePagamento AtualizarFormaDePagamento(int id, FormasDePagamentoUpdateDTO forma, string? status = null)
        {
            var formaJaExistente = _repo.BuscarPorForma(forma.forma);
            if (formaJaExistente != null && formaJaExistente.id != id && formaJaExistente.status.ToLower() == "ativo")
            {
                throw new InvalidOperationException("Forma de pagamento já existente.");

            }
            var formaexistente = _repo.BuscarPorId(id);
            if(formaexistente == null)
            {
                throw new InvalidOperationException("Forma de pagamento não encontrada.");
            }
            formaexistente.id = id;
            _repo.Atualizar(id, forma);
            return formaexistente;
           
        }
    }
}
