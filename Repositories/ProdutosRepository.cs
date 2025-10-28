using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg.OpenPgp;
using ZstdSharp.Unsafe;

namespace BackendDesapegaJa.Repositories
{
    public class ProdutosRepository : IProdutoRepository
    {
        private readonly string _connectionString;
        private readonly IUsuarioRepository _repoUser;
        private readonly ICategoriasRepository _repoCategoria;

        public ProdutosRepository(MySqlConnection connection, IUsuarioRepository repoUser, ICategoriasRepository repoCategoria, IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _repoUser = repoUser;
            _repoCategoria = repoCategoria;
        }

        public IEnumerable<Produto> ListarTodos(string? status = null)
        {
           
            var produtos = new List<Produto>();

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            string sql = "SELECT * FROM produtos";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }
            var cmd = new MySqlCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("status", status);
            }
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                produtos.Add(new Produto
                {
                    nome = reader.IsDBNull(reader.GetOrdinal("nome")) ? "" : reader.GetString("nome"),
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 :  reader.GetInt32("usuario_id"),
                    preco = reader.IsDBNull(reader.GetOrdinal("preco")) ? 0 : reader.GetInt32("preco"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" :  reader.GetString("descricao"),
                    data_post = reader.IsDBNull(reader.GetOrdinal("data_post")) ? "" :  reader.GetString("data_post"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" :  reader.GetString("status"),
                    categoria_id = reader.IsDBNull(reader.GetOrdinal("categoria_id")) ? 0 :  reader.GetInt32("categoria_id"),
                    estoque = reader.IsDBNull(reader.GetOrdinal("estoque")) ? 0 : reader.GetInt32("estoque"),
                    imagem = reader.IsDBNull(reader.GetOrdinal("imagem")) ? "" : reader.GetString("imagem")
                });
            }
         

            return produtos;
        }
        public Produto? BuscarPorNome(string nome)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT * FROM produtos WHERE LOWER(nome) = LOWER(@nome)", connection);
            cmd.Parameters.AddWithValue("@nome", nome.Trim());
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var produtoRes = new Produto
                {
                    nome = reader.IsDBNull(reader.GetOrdinal("nome")) ? "" : reader.GetString("nome"),
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    preco = reader.IsDBNull(reader.GetOrdinal("preco")) ? 0 : reader.GetInt32("preco"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    data_post = reader.IsDBNull(reader.GetOrdinal("data_post")) ? "" : reader.GetString("data_post"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status"),
                    categoria_id = reader.IsDBNull(reader.GetOrdinal("categoria_id")) ? 0 : reader.GetInt32("categoria_id"),
                    estoque = reader.IsDBNull(reader.GetOrdinal("estoque")) ? 0 : reader.GetInt32("estoque"),
                    imagem = reader.IsDBNull(reader.GetOrdinal("imagem")) ? "" : reader.GetString("imagem")
                };
               
                return produtoRes;
            }
            return null;
        }

