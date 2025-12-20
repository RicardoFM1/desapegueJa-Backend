using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

public class CategoriasService
{
    private readonly ICategoriasRepository _repo;

    public CategoriasService(ICategoriasRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Categorias>> ObterCategoriasAsync(string? status = null)
        => await _repo.ListarTodosAsync(status);

    public async Task<Categorias> BuscarCategoriaPorIdAsync(int id, string? status)
    {
        var categoria = await _repo.BuscarPorIdAsync(id, status);
        return categoria ?? throw new InvalidOperationException("Categoria não encontrada");
    }

    public async Task<Categorias> CriarCategoriaAsync(Categorias categorias)
    {
        var existente = await _repo.BuscarPorNomeAsync(categorias.Nome);
        if (existente != null && existente.status == "ativo")
            throw new InvalidOperationException("O nome da categoria já existe");

        await _repo.AdicionarAsync(categorias);
        return categorias;
    }

    public async Task<Categorias> AtualizarCategoriaAsync(int id, CategoriasUpdateDTO categorias)
    {
        await _repo.AtualizarAsync(id, categorias);
        return await BuscarCategoriaPorIdAsync(id, null);
    }
}
