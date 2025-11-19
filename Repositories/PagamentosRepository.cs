using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;
using System;

namespace BackendDesapegaJa.Repositories
{
    public class PagamentosRepository : IPagamentosRepository
    {
        private readonly string _connectionString;

        public PagamentosRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<Pagamentos> ListarTodos(string? status = null)
        {
            var pagamentos = new List<Pagamentos>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Pagamentos";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " WHERE status = @status";

            using var cmd = new MySqlCommand(sql, connection);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pagamentos.Add(new Pagamentos
                {
                    id = reader.GetInt32("id"),
                    usuario_id = reader.GetInt32("usuario_id"),
                    forma_pagamento_id = reader.GetInt32("forma_pagamento_id"),
                    status_pagamento_id = reader.GetInt32("status_pagamento_id"),
                    ordem_id = reader.GetInt32("ordem_id"),
                    valor = reader.GetInt32("valor"),
                    observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? null : reader.GetString("observacao"),
                    createdAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? null : reader.GetDateTime("created_at"),
                    updatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime("updated_at"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString("status")
                });
            }

            return pagamentos;
        }

        public Pagamentos BuscarPorId(int id, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Pagamentos WHERE id = @id";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Pagamentos
                {
                    id = reader.GetInt32("id"),
                    usuario_id = reader.GetInt32("usuario_id"),
                    forma_pagamento_id = reader.GetInt32("forma_pagamento_id"),
                    status_pagamento_id = reader.GetInt32("status_pagamento_id"),
                    ordem_id = reader.GetInt32("ordem_id"),
                    valor = reader.GetInt32("valor"),
                    observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? null : reader.GetString("observacao"),
                    createdAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? null : reader.GetDateTime("created_at"),
                    updatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime("updated_at"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString("status")
                };
            }
            return null!;
        }

        public void Adicionar(Pagamentos pagamento)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            pagamento.createdAt = DateTime.UtcNow;
            pagamento.status ??= "ativo";

            using var cmd = new MySqlCommand(
                "INSERT INTO Pagamentos (usuario_id, forma_pagamento_id, status_pagamento_id, ordem_id, valor, observacao, created_at, status) " +
                "VALUES (@usuario_id, @forma_pagamento_id, @status_pagamento_id, @ordem_id, @valor, @observacao, @createdAt, @status); " +
                "SELECT LAST_INSERT_ID();", connection);

            cmd.Parameters.AddWithValue("@usuario_id", pagamento.usuario_id);
            cmd.Parameters.AddWithValue("@forma_pagamento_id", pagamento.forma_pagamento_id);
            cmd.Parameters.AddWithValue("@status_pagamento_id", pagamento.status_pagamento_id);
            cmd.Parameters.AddWithValue("@ordem_id", pagamento.ordem_id);
            cmd.Parameters.AddWithValue("@valor", pagamento.valor);
            cmd.Parameters.AddWithValue("@observacao", pagamento.observacao);
            cmd.Parameters.AddWithValue("@createdAt", pagamento.createdAt);
            cmd.Parameters.AddWithValue("@status", pagamento.status);

            pagamento.id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public Pagamentos Atualizar(int id, PagamentosUpdateDTO pagamento, string? statusQuery = null)
        {
            var existente = BuscarPorId(id, statusQuery);
            if (existente == null)
                throw new InvalidOperationException("Pagamento não encontrado");

            int usuarioIdFinal = pagamento.usuario_id ?? existente.usuario_id;
            int formaPagamentoIdFinal = pagamento.forma_pagamento_id ?? existente.forma_pagamento_id;
            int statusPagamentoIdFinal = pagamento.status_pagamento_id ?? existente.status_pagamento_id;
            int ordemIdFinal = pagamento.ordem_id ?? existente.ordem_id;
            int valorFinal = pagamento.valor ?? existente.valor;
            string? observacaoFinal = string.IsNullOrWhiteSpace(pagamento.observacao) ? existente.observacao : pagamento.observacao;
            DateTime createdAtFinal = pagamento.createdAt ?? existente.createdAt ?? DateTime.UtcNow;
            DateTime updatedAtFinal = DateTime.UtcNow;
            string? statusFinal = string.IsNullOrWhiteSpace(pagamento.status) ? existente.status : pagamento.status;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand(
                "UPDATE Pagamentos SET usuario_id = @usuario_id, forma_pagamento_id = @forma_pagamento_id, status_pagamento_id = @status_pagamento_id, " +
                "ordem_id = @ordem_id, valor = @valor, observacao = @observacao, created_at = @createdAt, updated_at = @updatedAt, status = @status WHERE id = @id", connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioIdFinal);
            cmd.Parameters.AddWithValue("@forma_pagamento_id", formaPagamentoIdFinal);
            cmd.Parameters.AddWithValue("@status_pagamento_id", statusPagamentoIdFinal);
            cmd.Parameters.AddWithValue("@ordem_id", ordemIdFinal);
            cmd.Parameters.AddWithValue("@valor", valorFinal);
            cmd.Parameters.AddWithValue("@observacao", observacaoFinal);
            cmd.Parameters.AddWithValue("@createdAt", createdAtFinal);
            cmd.Parameters.AddWithValue("@updatedAt", updatedAtFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);

            cmd.ExecuteNonQuery();

            return new Pagamentos
            {
                id = id,
                usuario_id = usuarioIdFinal,
                forma_pagamento_id = formaPagamentoIdFinal,
                status_pagamento_id = statusPagamentoIdFinal,
                ordem_id = ordemIdFinal,
                valor = valorFinal,
                observacao = observacaoFinal,
                createdAt = createdAtFinal,
                updatedAt = updatedAtFinal,
                status = statusFinal
            };
        }
    }
}
