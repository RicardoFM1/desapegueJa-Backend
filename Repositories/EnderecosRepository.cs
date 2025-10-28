using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;

namespace BackendDesapegaJa.Repositories
{
    public class EnderecosRepository : IEnderecoRepository
    {
        private readonly MySqlConnection _connection;
        private readonly IUsuarioRepository _repoUser;

        public EnderecosRepository(MySqlConnection connection, IUsuarioRepository repoUser)
        {
            _connection = connection;
            _repoUser = repoUser;
        }
        public IEnumerable<Enderecos> ListarTodos(string? status = null)
        {
            var enderecos = new List<Enderecos>();

            _connection.Open();

            string sql = "SELECT * FROM enderecos";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }

            var cmd = new MySqlCommand(sql, _connection);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var endereco = new Enderecos
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    bairro = reader.IsDBNull(reader.GetOrdinal("bairro")) ? null : reader.GetString("bairro"),
                    cidade = reader.IsDBNull(reader.GetOrdinal("cidade")) ? null : reader.GetString("cidade"),
                    estado = reader.IsDBNull(reader.GetOrdinal("estado")) ? null : reader.GetString("estado"),
                    rua = reader.IsDBNull(reader.GetOrdinal("rua")) ? null : reader.GetString("rua"),
                    numero = reader.IsDBNull(reader.GetOrdinal("numero")) ? 0 : reader.GetInt32("numero"),
                    complemento = reader.IsDBNull(reader.GetOrdinal("complemento")) ? null : reader.GetString("complemento"),
                    tipo_de_logradouro = reader.IsDBNull(reader.GetOrdinal("tipo_de_logradouro")) ? null : reader.GetString("tipo_de_logradouro"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };

                enderecos.Add(endereco);
            }

