using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace BackendDesapegaJa.Repositories
{
    public class UsuariosRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuariosRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<Usuario> ListarTodos(string? status = null)
        {
            var usuarios = new List<Usuario>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            string sql = "SELECT * FROM Usuarios";
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }
            using var cmd = new MySqlCommand(sql, connection);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                usuarios.Add(MapUsuario(reader));
            }
            return usuarios;
        }

        public Usuario? BuscarPorEmail(string email, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            string sql = "SELECT * FROM Usuarios WHERE LOWER(email)=LOWER(@email)";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@email", email.Trim());
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapUsuario(reader) : null;
        }

        public Usuario? BuscarPorId(int? id, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Usuarios WHERE id=@id";
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
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapUsuario(reader) : null;
        }

        public Usuario? BuscarPorCpf(string cpf, string? status = null)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return null;
            }

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            string sql = "SELECT * FROM Usuarios WHERE cpf=@cpf";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@cpf", long.Parse(cpf));
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapUsuario(reader) : null;
        }

        public void Adicionar(Usuario usuario)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Usuarios 
                (email, senha, status, admin, telefone, rg, cpf, cep, foto_de_perfil, data_de_nascimento)
                VALUES (@Email,@Senha,@Status,@Admin,@Telefone,@Rg,@Cpf,@Cep,@Foto,@Nascimento);
                SELECT LAST_INSERT_ID();", connection);

            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@Senha", usuario.Senha);
            cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(usuario.status) ? "ativo" : usuario.status);
            cmd.Parameters.AddWithValue("@Admin", usuario.Admin ? 1 : 0);
            cmd.Parameters.AddWithValue("@Telefone", string.IsNullOrWhiteSpace(usuario.Telefone) ? (object)DBNull.Value : usuario.Telefone);
            cmd.Parameters.AddWithValue("@Rg", usuario.Rg ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Cpf", string.IsNullOrWhiteSpace(usuario.Cpf) ? (object)DBNull.Value : long.Parse(usuario.Cpf));
            cmd.Parameters.AddWithValue("@Cep", string.IsNullOrWhiteSpace(usuario.Cep) ? (object)DBNull.Value : usuario.Cep);
            cmd.Parameters.AddWithValue("@Foto", string.IsNullOrWhiteSpace(usuario.Foto_De_Perfil) ? (object)DBNull.Value : usuario.Foto_De_Perfil);
            cmd.Parameters.AddWithValue("@Nascimento", string.IsNullOrWhiteSpace(usuario.data_de_nascimento) ? (object)DBNull.Value : usuario.data_de_nascimento);

            usuario.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Atualizar(int id, Usuario usuario, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                UPDATE Usuarios SET
                    email=@Email,
                    senha=@Senha,
                    status=@Status,
                    admin=@Admin,
                    telefone=@Telefone,
                    rg=@Rg,
                    cpf=@Cpf,
                    cep=@Cep,
                    foto_de_perfil=@Foto,
                    data_de_nascimento=@Nascimento
                WHERE id=@Id";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }
            using var cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@Senha", usuario.Senha);
            cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(usuario.status) ? "ativo" : usuario.status);
            cmd.Parameters.AddWithValue("@Admin", usuario.Admin ? 1 : 0);
            cmd.Parameters.AddWithValue("@Telefone", string.IsNullOrWhiteSpace(usuario.Telefone) ? (object)DBNull.Value : usuario.Telefone);
            cmd.Parameters.AddWithValue("@Rg", usuario.Rg ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Cpf", string.IsNullOrWhiteSpace(usuario.Cpf) ? (object)DBNull.Value : long.Parse(usuario.Cpf));
            cmd.Parameters.AddWithValue("@Cep", string.IsNullOrWhiteSpace(usuario.Cep) ? (object)DBNull.Value : usuario.Cep);
            cmd.Parameters.AddWithValue("@Foto", string.IsNullOrWhiteSpace(usuario.Foto_De_Perfil) ? (object)DBNull.Value : usuario.Foto_De_Perfil);
            cmd.Parameters.AddWithValue("@Nascimento", string.IsNullOrWhiteSpace(usuario.data_de_nascimento) ? (object)DBNull.Value : usuario.data_de_nascimento);

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            cmd.ExecuteNonQuery();
        }

        private Usuario MapUsuario(MySqlDataReader reader)
        {
            return new Usuario
            {
                Id = reader.GetInt32("id"),
                Email = reader.GetString("email"),
                Senha = reader.GetString("senha"),
                status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString("status"),
                Admin = reader.IsDBNull(reader.GetOrdinal("admin")) ? false : reader.GetBoolean("admin"),
                Telefone = reader.IsDBNull(reader.GetOrdinal("telefone")) ? null : reader.GetString("telefone"),
                Rg = reader.IsDBNull(reader.GetOrdinal("rg")) ? null : reader.GetInt32("rg"),
                Cpf = reader.IsDBNull(reader.GetOrdinal("cpf")) ? null : reader.GetInt64("cpf").ToString("D11"),
                Cep = reader.IsDBNull(reader.GetOrdinal("cep")) ? null : reader.GetString("cep"),
                Foto_De_Perfil = reader.IsDBNull(reader.GetOrdinal("foto_de_perfil")) ? null : reader.GetString("foto_de_perfil"),
                data_de_nascimento = reader.IsDBNull(reader.GetOrdinal("data_de_nascimento")) ? null : reader.GetString("data_de_nascimento"),
            };
        }
    }
}
