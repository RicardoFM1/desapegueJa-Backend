using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Npgsql;
using System.Data;

namespace BackendDesapegaJa.Repositories
{
    public class EnderecosRepository : IEnderecoRepository
    {
        private readonly string _connectionString;
        private readonly IUsuarioRepository _repoUser;

        public EnderecosRepository(IConfiguration config, IUsuarioRepository repoUser)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _repoUser = repoUser;
        }

        public async Task<IEnumerable<Enderecos>> ListarTodosAsync(string? status = null)
        {
            var enderecos = new List<Enderecos>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM enderecos";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " WHERE status = @status";

            await using var cmd = new NpgsqlCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                enderecos.Add(Mapper(reader));
            }

            return enderecos;
        }

       
        public async Task<Enderecos> ListarAtivoASync(int? usuarioId, string? status = null)
        {
            Enderecos endereco = null;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM enderecos WHERE usuario_id = @usuario_id";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
                endereco = Mapper(reader);

            return endereco;
        }

        
        private async Task DesativarOutrosEnderecosAtivosAsync(int usuarioId, int? enderecoIdExcluir = null)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "UPDATE enderecos SET status = 'inativo' WHERE usuario_id = @usuario_id AND status = 'ativo'";

            if (enderecoIdExcluir.HasValue)
                sql += " AND id != @endereco_id_excluir";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            if (enderecoIdExcluir.HasValue)
                cmd.Parameters.AddWithValue("@endereco_id_excluir", enderecoIdExcluir.Value);

            await cmd.ExecuteNonQueryAsync();
        }   

        public async Task AdicionarAsync(Enderecos enderecos, string? status = null)
        {
            var usuario = await _repoUser.BuscarPorIdAsync(enderecos.usuario_id);

            if (usuario == null)
                throw new InvalidOperationException("Usuário referenciado não encontrado");

            if (!string.Equals(usuario.status, "ativo", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Não é possível adicionar um endereço a um usuário inativo");

            var statusFinal = string.IsNullOrWhiteSpace(enderecos.status) ? "ativo" : enderecos.status;

            if (statusFinal == "ativo")
                DesativarOutrosEnderecosAtivosAsync(enderecos.usuario_id);

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO enderecos 
                (usuario_id, cep, numero, bairro, cidade, estado, rua, tipo_de_endereco, tipo_de_logradouro, complemento, status)
                VALUES
                (@usuario_id, @cep, @numero, @bairro, @cidade, @estado, @rua, @tipo_de_endereco, @tipo_de_logradouro, @complemento, @status)
                RETURNING id;
            ";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@usuario_id", enderecos.usuario_id);
            cmd.Parameters.AddWithValue("@cep", (object?)enderecos.Cep ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@numero", (object?)enderecos.numero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bairro", (object?)enderecos.bairro ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cidade", (object?)enderecos.cidade ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@estado", (object?)enderecos.estado ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rua", (object?)enderecos.rua ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tipo_de_endereco", (object?)enderecos.tipo_de_endereco ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tipo_de_logradouro", (object?)enderecos.tipo_de_logradouro ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@complemento", (object?)enderecos.complemento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", statusFinal);

            enderecos.id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public async Task<Enderecos?> BuscarPorIdAsync(int? id, string? status = null)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM enderecos WHERE id = @id";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

           await using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
                return Mapper(reader);

            return null;
        }

        public async Task<Enderecos?> BuscarPorUsuarioIdAtivoAsync(int? id, string? status = null)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM enderecos WHERE usuario_id = @id AND status = 'ativo'";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
                return Mapper(reader);

            return null;
        }

        public async Task<IEnumerable<Enderecos?>> BuscarPorUsuarioIdAsync(int? id, string? status = null)
        {
            var lista = new List<Enderecos>();

           await using var connection = new NpgsqlConnection(_connectionString);
           await connection.OpenAsync();

            string sql = "SELECT * FROM enderecos WHERE usuario_id = @id";

            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND status = @status";

           await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@status", status);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
                lista.Add(Mapper(reader));

            return lista;
        }
        private object DbOr(object? value)
        {
            return value ?? DBNull.Value;
        }

        public async Task AtualizarPorIdAsync(int id, EnderecosUpdateDTO enderecos, string? status = null)
        {
            var atual = await BuscarPorIdAsync(id);
            if (atual == null)
                throw new InvalidOperationException("Nenhum endereço encontrado com esse ID.");

            var statusFinal = string.IsNullOrWhiteSpace(enderecos.status) ? atual.status : enderecos.status;

            if (statusFinal == "ativo")
                DesativarOutrosEnderecosAtivosAsync(atual.usuario_id, id);

            string sql =
                @"UPDATE enderecos SET 
                    cep = @cep,
                    numero = @numero,
                    bairro = @bairro,
                    cidade = @cidade,
                    estado = @estado,
                    rua = @rua,
                    tipo_de_endereco = @tipo_de_endereco,
                    tipo_de_logradouro = @tipo_de_logradouro,
                    complemento = @complemento,
                    status = @status
                  WHERE id = @id";

           await  using var connection = new NpgsqlConnection(_connectionString);
           await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.Parameters.AddWithValue("@cep", DbOr(enderecos.Cep ?? atual.Cep));
            cmd.Parameters.AddWithValue("@numero", DbOr(enderecos.numero ?? atual.numero));
            cmd.Parameters.AddWithValue("@bairro", DbOr(enderecos.bairro ?? atual.bairro));
            cmd.Parameters.AddWithValue("@cidade", DbOr(enderecos.cidade ?? atual.cidade));
            cmd.Parameters.AddWithValue("@estado", DbOr(enderecos.estado ?? atual.estado));
            cmd.Parameters.AddWithValue("@rua", DbOr(enderecos.rua ?? atual.rua));
            cmd.Parameters.AddWithValue("@tipo_de_endereco", DbOr(enderecos.tipo_de_endereco ?? atual.tipo_de_endereco));
            cmd.Parameters.AddWithValue("@tipo_de_logradouro", DbOr(enderecos.tipo_de_logradouro ?? atual.tipo_de_logradouro));
            cmd.Parameters.AddWithValue("@complemento", DbOr(enderecos.complemento ?? atual.complemento));
            cmd.Parameters.AddWithValue("@status", DbOr(statusFinal));


            await cmd.ExecuteNonQueryAsync();
        }

       
        private Enderecos Mapper(NpgsqlDataReader reader)
        {
            return new Enderecos
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                usuario_id = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                Cep = reader["cep"] as string,
                bairro = reader["bairro"] as string,
                cidade = reader["cidade"] as string,
                estado = reader["estado"] as string,
                rua = reader["rua"] as string,
                numero = reader["numero"] as string,
                complemento = reader["complemento"] as string,
                tipo_de_endereco = reader["tipo_de_endereco"] as string,
                tipo_de_logradouro = reader["tipo_de_logradouro"] as string,
                status = reader["status"] as string
            };
        }
    }
}
