using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class PagamentoService
    {
        private readonly IPagamentosRepository _repo;
        private readonly IUsuarioRepository _repoUser;
        private readonly IFormasDePagamentoRepository _repoFormaPagamento;
        private readonly IStatusDePagamentoRepository _repoStatusPagamento;

        public PagamentoService(
            IPagamentosRepository repo,
            IUsuarioRepository user,
            IFormasDePagamentoRepository formasPagamento,
            IStatusDePagamentoRepository statusPagamento
        )
        {
            _repo = repo;
            _repoUser = user;
            _repoFormaPagamento = formasPagamento;
            _repoStatusPagamento = statusPagamento;
        }

        public IEnumerable<Pagamentos> GetPagamentos(string? status = null)
        {
            return _repo.ListarTodos(status);
        }

        public Pagamentos GetPagamentosById(int id, string? status = null)
        {
            var pagamento = _repo.BuscarPorId(id, status);
            if (pagamento == null)
                throw new InvalidOperationException("Não foi possível encontrar esse pagamento");
            return pagamento;
        }

        public Pagamentos CriarPagamento(Pagamentos pagamento)
        {
            var usuario = _repoUser.BuscarPorId(pagamento.usuario_id);
            var formaPagamento = _repoFormaPagamento.BuscarPorId(pagamento.forma_pagamento_id);
            var statusPagamento = _repoStatusPagamento.BuscarPorId(pagamento.status_pagamento_id);

            if (usuario == null || usuario.status.ToLower() == "inativo")
                throw new InvalidOperationException("Usuário referenciado não encontrado e/ou inativo");

            if (formaPagamento == null || formaPagamento.status.ToLower() == "inativo")
                throw new InvalidOperationException("Forma de pagamento referenciada não encontrada e/ou inativa");

            if (statusPagamento == null || statusPagamento.status.ToLower() == "inativo")
                throw new InvalidOperationException("Status de pagamento não encontrado e/ou inativo");

            pagamento.createdAt = DateTime.UtcNow;
            pagamento.status = string.IsNullOrWhiteSpace(pagamento.status) ? "ativo" : pagamento.status;

            _repo.Adicionar(pagamento);
            return pagamento;
        }

        public Pagamentos AtualizarPagamentos(int id, PagamentosUpdateDTO pagamento, string? statusQuery = null)
        {
            var existente = _repo.BuscarPorId(id, statusQuery);
            if (existente == null)
                throw new InvalidOperationException("Pagamento não encontrado");

            int usuarioIdFinal = pagamento.usuario_id ?? existente.usuario_id;
            int formaPagamentoIdFinal = pagamento.forma_pagamento_id ?? existente.forma_pagamento_id;
            int statusPagamentoIdFinal = pagamento.status_pagamento_id ?? existente.status_pagamento_id;
            int ordemIdFinal = pagamento.ordem_id ?? existente.ordem_id;
            int valorFinal = pagamento.valor ?? existente.valor;
            string observacaoFinal = string.IsNullOrWhiteSpace(pagamento.observacao) ? existente.observacao : pagamento.observacao;
            DateTime createdAtFinal = pagamento.createdAt ?? existente.createdAt ?? DateTime.UtcNow;
            DateTime updatedAtFinal = DateTime.UtcNow;
            string statusFinal = string.IsNullOrWhiteSpace(pagamento.status) ? existente.status : pagamento.status;

            var usuario = _repoUser.BuscarPorId(usuarioIdFinal);
            var formaPagamento = _repoFormaPagamento.BuscarPorId(formaPagamentoIdFinal);
            var statusPagamento = _repoStatusPagamento.BuscarPorId(statusPagamentoIdFinal);

            if (usuario == null || usuario.status.ToLower() == "inativo")
                throw new InvalidOperationException("Usuário referenciado não encontrado e/ou inativo");

            if (formaPagamento == null || formaPagamento.status.ToLower() == "inativo")
                throw new InvalidOperationException("Forma de pagamento referenciada não encontrada e/ou inativa");

            if (statusPagamento == null || statusPagamento.status.ToLower() == "inativo")
                throw new InvalidOperationException("Status de pagamento não encontrado e/ou inativo");

            return _repo.Atualizar(id, new PagamentosUpdateDTO
            {
                usuario_id = usuarioIdFinal,
                forma_pagamento_id = formaPagamentoIdFinal,
                status_pagamento_id = statusPagamentoIdFinal,
                ordem_id = ordemIdFinal,
                valor = valorFinal,
                observacao = observacaoFinal,
                createdAt = createdAtFinal,
                updatedAt = updatedAtFinal,
                status = statusFinal
            });
        }
    }
}
