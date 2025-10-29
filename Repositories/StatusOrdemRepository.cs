using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;

namespace BackendDesapegaJa.Repositories
{
    public class StatusOrdemRepository : IStatusOrdemRepository
    {
        public readonly string _connectionString;

        public StatusOrdemRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<StatusOrdem> ListarTodos(string? status = null)
        {

            var statusordem = new List<StatusOrdem>();
            var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM status_ordem";
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
                statusordem.Add(new StatusOrdem
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                });

            }
            return statusordem;
        }
        public StatusOrdem BuscarPorDescricao(string descricao, string? status = null)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM status_ordem WHERE descricao = @descricao";

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
            StatusOrdem? statusordem = null;
            while (reader.Read())
            {
                statusordem = new StatusOrdem
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")

                };
            }
            reader.Close();

            return statusordem;
        }

        public StatusOrdem BuscarPorId(int? id, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM status_ordem WHERE id = @id";

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
            StatusOrdem? statusordem = null;
            while (reader.Read())
            {
                statusordem = new StatusOrdem
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
            }
            reader.Close();

            return statusordem;
        }

        public void Adicionar(StatusOrdem status)
        {

            var statusExistente = BuscarPorDescricao(status.descricao);
            if (statusExistente != null)
            {
                throw new InvalidOperationException("A descrição do status de ordem de pagamento já existe");
            }

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("INSERT INTO status_ordem (descricao, status) VALUES (@descricao, @status); SELECT LAST_INSERT_ID();", connection);
            cmd.Parameters.AddWithValue("@descricao", status.descricao);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(status.status) ? "ativo" : status.status);

            var novoId = Convert.ToInt32(cmd.ExecuteScalar());
            status.id = novoId;


        }

        public StatusOrdem? Atualizar(int id, StatusOrdemUpdateDTO status, string? statusquery = null)
        {
            var statusExistente = BuscarPorId(id, statusquery);
            if (statusExistente == null)
            {
                throw new InvalidOperationException("Status de ordem não encontrado.");
            }


            var descricaoFinal = string.IsNullOrWhiteSpace(status.descricao) ? statusExistente.descricao : status.descricao;
            var statusFinal = string.IsNullOrWhiteSpace(status.status) ? statusExistente.status : status.status;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("UPDATE status_ordem SET descricao = @descricao, status = @status WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@descricao", descricaoFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);

            cmd.ExecuteNonQuery();

            return new StatusOrdem
            {
                id = id,
                descricao = descricaoFinal,
                status = statusFinal
            };
        }
    }
}
