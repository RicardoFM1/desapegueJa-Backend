using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;

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
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM ordem_de_compra";

            using var cmd = new MySqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ordens.Add(new OrdemDeCompra
                {
                    id = reader.GetInt32("id"),
                    usuario_id = reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.GetInt32("status_ordem_id"),
                    valor_total = reader.GetInt32("valor_total"),
                    created_at = reader.GetString("created_at")
                });
            }

            return ordens;
        }

        public OrdemDeCompra BuscarPorId(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM ordem_de_compra WHERE id = @id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new OrdemDeCompra
                {
                    id = reader.GetInt32("id"),
                    usuario_id = reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.GetInt32("status_ordem_id"),
                    valor_total = reader.GetInt32("valor_total"),
                    created_at = reader.GetString("created_at")
                };
            }

            return null;
        }


        public IEnumerable<OrdemDeCompra> BuscarPorUsuarioId(int usuarioId)
        {
            var ordens = new List<OrdemDeCompra>();

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM ordem_de_compra WHERE usuario_id = @usuario_id";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ordens.Add(new OrdemDeCompra
                {
                    id = reader.GetInt32("id"),
                    usuario_id = reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.GetInt32("status_ordem_id"),
                    valor_total = reader.GetInt32("valor_total"),
                    created_at = reader.GetString("created_at")
                });
            }

            return ordens;
        }

        public IEnumerable<OrdemDeCompra> BuscarPorStatusDeCompraId(int statusId)
        {
            var ordens = new List<OrdemDeCompra>();

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM ordem_de_compra WHERE status_ordem_id = @status_ordem_id";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@status_ordem_id", statusId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ordens.Add(new OrdemDeCompra
                {
                    id = reader.GetInt32("id"),
                    usuario_id = reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.GetInt32("status_ordem_id"),
                    valor_total = reader.GetInt32("valor_total"),
                    created_at = reader.GetString("created_at")
                });
            }

            return ordens;
        }

        public void Adicionar(OrdemDeCompra ordem)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = @"INSERT INTO ordem_de_compra 
                           (usuario_id, status_ordem_id, valor_total) 
                           VALUES (@usuario_id, @status_ordem_id, @valor_total);
                           SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@usuario_id", ordem.usuario_id);
            cmd.Parameters.AddWithValue("@status_ordem_id", ordem.status_ordem_id);
            cmd.Parameters.AddWithValue("@valor_total", ordem.valor_total);

            var novoId = Convert.ToInt32(cmd.ExecuteScalar());
            ordem.id = novoId;
        }

        public OrdemDeCompra Atualizar(int id, OrdemDeCompraUpdateDTO dto)
        {
            var existente = BuscarPorId(id);
            if (existente == null)
            {
                throw new InvalidOperationException("Nenhuma ordem de compra encontrada.");
            }

            int usuarioFinal = dto.usuario_id ?? existente.usuario_id;
            int statusFinal = dto.status_ordem_id ?? existente.status_ordem_id;
            int valorFinal = dto.valor_total ?? existente.valor_total;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = @"UPDATE ordem_de_compra 
                           SET usuario_id = @usuario_id,
                               status_ordem_id = @status_ordem_id,
                               valor_total = @valor_total
                           WHERE id = @id";

            using var cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioFinal);
            cmd.Parameters.AddWithValue("@status_ordem_id", statusFinal);
            cmd.Parameters.AddWithValue("@valor_total", valorFinal);

            cmd.ExecuteNonQuery();

            return new OrdemDeCompra
            {
                id = id,
                usuario_id = usuarioFinal,
                status_ordem_id = statusFinal,
                valor_total = valorFinal,
                created_at = existente.created_at
            };
        }
        public void Deletar(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM ordem_de_compra WHERE id = @id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }

    }
}
