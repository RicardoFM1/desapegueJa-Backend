using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Org.BouncyCastle.Asn1.Mozilla;

namespace BackendDesapegaJa.Services
{
    public class OrdemDeCompraService
    {
        private readonly IOrdemDeCompraRepository _repo;
        private readonly IUsuarioRepository _repoUser;
        private readonly IStatusOrdemRepository _repoStatusOrdem;
        private readonly IProdutoRepository _repoProduto;

        public OrdemDeCompraService(
            IOrdemDeCompraRepository repo, 
            IUsuarioRepository user, 
            IStatusOrdemRepository status, 
            IProdutoRepository produto
            )
        {
            _repo = repo;
            _repoUser = user;
            _repoStatusOrdem = status;
            _repoProduto = produto;
        }


        public IEnumerable<OrdemDeCompra> GetOrdensDeCompras()
        {
            return _repo.ListarTodos();
        }
        
        public OrdemDeCompra GetById(int id)
        {
            var ordem = _repo.BuscarPorId(id);
            if(ordem == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar essa ordem de compra");
            }
            return ordem;
        }

        public OrdemDeCompra CriarOrdemDeCompra(OrdemDeCompra ordem)
        {
            
            var usuarioExistente = _repoUser.BuscarPorId(ordem.usuario_id);
            var produtoExistente = _repoProduto.BuscarPorId(ordem.produto_id);
            var statusOrdemExistente = _repoStatusOrdem.BuscarPorId(ordem.status_ordem_id);
            if (usuarioExistente == null)
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            }
            if (produtoExistente == null)
            {
                throw new InvalidOperationException("Produto referenciado não encontrado");
            }
            if (statusOrdemExistente == null)
            {
                throw new InvalidOperationException("Status de ordem de compra não encontrado");
            }
            _repo.Adicionar(ordem);
            return ordem;

        }
        public OrdemDeCompra? AtualizarOrdemDeCompra(int id, OrdemDeCompraUpdateDTO ordem)
        {
            
            var ordemDeCompraExistente = _repo.BuscarPorId(id);

            int produtoIdFinal = ordem.produto_id ?? ordemDeCompraExistente.produto_id;
            int usuarioIdFinal = ordem.usuario_id ?? ordemDeCompraExistente.usuario_id;
            int statusOrdemFinal = ordem.status_ordem_id ?? ordemDeCompraExistente.status_ordem_id;

            var usuarioExistente = _repoUser.BuscarPorId(usuarioIdFinal);
            var produtoExistente = _repoProduto.BuscarPorId(produtoIdFinal);
            var statusOrdemExistente = _repoStatusOrdem.BuscarPorId(statusOrdemFinal);


            if (ordemDeCompraExistente == null)
            {
                throw new InvalidOperationException("Ordem de compra não existe");
            }

            if (produtoExistente == null)
            {
                throw new InvalidOperationException("Produto referenciado não encontrado");
            }

            if (usuarioExistente == null)
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            }
            if (statusOrdemExistente == null)
            {
                throw new InvalidOperationException("Status de ordem de compra referenciado não encontrado");
            }
           
           
            var ordemDeCompraAtualizada = _repo.Atualizar(id, ordem);
            return ordemDeCompraAtualizada;
        }
    }
}
