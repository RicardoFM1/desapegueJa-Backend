using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Org.BouncyCastle.Crypto.Prng;

namespace BackendDesapegaJa.Services
{
    public class CarrinhoService
    {
        public readonly ICarrinhoRepository _repo;
        public readonly IUsuarioRepository _repoUser;
        public readonly IProdutoRepository _repoProduto;

        public CarrinhoService(ICarrinhoRepository repo, IUsuarioRepository repoUser, IProdutoRepository repoProduto)
        {
            _repo = repo;
            _repoUser = repoUser;
            _repoProduto = repoProduto;
        }

        public IEnumerable<Carrinho> BuscarCarrinho()
        {
            return _repo.ListarTodos();
        }

        public Carrinho BuscarPorCarrinhoId(int id)
        {
            var carrinho = _repo.BuscarPorId(id);
            if (carrinho == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar esse carrinho");
            }
            return carrinho;
        }

        public Carrinho CriarCarrinho(Carrinho carrinho)
        {
            var usuarioExistente = _repoUser.BuscarPorId(carrinho.usuario_id);
            var produtoExistente = _repoProduto.BuscarPorId(carrinho.produto_id);

            if(!string.Equals(usuarioExistente.status, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Não é possível adicionar ao carrinho um usuário que não está ativo");
            }

            if (!string.Equals(produtoExistente.status, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Não é possível adicionar ao carrinho um produto que não está ativo");
            }

            if (carrinho.quantidade > produtoExistente.estoque)
            {
                throw new InvalidOperationException("Essa quantidade do produto não está disponível");
            }

            if (carrinho.quantidade <= 0)
            {
                throw new InvalidOperationException("Não é possível adicionar um item no carrinho com quantidade 0");
            }

            if(usuarioExistente == null)
            {
                throw new InvalidOperationException("Referência do usuário não encontrado");
            }
            if(produtoExistente == null)
            {
                throw new InvalidOperationException("Referência do produto não encontrado");
            }
            _repo.Adicionar(carrinho);
            return carrinho;
        }
        public CarrinhoUpdateDTO AtualizarCarrinho(int usuarioId, int produtoId, CarrinhoUpdateDTO carrinho)
        {
           
            var CarrinhoExistente = _repo.BuscarPorUsuarioEProduto(usuarioId, produtoId);
           
            if (CarrinhoExistente == null)
            {
                throw new InvalidOperationException("Nenhum item do carrinho com esse usuario e/ou produto encontrado");
            }

            var produtoExistente = _repoProduto.BuscarPorId(produtoId);
            var usuarioExistente = _repoUser.BuscarPorId(usuarioId);

            if (!string.Equals(usuarioExistente.status, "ativo", StringComparison.OrdinalIgnoreCase))
           {
                throw new InvalidOperationException("Não é possível adicionar ao carrinho um usuário que não está ativo");
            }

            if (!string.Equals(produtoExistente.status, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Não é possível atualizar um produto inativo de um carrinho");
            }

            if(carrinho.quantidade > produtoExistente.estoque)
            {
                throw new InvalidOperationException("Essa quantidade do produto não está disponível");
            }

            var quantidadeFinal = carrinho.quantidade.HasValue ? carrinho.quantidade.Value : CarrinhoExistente.quantidade;
            
            _repo.Atualizar(usuarioId, produtoId, carrinho);

            var carrinhoAtualizado = new CarrinhoUpdateDTO
            {
                produto_id = produtoId,
                usuario_id = usuarioId,
                quantidade = quantidadeFinal
            };

            return carrinhoAtualizado;
            
        }
        public void DeletarCarrinho(int usuarioId, int produtoId)
        {
            var CarrinhoExistente = _repo.BuscarPorUsuarioEProduto(usuarioId, produtoId);
            if (CarrinhoExistente == null)
            {
                throw new InvalidOperationException("Nenhum usuário e/ou produto encontrado neste carrinho");
            }
           
            _repo.Deletar(usuarioId, produtoId);  
        }
    }
}