            _connection.Close();
            return enderecos;
        }
        public void Adicionar(Enderecos enderecos, string? status = null)
        {
            var usuarioExistente = _repoUser.BuscarPorId(enderecos.usuario_id);

            if (!string.Equals(usuarioExistente.status, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Não é possível adicionar um endereço a um usuário que não está ativo");
            }

            if (usuarioExistente == null)
            {
                throw new InvalidOperationException("Usuario referenciado não encontrado");
            }
            _connection.Open();
            var cmd = new MySqlCommand("INSERT INTO enderecos (usuario_id, numero, bairro, cidade, estado, rua, tipo_de_logradouro, complemento, status) " +
                "VALUES(@usuario_id, @numero, @bairro, @cidade, @estado, @rua, @tipo_de_logradouro, @complemento, @status); SELECT LAST_INSERT_ID();", _connection);
            cmd.Parameters.AddWithValue("@usuario_id", enderecos.usuario_id);
            cmd.Parameters.AddWithValue("@numero", enderecos.numero);
            cmd.Parameters.AddWithValue("@bairro", enderecos.bairro);
            cmd.Parameters.AddWithValue("@estado", enderecos.estado);
            cmd.Parameters.AddWithValue("@rua", enderecos.rua);
            cmd.Parameters.AddWithValue("@cidade", enderecos.cidade);
            cmd.Parameters.AddWithValue("@tipo_de_logradouro", enderecos.tipo_de_logradouro);
            cmd.Parameters.AddWithValue("@complemento", enderecos.complemento);
            cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(enderecos.status) ? "ativo" : enderecos.status);

            int novoId = Convert.ToInt32(cmd.ExecuteScalar());
            enderecos.id = novoId;


            _connection.Close();

        }

        public Enderecos? BuscarPorId(int? id, string? status = null)
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            string sql = "SELECT * FROM enderecos WHERE id = @id";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @status";
            }
            using var cmd = new MySqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@id", id);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }
            var reader = cmd.ExecuteReader();
            Enderecos? enderecos = null;
            if (reader.Read())
            {
                enderecos = new Enderecos
                {
                    id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                    usuario_id = reader.IsDBNull(reader.GetOrdinal("usuario_id")) ? 0 : reader.GetInt32("usuario_id"),
                    bairro = reader.IsDBNull(reader.GetOrdinal("bairro")) ? null : reader.GetString("bairro"),
                    cidade = reader.IsDBNull(reader.GetOrdinal("cidade")) ? null : reader.GetString("cidade"),
                    estado = reader.IsDBNull(reader.GetOrdinal("estado")) ? null : reader.GetString("estado"),
                    rua = reader.IsDBNull(reader.GetOrdinal("rua")) ? null : reader.GetString("rua"),
                    numero = reader.IsDBNull(reader.GetOrdinal("numero")) ? 0 : reader.GetInt32("numero"),
                    complemento = reader.IsDBNull(reader.GetOrdinal("complemento")) ? null : reader.GetString("complemento"),
                    tipo_de_logradouro = reader.IsDBNull(reader.GetOrdinal("tipo_de_logradouro")) ? null : reader.GetString("tipo_de_logradouro"),
                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
                };


            }
            reader.Close();
            _connection.Close();
            return enderecos;

        }
        public void Atualizar(int id, EnderecosUpdateDTO enderecos, string? status = null)
        {
            var usuarioExistente = _repoUser.BuscarPorId(enderecos.usuario_id);
            var enderecoExistente = BuscarPorId(id);
            if (usuarioExistente == null)
            {
                throw new InvalidOperationException("Nenhum usuario encontrado");
            }

            if(!string.Equals(usuarioExistente.status, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Não é possível atualizar um endereço de um usuario inativo");
            }
            if (!string.Equals(enderecoExistente.status, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Não é possível atualizar um endereço inativo");
            }
            var cidadeFinal = string.IsNullOrWhiteSpace(enderecos.cidade) ? enderecoExistente.cidade : enderecos.cidade;
            var usuarioIdFinal = enderecos.usuario_id ?? enderecoExistente.usuario_id;
            var estadoFinal = string.IsNullOrWhiteSpace(enderecos.estado) ? enderecoExistente.estado : enderecos.estado;
            var bairroFinal = string.IsNullOrWhiteSpace(enderecos.bairro) ? enderecoExistente.bairro : enderecos.bairro;
            var ruaFinal = string.IsNullOrWhiteSpace(enderecos.rua) ? enderecoExistente.rua : enderecos.rua;
            var logradouroFinal = string.IsNullOrWhiteSpace(enderecos.tipo_de_logradouro) ? enderecoExistente.tipo_de_logradouro : enderecos.tipo_de_logradouro;
            var complementoFinal = string.IsNullOrWhiteSpace(enderecos.complemento) ? enderecoExistente.complemento : enderecos.complemento;
            var numeroFinal = enderecos.numero.HasValue ? enderecos.numero.Value : enderecoExistente.numero;
            var statusFinal = string.IsNullOrWhiteSpace(enderecoExistente.status) ? enderecoExistente.status : enderecos.status;

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            var cmd = new MySqlCommand("UPDATE enderecos SET usuario_id = @usuario_id," +
                "numero = @numero, bairro = @bairro, cidade = @cidade, estado = @estado, rua = @rua, tipo_de_logradouro = @tipo_de_logradouro, complemento = @complemento, status = @status WHERE id = @id", _connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioIdFinal);
            cmd.Parameters.AddWithValue("@numero", numeroFinal);
            cmd.Parameters.AddWithValue("@bairro", bairroFinal);
            cmd.Parameters.AddWithValue("@estado", estadoFinal);
            cmd.Parameters.AddWithValue("@rua", ruaFinal);
            cmd.Parameters.AddWithValue("@cidade", cidadeFinal);
            cmd.Parameters.AddWithValue("@tipo_de_logradouro", logradouroFinal);
            cmd.Parameters.AddWithValue("@complemento", complementoFinal);
            cmd.Parameters.AddWithValue("@status", statusFinal);
            cmd.ExecuteNonQuery();
            _connection.Close();
        }

    }
}
