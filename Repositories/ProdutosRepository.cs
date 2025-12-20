using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;

public class ProdutosRepository : IProdutoRepository
{
    private readonly string _connectionString;
    private readonly IUsuarioRepository _repoUser;
    private readonly ICategoriasRepository _repoCategoria;

    public ProdutosRepository(
        IUsuarioRepository repoUser,
        ICategoriasRepository repoCategoria,
        IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
        _repoUser = repoUser;
        _repoCategoria = repoCategoria;
    }

    public async Task<(IEnumerable<Produto> produtos, int total)> ListarTodosAsync(
        string? status = null, int offset = 0, int limit = 10)
    {
        var produtos = new List<Produto>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string countSql = "SELECT COUNT(*) FROM produtos";
        if (!string.IsNullOrWhiteSpace(status))
            countSql += " WHERE status = @status";

        await using var countCmd = new NpgsqlCommand(countSql, connection);
        if (!string.IsNullOrWhiteSpace(status))
            countCmd.Parameters.AddWithValue("@status", status);

        int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

        string sql = "SELECT * FROM produtos";
        if (!string.IsNullOrWhiteSpace(status))
            sql += " WHERE status = @status";

        sql += " ORDER BY id DESC LIMIT @limit OFFSET @offset";

        await using var cmd = new NpgsqlCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(status))
            cmd.Parameters.AddWithValue("@status", status);

        cmd.Parameters.AddWithValue("@limit", limit);
        cmd.Parameters.AddWithValue("@offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            produtos.Add(Map(reader));
        }

        return (produtos, total);
    }

    public async Task<IEnumerable<Produto>> BuscarPorNomeAsync(string nome, string? status = null)
    {
        var produtos = new List<Produto>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "SELECT * FROM produtos WHERE LOWER(nome) = LOWER(@nome)";
        if (!string.IsNullOrWhiteSpace(status))
            sql += " AND status = @status";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@nome", nome.Trim());
        if (!string.IsNullOrWhiteSpace(status))
            cmd.Parameters.AddWithValue("@status", status);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            produtos.Add(Map(reader));

        return produtos;
    }

    public async Task<IEnumerable<Produto>> BuscarPorUsuarioIdAsync(int? id, string? status = null)
    {
        var produtos = new List<Produto>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "SELECT * FROM produtos WHERE usuario_id = @id";
        if (!string.IsNullOrWhiteSpace(status))
            sql += " AND status = @status";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        if (!string.IsNullOrWhiteSpace(status))
            cmd.Parameters.AddWithValue("@status", status);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            produtos.Add(Map(reader));

        return produtos;
    }

    public async Task<Produto?> BuscarPorIdAsync(int? id, string? status = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "SELECT * FROM produtos WHERE id = @id";
        if (!string.IsNullOrWhiteSpace(status))
            sql += " AND status = @status";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        if (!string.IsNullOrWhiteSpace(status))
            cmd.Parameters.AddWithValue("@status", status);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task AdicionarAsync(Produto produto)
    {
        if (await _repoUser.BuscarPorIdAsync(produto.usuario_id) == null)
            throw new InvalidOperationException("Usuário não encontrado");

        if (_repoCategoria.BuscarPorId(produto.categoria_id) == null)
            throw new InvalidOperationException("Categoria não encontrada");

        if (produto.estoque <= 0)
            throw new InvalidOperationException("Estoque inválido");

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            INSERT INTO produtos (usuario_id, nome, preco, descricao, categoria_id, estoque, status, data_post, imagem)
            VALUES (@usuario_id, @nome, @preco, @descricao, @categoria_id, @estoque, @status, @data_post, @imagem)
            RETURNING id;
        ";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuario_id", produto.usuario_id);
        cmd.Parameters.AddWithValue("@nome", produto.nome);
        cmd.Parameters.AddWithValue("@preco", produto.preco);
        cmd.Parameters.AddWithValue("@descricao", produto.descricao);
        cmd.Parameters.AddWithValue("@categoria_id", produto.categoria_id);
        cmd.Parameters.AddWithValue("@estoque", produto.estoque);
        cmd.Parameters.AddWithValue("@status", produto.status ?? "ativo");
        cmd.Parameters.AddWithValue("@data_post", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@imagem", produto.imagem);

        produto.id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<Produto?> AtualizarAsync(int id, ProdutoUpdateDTO produto, string? status = null)
    {
        var existente = await BuscarPorIdAsync(id, status);
        if (existente == null)
            throw new InvalidOperationException("Produto não encontrado");

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            UPDATE produtos SET
                nome = @nome,
                preco = @preco,
                descricao = @descricao,
                categoria_id = @categoria_id,
                imagem = @imagem,
                estoque = @estoque,
                status = @status
            WHERE id = @id;
        ";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@nome", produto.nome ?? existente.nome);
        cmd.Parameters.AddWithValue("@preco", produto.preco ?? existente.preco);
        cmd.Parameters.AddWithValue("@descricao", produto.descricao ?? existente.descricao);
        cmd.Parameters.AddWithValue("@categoria_id", produto.categoria_id ?? existente.categoria_id);
        cmd.Parameters.AddWithValue("@imagem", produto.imagem ?? existente.imagem);
        cmd.Parameters.AddWithValue("@estoque", produto.estoque ?? existente.estoque);
        cmd.Parameters.AddWithValue("@status", produto.status ?? existente.status);

        await cmd.ExecuteNonQueryAsync();

        return await BuscarPorIdAsync(id);
    }

    private Produto Map(NpgsqlDataReader reader)
    {
        return new Produto
        {
            id = reader.GetInt32(reader.GetOrdinal("id")),
            usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
            nome = reader["nome"] as string ?? "",
            preco = reader.GetInt32(reader.GetOrdinal("preco")),
            descricao = reader["descricao"] as string ?? "",
            data_post = reader["data_post"].ToString() ?? "",
            status = reader["status"] as string ?? "",
            categoria_id = reader.GetInt32(reader.GetOrdinal("categoria_id")),
            estoque = reader.GetInt32(reader.GetOrdinal("estoque")),
            imagem = reader["imagem"] as string ?? ""
        };
    }
}
