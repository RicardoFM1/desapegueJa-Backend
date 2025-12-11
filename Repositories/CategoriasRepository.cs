using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;
using System.Data;

namespace BackendDesapegaJa.Repositories
{
    public class CategoriasRepository : ICategoriasRepository
    {
        private readonly string _connectionString;

        public CategoriasRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<Categorias> ListarTodos(string? status = null)
        {
            var categorias = new List<Categorias>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM categorias";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " WHERE status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
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

        public Categorias? BuscarPorNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return null;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand(
                "SELECT * FROM categorias WHERE LOWER(nome) = LOWER(@nome)", connection);

            cmd.Parameters.AddWithValue("@nome", nome.Trim());

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Categorias
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                    Cor = reader.GetString(reader.GetOrdinal("cor")),
                    status = reader.GetString(reader.GetOrdinal("status"))
                };
            }

            return null;
        }

        public Categorias BuscarPorId(int? id, string? status = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM categorias WHERE id = @id";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new Categorias
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                    Cor = reader.GetString(reader.GetOrdinal("cor")),
                    status = reader.GetString(reader.GetOrdinal("status"))
                };
            }

            return null;
        }

        public void Adicionar(Categorias categorias)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                INSERT INTO categorias (nome, status, cor)
                VALUES (@nome, @status, @cor)
                RETURNING id;
            ";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@nome", categorias.Nome);
            cmd.Parameters.AddWithValue("@status",
                string.IsNullOrWhiteSpace(categorias.status) ? "ativo" : categorias.status);
            cmd.Parameters.AddWithValue("@cor", categorias.Cor);

            categorias.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Atualizar(int id, CategoriasUpdateDTO categorias)
        {
            var existente = BuscarPorId(id);
            if (existente == null)
                throw new InvalidOperationException("Nenhuma categoria encontrada");

            var nomeFinal = string.IsNullOrWhiteSpace(categorias.Nome)
                ? existente.Nome : categorias.Nome;

            var corFinal = string.IsNullOrWhiteSpace(categorias.Cor)
                ? existente.Cor : categorias.Cor;

            var statusFinal = string.IsNullOrWhiteSpace(categorias.status)
                ? existente.status : categorias.status;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                UPDATE categorias
                SET nome = @nome, status = @status, cor = @cor
                WHERE id = @id;
            ";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nome", nomeFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);
            cmd.Parameters.AddWithValue("@cor", corFinal);

            cmd.ExecuteNonQuery();
        }
    }
}
