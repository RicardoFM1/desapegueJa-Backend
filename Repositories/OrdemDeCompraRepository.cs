using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;

namespace BackendDesapegaJa.Repositories
{
    public class OrdemDeCompraRepository : IOrdemDeCompraRepository
    {
        private readonly string _connectionString;

        public OrdemDeCompraRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<OrdemDeCompra> ListarTodos()
        {
            var ordens = new List<OrdemDeCompra>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM ordem_de_compra";

            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ordens.Add(new OrdemDeCompra
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    status_ordem_id = reader.GetInt32(reader.GetOrdinal("status_ordem_id")),
                    valor_total = reader.GetInt32(reader.GetOrdinal("valor_total")),
                    
                    metodo_entrega = reader.IsDBNull(reader.GetOrdinal("metodo_entrega"))
                        ? "combinar"
                        : reader.GetString(reader.GetOrdinal("metodo_entrega")),
                    created_at = reader.GetDateTime(reader.GetOrdinal("created_at"))
                });
            }

            return ordens;
        }

        public OrdemDeCompra? BuscarPorId(int? id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM ordem_de_compra WHERE id = @id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new OrdemDeCompra
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    status_ordem_id = reader.GetInt32(reader.GetOrdinal("status_ordem_id")),
                    valor_total = reader.GetInt32(reader.GetOrdinal("valor_total")),
                   
                    metodo_entrega = reader.IsDBNull(reader.GetOrdinal("metodo_entrega"))
                        ? "combinar"
                        : reader.GetString(reader.GetOrdinal("metodo_entrega")),
                    created_at = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };
            }

            return null;
        }

        public OrdemDeCompra? BuscarPorUsuarioId(int usuarioId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"SELECT * FROM ordem_de_compra 
                           WHERE usuario_id = @usuario_id
                           LIMIT 1";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new OrdemDeCompra
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    status_ordem_id = reader.GetInt32(reader.GetOrdinal("status_ordem_id")),
                    valor_total = reader.GetInt32(reader.GetOrdinal("valor_total")),
              
                    metodo_entrega = reader.IsDBNull(reader.GetOrdinal("metodo_entrega"))
                        ? "combinar"
                        : reader.GetString(reader.GetOrdinal("metodo_entrega")),
                    created_at = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };
            }

            return null;
        }

        public IEnumerable<OrdemDeCompra> BuscarPorStatusDeCompraId(int statusId)
        {
            var ordens = new List<OrdemDeCompra>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM ordem_de_compra WHERE status_ordem_id = @status_ordem_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@status_ordem_id", statusId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ordens.Add(new OrdemDeCompra
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    status_ordem_id = reader.GetInt32(reader.GetOrdinal("status_ordem_id")),
                    valor_total = reader.GetInt32(reader.GetOrdinal("valor_total")),
                    
                    metodo_entrega = reader.IsDBNull(reader.GetOrdinal("metodo_entrega"))
                        ? "combinar"
                        : reader.GetString(reader.GetOrdinal("metodo_entrega")),
                    created_at = reader.GetDateTime(reader.GetOrdinal("created_at"))
                });
            }

            return ordens;
        }

        public void Adicionar(OrdemDeCompraCreateDTO ordem)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                INSERT INTO ordem_de_compra 
                (usuario_id, status_ordem_id, valor_total, metodo_entrega)
                VALUES (@usuario_id, @status_ordem_id, @valor_total, @metodo_entrega)
                RETURNING id;
            ";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", ordem.usuario_id);
            cmd.Parameters.AddWithValue("@status_ordem_id", ordem.status_ordem_id);
            cmd.Parameters.AddWithValue("@valor_total", ordem.valor_total);
            
            cmd.Parameters.AddWithValue("@metodo_entrega",
                string.IsNullOrWhiteSpace(ordem.metodo_entrega) ? "combinar" : ordem.metodo_entrega);

            ordem.id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public OrdemDeCompra Atualizar(int id, OrdemDeCompraUpdateDTO dto)
        {
            var existente = BuscarPorId(id);
            if (existente == null)
                throw new InvalidOperationException("Nenhuma ordem de compra encontrada.");

            int usuarioFinal = dto.usuario_id ?? existente.usuario_id;
            int statusFinal = dto.status_ordem_id ?? existente.status_ordem_id;
            int valorFinal = dto.valor_total ?? existente.valor_total;
            string metodoEntregaFinal = string.IsNullOrWhiteSpace(dto.metodo_entrega)
                ? existente.metodo_entrega
                : dto.metodo_entrega;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                UPDATE ordem_de_compra 
                SET usuario_id = @usuario_id,
                    status_ordem_id = @status_ordem_id,
                    valor_total = @valor_total,
                    metodo_entrega = @metodo_entrega
                WHERE id = @id;
            ";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioFinal);
            cmd.Parameters.AddWithValue("@status_ordem_id", statusFinal);
            cmd.Parameters.AddWithValue("@valor_total", valorFinal);
            cmd.Parameters.AddWithValue("@metodo_entrega", metodoEntregaFinal);

            cmd.ExecuteNonQuery();

            return new OrdemDeCompra
            {
                id = id,
                usuario_id = usuarioFinal,
                status_ordem_id = statusFinal,
                valor_total = valorFinal,
                metodo_entrega = metodoEntregaFinal,
                created_at = existente.created_at
            };
        }

        public void DeletarPorUsuarioId(int usuarioId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM ordem_de_compra WHERE usuario_id = @usuario_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            cmd.ExecuteNonQuery();
        }

        public void DeletarOrdemEmAberto(int usuarioId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM ordem_de_compra WHERE usuario_id = @usuario_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            cmd.ExecuteNonQuery();
        }

        public void DeletarOrdemEmAbertoPorOrdemId(int ordemId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM ordem_de_compra WHERE id = @ordem_id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ordem_id", ordemId);

            cmd.ExecuteNonQuery();
        }
    }
}
