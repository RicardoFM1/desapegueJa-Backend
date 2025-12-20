using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT id, nome, email, senha, status, admin, telefone, cpf, foto_de_perfil, 
                       data_de_nascimento, google_id 
                FROM usuarios";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " WHERE status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                usuarios.Add(MapUsuario(reader));

            return usuarios;
        }

        public Usuario? BuscarPorNome(string nome, string? status = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT id, nome, email, senha, status, admin, telefone, cpf, foto_de_perfil, 
                       data_de_nascimento, google_id 
                FROM usuarios 
                WHERE LOWER(nome)=LOWER(@nome)";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@nome", nome.Trim());

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();

            return reader.Read() ? MapUsuario(reader) : null;
        }

        public Usuario? BuscarPorEmail(string email, string? status = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT id, nome, email, senha, status, admin, telefone, cpf, foto_de_perfil, 
                       data_de_nascimento, google_id 
                FROM usuarios 
                WHERE LOWER(email)=LOWER(@email)";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@email", email.Trim());

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();

            return reader.Read() ? MapUsuario(reader) : null;
        }

        public Usuario? BuscarPorId(int? id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT id, nome, email, senha, status, admin, telefone, cpf, foto_de_perfil, 
                       data_de_nascimento, google_id 
                FROM usuarios 
                WHERE id=@id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            return reader.Read() ? MapUsuario(reader) : null;
        }

        public Usuario? BuscarPorCpf(string cpf, string? status = null)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return null;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT id, nome, email, senha, status, admin, telefone, cpf, foto_de_perfil, 
                       data_de_nascimento, google_id 
                FROM usuarios 
                WHERE cpf=@cpf";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@cpf", cpf);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = cmd.ExecuteReader();

            return reader.Read() ? MapUsuario(reader) : null;
        }

        public async Task<Dictionary<int, string>> BuscarCepsPorIdsAsync(IEnumerable<int> usuariosIds)
        {
            var ceps = new Dictionary<int, string>();

            if (usuariosIds == null || !usuariosIds.Any())
            {
                return ceps;
            }
 
            string ids = string.Join(", ", usuariosIds);

            
            string sql = $@"
            SELECT id, cep
            FROM usuarios
            WHERE id IN ({ids});"; 

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, connection);
            

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32(reader.GetOrdinal("id"));
                string cep = reader.IsDBNull(reader.GetOrdinal("cep")) ? string.Empty : reader.GetString(reader.GetOrdinal("cep"));

                ceps.Add(id, cep);
            }

            return ceps;
        }

        public void Adicionar(Usuario usuario)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO usuarios 
                    (email, senha, status, admin, telefone, cpf, foto_de_perfil, 
                     data_de_nascimento, nome, google_id)
                VALUES 
                    (@Email, @Senha, @Status, @Admin, @Telefone, @Cpf, @Foto,
                     @Nascimento, @Nome, @GoogleId)
                RETURNING id;", connection);

            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@Senha", usuario.Senha ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(usuario.status) ? "ativo" : usuario.status);
            cmd.Parameters.AddWithValue("@Admin", usuario.Admin);
            cmd.Parameters.AddWithValue("@Telefone", (object?)usuario.Telefone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Cpf", (object?)usuario.Cpf ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Foto", (object?)usuario.Foto_De_Perfil ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nascimento", (object?)usuario.data_de_nascimento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nome", (object?)usuario.Nome ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GoogleId", (object?)usuario.GoogleId ?? DBNull.Value);

            usuario.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Atualizar(int id, UsuarioUpdateDTO usuario, string? statusQuery = null)
        {
            var existente = BuscarPorId(id);
            if (existente == null)
                throw new InvalidOperationException("Nenhum usuário encontrado.");

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var emailFinal = usuario.Email ?? existente.Email;
            var senhaFinal = usuario.Senha ?? existente.Senha;
            var statusFinal = usuario.status ?? existente.status;
            var adminFinal = usuario.Admin ?? existente.Admin;
            var telefoneFinal = usuario.Telefone ?? existente.Telefone;
            var cpfFinal = usuario.Cpf ?? existente.Cpf;
            var fotoFinal = usuario.Foto_De_Perfil ?? existente.Foto_De_Perfil;
            var nascimentoFinal = usuario.data_de_nascimento ?? existente.data_de_nascimento;
            var nomeFinal = usuario.Nome ?? existente.Nome;
            var googleIdFinal = usuario.GoogleId ?? existente.GoogleId;

            string sql = @"
                UPDATE usuarios SET
                    email=@Email,
                    senha=@Senha,
                    status=@Status,
                    admin=@Admin,
                    telefone=@Telefone,
                    cpf=@Cpf,
                    foto_de_perfil=@Foto,
                    data_de_nascimento=@Nascimento,
                    nome=@Nome,
                    google_id=@GoogleId
                WHERE id=@Id";

            if (!string.IsNullOrWhiteSpace(statusQuery))
                sql += " AND status = @statusQuery";

            using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Email", emailFinal);
            cmd.Parameters.AddWithValue("@Senha", senhaFinal);
            cmd.Parameters.AddWithValue("@Status", statusFinal);
            cmd.Parameters.AddWithValue("@Admin", adminFinal);
            cmd.Parameters.AddWithValue("@Telefone", (object?)telefoneFinal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Cpf", (object?)cpfFinal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Foto", (object?)fotoFinal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nascimento", (object?)nascimentoFinal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nome", (object?)nomeFinal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GoogleId", (object?)googleIdFinal ?? DBNull.Value);

            if (!string.IsNullOrWhiteSpace(statusQuery))
                cmd.Parameters.AddWithValue("@statusQuery", statusQuery);

            cmd.ExecuteNonQuery();
        }

        private Usuario MapUsuario(NpgsqlDataReader reader)
        {
            return new Usuario
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Senha = reader.GetString(reader.GetOrdinal("senha")),
                status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                Admin = reader.IsDBNull(reader.GetOrdinal("admin")) ? false : reader.GetBoolean(reader.GetOrdinal("admin")),
                Telefone = reader.IsDBNull(reader.GetOrdinal("telefone")) ? null : reader.GetString(reader.GetOrdinal("telefone")),
                Cpf = reader.IsDBNull(reader.GetOrdinal("cpf")) ? null : reader.GetString(reader.GetOrdinal("cpf")),
                Foto_De_Perfil = reader.IsDBNull(reader.GetOrdinal("foto_de_perfil")) ? null : reader.GetString(reader.GetOrdinal("foto_de_perfil")),
                data_de_nascimento = reader.IsDBNull(reader.GetOrdinal("data_de_nascimento")) ? null : reader.GetString(reader.GetOrdinal("data_de_nascimento")),
                Nome = reader.IsDBNull(reader.GetOrdinal("nome")) ? null : reader.GetString(reader.GetOrdinal("nome")),
                GoogleId = reader.IsDBNull(reader.GetOrdinal("google_id")) ? null : reader.GetString(reader.GetOrdinal("google_id"))
            };
        }

    

        public async Task<Usuario?> BuscarPorEmailAsync(string email, string? status = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                SELECT id, nome, email, senha, status, admin, telefone, cpf, foto_de_perfil, 
                       data_de_nascimento, google_id 
                FROM usuarios 
                WHERE LOWER(email)=LOWER(@email)";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@email", email.Trim());

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapUsuario(reader) : null;
        }

        public async Task<Usuario?> AdicionarAsync(Usuario usuario)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO usuarios 
                    (email, senha, status, admin, telefone, cpf, foto_de_perfil,
                     data_de_nascimento, nome, google_id)
                VALUES 
                    (@Email, @Senha, @Status, @Admin, @Telefone, @Cpf, @Foto,
                     @Nascimento, @Nome, @GoogleId)
                RETURNING id;", connection);

            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@Senha", (object?)usuario.Senha ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(usuario.status) ? "ativo" : usuario.status);
            cmd.Parameters.AddWithValue("@Admin", usuario.Admin);
            cmd.Parameters.AddWithValue("@Telefone", (object?)usuario.Telefone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Cpf", (object?)usuario.Cpf ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Foto", (object?)usuario.Foto_De_Perfil ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nascimento", (object?)usuario.data_de_nascimento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nome", (object?)usuario.Nome ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GoogleId", (object?)usuario.GoogleId ?? DBNull.Value);

            usuario.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return usuario;
        }
    }
}
