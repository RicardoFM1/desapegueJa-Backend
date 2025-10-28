using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class OrdemDeCompra
    {
        public int id { get; set; }

        [Required(ErrorMessage = "A referência do produto deve ser preenchida.")]
        public int produto_id { get; set; }
        [Required(ErrorMessage = "A referência do usuario deve ser preenchida.")]
        public int usuario_id { get; set; }
        [Required(ErrorMessage = "A referência do status da ordem de compra deve ser preenchida.")]
        public int status_ordem_id { get; set; }
    }
    public class OrdemDeCompraUpdateDTO
    {
        public int? produto_id { get; set; }

        public int? usuario_id { get; set; }

        public int? status_ordem_id { get; set; }
    }
}
