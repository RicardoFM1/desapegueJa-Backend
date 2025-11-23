using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Helpers;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class PagamentoService
    {
        private readonly IPagamentosRepository _repo;
        private readonly IUsuarioRepository _repoUser;
        private readonly IFormasDePagamentoRepository _repoFormaPagamento;
        private readonly IStatusDePagamentoRepository _repoStatusPagamento;
        private readonly IConfiguration _config;
        private readonly IOrdemDeCompraRepository _repoOrdem;
        private readonly MercadoPagoIntegration _mercadoPago;

        public PagamentoService(
            IPagamentosRepository repo,
            IUsuarioRepository user,
            IFormasDePagamentoRepository formasPagamento,
            IStatusDePagamentoRepository statusPagamento,
            IConfiguration config,
            IOrdemDeCompraRepository repoOrdem,
            MercadoPagoIntegration mercadoPago
        )
        {
            _repo = repo;
            _repoUser = user;
            _repoFormaPagamento = formasPagamento;
            _repoStatusPagamento = statusPagamento;
            _config = config;
            _repoOrdem = repoOrdem;
            _mercadoPago = mercadoPago;
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
        public Pagamentos GetPagamentoByTransacaoIdExterno(string transacaoIdExterno)
        {
            
            var pagamento = _repo.BuscarPorUUID(transacaoIdExterno);

            if (pagamento == null)
                throw new InvalidOperationException($"Não foi possível encontrar o pagamento com o ID de transação externo '{transacaoIdExterno}'.");

            return pagamento;
        }
        public int GetStatusIdByNome(string nome)
        {
           
            string nomeNormalizado = nome.ToLower();

            
            var status = _repoStatusPagamento.BuscarPorDescricao(nomeNormalizado);

           
            return status?.id ?? 0;
        }

        public IEnumerable<Pagamentos> ListarPagamentosExpirados()
        {
           
            return _repo.ListarExpirados(DateTime.UtcNow, (int)StatusPagamento.pendente);
        }

        public async Task<Pagamentos> CriarPagamentoAsync(Pagamentos pagamento)
        {
            var pagamentoExistente = _repo.BuscarPagamentoEmAberto(pagamento.usuario_id);
            if (pagamentoExistente != null)
                throw new InvalidOperationException("O usuário já possui um pagamento em aberto.");

            var usuario = _repoUser.BuscarPorId(pagamento.usuario_id);
            var formaPagamento = _repoFormaPagamento.BuscarPorId(pagamento.forma_pagamento_id);
            

            if (usuario == null || usuario.status.ToLower() == "inativo")
                throw new InvalidOperationException("Usuário inválido");

            if (formaPagamento == null || formaPagamento.status.ToLower() == "inativo")
                throw new InvalidOperationException("Forma de pagamento inválida");

            var ordem = _repoOrdem.BuscarPorId(pagamento.ordem_id)
                ?? throw new InvalidOperationException("Ordem de compra não encontrada.");

            pagamento.createdAt = DateTime.UtcNow;

            
            string uuid = Guid.NewGuid().ToString();
            pagamento.pagamento_uuid = uuid;

            pagamento.status_pagamento_id = (int)StatusPagamento.pendente;

            await _repo.AdicionarAsync(pagamento);

          
            if (formaPagamento.forma.ToLower().Contains("pix"))
            {
                var dadosCobranca = await _mercadoPago.CriarCobrancaPixAsync(ordem, usuario, uuid);

                var updateDto = new PagamentosUpdateDTO
                {
                    pix_copia_codigo = dadosCobranca.PixCopiaCodigo,
                    pix_qr_code = dadosCobranca.PixQrCodeBase64 ?? dadosCobranca.PixCopiaCodigo,
                    expiracao = dadosCobranca.Expiracao,
                    status_pagamento_id = (int)StatusPagamento.pendente,
                    pagamento_uuid = uuid,
                    updatedAt = DateTime.UtcNow
                };

                _repo.Atualizar(pagamento.usuario_id, updateDto);
            }

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
            int ordemIdFinal = (int)(pagamento.ordem_id ?? existente.ordem_id);
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
        public Pagamentos? BuscarEPersistirUUID(string novoUuidMp, int novoStatusId, int? valorPago)
        {
          
            var pagamento = _repo.BuscarPorUUID(novoUuidMp);

      
            if (pagamento != null)
            {
              
                if (pagamento.status_pagamento_id == (int)StatusPagamento.pago) return pagamento;

                AtualizarStatusPorUsuario(pagamento.usuario_id, novoStatusId, valorPago, novoUuidMp);
                return pagamento;
            }

            

            return null; 
        }


        public void AtualizarStatusPorUsuario(int usuarioId, int novoStatusId, int? valorPago, string novoUuidMp)
        {
            var updateDto = new PagamentosUpdateDTO
            {
                status_pagamento_id = (int)novoStatusId,
                valor_pago = valorPago,
                pagamento_uuid = novoUuidMp 
            };

           
            _repo.Atualizar(usuarioId, updateDto);
        }



        public void AtualizarStatusPagamentoPorReferencia(string transacaoIdReferencia, int novoStatusId, int? valorPago)
        {
         
            var pagamento = _repo.BuscarPorUUID(transacaoIdReferencia);

          
            if (pagamento == null)
            {
                int idStatusPendente = GetStatusIdByNome("pendente");

                if (idStatusPendente == 0)
                    throw new InvalidOperationException("Erro: Status de pagamento 'pendente' não encontrado no DB.");

                var pagamentoContingencia = _repo.BuscarUltimoPagamentoPendente(idStatusPendente);

                if (pagamentoContingencia != null)
                {
                    Console.WriteLine($"[AVISO CRÍTICO] UUID '{transacaoIdReferencia}' não encontrado. Usando pagamento pendente como fallback.");
                    pagamento = pagamentoContingencia;
                }
            }

            if (pagamento == null)
                throw new InvalidOperationException("Pagamento não encontrado pela referência da transação.");

           
            var updateDto = new PagamentosUpdateDTO
            {
                status_pagamento_id = novoStatusId,
                valor_pago = valorPago,
                pagamento_uuid = transacaoIdReferencia
            };

            _repo.Atualizar(pagamento.usuario_id, updateDto);

           
        }



        public void DeletarPagamentoPorUsuarioId(int usuarioId)
        {
            var pagamento = _repo.BuscarPorUsuarioId(usuarioId);
            if (pagamento == null)
            {
                throw new InvalidOperationException("Pagamento não encontrado para esse usuário");
            }
 

            
            _repo.DeletarPorUsuarioId(usuarioId);
            _repoOrdem.DeletarPorUsuarioId(usuarioId);
        }
    }
}
