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
        private readonly IOrdemProdutoRepository _repoOrdemProduto;
        private readonly MercadoPagoIntegration _mercadoPago;
        private readonly IProdutoRepository _repoProduto;

        public PagamentoService(
            IPagamentosRepository repo,
            IUsuarioRepository user,
            IFormasDePagamentoRepository formasPagamento,
            IStatusDePagamentoRepository statusPagamento,
            IConfiguration config,
            IOrdemDeCompraRepository repoOrdem,
            MercadoPagoIntegration mercadoPago,
            IProdutoRepository repoProduto,
            IOrdemProdutoRepository repoOrdemProduto
        )
        {
            _repo = repo;
            _repoUser = user;
            _repoFormaPagamento = formasPagamento;
            _repoStatusPagamento = statusPagamento;
            _config = config;
            _repoOrdem = repoOrdem;
            _mercadoPago = mercadoPago;
            _repoProduto = repoProduto;
            _repoOrdemProduto = repoOrdemProduto;
        }

        public IEnumerable<Pagamentos> GetPagamentos()
        {
            return _repo.ListarTodos();
        }

        public IEnumerable<Pagamentos> GetPagamentoByUsuarioId(int usuarioId)
        {
            var pagamentos = _repo.BuscarPorUsuarioId(usuarioId);
            if (pagamentos == null)
                throw new InvalidOperationException("Não foi possível encontrar os pagamentos");
            return pagamentos;
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


            var ordemProduto = _repoOrdemProduto.BuscarPorUsuarioId(pagamento.usuario_id)
                ?? throw new InvalidOperationException("Oredm de produto não encontrado.");


            var itensOrdem = _repoOrdemProduto.BuscarProdutosPorOrdemId(ordem.id);

            if (!itensOrdem.Any())
                throw new InvalidOperationException("Ordem de produto não encontrado.");



            try
            {
                foreach (var itemOrdem in itensOrdem)
                {
                    var produto = _repoProduto.BuscarPorId(itemOrdem.produto_id)
                        ?? throw new InvalidOperationException($"Produto ID {itemOrdem.produto_id} não encontrado.");


                    if (produto.estoque < itemOrdem.quantidade)
                    {

                        throw new InvalidOperationException(
                            $"O produto '{produto.nome}' não possui estoque suficiente ({produto.estoque} restante) para a quantidade solicitada ({itemOrdem.quantidade})."
                        );
                    }


                    int novoEstoque = (int)(produto.estoque - itemOrdem.quantidade);
                    var updateEstoqueDto = new ProdutoUpdateDTO { estoque = novoEstoque };
                    _repoProduto.Atualizar(produto.id, updateEstoqueDto);
                }



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

                    _repo.Atualizar(pagamento.pagamento_uuid, updateDto);
                }

                return pagamento;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("estoque suficiente"))
            {
                

                try
                {
                   
                
                    _repoOrdem.DeletarOrdemEmAberto(ordem.id);

                }
                catch (Exception cleanupEx)
                {

                    Console.Error.WriteLine($"ERRO CRÍTICO ao limpar a ordem {ordem.id} após falha de estoque: {cleanupEx.Message}");

                }


                throw;
            }
        }



        public Pagamentos AtualizarPagamento(string pagamentoUUID, PagamentosUpdateDTO pagamento)
        {
            var existente = _repo.BuscarPorUUID(pagamentoUUID);
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

            return _repo.Atualizar(pagamentoUUID, new PagamentosUpdateDTO
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

                AtualizarStatusPorUsuario(pagamento.pagamento_uuid, novoStatusId, valorPago, novoUuidMp);
                return pagamento;
            }

            

            return null; 
        }


        public void AtualizarStatusPorUsuario(string pagamentoUUID, int novoStatusId, int? valorPago, string novoUuidMp)
        {
            var updateDto = new PagamentosUpdateDTO
            {
                status_pagamento_id = (int)novoStatusId,
                valor_pago = valorPago,
                pagamento_uuid = novoUuidMp 
            };

           
            _repo.Atualizar(pagamentoUUID, updateDto);
        }



        public void AtualizarStatusPagamentoPorReferencia(string transacaoIdReferencia, int novoStatusId, int? valorPago)
        {

            var pagamento = _repo.BuscarPorUUID(transacaoIdReferencia);

           
            if (pagamento == null)
            {
                Console.WriteLine($"[ERRO WEBHOOK] Pagamento com UUID '{transacaoIdReferencia}' não encontrado. Requisição descartada.");
                throw new InvalidOperationException($"Pagamento não encontrado para o UUID: {transacaoIdReferencia}");
            }

           
            var updateDto = new PagamentosUpdateDTO
            {
                status_pagamento_id = novoStatusId,
                valor_pago = valorPago,
               
                pagamento_uuid = transacaoIdReferencia
            };

          
            _repo.Atualizar(pagamento.pagamento_uuid, updateDto);

            if (novoStatusId == 2) 
            {
                try
                {

                    _repoOrdem.DeletarOrdemEmAberto(pagamento.usuario_id);
                    _repo.DeletarCarrinhoUsuarioId(pagamento.usuario_id);
                    Console.WriteLine($"Ordem de compra do usuário {pagamento.usuario_id} deletada com sucesso após pagamento.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"ERRO ao deletar ordem de compra: {ex.Message}");
                }
            }
            if (novoStatusId == 5)
            {
                try
                {
                    _repoOrdem.DeletarOrdemEmAberto(pagamento.usuario_id);
                    Console.WriteLine($"Ordem expirada removida do usuário {pagamento.usuario_id}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"ERRO ao remover ordem expirada: {ex.Message}");
                }
            }
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
