using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class Produto
    {
        public int id { get; set; }

        [Required(ErrorMessage = "O campo 'nome' deve ser preenchido.")]
        public string nome { get; set; }

        [Required(ErrorMessage = "A referência do usuario deve ser preenchida.")]
        public int usuario_id { get; set; }

        [Required(ErrorMessage = "O campo 'preço' deve ser preenchido.")]
        public int preco {  get; set; }
        public string ?descricao { get; set; }
        public string ?data_post { get; set; }
        public string ?status { get; set; }

        [Required(ErrorMessage = "A referência da categoria deve ser preenchida.")]
        public int categoria_id { get; set; }

        public string ?imagem { get; set; }

        public int ?estoque { get; set; }
    }
    public class ProdutoUpdateDTO
    {
        public string? nome { get; set; }
        public int? usuario_id { get; set; }
        public int? preco { get; set; }
        public string? descricao { get; set; }
        public string? data_post { get; set; }
        public string? status { get; set; }
        public int? categoria_id { get; set; }
        public string? imagem { get; set; }
        public int? estoque { get; set; }

    }


}
