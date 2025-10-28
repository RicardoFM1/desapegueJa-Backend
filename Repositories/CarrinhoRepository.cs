using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;

namespace BackendDesapegaJa.Repositories
{
    public class CarrinhoRepository : ICarrinhoRepository
    {
        private readonly string _connectionString;

        public CarrinhoRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<Carrinho> ListarTodos()
        {
            var carrinhos = new List<Carrinho>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            var cmd = new MySqlCommand("SELECT * FROM carrinho", connection);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                carrinhos.Add(new Carrinho
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    produto_id = reader.IsDBNull(reader.GetOrdinal("produto_id")) ? 0 : reader.GetInt32("produto_id"),
                    quantidade = reader.IsDBNull(reader.GetOrdinal("quantidade")) ? 0 : reader.GetInt32("quantidade")
                });
            }
            reader.Close();
            return carrinhos;
        }

        public Carrinho? BuscarPorUsuarioEProduto(int usuarioId, int produtoId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            Carrinho? carrinho = null;

            using var cmd = new MySqlCommand("SELECT * FROM carrinho WHERE usuario_id = @usuario_id AND produto_id = @produto_id", connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
            cmd.Parameters.AddWithValue("@produto_id", produtoId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                carrinho = new Carrinho
                {
                    id = reader.GetInt32("id"),
                    usuario_id = reader.GetInt32("usuario_id"),
                    produto_id = reader.GetInt32("produto_id"),
                    quantidade = reader.IsDBNull(reader.GetOrdinal("quantidade")) ? 0 : reader.GetInt32("quantidade")
                };
            }

            return carrinho;
        }

        public Carrinho BuscarPorId(int? id)
        {
            Carrinho? carrinho = null;
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("SELECT * FROM carrinho WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                carrinho = new Carrinho
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    produto_id = reader.IsDBNull(reader.GetOrdinal("produto_id")) ? 0 : reader.GetInt32("produto_id"),
                    quantidade = reader.IsDBNull(reader.GetOrdinal("quantidade")) ? 0 : reader.GetInt32("quantidade")
                };
            }
            reader.Close();

            return carrinho;

        }
        public void Adicionar(Carrinho carrinho)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            var cmd = new MySqlCommand("INSERT INTO carrinho (usuario_id, produto_id, quantidade) VALUES(@usuario_id, @produto_id, @quantidade); SELECT LAST_INSERT_ID();", connection);
            cmd.Parameters.AddWithValue("@usuario_id", carrinho.usuario_id);
            cmd.Parameters.AddWithValue("@produto_id", carrinho.produto_id);
            cmd.Parameters.AddWithValue("@quantidade", carrinho.quantidade);
            
            int novoId = Convert.ToInt32(cmd.ExecuteScalar());
            carrinho.id = novoId;

        }

        public void Atualizar(int usuarioId, int produtoId, CarrinhoUpdateDTO carrinho)
        {
             
            var CarrinhoExistente = BuscarPorUsuarioEProduto(usuarioId, produtoId);
            if (CarrinhoExistente == null)
            {
                throw new InvalidOperationException("Nenhum item do carrinho com esse usuario e/ou produto encontrado");
            }


            var quantidadeFinal = carrinho.quantidade.HasValue ? carrinho.quantidade.Value : CarrinhoExistente.quantidade;
            var produtoIdFinal = carrinho.produto_id.HasValue ? carrinho.produto_id.Value : CarrinhoExistente.produto_id;


            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("UPDATE carrinho SET produto_id = @produto_id, quantidade = @quantidade WHERE usuario_id = @usuario_id AND produto_id = @produto_id", connection);

            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
            cmd.Parameters.AddWithValue("@quantidade", quantidadeFinal);
            cmd.Parameters.AddWithValue("@produto_id", produtoIdFinal);
            cmd.ExecuteNonQuery();

        }
        public void Deletar(int usuarioId, int produtoId)
        {
            var CarrinhoExistente = BuscarPorUsuarioEProduto(usuarioId, produtoId);
            if (CarrinhoExistente == null)
            {
                throw new InvalidOperationException("Nenhum usuário e/ou produto encontrado neste carrinho");
            }
           

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand("DELETE FROM carrinho WHERE usuario_id = @usuario_id AND produto_id = @produto_id", connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
            cmd.Parameters.AddWithValue("@produto_id", produtoId);
            cmd.ExecuteNonQuery();
        }
    }
}
