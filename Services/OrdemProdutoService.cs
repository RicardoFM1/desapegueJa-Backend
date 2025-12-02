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
            IProdutoRepository repoProd)
        {
            _repo = repo;
            _repoOrdemCompra = repoOC;
            _repoProduto = repoProd;
        }

        public IEnumerable<OrdemProduto> ListarTodos()
        {
            return _repo.ListarTodos();
        }

        public OrdemProduto BuscarPorUsuarioId(int usuarioId)
        {
            var item = _repo.BuscarPorUsuarioId(usuarioId);
            if (item == null)
                throw new InvalidOperationException("OrdemProduto do usuário não encontrada.");
            return item;
        }

        public OrdemProduto CriarOrdemProduto(int usuarioId, OrdemProduto ordemProduto)
        {
            var existente = _repo.BuscarPorUsuarioId(usuarioId);
            if (existente != null)
                throw new InvalidOperationException("O usuário já possui uma OrdemProduto.");

            var ordemCompra = _repoOrdemCompra.BuscarPorUsuarioId(usuarioId);
            if (ordemCompra == null)
                throw new InvalidOperationException("O usuário não possui uma Ordem de Compra.");

            var produto = _repoProduto.BuscarPorId(ordemProduto.produto_id);
            if (produto == null || produto.status.ToLower() == "inativo")
                throw new InvalidOperationException("Produto não encontrado ou inativo.");

            if (ordemProduto.quantidade <= 0)
                throw new InvalidOperationException("Quantidade deve ser maior que zero.");

            if (produto.estoque < ordemProduto.quantidade)
                throw new InvalidOperationException(
                    $"Estoque insuficiente para '{produto.nome}'. Disponível: {produto.estoque}."
                );

            ordemProduto.ordem_id = ordemCompra.id;
            _repo.Adicionar(ordemProduto);

            return ordemProduto;
        }

        public OrdemProduto AtualizarOrdemProduto(int usuarioId, OrdemProdutoUpdateDTO dto)
        {
            var existente = _repo.BuscarPorUsuarioId(usuarioId);
            if (existente == null)
                throw new InvalidOperationException("O usuário não possui uma OrdemProduto.");

            var produto = _repoProduto.BuscarPorId(dto.produto_id ?? existente.produto_id);
            if (produto == null || produto.status.ToLower() == "inativo")
                throw new InvalidOperationException("Produto não encontrado ou inativo.");

            if ((dto.quantidade ?? existente.quantidade) <= 0)
                throw new InvalidOperationException("Quantidade deve ser maior que zero.");

            return _repo.AtualizarPorUsuarioId(usuarioId, dto);
        }

        
    }
}
