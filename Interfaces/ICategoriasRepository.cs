using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface ICategoriasRepository
    {
        IEnumerable<Categorias> ListarTodos(string? status = null);

        void Adicionar(Categorias categoria);

        Categorias BuscarPorNome(string nome);

        Categorias BuscarPorId(int? id, string? status = null);

        void Atualizar(int id, CategoriasUpdateDTO categorias);
    }
}
