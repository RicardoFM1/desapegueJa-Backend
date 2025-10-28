using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
    public class CategoriasService 
    {
        public readonly ICategoriasRepository _repo;

        public CategoriasService(ICategoriasRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<Categorias> ObterCategorias(string? status = null)
        {
            return _repo.ListarTodos(status);
        }

        public Categorias BuscarCategoriaPorId(int id, string? status)
        {
            var categoria = _repo.BuscarPorId(id, status);
            if(categoria == null)
            {
                throw new InvalidOperationException("Não foi possível encontrar essa categoria");
            }
            return categoria;
        }

        public Categorias CriarCategoria(Categorias categorias)
        {
            var nomeExistente = _repo.BuscarPorNome(categorias.Nome);
            if (nomeExistente != null && nomeExistente.status.ToLower() == "ativo")
            {
                throw new InvalidOperationException("O nome da categoria já existe");
            }
            
            _repo.Adicionar(categorias);
            return categorias;
        }
        public Categorias AtualizarCategoria(int id, CategoriasUpdateDTO categorias)
        {
            var nomeExistente = _repo.BuscarPorNome(categorias.Nome);
            if (nomeExistente != null && nomeExistente.Id != id && nomeExistente.status.ToLower() == "ativo")
            {
                throw new InvalidOperationException("O nome da categoria já existe");
            }
            var categoriaExistente = _repo.BuscarPorId(id);
            if(categoriaExistente == null)
            {
                throw new InvalidOperationException("Nenhuma categoria encontrada");
            }

            categoriaExistente.Id = id;
            _repo.Atualizar(id, categorias);
            return categoriaExistente;
        }
    }
}
