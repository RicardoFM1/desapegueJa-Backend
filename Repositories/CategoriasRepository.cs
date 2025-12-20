using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;

namespace BackendDesapegaJa.Repositories
{
    public class CategoriasRepository : ICategoriasRepository
    {
        private readonly string _connectionString;

        public CategoriasRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Categorias>> ListarTodosAsync(string? status = null)
        {
            var categorias = new List<Categorias>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM categorias";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " WHERE status = @status";

            await using var cmd = new NpgsqlCommand(sql, connection);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categorias.Add(new Categorias
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                    Cor = reader.GetString(reader.GetOrdinal("cor")),
                    status = reader.GetString(reader.GetOrdinal("status"))
                });
            }

            return categorias;
        }

        public async Task<Categorias?> BuscarPorNomeAsync(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return null;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM categorias WHERE LOWER(nome) = LOWER(@nome)", connection);

            cmd.Parameters.AddWithValue("@nome", nome.Trim());

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync()
                ? new Categorias
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                    Cor = reader.GetString(reader.GetOrdinal("cor")),
                    status = reader.GetString(reader.GetOrdinal("status"))
                }
                : null;
        }

        public async Task<Categorias?> BuscarPorIdAsync(int id, string? status = null)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM categorias WHERE id = @id";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync()
                ? new Categorias
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                    Cor = reader.GetString(reader.GetOrdinal("cor")),
                    status = reader.GetString(reader.GetOrdinal("status"))
                }
                : null;
        }

        public async Task AdicionarAsync(Categorias categorias)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO categorias (nome, status, cor)
                VALUES (@nome, @status, @cor)
                RETURNING id;
            ";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@nome", categorias.Nome);
            cmd.Parameters.AddWithValue("@status",
                string.IsNullOrWhiteSpace(categorias.status) ? "ativo" : categorias.status);
            cmd.Parameters.AddWithValue("@cor", categorias.Cor);

            categorias.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task AtualizarAsync(int id, CategoriasUpdateDTO categorias)
        {
            var existente = await BuscarPorIdAsync(id);
            if (existente == null)
                throw new InvalidOperationException("Nenhuma categoria encontrada");

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                UPDATE categorias
                SET nome = @nome, status = @status, cor = @cor
                WHERE id = @id;
            ";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nome", categorias.Nome ?? existente.Nome);
            cmd.Parameters.AddWithValue("@status", categorias.status ?? existente.status);
            cmd.Parameters.AddWithValue("@cor", categorias.Cor ?? existente.Cor);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
