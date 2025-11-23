using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Data;

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

            string sql = "SELECT * FROM Pagamentos ORDER BY created_at DESC";
            using var cmd = new MySqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pagamentos.Add(MapReaderToPagamento(reader));
            }

            return pagamentos;
        }

        

        public Pagamentos BuscarUltimoPagamentoPendente(int idStatusPendente)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            
            string sql = @"
        SELECT * FROM Pagamentos 
        WHERE status_pagamento_id = @idStatusPendente 
        ORDER BY created_at DESC
        LIMIT 1";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@idStatusPendente", idStatusPendente);

            using var reader = cmd.ExecuteReader();

            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public IEnumerable<Pagamentos> ListarExpirados(DateTime dataLimite, int idStatusPendente)
        {
            var pagamentos = new List<Pagamentos>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

           
            string sql = @"
        SELECT p.* FROM Pagamentos p
        WHERE p.expiracao IS NOT NULL 
        AND p.expiracao < @dataLimite
        AND p.status_pagamento_id = @idStatusPendente";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@dataLimite", dataLimite);
       
            cmd.Parameters.AddWithValue("@idStatusPendente", idStatusPendente);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pagamentos.Add(MapReaderToPagamento(reader));
            }

            return pagamentos;
        }

        public Pagamentos? BuscarPorId(int? id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Pagamentos WHERE id = @id ORDER BY created_at DESC LIMIT 1";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public Pagamentos? BuscarPorUsuarioId(int usuarioId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Pagamentos WHERE usuario_id = @usuario_id ORDER BY created_at DESC LIMIT 1";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public Pagamentos? BuscarPagamentoEmAberto(int usuarioId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

          
            string sql = "SELECT * FROM Pagamentos WHERE usuario_id = @usuario_id AND status_pagamento_id = 1 ORDER BY created_at DESC LIMIT 1";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            using var reader = cmd.ExecuteReader();
          
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public Pagamentos? BuscarPorTransacaoIdExterno(string transacaoIdExterno)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

           
            string sql = "SELECT * FROM Pagamentos WHERE pagamento_uuid = @transacaoIdExterno";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@transacaoIdExterno", transacaoIdExterno);

            using var reader = cmd.ExecuteReader();
         
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public Pagamentos? BuscarPorUUID(string uuid)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Pagamentos WHERE pagamento_uuid = @uuid";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@uuid", uuid);

            using var reader = cmd.ExecuteReader();
            
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        private Pagamentos MapReaderToPagamento(MySqlDataReader reader)
        {
            return new Pagamentos
            {
                id = reader.GetInt32("id"),
                usuario_id = reader.GetInt32("usuario_id"),
                forma_pagamento_id = reader.GetInt32("forma_pagamento_id"),
                status_pagamento_id = reader.GetInt32("status_pagamento_id"),
                ordem_id = reader.IsDBNull("ordem_id") ? null : (int?)reader.GetInt32("ordem_id"),
                valor = reader.GetInt32("valor"),

                observacao = reader.IsDBNull("observacao") ? null : reader.GetString("observacao"),

                pagamento_uuid = reader.IsDBNull("pagamento_uuid") ? null : reader.GetString("pagamento_uuid"),

                createdAt = reader.IsDBNull("created_at") ? null : reader.GetDateTime("created_at"),
                updatedAt = reader.IsDBNull("updated_at") ? null : reader.GetDateTime("updated_at"),

                pix_qr_code = reader.IsDBNull("pix_qr_code") ? null : reader.GetString("pix_qr_code"),
                pix_copia_codigo = reader.IsDBNull("pix_copia_codigo") ? null : reader.GetString("pix_copia_codigo"),
                boleto_url = reader.IsDBNull("boleto_url") ? null : reader.GetString("boleto_url"),

                expiracao = reader.IsDBNull("expiracao") ? null : reader.GetDateTime("expiracao"),
                valor_pago = reader.IsDBNull("valor_pago") ? null : reader.GetInt32("valor_pago"),
            };
        }

        public async Task AdicionarAsync(Pagamentos pagamento)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var existente = BuscarPagamentoEmAberto(pagamento.usuario_id);
            if (existente != null)
                throw new InvalidOperationException("Este usuário já possui um pagamento gerado.");

            pagamento.createdAt = DateTime.UtcNow;

           
            pagamento.pagamento_uuid = Guid.NewGuid().ToString();

            var cmd = new MySqlCommand(
                @"INSERT INTO Pagamentos 
                    (usuario_id, forma_pagamento_id, status_pagamento_id, ordem_id, valor, observacao,
                     pagamento_uuid, created_at, pix_qr_code, pix_copia_codigo, boleto_url, expiracao, valor_pago)
                  VALUES 
                    (@usuario_id, @forma_pagamento_id, @status_pagamento_id, @ordem_id, @valor, @observacao,
                     @pagamento_uuid, @createdAt, @pix_qr_code, @pix_copia_codigo, @boleto_url, @expiracao, @valor_pago);
                  SELECT LAST_INSERT_ID();", connection);

            cmd.Parameters.AddWithValue("@usuario_id", pagamento.usuario_id);
            cmd.Parameters.AddWithValue("@forma_pagamento_id", pagamento.forma_pagamento_id);
            cmd.Parameters.AddWithValue("@status_pagamento_id", pagamento.status_pagamento_id);
            cmd.Parameters.AddWithValue("@ordem_id", pagamento.ordem_id);
            cmd.Parameters.AddWithValue("@valor", pagamento.valor);
            cmd.Parameters.AddWithValue("@observacao", pagamento.observacao ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@pagamento_uuid", pagamento.pagamento_uuid);

            cmd.Parameters.AddWithValue("@createdAt", pagamento.createdAt);
            cmd.Parameters.AddWithValue("@pix_qr_code", pagamento.pix_qr_code ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pix_copia_codigo", pagamento.pix_copia_codigo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@boleto_url", pagamento.boleto_url ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@expiracao", pagamento.expiracao ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@valor_pago", pagamento.valor_pago ?? (object)DBNull.Value);

            pagamento.id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

     
        public Pagamentos Atualizar(string pagamentoUUID, PagamentosUpdateDTO pagamento)
        {
            var existente = BuscarPorUUID(pagamentoUUID);
            if (existente == null)
                throw new InvalidOperationException("Pagamento não encontrado para este usuário");

            int formaPagamentoIdFinal = pagamento.forma_pagamento_id ?? existente.forma_pagamento_id;
            int statusFinal = pagamento.status_pagamento_id ?? existente.status_pagamento_id;
            int? ordemIdFinal = pagamento.ordem_id ?? existente.ordem_id;
            int valorFinal = pagamento.valor ?? existente.valor;

            string? observacaoFinal = string.IsNullOrWhiteSpace(pagamento.observacao)
                                        ? existente.observacao
                                        : pagamento.observacao;

            string? pixQrFinal = pagamento.pix_qr_code ?? existente.pix_qr_code;
            string? pixCopiaFinal = pagamento.pix_copia_codigo ?? existente.pix_copia_codigo;
            string? boletoFinal = pagamento.boleto_url ?? existente.boleto_url;
            DateTime? expiracaoFinal = pagamento.expiracao ?? existente.expiracao;
            int? pagoFinal = pagamento.valor_pago ?? existente.valor_pago;
            string? pagamentoUuidFinal = pagamento.pagamento_uuid ?? existente.pagamento_uuid; 

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand(
                @"UPDATE Pagamentos SET
                    forma_pagamento_id = @forma,
                    status_pagamento_id = @status,
                    ordem_id = @ordem,
                    valor = @valor,
                    observacao = @obs,
                    updated_at = @updated,
                    pix_qr_code = @qr,
                    pix_copia_codigo = @copia,
                    boleto_url = @boleto,
                    expiracao = @exp,
                    valor_pago = @pago,
                    pagamento_uuid = @uuid
                  WHERE pagamento_uuid = @uuid_existente", connection);

            cmd.Parameters.AddWithValue("@uuid_existente", pagamentoUUID);
            cmd.Parameters.AddWithValue("@forma", formaPagamentoIdFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);
            cmd.Parameters.AddWithValue("@ordem", ordemIdFinal);
            cmd.Parameters.AddWithValue("@valor", valorFinal);
            cmd.Parameters.AddWithValue("@obs", observacaoFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@updated", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@qr", pixQrFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@copia", pixCopiaFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@boleto", boletoFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@exp", expiracaoFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pago", pagoFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@uuid", pagamentoUuidFinal ?? (object)DBNull.Value); 

            cmd.ExecuteNonQuery();

            return BuscarPorUUID(pagamentoUUID)!;
        }

      
        public void DeletarPorUsuarioId(int usuarioId)
        {
            var existente = BuscarPorUsuarioId(usuarioId);
            if (existente == null)
                throw new InvalidOperationException("Nenhum pagamento encontrado para este usuário");

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("DELETE FROM Pagamentos WHERE usuario_id = @usuario_id", connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
            cmd.ExecuteNonQuery();
        }
    }
}
