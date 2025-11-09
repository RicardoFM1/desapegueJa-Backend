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
            string sql = "SELECT id, nome, email, senha, status, admin, telefone, cpf, cep, foto_de_perfil, data_de_nascimento FROM Usuarios";
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

        public Usuario? BuscarPorNome(string nome, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            string sql = "SELECT id, nome, email, senha, status, admin, telefone, cpf, cep, foto_de_perfil, data_de_nascimento FROM Usuarios WHERE LOWER(nome)=LOWER(@nome)";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@nome", nome.Trim());
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapUsuario(reader) : null;
        }

        public Usuario? BuscarPorEmail(string email, string? status = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            string sql = "SELECT id, nome, email, senha, status, admin, telefone, cpf, cep, foto_de_perfil, data_de_nascimento FROM Usuarios WHERE LOWER(email)=LOWER(@email)";

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

            string sql = "SELECT id, nome, email, senha, status, admin, telefone, cpf, cep, foto_de_perfil, data_de_nascimento FROM Usuarios WHERE id=@id";
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
            string sql = "SELECT id, nome, email, senha, status, admin, telefone, cpf, cep, foto_de_perfil, data_de_nascimento FROM Usuarios WHERE cpf=@cpf";

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
                (email, senha, status, admin, telefone, cpf, cep, foto_de_perfil, data_de_nascimento, nome)
                VALUES (@Email,@Senha,@Status,@Admin,@Telefone,@Rg,@Cpf,@Cep,@Foto,@Nascimento, @Nome);
                SELECT LAST_INSERT_ID();", connection);

            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@Senha", usuario.Senha);
            cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(usuario.status) ? "ativo" : usuario.status);
            cmd.Parameters.AddWithValue("@Admin", usuario.Admin ? 1 : 0);
            cmd.Parameters.AddWithValue("@Telefone", string.IsNullOrWhiteSpace(usuario.Telefone) ? (object)DBNull.Value : usuario.Telefone);
            cmd.Parameters.AddWithValue("@Cpf", string.IsNullOrWhiteSpace(usuario.Cpf) ? (object)DBNull.Value : long.Parse(usuario.Cpf));
            cmd.Parameters.AddWithValue("@Cep", string.IsNullOrWhiteSpace(usuario.Cep) ? (object)DBNull.Value : usuario.Cep);
            cmd.Parameters.AddWithValue("@Foto", string.IsNullOrWhiteSpace(usuario.Foto_De_Perfil) ? (object)DBNull.Value : usuario.Foto_De_Perfil);
            cmd.Parameters.AddWithValue("@Nascimento", string.IsNullOrWhiteSpace(usuario.data_de_nascimento) ? (object)DBNull.Value : usuario.data_de_nascimento);
            cmd.Parameters.AddWithValue("@Nome", string.IsNullOrWhiteSpace(usuario.Nome) ? (object)DBNull.Value : usuario.Nome);

            usuario.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Atualizar(int id, UsuarioUpdateDTO usuario, string? status = null)
        {
            var existente = BuscarPorId(id, status);
            if (existente == null || existente.status == "inativo")
            {
               throw new InvalidOperationException("Nenhum usuário encontrado.");
            }

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var emailFinal = string.IsNullOrWhiteSpace(usuario.Email) ? existente.Email : usuario.Email;
            var senhaFinal = string.IsNullOrWhiteSpace(usuario.Senha)
            ? existente.Senha
            : usuario.Senha;


            var statusFinal = string.IsNullOrWhiteSpace(usuario.status) ? existente.status : usuario.status;
            bool adminFinal;

            if (usuario.Admin.ToString() == existente.Admin.ToString())
            {
               
                adminFinal = existente.Admin;
            }
            else
            {

                adminFinal = usuario.Admin.Value ;
            }


            var telefoneFinal = string.IsNullOrWhiteSpace(usuario.Telefone) ? existente.Telefone : usuario.Telefone;
            var cpfFinal = string.IsNullOrWhiteSpace(usuario.Cpf) ? existente.Cpf : usuario.Cpf;
            var cepFinal = string.IsNullOrWhiteSpace(usuario.Cep) ? existente.Cep : usuario.Cep;
            var fotoPerfilFinal = string.IsNullOrWhiteSpace(usuario.Foto_De_Perfil) ? existente.Foto_De_Perfil : usuario.Foto_De_Perfil;
            var dataNascimentoFinal = string.IsNullOrWhiteSpace(usuario.data_de_nascimento) ? existente.data_de_nascimento : usuario.data_de_nascimento;
            var nomeFinal = string.IsNullOrWhiteSpace(usuario.Nome) ? existente.Nome : usuario.Nome;

            string sql = @"
                UPDATE Usuarios SET
                    email=@Email,
                    senha=@Senha,
                    status=@Status,
                    admin=@Admin,
                    telefone=@Telefone,
                    cpf=@Cpf,
                    cep=@Cep,
                    foto_de_perfil=@Foto,
                    data_de_nascimento=@Nascimento,
                    nome=@Nome
                WHERE id=@Id";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }
            using var cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Email", emailFinal);
            cmd.Parameters.AddWithValue("@Senha", senhaFinal);
            cmd.Parameters.AddWithValue("@Status", statusFinal);
            cmd.Parameters.AddWithValue("@Admin", adminFinal);
            cmd.Parameters.AddWithValue("@Telefone", telefoneFinal);
            cmd.Parameters.AddWithValue("@Cpf", cpfFinal);
            cmd.Parameters.AddWithValue("@Cep", cepFinal);
            cmd.Parameters.AddWithValue("@Foto", fotoPerfilFinal);
            cmd.Parameters.AddWithValue("@Nascimento", dataNascimentoFinal);
            cmd.Parameters.AddWithValue("@Nome", nomeFinal);

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
                Cpf = reader.IsDBNull(reader.GetOrdinal("cpf")) ? null : reader.GetInt64("cpf").ToString("D11"),
                Cep = reader.IsDBNull(reader.GetOrdinal("cep")) ? null : reader.GetString("cep"),
                Foto_De_Perfil = reader.IsDBNull(reader.GetOrdinal("foto_de_perfil")) ? null : reader.GetString("foto_de_perfil"),
                data_de_nascimento = reader.IsDBNull(reader.GetOrdinal("data_de_nascimento")) ? null : reader.GetString("data_de_nascimento"),
                Nome = reader.IsDBNull(reader.GetOrdinal("nome")) ? null : reader.GetString("nome")
            };
        }
    }
}
