using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class LoginDto
    {
        [Required(ErrorMessage = "O campo 'email' deve ser preenchido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo 'senha' deve ser preenchido.")]
        public string Senha { get; set; } = string.Empty;



    }
}
