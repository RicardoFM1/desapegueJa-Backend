using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class OrdemDeCompra
    {
        public int id { get; set; }
        public int usuario_id { get; set; }
        public int status_ordem_id { get; set; }
        public int valor_total { get; set; }
        public DateTime created_at { get; set; }

      
        public string metodo_entrega { get; set; }
    }


    public class OrdemDeCompraCreateDTO
    {
        public int id { get; set; }
        public int usuario_id { get; set; }
        public int status_ordem_id { get; set; }
        public int valor_total { get; set; }
        public string metodo_entrega { get; set; }
        public List<OrdemProdutoCreateDTO> itens { get; set; } = new();

        public DateTime created_at { get; set; }
    }

    public class OrdemDeCompraUpdateDTO
    {

        public int? usuario_id { get; set; }

        public int? status_ordem_id { get; set; }

        public int? valor_total { get; set; }

        public string? metodo_entrega { get; set; }

        public DateTime created_at { get; set; }
    }
}