        public Produto? BuscarPorId(int? id, string? status)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM produtos WHERE id = @id";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            using var cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@id", id);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            var reader = cmd.ExecuteReader();
            Produto? produto = null;
            if (reader.Read())
            {
                produto = new Produto
                {
                    nome = reader.IsDBNull(reader.GetOrdinal("nome")) ? "" : reader.GetString("nome"),
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    preco = reader.IsDBNull(reader.GetOrdinal("preco")) ? 0 : reader.GetInt32("preco"),
                    descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                    data_post = reader.IsDBNull(reader.GetOrdinal("data_post")) ? "" : reader.GetString("data_post"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status"),
                    categoria_id = reader.IsDBNull(reader.GetOrdinal("categoria_id")) ? 0 : reader.GetInt32("categoria_id"),
                    estoque = reader.IsDBNull(reader.GetOrdinal("estoque")) ? 0 : reader.GetInt32("estoque"),
                    imagem = reader.IsDBNull(reader.GetOrdinal("imagem")) ? "" : reader.GetString("imagem")
                };
                
                
            }
            reader.Close();
            return produto;
            
        }
        public void Adicionar(Produto produto)
        {
            
            var usuarioExistente = _repoUser.BuscarPorId(produto.usuario_id);
            var categoriaExistente = _repoCategoria.BuscarPorId(produto.categoria_id);
           
            if(usuarioExistente == null)
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            }
            if(categoriaExistente == null)
            {
                throw new InvalidOperationException("Categoria referenciada não encontrada");
            }
            if (produto.estoque <= 0)
            {
                throw new InvalidOperationException("O estoque deve ser maior que 0");
            }

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var data = DateTime.UtcNow;
            var dataFormatada = data.ToString("dd/MM/yyyy HH:mm:ss");

            using var cmd = new MySqlCommand(
                "INSERT INTO produtos (usuario_id, nome, preco, descricao, categoria_id, estoque, status, data_post) " +
                "VALUES(@usuarioId, @nome, @preco, @descricao, @categoria_id, @estoque, @status, @data_post); SELECT LAST_INSERT_ID();", connection);

            cmd.Parameters.AddWithValue("@usuarioId", produto.usuario_id);
            cmd.Parameters.AddWithValue("@nome", produto.nome);
            cmd.Parameters.AddWithValue("@preco", produto.preco);
            cmd.Parameters.AddWithValue("@descricao", produto.descricao);
            cmd.Parameters.AddWithValue("@categoria_id", produto.categoria_id);
            cmd.Parameters.AddWithValue("@estoque", produto.estoque);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(produto.status) ? "ativo" : produto.status);
            cmd.Parameters.AddWithValue("@data_post", dataFormatada);

            int idNovo = Convert.ToInt32(cmd.ExecuteScalar());
            produto.id = idNovo;

         
        }
        public Produto? Atualizar(int id, string? status, ProdutoUpdateDTO produto)
        {

            var existente = BuscarPorId(id, status);
            var usuarioIdFinal = produto.usuario_id ?? existente.usuario_id;
            var categoriaIdFinal = produto.categoria_id ?? existente.categoria_id;

            var usuarioExistente = _repoUser.BuscarPorId(usuarioIdFinal);
            var categoriaExistente = _repoCategoria.BuscarPorId(categoriaIdFinal);
            if (existente == null)
            {
                throw new InvalidOperationException("Produto não encontrado");
            }
            if (usuarioExistente == null)
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            }
            if (categoriaExistente == null)
            {
                throw new InvalidOperationException("Categoria referenciada não encontrada");
            }
            if (produto.estoque <= 0)
            {
                throw new InvalidOperationException("O estoque deve ser maior que 0");
            }

            var nomeFinal = string.IsNullOrWhiteSpace(produto.nome) ? existente.nome : produto.nome;
            var imagemFinal = string.IsNullOrWhiteSpace(produto.imagem) ? existente.imagem : produto.imagem;
            var dataPostFinal = string.IsNullOrWhiteSpace(produto.data_post) ? existente.data_post : produto.data_post;
            var descricaoFinal = string.IsNullOrWhiteSpace(produto.descricao) ? existente.descricao : produto.descricao;
            var statusFinal = string.IsNullOrWhiteSpace(produto.status) ? existente.status : produto.status;
            var precoFinal = produto.preco ?? existente.preco;
            var estoqueFinal = produto.estoque != 0 ? produto.estoque : existente.estoque;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(@"
            UPDATE produtos 
            SET nome = @nome, usuario_id = @usuario_id, preco = @preco, descricao = @descricao, 
            categoria_id = @categoria_id, imagem = @imagem, data_post = @data_post, estoque = @estoque, status = @status
            WHERE id = @id", connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioIdFinal);
            cmd.Parameters.AddWithValue("@imagem", imagemFinal);
            cmd.Parameters.AddWithValue("@data_post", dataPostFinal);
            cmd.Parameters.AddWithValue("@nome", nomeFinal);
            cmd.Parameters.AddWithValue("@preco", precoFinal);
            cmd.Parameters.AddWithValue("@descricao", descricaoFinal);
            cmd.Parameters.AddWithValue("@categoria_id", categoriaIdFinal);
            cmd.Parameters.AddWithValue("@estoque", estoqueFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);
            cmd.ExecuteNonQuery();

            return new Produto
            {
                id = id,
                nome = nomeFinal,
                categoria_id = categoriaIdFinal,
                usuario_id = usuarioIdFinal,
                descricao = descricaoFinal,
                data_post = dataPostFinal,
                imagem = imagemFinal,
                estoque = estoqueFinal,
                preco = precoFinal,
                status = statusFinal
            };
        }
    }
}
