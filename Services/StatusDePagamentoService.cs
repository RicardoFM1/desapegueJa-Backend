using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class StatusDePagamentoService
    {
        private readonly IStatusDePagamentoRepository _repo;

        public StatusDePagamentoService(IStatusDePagamentoRepository repo)
        {
            _repo = repo;
        }
        public IEnumerable<StatusDePagamento> GetStatusDePagamento()
        {
            return _repo.ListarTodos();
        }

        public StatusDePagamento GetStatusDePagamentoById(int id)
        {
            var status = _repo.BuscarPorId(id);
            if(status == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar esse status de pagamento");
            }
                return status;
        }

        public StatusDePagamento CriarStatusDePagamento(StatusDePagamento status)
        {
            var statusExistente = _repo.BuscarPorDescricao(status.descricao);
            if (statusExistente != null && statusExistente.status.ToLower() == "ativo")
            {
                throw new InvalidOperationException("Descrição do status de pagamento já existente.");

            }
            _repo.Adicionar(status);
            return status;
        }
        public StatusDePagamento? AtualizarStatusDePagamento(int id, StatusDePagamentoUpdateDTO status)
        {
            var descricaoExistente = _repo.BuscarPorDescricao(status.descricao);
            if (descricaoExistente != null && descricaoExistente.id != id && descricaoExistente.status.ToLower() == "ativo")
            {
                throw new InvalidOperationException("Descrição do status de pagamento já existente.");

            }
            var statusExistente = _repo.BuscarPorId(id);
            if (statusExistente == null)
            {
                throw new InvalidOperationException("Status de pagamento não encontrado.");
            }
            statusExistente.id = id;
            _repo.Atualizar(id, status);
            return statusExistente;

        }
    }
}
