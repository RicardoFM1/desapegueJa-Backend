using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;

namespace BackendDesapegaJa.Repositories
{
    public class OrdemProdutoRepository : IOrdemProdutoRepository
    {
        private readonly string _connectionString;

        public OrdemProdutoRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<OrdemProduto> ListarTodos()
        {
            var lista = new List<OrdemProduto>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM ordem_produto";
            using var cmd = new MySqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new OrdemProduto
                {
                    id = reader.GetInt32("id"),
                    ordem_id = reader.GetInt32("ordem_id"),
                    produto_id = reader.GetInt32("produto_id"),
                    quantidade = reader.GetInt32("quantidade"),
                    preco_unitario = reader.GetInt32("preco_unitario")
                });
            }

            return lista;
        }

        public OrdemProduto? BuscarPorUsuarioId(int usuarioId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT op.* 
                FROM ordem_produto op
                INNER JOIN ordem_de_compra oc ON op.ordem_id = oc.id
                WHERE oc.usuario_id = @usuario_id
                LIMIT 1";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new OrdemProduto
                {
                    id = reader.GetInt32("id"),
                    ordem_id = reader.GetInt32("ordem_id"),
                    produto_id = reader.GetInt32("produto_id"),
                    quantidade = reader.GetInt32("quantidade"),
                    preco_unitario = reader.GetInt32("preco_unitario")
                };
            }

            return null;
        }

        public IEnumerable<OrdemProduto> BuscarProdutosPorOrdemId(int ordemId)
        {
            var lista = new List<OrdemProduto>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            
            string sql = @"
        SELECT * FROM ordem_produto 
        WHERE ordem_id = @ordem_id";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ordem_id", ordemId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new OrdemProduto
                {
                    id = reader.GetInt32("id"),
                    ordem_id = reader.GetInt32("ordem_id"),
                    produto_id = reader.GetInt32("produto_id"),
                    quantidade = reader.GetInt32("quantidade"),
                    preco_unitario = reader.GetInt32("preco_unitario")
                });
            }

            return lista;
        }

        public void Adicionar(OrdemProduto ordemProduto)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = @"INSERT INTO ordem_produto (ordem_id, produto_id, quantidade, preco_unitario) 
                           VALUES (@ordem_id, @produto_id, @quantidade, @preco_unitario);
                           SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ordem_id", ordemProduto.ordem_id);
            cmd.Parameters.AddWithValue("@produto_id", ordemProduto.produto_id);
            cmd.Parameters.AddWithValue("@quantidade", ordemProduto.quantidade);
            cmd.Parameters.AddWithValue("@preco_unitario", ordemProduto.preco_unitario);

            ordemProduto.id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public OrdemProduto AtualizarPorUsuarioId(int usuarioId, OrdemProdutoUpdateDTO dto)
        {
            var existente = BuscarPorUsuarioId(usuarioId);
            if (existente == null)
                throw new InvalidOperationException("Ordem Produto do usuário não existe.");

            int produtoFinal = dto.produto_id ?? existente.produto_id;
            int quantidadeFinal = dto.quantidade ?? existente.quantidade;
            int precoFinal = dto.preco_unitario ?? existente.preco_unitario;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = @"UPDATE ordem_produto
                           SET produto_id=@produto_id, quantidade=@quantidade, preco_unitario=@preco_unitario
                           WHERE id=@id";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", existente.id);
            cmd.Parameters.AddWithValue("@produto_id", produtoFinal);
            cmd.Parameters.AddWithValue("@quantidade", quantidadeFinal);
            cmd.Parameters.AddWithValue("@preco_unitario", precoFinal);

            cmd.ExecuteNonQuery();

            return new OrdemProduto
            {
                id = existente.id,
                ordem_id = existente.ordem_id,
                produto_id = produtoFinal,
                quantidade = quantidadeFinal,
                preco_unitario = precoFinal
            };
        }

       
    }
}
