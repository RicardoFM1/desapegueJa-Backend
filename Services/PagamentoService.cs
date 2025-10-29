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
            var pagamentos = _repo.BuscarPorId(id, status);
            if(pagamentos == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar esse pagamento");
            }
            return pagamentos;
        }

        public Pagamentos CriarPagamento(Pagamentos pagamento)
        {
           
            var usuarioExistente = _repoUser.BuscarPorId(pagamento.usuario_id);
            var formaPagamentoExistente = _repoFormaPagamento.BuscarPorId(pagamento.formas_de_pagamento_id);
            var statusPagamentoExistente = _repoStatusPagamento.BuscarPorId(pagamento.status_de_pagamento_id);
            if (usuarioExistente == null)
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            }
            if (formaPagamentoExistente == null)
            {
                throw new InvalidOperationException("Forma de pagamento referenciada não encontrada");
            }
            if (statusPagamentoExistente == null)
            {
                throw new InvalidOperationException("Status de pagamento não encontrado");
            }
            _repo.Adicionar(pagamento);
            return pagamento;

        }
        public Pagamentos? AtualizarPagamentos(int id, PagamentosUpdateDTO pagamento, string? statusquery = null)
        {
           
            var pagamentoExistente = _repo.BuscarPorId(id, statusquery);

            int usuarioIdFinal = pagamento.usuario_id ?? pagamentoExistente.usuario_id;
            int formasPagamentoIdFinal = pagamento.formas_de_pagamento_id ?? pagamentoExistente.formas_de_pagamento_id;
            int statusPagamentoIdFinal = pagamento.status_de_pagamento_id ?? pagamentoExistente.status_de_pagamento_id;
            var observacao = string.IsNullOrWhiteSpace(pagamento.observacao) ? pagamentoExistente.observacao : pagamento.observacao;
            var createdAt = string.IsNullOrWhiteSpace(pagamento.createdAt) ? pagamentoExistente.createdAt : pagamento.createdAt;
            var updatedAt = string.IsNullOrWhiteSpace(pagamento.updatedAt) ? pagamentoExistente.updatedAt : pagamento.updatedAt;
            var status = string.IsNullOrWhiteSpace(pagamento.status) ? pagamentoExistente.status : pagamento.status;

            var usuarioExistente = _repoUser.BuscarPorId(usuarioIdFinal);
            var formaPagamentoExistente = _repoFormaPagamento.BuscarPorId(formasPagamentoIdFinal);
            var statusPagamentoExistente = _repoStatusPagamento.BuscarPorId(statusPagamentoIdFinal);




            if (usuarioExistente == null)
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            }
            if (formaPagamentoExistente == null)
            {
                throw new InvalidOperationException("Forma de pagamento referenciada não encontrada");
            }
            if (statusPagamentoExistente == null)
            {
                throw new InvalidOperationException("Status de pagamento referenciado não encontrado");
            }


            var pagamentoAtualizado = _repo.Atualizar(id, pagamento);
            return pagamentoAtualizado;
        }
    }
}
