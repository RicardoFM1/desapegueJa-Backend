using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using System.Net.WebSockets;

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
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * from formas_de_pagamentos";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }

            using var cmd = new MySqlCommand(sql, connection);
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

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM formas_de_pagamentos WHERE forma = @forma";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@forma", forma);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            using var reader = cmd.ExecuteReader();
            FormasDePagamento? formadepagamento = null;
            while (reader.Read())
            {
                formadepagamento = new FormasDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    forma = reader.IsDBNull(reader.GetOrdinal("forma")) ? "" : reader.GetString("forma"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
            }
            reader.Close();
            return formadepagamento;
        }

        public FormasDePagamento BuscarPorId(int? id, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM formas_de_pagamentos WHERE id = @id";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            using var reader = cmd.ExecuteReader();
            FormasDePagamento? formadepagamento = null;
            while (reader.Read())
            {
                formadepagamento = new FormasDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    forma = reader.IsDBNull(reader.GetOrdinal("forma")) ? "" : reader.GetString("forma"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
            }
            reader.Close();
            return formadepagamento;
        }

        public void Adicionar(FormasDePagamento forma)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("INSERT INTO formas_de_pagamentos (forma, status) VALUES (@forma, @status); SELECT LAST_INSERT_ID();", connection);
            cmd.Parameters.AddWithValue("@forma", forma.forma);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(forma.status) ? "ativo" : forma.status);

            var novoId = Convert.ToInt32(cmd.ExecuteScalar());
            forma.id = novoId;

            
        }

        public void Atualizar(int id, FormasDePagamentoUpdateDTO formas, string? status = null)
        {
            var formaExistente = BuscarPorId(id, status);
            if (formaExistente == null)
            {
                throw new InvalidOperationException("Nenhuma forma de pagamento encontrada");
            }
            var formaFinal = string.IsNullOrWhiteSpace(formas.forma) ? formaExistente.forma : formas.forma;
            var statusFinal = string.IsNullOrWhiteSpace(formas.status) ? formaExistente.status : formas.status;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("UPDATE formas_de_pagamentos SET forma = @forma, status = @status WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@forma", formaFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);
            cmd.ExecuteNonQuery();

        }
    }
}
