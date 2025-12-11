using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;

namespace BackendDesapegaJa.Repositories
{
    public class ProdutosRepository : IProdutoRepository
    {
        private readonly string _connectionString;
        private readonly IUsuarioRepository _repoUser;
        private readonly ICategoriasRepository _repoCategoria;

        public ProdutosRepository(IUsuarioRepository repoUser, ICategoriasRepository repoCategoria, IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _repoUser = repoUser;
            _repoCategoria = repoCategoria;
        }

        public (IEnumerable<Produto> produtos, int total) ListarTodos(string? status = null, int offset = 0, int limit = 10)
        {
            var produtos = new List<Produto>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string countSql = "SELECT COUNT(*) FROM produtos";
            if (!string.IsNullOrWhiteSpace(status))
                countSql += " WHERE status = @status";

            var countCmd = new NpgsqlCommand(countSql, connection);
            if (!string.IsNullOrWhiteSpace(status))
                countCmd.Parameters.AddWithValue("@status", status);

            int total = Convert.ToInt32(countCmd.ExecuteScalar());

            string sql = "SELECT * FROM produtos";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " WHERE status = @status";

            sql += " ORDER BY id DESC LIMIT @limit OFFSET @offset";

            var cmd = new NpgsqlCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@offset", offset);

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                produtos.Add(new Produto
                {
                    nome = reader["nome"] as string ?? "",
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    preco = reader.GetInt32(reader.GetOrdinal("preco")),
                    descricao = reader["descricao"] as string ?? "",
                    data_post = reader["data_post"] as string ?? "",
                    status = reader["status"] as string ?? "",
                    categoria_id = reader.GetInt32(reader.GetOrdinal("categoria_id")),
                    estoque = reader.GetInt32(reader.GetOrdinal("estoque")),
                    imagem = reader["imagem"] as string ?? ""
                });
            }

            return (produtos, total);
        }

        public IEnumerable<Produto?> BuscarPorNome(string nome, string? status = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM produtos WHERE LOWER(nome) = LOWER(@nome)";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@nome", nome.Trim());
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            var produtos = new List<Produto>();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                produtos.Add(new Produto
                {
                    nome = reader["nome"] as string ?? "",
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    preco = reader.GetInt32(reader.GetOrdinal("preco")),
                    descricao = reader["descricao"] as string ?? "",
                    data_post = reader["data_post"] as string ?? "",
                    status = reader["status"] as string ?? "",
                    categoria_id = reader.GetInt32(reader.GetOrdinal("categoria_id")),
                    estoque = reader.GetInt32(reader.GetOrdinal("estoque")),
                    imagem = reader["imagem"] as string ?? ""
                });
            }

            return produtos;
        }

        public IEnumerable<Produto?> BuscarPorUsuarioID(int? id, string? status = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM produtos WHERE usuario_id = @usuario_id";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", id);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            var produtos = new List<Produto>();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                produtos.Add(new Produto
                {
                    nome = reader["nome"] as string ?? "",
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    preco = reader.GetInt32(reader.GetOrdinal("preco")),
                    descricao = reader["descricao"] as string ?? "",
                    data_post = reader["data_post"] as string ?? "",
                    status = reader["status"] as string ?? "",
                    categoria_id = reader.GetInt32(reader.GetOrdinal("categoria_id")),
                    estoque = reader.GetInt32(reader.GetOrdinal("estoque")),
                    imagem = reader["imagem"] as string ?? ""
                });
            }

            return produtos;
        }

        public Produto? BuscarPorId(int? id, string? status = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM produtos WHERE id = @id";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Produto
                {
                    nome = reader["nome"] as string ?? "",
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    preco = reader.GetInt32(reader.GetOrdinal("preco")),
                    descricao = reader["descricao"] as string ?? "",
                    data_post = reader["data_post"] as string ?? "",
                    status = reader["status"] as string ?? "",
                    categoria_id = reader.GetInt32(reader.GetOrdinal("categoria_id")),
                    estoque = reader.GetInt32(reader.GetOrdinal("estoque")),
                    imagem = reader["imagem"] as string ?? ""
                };
            }

            return null;
        }

        public void Adicionar(Produto produto)
        {
            var usuarioExistente = _repoUser.BuscarPorId(produto.usuario_id);
            var categoriaExistente = _repoCategoria.BuscarPorId(produto.categoria_id);

            if (usuarioExistente == null)
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            if (categoriaExistente == null)
                throw new InvalidOperationException("Categoria referenciada não encontrada");
            if (produto.estoque <= 0)
                throw new InvalidOperationException("O estoque deve ser maior que 0");

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                INSERT INTO produtos (usuario_id, nome, preco, descricao, categoria_id, estoque, status, data_post, imagem)
                VALUES (@usuarioId, @nome, @preco, @descricao, @categoria_id, @estoque, @status, @data_post, @imagem)
                RETURNING id;";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@usuarioId", produto.usuario_id);
            cmd.Parameters.AddWithValue("@nome", produto.nome);
            cmd.Parameters.AddWithValue("@preco", produto.preco);
            cmd.Parameters.AddWithValue("@descricao", produto.descricao);
            cmd.Parameters.AddWithValue("@categoria_id", produto.categoria_id);
            cmd.Parameters.AddWithValue("@estoque", produto.estoque);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(produto.status) ? "ativo" : produto.status);
            cmd.Parameters.AddWithValue("@data_post", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"));
            cmd.Parameters.AddWithValue("@imagem", produto.imagem);

            produto.id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public Produto? Atualizar(int id, ProdutoUpdateDTO produto, string? status = null)
        {
            var existente = BuscarPorId(id, status);
            if (existente == null)
                throw new InvalidOperationException("Produto não encontrado");

            var usuarioIdFinal = produto.usuario_id ?? existente.usuario_id;
            var categoriaIdFinal = produto.categoria_id ?? existente.categoria_id;

            if (_repoUser.BuscarPorId(usuarioIdFinal) == null)
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            if (_repoCategoria.BuscarPorId(categoriaIdFinal) == null)
                throw new InvalidOperationException("Categoria referenciada não encontrada");

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                UPDATE produtos
                SET nome = @nome, usuario_id = @usuario_id, preco = @preco, descricao = @descricao,
                    categoria_id = @categoria_id, imagem = @imagem, data_post = @data_post,
                    estoque = @estoque, status = @status
                WHERE id = @id;
            ";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioIdFinal);
            cmd.Parameters.AddWithValue("@nome", produto.nome ?? existente.nome);
            cmd.Parameters.AddWithValue("@preco", produto.preco ?? existente.preco);
            cmd.Parameters.AddWithValue("@descricao", produto.descricao ?? existente.descricao);
            cmd.Parameters.AddWithValue("@categoria_id", categoriaIdFinal);
            cmd.Parameters.AddWithValue("@imagem", produto.imagem ?? existente.imagem);
            cmd.Parameters.AddWithValue("@data_post", produto.data_post ?? existente.data_post);
            cmd.Parameters.AddWithValue("@estoque", produto.estoque ?? existente.estoque);
            cmd.Parameters.AddWithValue("@status", produto.status ?? existente.status);

            cmd.ExecuteNonQuery();

            return BuscarPorId(id);
        }
    }
}
