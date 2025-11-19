using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class OrdemDeCompraService
    {
        private readonly IOrdemDeCompraRepository _repo;
        private readonly IUsuarioRepository _repoUser;
        private readonly IStatusOrdemRepository _repoStatusOrdem;
        private readonly IProdutoRepository _repoProduto;
        private readonly IOrdemProdutoRepository _repoOrdemProduto;

        public OrdemDeCompraService(
            IOrdemDeCompraRepository repo,
            IUsuarioRepository user,
            IStatusOrdemRepository status,
            IProdutoRepository produto,
            IOrdemProdutoRepository ordemProdutoRepo
        )
        {
            _repo = repo;
            _repoUser = user;
            _repoStatusOrdem = status;
            _repoProduto = produto;
            _repoOrdemProduto = ordemProdutoRepo;
        }

        public IEnumerable<OrdemDeCompra> GetOrdensDeCompras()
        {
            return _repo.ListarTodos();
        }

        public OrdemDeCompra GetById(int id)
        {
            var ordem = _repo.BuscarPorId(id);
            if (ordem == null)
                throw new InvalidOperationException("Não foi possível encontrar essa ordem de compra");

            return ordem;
        }

        public OrdemDeCompra CriarOrdemDeCompra(OrdemDeCompra ordem, List<OrdemProduto> itens)
        {
            var usuario = _repoUser.BuscarPorId(ordem.usuario_id);
            var status = _repoStatusOrdem.BuscarPorId(ordem.status_ordem_id);

            if (usuario == null || usuario.status.ToLower() == "inativo")
                throw new InvalidOperationException("Usuário não encontrado e/ou inativo");

            if (status == null || status.status.ToLower() == "inativo")
                throw new InvalidOperationException("Status da ordem inválido");

            decimal total = 0;

            foreach (var item in itens)
            {
                var produto = _repoProduto.BuscarPorId(item.produto_id);

                if (produto == null || produto.status.ToLower() == "inativo")
                    throw new InvalidOperationException($"Produto ID {item.produto_id} não encontrado/inativo.");

                
                item.preco_unitario = produto.preco;

                total += produto.preco * item.quantidade;
            }


            ordem.valor_total = (int)total;

            ordem.created_at = DateTime.UtcNow;

            _repo.Adicionar(ordem);

            foreach (var item in itens)
            {
                item.ordem_id = ordem.id;
                _repoOrdemProduto.Adicionar(item);
            }

            return ordem;
        }


        public OrdemDeCompra AtualizarOrdemDeCompra(int id, OrdemDeCompraUpdateDTO ordem)
        {
            var existente = _repo.BuscarPorId(id);
            if (existente == null)
                throw new InvalidOperationException("Ordem de compra não existe");

            int usuarioIdFinal = ordem.usuario_id ?? existente.usuario_id;
            int statusOrdemFinal = ordem.status_ordem_id ?? existente.status_ordem_id;

            var usuario = _repoUser.BuscarPorId(usuarioIdFinal);
            var status = _repoStatusOrdem.BuscarPorId(statusOrdemFinal);

            if (usuario == null || usuario.status.ToLower() == "inativo")
                throw new InvalidOperationException("Usuário não encontrado e/ou inativo");

            if (status == null || status.status.ToLower() == "inativo")
                throw new InvalidOperationException("Status de ordem inválido");

            return _repo.Atualizar(id, ordem);
        }
    }
}
