using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class StatusDePagamento
    {
        public int id { get; set; }

        [Required(ErrorMessage = "O campo 'descrição' deve ser preenchido.")]
        public string descricao { get; set; }

        public string ?status { get; set; }
    }
    public class StatusDePagamentoUpdateDTO
    {
        public string? descricao { get; set; }

        public string? status { get; set; }
    }
}
