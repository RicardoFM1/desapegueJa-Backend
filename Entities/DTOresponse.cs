namespace BackendDesapegaJa.Entities
{
    public class DTOresponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool Admin { get; set; } = false;

        public string Nome { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string Data_Nascimento { get; set; } = string.Empty;

        public string Foto_Perfil { get; set; } = string.Empty;
    }
}
