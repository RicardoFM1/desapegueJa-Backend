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

        public IEnumerable<Pagamentos> GetPagamentos()
        {
            return _repo.ListarTodos();
        }

        public Pagamentos GetPagamentoByUsuarioId(int usuarioId)
        {
            var pagamento = _repo.BuscarPorUsuarioId(usuarioId);
            if (pagamento == null)
                throw new InvalidOperationException("Não foi possível encontrar esse pagamento");
            return pagamento;
        }

        public Pagamentos CriarPagamento(Pagamentos pagamento)
        {
         
            var pagamentoExistente = _repo.BuscarPorUsuarioId(pagamento.usuario_id);
            if (pagamentoExistente != null)
                throw new InvalidOperationException("O usuário já possui um pagamento");

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

            _repo.Adicionar(pagamento);
            return pagamento;
        }

        public Pagamentos AtualizarPagamento(int usuarioId, PagamentosUpdateDTO pagamento)
        {
            var existente = _repo.BuscarPorUsuarioId(usuarioId);
            if (existente == null)
                throw new InvalidOperationException("Pagamento não encontrado");

            int usuarioIdFinal = pagamento.usuario_id ?? existente.usuario_id;
            int formaPagamentoIdFinal = pagamento.forma_pagamento_id ?? existente.forma_pagamento_id;
            int statusPagamentoIdFinal = pagamento.status_pagamento_id ?? existente.status_pagamento_id;
            int ordemIdFinal = pagamento.ordem_id ?? existente.ordem_id;
            int valorFinal = pagamento.valor ?? existente.valor;
            string? observacaoFinal = string.IsNullOrWhiteSpace(pagamento.observacao) ? existente.observacao : pagamento.observacao;
            DateTime createdAtFinal = pagamento.createdAt ?? existente.createdAt ?? DateTime.UtcNow;
            DateTime updatedAtFinal = DateTime.UtcNow;

            string? pixQrFinal = pagamento.pix_qr_code ?? existente.pix_qr_code;
            string? pixCopiaFinal = pagamento.pix_copia_codigo ?? existente.pix_copia_codigo;
            string? boletoUrlFinal = pagamento.boleto_url ?? existente.boleto_url;
            string? pagUUIDFinal = pagamento.pagamento_uuid ?? existente.pagamento_uuid;
            DateTime? expiracaoFinal = pagamento.expiracao ?? existente.expiracao;
            int? valorPagoFinal = pagamento.valor_pago ?? existente.valor_pago;

            var usuario = _repoUser.BuscarPorId(usuarioIdFinal);
            var formaPagamento = _repoFormaPagamento.BuscarPorId(formaPagamentoIdFinal);
            var statusPagamento = _repoStatusPagamento.BuscarPorId(statusPagamentoIdFinal);

            if (usuario == null || usuario.status.ToLower() == "inativo")
                throw new InvalidOperationException("Usuário referenciado não encontrado e/ou inativo");

            if (formaPagamento == null || formaPagamento.status.ToLower() == "inativo")
                throw new InvalidOperationException("Forma de pagamento referenciada não encontrada e/ou inativa");

            if (statusPagamento == null || statusPagamento.status.ToLower() == "inativo")
                throw new InvalidOperationException("Status de pagamento não encontrado e/ou inativo");

            return _repo.Atualizar(usuarioId, new PagamentosUpdateDTO
            {
                usuario_id = usuarioIdFinal,
                forma_pagamento_id = formaPagamentoIdFinal,
                status_pagamento_id = statusPagamentoIdFinal,
                ordem_id = ordemIdFinal,
                valor = valorFinal,
                observacao = observacaoFinal,
                createdAt = createdAtFinal,
                updatedAt = updatedAtFinal,
                pix_qr_code = pixQrFinal,
                pix_copia_codigo = pixCopiaFinal,
                boleto_url = boletoUrlFinal,
                expiracao = expiracaoFinal,
                valor_pago = valorPagoFinal,
                pagamento_uuid = pagUUIDFinal
            });
        }

        public void DeletarPagamentoPorUsuarioId(int usuarioId)
        {
            var pagamento = _repo.BuscarPorUsuarioId(usuarioId);
            if (pagamento == null)
                throw new InvalidOperationException("Pagamento não encontrado para esse usuário");

            _repo.DeletarPorUsuarioId(usuarioId);
        }
    }
}
