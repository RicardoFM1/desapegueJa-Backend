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

        public IEnumerable<StatusDePagamento> ListarTodos()
        {
            var status = new List<StatusDePagamento>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();


            var cmd = new MySqlCommand("SELECT * from status_de_pagamento", connection);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                status.Add(new StatusDePagamento
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                });
            }
         
            return status;
        }

        public StatusDePagamento BuscarPorDescricao(string descricao)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("SELECT * FROM status_de_pagamento WHERE descricao = @descricao", connection);
            cmd.Parameters.AddWithValue("@descricao", descricao);
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

        public StatusDePagamento BuscarPorId(int? id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("SELECT * FROM status_de_pagamento WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
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

        public StatusDePagamento Atualizar(int id, StatusDePagamentoUpdateDTO status)
        {
            var statusExistente = BuscarPorId(id);
            if (statusExistente == null)
            {
                throw new InvalidOperationException("Status de pagamento não encontrada.");
            }


            var descricaoFinal = string.IsNullOrWhiteSpace(status.descricao) ? statusExistente.descricao : status.descricao;
            var statusFinal = string.IsNullOrWhiteSpace(status.status) ? statusExistente.status : status.status;

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
