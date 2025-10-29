using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace BackendDesapegaJa.Repositories
{
    public class StatusDePagamentoRepository : IStatusDePagamentoRepository
    {
        private readonly string _connectionString;

        public StatusDePagamentoRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<StatusDePagamento> ListarTodos(string? status = null)
        {
            var statuslist = new List<StatusDePagamento>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * from status_de_pagamento";
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }
            var cmd = new MySqlCommand(sql, connection);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                statuslist.Add(new StatusDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                });
            }
         
            return statuslist;
        }

        public StatusDePagamento BuscarPorDescricao(string descricao, string? status = null)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM status_de_pagamento WHERE descricao = @descricao";
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            var cmd = new MySqlCommand(sql, connection);
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
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")

                };
            }
            reader.Close();
       
            return statusDePagamento;
        }

        public StatusDePagamento BuscarPorId(int? id, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM status_de_pagamento WHERE id = @id";
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }
            var cmd = new MySqlCommand(sql, connection);
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
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
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



            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("INSERT INTO status_de_pagamento (descricao, status) VALUES (@descricao, @status); SELECT LAST_INSERT_ID();", connection);
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


            var descricaoFinal = string.IsNullOrWhiteSpace(statuspagamento.descricao) ? statusExistente.descricao : statuspagamento.descricao;
            var statusFinal = string.IsNullOrWhiteSpace(statuspagamento.status) ? statusExistente.status : statuspagamento.status;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("UPDATE status_de_pagamento SET descricao = @descricao, status = @status WHERE id = @id", connection);
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
