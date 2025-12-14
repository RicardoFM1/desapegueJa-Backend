using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;

namespace BackendDesapegaJa.Repositories
{
    public class StatusDePagamentoRepository : IStatusDePagamentoRepository
    {
        private readonly NpgsqlConnection _connection;

        public StatusDePagamentoRepository(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public IEnumerable<StatusDePagamento> ListarTodos(string? status = null)
        {
            var statuslist = new List<StatusDePagamento>();

            using var connection = _connection;
            connection.Open();

            string sql = "SELECT * FROM status_de_pagamento";
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }

            var cmd = new NpgsqlCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                statuslist.Add(new StatusDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32(reader.GetOrdinal("id")),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString(reader.GetOrdinal("descricao")),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString(reader.GetOrdinal("status"))
                });
            }

            return statuslist;
        }

        public StatusDePagamento BuscarPorDescricao(string descricao, string? status = null)
        {
            using var connection = _connection;
            connection.Open();

            string sql = "SELECT * FROM status_de_pagamento WHERE descricao = @descricao";
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@descricao", descricao);

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            var reader = cmd.ExecuteReader();
            StatusDePagamento? statusDePagamento = null;

            while (reader.Read())
            {
                statusDePagamento = new StatusDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32(reader.GetOrdinal("id")),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString(reader.GetOrdinal("descricao")),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString(reader.GetOrdinal("status"))
                };
            }

            reader.Close();
            return statusDePagamento;
        }

        public StatusDePagamento BuscarPorId(int? id, string? status = null)
        {
            using var connection = _connection;
            connection.Open();

            string sql = "SELECT * FROM status_de_pagamento WHERE id = @id";
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            var reader = cmd.ExecuteReader();
            StatusDePagamento? statusdepagamento = null;

            while (reader.Read())
            {
                statusdepagamento = new StatusDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32(reader.GetOrdinal("id")),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString(reader.GetOrdinal("descricao")),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString(reader.GetOrdinal("status"))
                };
            }

            reader.Close();
            return statusdepagamento;
        }

        public void Adicionar(StatusDePagamento status)
        {
            var statusExistente = BuscarPorDescricao(status.descricao);
            if (statusExistente != null)
            {
                throw new InvalidOperationException("A descrição do status de pagamento já existe");
            }

            using var connection = _connection;
            connection.Open();

            var cmd = new NpgsqlCommand(
                @"INSERT INTO status_de_pagamento (descricao, status) 
                  VALUES (@descricao, @status) 
                  RETURNING id;", connection);

            cmd.Parameters.AddWithValue("@descricao", status.descricao);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(status.status) ? "ativo" : status.status);

            var novoId = Convert.ToInt32(cmd.ExecuteScalar());
            status.id = novoId;
        }

        public StatusDePagamento Atualizar(int id, StatusDePagamentoUpdateDTO statuspagamento, string? status = null)
        {
            var statusExistente = BuscarPorId(id);
            if (statusExistente == null)
            {
                throw new InvalidOperationException("Status de pagamento não encontrada.");
            }

            var descricaoFinal = string.IsNullOrWhiteSpace(statuspagamento.descricao)
                ? statusExistente.descricao
                : statuspagamento.descricao;

            var statusFinal = string.IsNullOrWhiteSpace(statuspagamento.status)
                ? statusExistente.status
                : statuspagamento.status;

            using var connection = _connection;
            connection.Open();

            var cmd = new NpgsqlCommand(
                @"UPDATE status_de_pagamento 
                  SET descricao = @descricao, status = @status 
                  WHERE id = @id", connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@descricao", descricaoFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);

            cmd.ExecuteNonQuery();

            return new StatusDePagamento
            {
                id = id,
                descricao = descricaoFinal,
                status = statusFinal
            };
        }
    }
}
