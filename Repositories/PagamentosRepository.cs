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
                pagamentos.Add(MapReaderToPagamento(reader));
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
                return MapReaderToPagamento(reader);

            return null!;
        }

        private Pagamentos MapReaderToPagamento(MySqlDataReader reader)
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
                status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString("status"),
                pix_qr_code = reader.IsDBNull(reader.GetOrdinal("pix_qr_code")) ? null : reader.GetString("pix_qr_code"),
                pix_copia_codigo = reader.IsDBNull(reader.GetOrdinal("pix_copia_codigo")) ? null : reader.GetString("pix_copia_codigo"),
                boleto_url = reader.IsDBNull(reader.GetOrdinal("boleto_url")) ? null : reader.GetString("boleto_url"),
                expiracao = reader.IsDBNull(reader.GetOrdinal("expiracao")) ? null : reader.GetDateTime("expiracao"),
                valor_pago = reader.IsDBNull(reader.GetOrdinal("valor_pago")) ? null : reader.GetInt32("valor_pago")
            };
        }

        public void Adicionar(Pagamentos pagamento)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            pagamento.createdAt = DateTime.UtcNow;
            pagamento.status ??= "ativo";

            using var cmd = new MySqlCommand(
                @"INSERT INTO Pagamentos 
                    (usuario_id, forma_pagamento_id, status_pagamento_id, ordem_id, valor, observacao, created_at, status,
                     pix_qr_code, pix_copia_codigo, boleto_url, expiracao, valor_pago) 
                  VALUES 
                    (@usuario_id, @forma_pagamento_id, @status_pagamento_id, @ordem_id, @valor, @observacao, @createdAt, @status,
                     @pix_qr_code, @pix_copia_codigo, @boleto_url, @expiracao, @valor_pago);
                  SELECT LAST_INSERT_ID();", connection);

            cmd.Parameters.AddWithValue("@usuario_id", pagamento.usuario_id);
            cmd.Parameters.AddWithValue("@forma_pagamento_id", pagamento.forma_pagamento_id);
            cmd.Parameters.AddWithValue("@status_pagamento_id", pagamento.status_pagamento_id);
            cmd.Parameters.AddWithValue("@ordem_id", pagamento.ordem_id);
            cmd.Parameters.AddWithValue("@valor", pagamento.valor);
            cmd.Parameters.AddWithValue("@observacao", pagamento.observacao ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@createdAt", pagamento.createdAt);
            cmd.Parameters.AddWithValue("@status", pagamento.status);
            cmd.Parameters.AddWithValue("@pix_qr_code", pagamento.pix_qr_code ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pix_copia_codigo", pagamento.pix_copia_codigo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@boleto_url", pagamento.boleto_url ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@expiracao", pagamento.expiracao ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@valor_pago", pagamento.valor_pago ?? (object)DBNull.Value);

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

          
            string? pixQrFinal = pagamento.pix_qr_code ?? existente.pix_qr_code;
            string? pixCopiaFinal = pagamento.pix_copia_codigo ?? existente.pix_copia_codigo;
            string? boletoUrlFinal = pagamento.boleto_url ?? existente.boleto_url;
            DateTime? expiracaoFinal = pagamento.expiracao ?? existente.expiracao;
            int? valorPagoFinal = pagamento.valor_pago ?? existente.valor_pago;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand(
                @"UPDATE Pagamentos SET 
                    usuario_id = @usuario_id, 
                    forma_pagamento_id = @forma_pagamento_id, 
                    status_pagamento_id = @status_pagamento_id,
                    ordem_id = @ordem_id,
                    valor = @valor,
                    observacao = @observacao,
                    created_at = @createdAt,
                    updated_at = @updatedAt,
                    status = @status,
                    pix_qr_code = @pix_qr_code,
                    pix_copia_codigo = @pix_copia_codigo,
                    boleto_url = @boleto_url,
                    expiracao = @expiracao,
                    valor_pago = @valor_pago
                  WHERE id = @id", connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioIdFinal);
            cmd.Parameters.AddWithValue("@forma_pagamento_id", formaPagamentoIdFinal);
            cmd.Parameters.AddWithValue("@status_pagamento_id", statusPagamentoIdFinal);
            cmd.Parameters.AddWithValue("@ordem_id", ordemIdFinal);
            cmd.Parameters.AddWithValue("@valor", valorFinal);
            cmd.Parameters.AddWithValue("@observacao", observacaoFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@createdAt", createdAtFinal);
            cmd.Parameters.AddWithValue("@updatedAt", updatedAtFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);
            cmd.Parameters.AddWithValue("@pix_qr_code", pixQrFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pix_copia_codigo", pixCopiaFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@boleto_url", boletoUrlFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@expiracao", expiracaoFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@valor_pago", valorPagoFinal ?? (object)DBNull.Value);

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
                status = statusFinal,
                pix_qr_code = pixQrFinal,
                pix_copia_codigo = pixCopiaFinal,
                boleto_url = boletoUrlFinal,
                expiracao = expiracaoFinal,
                valor_pago = valorPagoFinal
            };
        }
    }
}
