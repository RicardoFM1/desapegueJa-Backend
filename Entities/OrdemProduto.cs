using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class OrdemProduto
    {
        public int id { get; set; }

        [Required(ErrorMessage = "A referência da ordem de compra deve ser preenchida.")]
        public int ordem_id { get; set; }

        [Required(ErrorMessage = "A referência do produto deve ser preenchida.")]
        public int produto_id { get; set; }

        [Required(ErrorMessage = "A quantidade deve ser preenchida.")]
        public int quantidade { get; set; }

        [Required(ErrorMessage = "O preço unitário deve ser informado.")]
        public int preco_unitario { get; set; }

        public int usuario_vendedor_id { get; set; }
    }

    public class OrdemProdutoCreateDTO
    {
        [Required(ErrorMessage = "A referência da ordem de compra deve ser preenchida.")]
        public int ordem_id { get; set; }

        [Required(ErrorMessage = "A referência do produto deve ser preenchida.")]
        public int produto_id { get; set; }

        [Required(ErrorMessage = "A quantidade deve ser preenchida.")]
        public int quantidade { get; set; }

        [Required(ErrorMessage = "O preço unitário deve ser informado.")]
        public int preco_unitario { get; set; }

        public int usuario_vendedor_id { get; set; }
    }

    public class OrdemProdutoUpdateDTO
    {
        public int? ordem_id { get; set; }

        public int? produto_id { get; set; }

        public int? quantidade { get; set; }

        public int? preco_unitario { get; set; }
    }
}
