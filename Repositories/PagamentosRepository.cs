using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;
using System;
using System.Data;

namespace BackendDesapegaJa.Repositories
{
    public class PagamentosRepository : IPagamentosRepository
    {
        private readonly string _connectionString;
        private readonly ICarrinhoRepository _repoCarrinho;

        public PagamentosRepository(IConfiguration config, ICarrinhoRepository repoCarrinho)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _repoCarrinho = repoCarrinho;
        }

        public IEnumerable<Pagamentos> ListarTodos()
        {
            var pagamentos = new List<Pagamentos>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM pagamentos ORDER BY created_at DESC";
            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pagamentos.Add(MapReaderToPagamento(reader));
            }

            return pagamentos;
        }

        public Pagamentos BuscarUltimoPagamentoPendente(int idStatusPendente)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT * FROM pagamentos
                WHERE status_pagamento_id = @status
                ORDER BY created_at DESC
                LIMIT 1";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@status", idStatusPendente);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public IEnumerable<Pagamentos> ListarExpirados(DateTime dataLimite, int idStatusPendente)
        {
            var pagamentos = new List<Pagamentos>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT * FROM pagamentos
                WHERE expiracao IS NOT NULL
                AND expiracao < @limite
                AND status_pagamento_id = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@limite", dataLimite);
            cmd.Parameters.AddWithValue("@status", idStatusPendente);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pagamentos.Add(MapReaderToPagamento(reader));
            }

            return pagamentos;
        }

        public Pagamentos? BuscarPorId(int? id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM pagamentos WHERE id = @id LIMIT 1";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public IEnumerable<Pagamentos?> BuscarPorUsuarioId(int usuarioId)
        {
            var pagamentos = new List<Pagamentos>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM pagamentos WHERE usuario_id = @uid ORDER BY created_at DESC";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@uid", usuarioId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pagamentos.Add(MapReaderToPagamento(reader));
            }
            return pagamentos;
        }

        public Pagamentos? BuscarPagamentoEmAberto(int usuarioId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT * FROM pagamentos
                WHERE usuario_id = @uid
                AND status_pagamento_id = 1
                ORDER BY created_at DESC
                LIMIT 1";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@uid", usuarioId);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public Pagamentos? BuscarPorTransacaoIdExterno(string transacaoIdExterno)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM pagamentos WHERE pagamento_uuid = @uuid";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@uuid", transacaoIdExterno);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        public Pagamentos? BuscarPorUUID(string uuid)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM pagamentos WHERE pagamento_uuid = @uuid LIMIT 1";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@uuid", uuid);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReaderToPagamento(reader) : null;
        }

        private Pagamentos MapReaderToPagamento(NpgsqlDataReader reader)
        {
            return new Pagamentos
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                forma_pagamento_id = reader.GetInt32(reader.GetOrdinal("forma_pagamento_id")),
                status_pagamento_id = reader.GetInt32(reader.GetOrdinal("status_pagamento_id")),
                ordem_id = reader.IsDBNull(reader.GetOrdinal("ordem_id")) ? null : reader.GetInt32(reader.GetOrdinal("ordem_id")),
                valor = reader.GetInt32(reader.GetOrdinal("valor")),
                observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? null : reader.GetString(reader.GetOrdinal("observacao")),
                pagamento_uuid = reader.IsDBNull(reader.GetOrdinal("pagamento_uuid")) ? null : reader.GetString(reader.GetOrdinal("pagamento_uuid")),
                createdAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? null : reader.GetDateTime(reader.GetOrdinal("created_at")),
                updatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime(reader.GetOrdinal("updated_at")),
                pix_qr_code = reader.IsDBNull(reader.GetOrdinal("pix_qr_code")) ? null : reader.GetString(reader.GetOrdinal("pix_qr_code")),
                pix_copia_codigo = reader.IsDBNull(reader.GetOrdinal("pix_copia_codigo")) ? null : reader.GetString(reader.GetOrdinal("pix_copia_codigo")),
                boleto_url = reader.IsDBNull(reader.GetOrdinal("boleto_url")) ? null : reader.GetString(reader.GetOrdinal("boleto_url")),
                expiracao = reader.IsDBNull(reader.GetOrdinal("expiracao")) ? null : reader.GetDateTime(reader.GetOrdinal("expiracao")),
                valor_pago = reader.IsDBNull(reader.GetOrdinal("valor_pago")) ? null : reader.GetInt32(reader.GetOrdinal("valor_pago")),
            };
        }

        public async Task AdicionarAsync(Pagamentos pagamento)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var existente = BuscarPagamentoEmAberto(pagamento.usuario_id);
            if (existente != null)
                throw new InvalidOperationException("Este usuário já possui um pagamento gerado.");

            pagamento.createdAt = DateTime.UtcNow;
            pagamento.pagamento_uuid = Guid.NewGuid().ToString();

            var sql = @"
                INSERT INTO pagamentos
                (usuario_id, forma_pagamento_id, status_pagamento_id, ordem_id, valor, observacao,
                 pagamento_uuid, created_at, pix_qr_code, pix_copia_codigo, boleto_url, expiracao, valor_pago)
                VALUES
                (@usuario_id, @forma_pagamento_id, @status_pagamento_id, @ordem_id, @valor, @observacao,
                 @pagamento_uuid, @createdAt, @pix_qr, @pix_copia, @boleto, @expiracao, @valor_pago)
                RETURNING id;";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@usuario_id", pagamento.usuario_id);
            cmd.Parameters.AddWithValue("@forma_pagamento_id", pagamento.forma_pagamento_id);
            cmd.Parameters.AddWithValue("@status_pagamento_id", pagamento.status_pagamento_id);
            cmd.Parameters.AddWithValue("@ordem_id", pagamento.ordem_id ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@valor", pagamento.valor);
            cmd.Parameters.AddWithValue("@observacao", pagamento.observacao ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@pagamento_uuid", pagamento.pagamento_uuid);
            cmd.Parameters.AddWithValue("@createdAt", pagamento.createdAt);

            cmd.Parameters.AddWithValue("@pix_qr", pagamento.pix_qr_code ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pix_copia", pagamento.pix_copia_codigo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@boleto", pagamento.boleto_url ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@expiracao", pagamento.expiracao ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@valor_pago", pagamento.valor_pago ?? (object)DBNull.Value);

            pagamento.id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public Pagamentos Atualizar(string pagamentoUUID, PagamentosUpdateDTO pagamento)
        {
            var existente = BuscarPorUUID(pagamentoUUID);
            if (existente == null)
                throw new InvalidOperationException("Pagamento não encontrado para este usuário");

            int formaFinal = pagamento.forma_pagamento_id ?? existente.forma_pagamento_id;
            int statusFinal = pagamento.status_pagamento_id ?? existente.status_pagamento_id;
            int? ordemFinal = pagamento.ordem_id ?? existente.ordem_id;
            int valorFinal = pagamento.valor ?? existente.valor;

            string? obsFinal = pagamento.observacao ?? existente.observacao;
            string? qrFinal = pagamento.pix_qr_code ?? existente.pix_qr_code;
            string? copiaFinal = pagamento.pix_copia_codigo ?? existente.pix_copia_codigo;
            string? boletoFinal = pagamento.boleto_url ?? existente.boleto_url;
            DateTime? expFinal = pagamento.expiracao ?? existente.expiracao;
            int? pagoFinal = pagamento.valor_pago ?? existente.valor_pago;
            string? uuidFinal = pagamento.pagamento_uuid ?? existente.pagamento_uuid;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                UPDATE pagamentos SET
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
                WHERE pagamento_uuid = @uuid_existente";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@uuid_existente", pagamentoUUID);
            cmd.Parameters.AddWithValue("@forma", formaFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);
            cmd.Parameters.AddWithValue("@ordem", ordemFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@valor", valorFinal);
            cmd.Parameters.AddWithValue("@obs", obsFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@updated", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@qr", qrFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@copia", copiaFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@boleto", boletoFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@exp", expFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pago", pagoFinal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@uuid", uuidFinal ?? (object)DBNull.Value);

            cmd.ExecuteNonQuery();

            return BuscarPorUUID(pagamentoUUID)!;
        }

        public void DeletarCarrinhoUsuarioId(int usuarioId)
        {
            var existente = _repoCarrinho.BuscarPorUsuarioId(usuarioId);
            if (existente == null)
                throw new InvalidOperationException("Nenhum carrinho encontrado para este usuário");

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var cmd = new NpgsqlCommand("DELETE FROM carrinho WHERE usuario_id = @uid", connection);
            cmd.Parameters.AddWithValue("@uid", usuarioId);
            cmd.ExecuteNonQuery();
        }

        public void DeletarPorUsuarioId(int usuarioId)
        {
            var existente = BuscarPorUsuarioId(usuarioId);
            if (existente == null)
                throw new InvalidOperationException("Nenhum pagamento encontrado para este usuário");

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            DeletarCarrinhoUsuarioId(usuarioId);

            var cmd = new NpgsqlCommand("DELETE FROM pagamentos WHERE usuario_id = @uid", connection);
            cmd.Parameters.AddWithValue("@uid", usuarioId);
            cmd.ExecuteNonQuery();
        }
    }
}
