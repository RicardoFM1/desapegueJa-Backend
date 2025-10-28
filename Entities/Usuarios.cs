using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo 'email' deve ser preenchido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo 'senha' deve ser preenchido.")]
        public string Senha { get; set; } = string.Empty;

        public string? Token { get; set; }

        public bool Admin { get; set; } = false;

        public string ?Telefone { get; set; }

        public int ?Rg { get; set; }

        public string ?Foto_De_Perfil { get; set; }

        public string ?data_de_nascimento { get; set; }

        [Required(ErrorMessage = "O campo 'cpf' deve ser preenchido.")]
        public string Cpf { get; set; }

        public string ?Cep { get; set; }

        public string ?status { get; set; }
    }
    public class UsuarioUpdateDTO
    {
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public string? Telefone { get; set; }
        public bool? Admin { get; set; } 
        public string? Foto_De_Perfil { get; set; }
        public string? data_de_nascimento { get; set; }
        public string? status { get; set; }
        public int? Rg { get; set; }
        public string? Cep { get; set; }
        public string? Cpf { get; set; }
    }
}
