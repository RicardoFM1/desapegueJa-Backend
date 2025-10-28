using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class FormasDePagamento
    {
        public int id { get; set; }

        [Required(ErrorMessage = "O campo 'forma' deve ser preenchido.")]
        public string forma { get; set; }

       
        public string ?status { get; set; }
    }
    public class FormasDePagamentoUpdateDTO
    {
        public string? forma { get; set; }
        public string? status { get; set; }
    }
}
