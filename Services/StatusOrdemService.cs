using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class StatusOrdemService
    {
        private readonly IStatusOrdemRepository _repo;

        public StatusOrdemService(IStatusOrdemRepository repo)
        {
            _repo = repo;
        }
        public IEnumerable<StatusOrdem> GetStatusDeOrdemDePagamento()
        {
            return _repo.ListarTodos();
        }

        public StatusOrdem GetStatusDeOrdemDePagamentoById(int id)
        {
            var status =  _repo.BuscarPorId(id);
            if(status == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar esse status de ordem de pagamento");
            }
            return status;
        }

        public StatusOrdem CriarStatusDeOrdemDePagamento(StatusOrdem status)
        {
            var statusExistente = _repo.BuscarPorDescricao(status.descricao);
            if (statusExistente != null && statusExistente.status.ToLower() == "ativo")
            {
                throw new InvalidOperationException("Descrição do status de ordem já existente.");

            }
            _repo.Adicionar(status);
            return status;
        }
        public StatusOrdem? AtualizarStatusDeOrdemDePagamento(int id, StatusOrdemUpdateDTO status)
        {
            var descricaoExistente = _repo.BuscarPorDescricao(status.descricao);
            if (descricaoExistente != null && descricaoExistente.id != id && descricaoExistente.status.ToLower() == "ativo")
            {
                throw new InvalidOperationException("Descrição do status de ordem já existente.");

            }
            var statusExistente = _repo.BuscarPorId(id);
            if (statusExistente == null)
            {
                throw new InvalidOperationException("Descrição de status de ordem de pagamento não encontrado.");
            }
            statusExistente.id = id;
            _repo.Atualizar(id, status);
            return statusExistente;

        }
    }
}
