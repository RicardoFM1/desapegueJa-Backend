using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class Carrinho
    {
        public int id { get; set; }

        [Required(ErrorMessage = "A referência do usuário deve ser preenchida")]
        public int usuario_id { get; set; }

        [Required(ErrorMessage = "A referência do produto deve ser preenchida")]
        public int produto_id { get; set;}

        [Required(ErrorMessage = "A quantidade deve ser informada")]
        public int quantidade { get; set; }
    }
    public class CarrinhoUpdateDTO
    {
        public int? usuario_id { get; set; }
        public int? produto_id { get; set; }

        public int? quantidade { get; set; }
    }
}
