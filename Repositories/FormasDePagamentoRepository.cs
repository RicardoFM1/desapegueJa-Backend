using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;
using System.Data;

namespace BackendDesapegaJa.Repositories
{
    public class FormasDePagamentoRepository : IFormasDePagamentoRepository
    {
        private readonly string _connectionString;

        public FormasDePagamentoRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<FormasDePagamento> ListarTodos(string? status = null)
        {
            var formas = new List<FormasDePagamento>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM formas_de_pagamentos";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }

            using var cmd = new NpgsqlCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                formas.Add(new FormasDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    forma = reader.IsDBNull(reader.GetOrdinal("forma")) ? "" : reader.GetString("forma"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                });
            }

            return formas;
        }

        public FormasDePagamento BuscarPorForma(string forma, string? status = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM formas_de_pagamentos WHERE forma = @forma";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@forma", forma);

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            using var reader = cmd.ExecuteReader();

            FormasDePagamento? found = null;

            while (reader.Read())
            {
                found = new FormasDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    forma = reader.IsDBNull(reader.GetOrdinal("forma")) ? "" : reader.GetString("forma"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
            }

            return found;
        }


        public FormasDePagamento BuscarPorId(int? id, string? status = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM formas_de_pagamentos WHERE id = @id";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            using var reader = cmd.ExecuteReader();

            FormasDePagamento? found = null;

            while (reader.Read())
            {
                found = new FormasDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    forma = reader.IsDBNull(reader.GetOrdinal("forma")) ? "" : reader.GetString("forma"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
            }

            return found;
        }


        public void Adicionar(FormasDePagamento forma)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

          
            string sql = @"
                INSERT INTO formas_de_pagamentos (forma, status)
                VALUES (@forma, @status)
                RETURNING id;
            ";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@forma", forma.forma);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(forma.status) ? "ativo" : forma.status);

            forma.id = Convert.ToInt32(cmd.ExecuteScalar());
        }


        public void Atualizar(int id, FormasDePagamentoUpdateDTO formas, string? statusFiltro = null)
        {
            var existente = BuscarPorId(id, statusFiltro);
            if (existente == null)
            {
                throw new InvalidOperationException("Nenhuma forma de pagamento encontrada");
            }

            string formaFinal = string.IsNullOrWhiteSpace(formas.forma) ? existente.forma : formas.forma;
            string statusFinal = string.IsNullOrWhiteSpace(formas.status) ? existente.status : formas.status;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                UPDATE formas_de_pagamentos 
                SET forma = @forma, status = @status
                WHERE id = @id;
            ";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@forma", formaFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);

            cmd.ExecuteNonQuery();
        }
    }
}
