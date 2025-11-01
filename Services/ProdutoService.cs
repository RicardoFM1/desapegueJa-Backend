using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class ProdutoService
    {
        private readonly IProdutoRepository _repo;
        private readonly IUsuarioRepository _repoUser;
        private readonly ICategoriasRepository _repoCategoria;

        public ProdutoService(IProdutoRepository repo, IUsuarioRepository repoUser, ICategoriasRepository repoCategoria)
        {
            _repo = repo;
            _repoUser = repoUser;
            _repoCategoria = repoCategoria;
        }
        public IEnumerable<Produto> ObterProdutos(string? status = null)
        {
            return _repo.ListarTodos(status);
        }

        public Produto GetProdutoById(int id, string? status = null)
        {
           var produto =_repo.BuscarPorId(id, status);
            if(produto == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar o produto");
            }
            return produto;
        }
        public Produto CriarProduto(Produto produto)
        {
            
            var usuarioExistente = _repoUser.BuscarPorId(produto.usuario_id);
            var categoriaExistente = _repoCategoria.BuscarPorId(produto.categoria_id);
            if (usuarioExistente == null || usuarioExistente.status.ToLower() == "inativo")
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado e/ou inativa");
            }
            if (categoriaExistente == null || usuarioExistente.status.ToLower() == "inativo")
            {
                throw new InvalidOperationException("Categoria referenciada não encontrada e/ou inativa");
            }
            if (produto.estoque <= 0)
            {
                throw new InvalidOperationException("O estoque deve ser maior que 0");
            }
            _repo.Adicionar(produto);
            return produto;

        }
        public Produto? AtualizarProduto(int id, ProdutoUpdateDTO produto, string? status = null)
        {
            var produtoExistente = _repo.BuscarPorId(id, status);
            
            var usuarioIdFinal = produto.usuario_id ?? produtoExistente.usuario_id;
            var categoriaIdFinal = produto.categoria_id ?? produtoExistente.categoria_id;

            var usuarioExistente = _repoUser.BuscarPorId(usuarioIdFinal);
            var categoriaExistente = _repoCategoria.BuscarPorId(categoriaIdFinal);
            
            if (produtoExistente == null)
            {
                throw new InvalidOperationException("Nenhum produto encontrado");
            }
            if (usuarioExistente == null || usuarioExistente.status.ToLower() == "inativo")
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado e/ou inativa");
            }
            if (categoriaExistente == null || usuarioExistente.status.ToLower() == "inativo")
            {
                throw new InvalidOperationException("Categoria referenciada não encontrada e/ou inativa");
            }
            if (produto.estoque <= 0)
            {
                throw new InvalidOperationException("O estoque deve ser maior que 0");
            }
           
            var produtoAtualizado = _repo.Atualizar(id, produto, status);
            return produtoAtualizado;
        }
    }
}
