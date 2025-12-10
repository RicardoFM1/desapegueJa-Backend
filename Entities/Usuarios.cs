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

        public bool Admin { get; set; } = false;

        public string ?Telefone { get; set; }

        public string ?Foto_De_Perfil { get; set; }

        public string ?data_de_nascimento { get; set; }

        [Required(ErrorMessage = "O campo 'cpf' deve ser preenchido.")]
        public string Cpf { get; set; }

 

        public string ?status { get; set; }

        [Required(ErrorMessage = "O campo 'nome' deve ser preenchido.")]
        public string Nome { get; set; }

        public string? GoogleId { get; set; }
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
       
        public string? Cpf { get; set; }
        public string ?Nome { get; set; }

        public string? GoogleId { get; set; }
    }
    public class CompletarCadastroDTO
    {

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "O Telefone é obrigatório.")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "A Data de Nascimento é obrigatória.")]
        public string DataDeNascimento { get; set; }

        
        [Required(ErrorMessage = "O CEP é obrigatório.")]
        public string Cep { get; set; }

       
        [Required(ErrorMessage = "O Número do endereço é obrigatório.")]
        public string Numero { get; set; }

        public string? Rua { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
    }
}
