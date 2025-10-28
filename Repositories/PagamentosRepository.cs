using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;

namespace BackendDesapegaJa.Repositories
{
    public class PagamentosRepository : IPagamentosRepository
    {
        private readonly string _connectionString;

        public PagamentosRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<Pagamentos> ListarTodos()
        {
            var pagamentos = new List<Pagamentos>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT * from Pagamentos", connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                pagamentos.Add(new Pagamentos
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    formas_de_pagamento_id = reader.IsDBNull(reader.GetOrdinal("formas_de_pagamento_id")) ? 0 : reader.GetInt32("formas_de_pagamento_id"),
                    status_de_pagamento_id = reader.IsDBNull(reader.GetOrdinal("status_de_pagamento_id")) ? 0 : reader.GetInt32("status_de_pagamento_id"),
                    observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? "" : reader.GetString("observacao"),
                    createdAt = reader.IsDBNull(reader.GetOrdinal("createdAt")) ? "" : reader.GetString("createdAt"),
                    updatedAt = reader.IsDBNull(reader.GetOrdinal("updatedAt")) ? "" : reader.GetString("updatedAT"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                }); 
            }

            return pagamentos;
        }

        public Pagamentos BuscarPorId(int? id)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT * FROM Pagamentos WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            Pagamentos? pagamentos = null;
            while (reader.Read())
            {
                pagamentos = new Pagamentos
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    formas_de_pagamento_id = reader.IsDBNull(reader.GetOrdinal("formas_de_pagamento_id")) ? 0 : reader.GetInt32("formas_de_pagamento_id"),
                    status_de_pagamento_id = reader.IsDBNull(reader.GetOrdinal("status_de_pagamento_id")) ? 0 : reader.GetInt32("status_de_pagamento_id"),
                    observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? "" : reader.GetString("observacao"),
                    createdAt = reader.IsDBNull(reader.GetOrdinal("createdAt")) ? "" : reader.GetString("createdAt"),
                    updatedAt = reader.IsDBNull(reader.GetOrdinal("updatedAt")) ? "" : reader.GetString("updatedAT"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
            }
            reader.Close();
            return pagamentos;
        }

       
        public Pagamentos BuscarPorUsuarioId(int? id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT * FROM Pagamentos WHERE usuario_id = @usuario_id", connection);
            cmd.Parameters.AddWithValue("@usuario_id", id);
            using var reader = cmd.ExecuteReader();
            Pagamentos? pagamentos = null;
            while (reader.Read())
            {
                pagamentos = new Pagamentos
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    formas_de_pagamento_id = reader.IsDBNull(reader.GetOrdinal("formas_de_pagamento_id")) ? 0 : reader.GetInt32("formas_de_pagamento_id"),
                    status_de_pagamento_id = reader.IsDBNull(reader.GetOrdinal("status_de_pagamento_id")) ? 0 : reader.GetInt32("status_de_pagamento_id"),
                    observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? "" : reader.GetString("observacao"),
                    createdAt = reader.IsDBNull(reader.GetOrdinal("createdAt")) ? "" : reader.GetString("createdAt"),
                    updatedAt = reader.IsDBNull(reader.GetOrdinal("updatedAt")) ? "" : reader.GetString("updatedAT"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
            }
            reader.Close();
            return pagamentos;
        }
        

        public void Adicionar(Pagamentos pagamento)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var data = DateTime.UtcNow;
            pagamento.createdAt = data.ToString("dd/MM/yyyy HH:mm:ss");

            using var cmd = new MySqlCommand("INSERT INTO Pagamentos (usuario_id, formas_de_pagamento_id, status_de_pagamento_id," +
                "observacao, createdAt, updatedAt, status) " +
                "VALUES (@usuario_id, @formas_de_pagamento_id, @status_de_pagamento_id, @observacao, @createdAt, @updatedAt, @status); SELECT LAST_INSERT_ID();", connection);
            cmd.Parameters.AddWithValue("@usuario_id", pagamento.usuario_id);
            cmd.Parameters.AddWithValue("@formas_de_pagamento_id", pagamento.formas_de_pagamento_id);
            cmd.Parameters.AddWithValue("@status_de_pagamento_id", pagamento.status_de_pagamento_id);
            cmd.Parameters.AddWithValue("@observacao", pagamento.observacao);
            cmd.Parameters.AddWithValue("@createdAt", pagamento.createdAt);
            cmd.Parameters.AddWithValue("@updatedAt", pagamento.updatedAt);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(pagamento.status) ? "ativo" : pagamento.status);

            var novoId = Convert.ToInt32(cmd.ExecuteScalar());
            pagamento.id = novoId;


        }

        public Pagamentos Atualizar(int id, PagamentosUpdateDTO pagamento)
        {
            var pagamentoExistente = BuscarPorId(id);
            if (pagamentoExistente == null)
            {
                throw new InvalidOperationException("Nenhum pagamento encontrado");
            }
            
            int usuarioIdFinal = pagamento.usuario_id.HasValue ? pagamento.usuario_id.Value : pagamentoExistente.usuario_id;
            int formasPagamentoIdFinal = pagamento.formas_de_pagamento_id.HasValue ? pagamento.formas_de_pagamento_id.Value : pagamentoExistente.formas_de_pagamento_id;
            int statusPagamentoIdFinal = pagamento.status_de_pagamento_id.HasValue ? pagamento.status_de_pagamento_id.Value : pagamentoExistente.status_de_pagamento_id;
            var observacao = string.IsNullOrWhiteSpace(pagamento.observacao) ? pagamentoExistente.observacao : pagamento.observacao;
            var createdAt = string.IsNullOrWhiteSpace(pagamento.createdAt) ? pagamentoExistente.createdAt : pagamento.createdAt;
            var updatedAt = string.IsNullOrWhiteSpace(pagamento.updatedAt) ? pagamentoExistente.updatedAt : pagamento.updatedAt;
            var status = string.IsNullOrWhiteSpace(pagamento.status) ? pagamentoExistente.status : pagamento.status;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("UPDATE Pagamentos SET " +
                "usuario_id = @usuario_id, formas_de_pagamento_id = @formas_de_pagamento_id, status_de_pagamento_id = @status_de_pagamento_id," +
                " observacao = @observacao, createdAt = @createdAt, updatedAt = @updatedAt, status = @status WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioIdFinal);
            cmd.Parameters.AddWithValue("@formas_de_pagamento_id", formasPagamentoIdFinal);
            cmd.Parameters.AddWithValue("@status_de_pagamento_id", statusPagamentoIdFinal);
            cmd.Parameters.AddWithValue("@observacao", observacao);
            cmd.Parameters.AddWithValue("@createdAt", createdAt);
            cmd.Parameters.AddWithValue("@updatedAt", updatedAt);
            cmd.Parameters.AddWithValue("@status", status);

            cmd.ExecuteNonQuery();
            return new Pagamentos
            {
                id = id,
                usuario_id = usuarioIdFinal,
                formas_de_pagamento_id = formasPagamentoIdFinal,
                status_de_pagamento_id = statusPagamentoIdFinal,
                observacao = observacao,
                createdAt = createdAt,
                updatedAt = updatedAt,
                status = status,

            };
        }
    }
}
