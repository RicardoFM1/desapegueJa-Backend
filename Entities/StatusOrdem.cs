using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class StatusOrdem
    {
        public int id { get; set; }

        [Required(ErrorMessage = "O campo 'descrição' deve ser preenchido.")]
        public string descricao { get; set; }

        public string ?status { get; set; }
    }
    public class StatusOrdemUpdateDTO
    {
        public string? descricao { get; set; }

        public string? status { get; set; }
    }
}
