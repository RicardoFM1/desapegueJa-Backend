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

            using var cmd = new MySqlCommand("SELECT * from ordem_de_compra", connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ordens.Add(new OrdemDeCompra
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    produto_id = reader.IsDBNull(reader.GetOrdinal("produto_id")) ? 0 : reader.GetInt32("produto_id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.IsDBNull(reader.GetOrdinal("status_ordem_id")) ? 0 : reader.GetInt32("status_ordem_id")
                });
            }

            return ordens;
        }

        public OrdemDeCompra BuscarPorId(int? id)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT * FROM ordem_de_compra WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            OrdemDeCompra? ordemdecompra = null;
            while (reader.Read())
            {
                ordemdecompra = new OrdemDeCompra
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    produto_id = reader.IsDBNull(reader.GetOrdinal("produto_id")) ? 0 : reader.GetInt32("produto_id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.IsDBNull(reader.GetOrdinal("status_ordem_id")) ? 0 : reader.GetInt32("status_ordem_id")
                };
            }
            reader.Close();
            return ordemdecompra;
        }

        public OrdemDeCompra BuscarPorProdutoId(int? id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT * FROM ordem_de_compra WHERE produto_id = @produto_id", connection);
            cmd.Parameters.AddWithValue("@produto_id", id);
            using var reader = cmd.ExecuteReader();
            OrdemDeCompra? ordemdecompra = null;
            while (reader.Read())
            {
                ordemdecompra = new OrdemDeCompra
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    produto_id = reader.IsDBNull(reader.GetOrdinal("produto_id")) ? 0 : reader.GetInt32("produto_id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.IsDBNull(reader.GetOrdinal("status_ordem_id")) ? 0 : reader.GetInt32("status_ordem_id")
                };
            }
            reader.Close();
            return ordemdecompra;
        }
        public OrdemDeCompra BuscarPorUsuarioId(int? id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT * FROM ordem_de_compra WHERE usuario_id = @usuario_id", connection);
            cmd.Parameters.AddWithValue("@usuario_id", id);
            using var reader = cmd.ExecuteReader();
            OrdemDeCompra? ordemdecompra = null;
            while (reader.Read())
            {
                ordemdecompra = new OrdemDeCompra
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    produto_id = reader.IsDBNull(reader.GetOrdinal("produto_id")) ? 0 : reader.GetInt32("produto_id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.IsDBNull(reader.GetOrdinal("status_ordem_id")) ? 0 : reader.GetInt32("status_ordem_id")
                };
            }
            reader.Close();
            return ordemdecompra;
        }
        public OrdemDeCompra BuscarPorStatusDeCompraId(int? id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT * FROM ordem_de_compra WHERE status_ordem_id = @status_ordem_id", connection);
            cmd.Parameters.AddWithValue("@status_ordem_id", id);
            using var reader = cmd.ExecuteReader();
            OrdemDeCompra? ordemdecompra = null;
            while (reader.Read())
            {
                ordemdecompra = new OrdemDeCompra
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    produto_id = reader.IsDBNull(reader.GetOrdinal("produto_id")) ? 0 : reader.GetInt32("produto_id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    status_ordem_id = reader.IsDBNull(reader.GetOrdinal("status_ordem_id")) ? 0 : reader.GetInt32("status_ordem_id")
                };
            }
            reader.Close();
            return ordemdecompra;
        }

        public void Adicionar(OrdemDeCompra ordem)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("INSERT INTO ordem_de_compra (produto_id, usuario_id, status_ordem_id) VALUES (@produto_id, @usuario_id, @status_ordem_id); SELECT LAST_INSERT_ID();", connection);
            cmd.Parameters.AddWithValue("@produto_id", ordem.produto_id);
            cmd.Parameters.AddWithValue("@usuario_id", ordem.usuario_id);
            cmd.Parameters.AddWithValue("@status_ordem_id", ordem.status_ordem_id);

            var novoId = Convert.ToInt32(cmd.ExecuteScalar());
            ordem.id = novoId;


        }

        public OrdemDeCompra Atualizar(int id, OrdemDeCompraUpdateDTO ordem)
        {
            var ordemExistente = BuscarPorId(id);
            if (ordemExistente == null)
            {
                throw new InvalidOperationException("Nenhuma ordem de compra encontrada");
            }
            int produtoIdFinal = ordem.produto_id.HasValue ? ordem.produto_id.Value : ordemExistente.produto_id;
            int usuarioIdFinal = ordem.usuario_id.HasValue ? ordem.usuario_id.Value : ordemExistente.usuario_id;
            int StatusOrdemFinal = ordem.status_ordem_id.HasValue ? ordem.status_ordem_id.Value : ordemExistente.status_ordem_id;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("UPDATE ordem_de_compra SET produto_id = @produto_id, usuario_id = @usuario_id, status_ordem_id = @status_ordem_id WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@produto_id", produtoIdFinal);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioIdFinal);
            cmd.Parameters.AddWithValue("@status_ordem_id", StatusOrdemFinal);
            
            cmd.ExecuteNonQuery();
            return new OrdemDeCompra
            {
                id = id,
                produto_id = produtoIdFinal,
                usuario_id = usuarioIdFinal,
                status_ordem_id = StatusOrdemFinal
            };
        }
    }
}
