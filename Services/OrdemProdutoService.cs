using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class OrdemProdutoService
    {
        private readonly IOrdemProdutoRepository _repo;
        private readonly IOrdemDeCompraRepository _repoOrdemCompra;
        private readonly IProdutoRepository _repoProduto;

        public OrdemProdutoService(
            IOrdemProdutoRepository repo,
            IOrdemDeCompraRepository repoOC,
            IProdutoRepository repoProd
        )
        {
            _repo = repo;
            _repoOrdemCompra = repoOC;
            _repoProduto = repoProd;
        }

        public IEnumerable<OrdemProduto> GetOrdemProdutos()
        {
            return _repo.ListarTodos();
        }

        public OrdemProduto GetById(int id)
        {
            var ordemProduto = _repo.BuscarPorId(id);

            if (ordemProduto == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar o item da ordem");
            }

            return ordemProduto;
        }

        public OrdemProduto CriarOrdemProduto(OrdemProduto ordemProduto)
        {
            var ordemExistente = _repoOrdemCompra.BuscarPorId(ordemProduto.ordem_id);
            var produtoExistente = _repoProduto.BuscarPorId(ordemProduto.produto_id);

            if (ordemExistente == null)
            {
                throw new InvalidOperationException("A ordem de compra referenciada não existe");
            }

            if (produtoExistente == null || produtoExistente.status.ToLower() == "inativo")
            {
                throw new InvalidOperationException("Produto referenciado não encontrado e/ou inativo");
            }

            if (ordemProduto.quantidade <= 0)
            {
                throw new InvalidOperationException("A quantidade deve ser maior que zero");
            }

            _repo.Adicionar(ordemProduto);

            return ordemProduto;
        }

        public OrdemProduto AtualizarOrdemProduto(int id, OrdemProdutoUpdateDTO dto)
        {
            var ordemProdutoExistente = _repo.BuscarPorId(id);

            if (ordemProdutoExistente == null)
            {
                throw new InvalidOperationException("O item da ordem não existe");
            }

            int ordemFinal = dto.ordem_id ?? ordemProdutoExistente.ordem_id;
            int produtoFinal = dto.produto_id ?? ordemProdutoExistente.produto_id;
            int quantidadeFinal = dto.quantidade ?? ordemProdutoExistente.quantidade;

            var ordemExistente = _repoOrdemCompra.BuscarPorId(ordemFinal);
            var produtoExistente = _repoProduto.BuscarPorId(produtoFinal);

            if (ordemExistente == null)
            {
                throw new InvalidOperationException("A ordem de compra referenciada não existe");
            }

            if (produtoExistente == null || produtoExistente.status.ToLower() == "inativo")
            {
                throw new InvalidOperationException("Produto referenciado não encontrado e/ou inativo");
            }

            if (quantidadeFinal <= 0)
            {
                throw new InvalidOperationException("A quantidade deve ser maior que zero");
            }

            var atualizado = _repo.Atualizar(id, dto);
            return atualizado;
        }
    }
}
