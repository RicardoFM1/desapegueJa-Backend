using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;
using System.Net.WebSockets;
using System.Runtime.ConstrainedExecution;

namespace BackendDesapegaJa.Repositories
{
    public class CategoriasRepository : ICategoriasRepository
    {
        private readonly string _connectionString;

        public CategoriasRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<Categorias> ListarTodos(string? status = null)
        {
            var categorias = new List<Categorias>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM categorias";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }

            var cmd = new MySqlCommand(sql, connection);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                categorias.Add(new Categorias
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    Nome = reader.IsDBNull(reader.GetOrdinal("nome")) ? "" : reader.GetString("nome"),
                    Cor = reader.IsDBNull(reader.GetOrdinal("cor")) ? "" : reader.GetString("cor"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")

                });
            }

            return categorias;
        }
        public Categorias? BuscarPorNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return null;
            }
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT * FROM categorias WHERE LOWER(nome) = LOWER(@nome)", connection);
            cmd.Parameters.AddWithValue("@nome", nome.Trim());
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var categoriaNova = new Categorias
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    Nome = reader.IsDBNull(reader.GetOrdinal("nome")) ? "" : reader.GetString("nome"),
                    Cor = reader.IsDBNull(reader.GetOrdinal("cor")) ? "" : reader.GetString("cor"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
               
                return categoriaNova;
            }

         
            return null;
        }
        public Categorias BuscarPorId(int? id, string? status = null)
        {
            Categorias? categoria = null;
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM categorias WHERE id = @id";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }

            var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                categoria = new Categorias
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    Nome = reader.IsDBNull(reader.GetOrdinal("nome")) ? "" : reader.GetString("nome"),
                    Cor = reader.IsDBNull(reader.GetOrdinal("cor")) ? "" : reader.GetString("cor"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };
            }
            reader.Close();
            
            return categoria;

        }
        public void Adicionar(Categorias categorias)
        {

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            var cmd = new MySqlCommand("INSERT INTO categorias (nome, status, cor) VALUES(@nome, @status, @cor); SELECT LAST_INSERT_ID();", connection);
            cmd.Parameters.AddWithValue("@nome", categorias.Nome);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(categorias.status) ? "ativo" : categorias.status);
            cmd.Parameters.AddWithValue("@cor", categorias.Cor);

            int novoId = Convert.ToInt32(cmd.ExecuteScalar());
            categorias.Id = novoId;


           

        }

        public void Atualizar(int id, CategoriasUpdateDTO categorias)
        {
            var categoriaExistente = BuscarPorId(id);
            if(categoriaExistente == null)
            {
                throw new InvalidOperationException("Nenhuma categoria encontrada");
            }
            var nomeFinal = string.IsNullOrWhiteSpace(categorias.Nome) ? categoriaExistente.Nome : categorias.Nome;
            var corFinal = string.IsNullOrWhiteSpace(categorias.Cor) ? categoriaExistente.Cor : categorias.Cor;
            var statusFinal = string.IsNullOrWhiteSpace(categorias.status) ? categoriaExistente.status : categorias.status;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("UPDATE categorias SET nome = @nome, status = @status, cor = @cor WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nome", nomeFinal);
            cmd.Parameters.AddWithValue("@cor", corFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);
            cmd.ExecuteNonQuery();
            
        }

    }
}
