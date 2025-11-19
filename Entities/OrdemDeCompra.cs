using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class OrdemDeCompra
    {
        public int id { get; set; }

        [Required(ErrorMessage = "A referência do usuario deve ser preenchida.")]
        public int usuario_id { get; set; }
        [Required(ErrorMessage = "A referência do status da ordem de compra deve ser preenchida.")]
        public int status_ordem_id { get; set; }

        public int valor_total { get; set; }

        public DateTime created_at { get; set; }
    }

    public class OrdemDeCompraCreateDTO
    {
        public int usuario_id { get; set; }
        public int status_ordem_id { get; set; }
        public int valor_total { get; set; }
        public List<OrdemProdutoCreateDTO> itens { get; set; } = new();
    }
    public class OrdemDeCompraUpdateDTO
    {

        public int? usuario_id { get; set; }

        public int? status_ordem_id { get; set; }

        public int? valor_total { get; set; }

        public DateTime created_at { get; set; }
    }
}
