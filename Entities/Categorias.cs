using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class Categorias
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo 'nome' deve ser preenchido.")]
        public string Nome { get; set; }

      
        public string ?status { get; set; }
    }
    public class CategoriasUpdateDTO
    {
        public string? Nome { get; set; }
        public string? status { get; set; }
    }
}
