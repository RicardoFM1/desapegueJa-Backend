using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class Pagamentos
    {
        public int id { get; set; }

        [Required(ErrorMessage = "A referência do usuário deve ser preenchida")]
        public int usuario_id { get; set; }

        [Required(ErrorMessage = "A referência da forma de pagamento deve ser preenchida")]
        public int formas_de_pagamento_id { get; set; }

        [Required(ErrorMessage = "A referência do status de pagamento deve ser preenchida")]
        public int status_de_pagamento_id { get; set; }

        public string? observacao { get; set; }

        
        public string? createdAt { get; set; }

        public string? updatedAt { get; set; }

        public string? status { get; set; }
    }
    public class PagamentosUpdateDTO
    {
        public int? usuario_id { get; set; }
        public int? formas_de_pagamento_id { get; set; }
        public int? status_de_pagamento_id { get; set; }
        public string? observacao { get; set; }
        public string? createdAt { get; set; }
        public string? updatedAt { get; set; }
        public string? status { get; set; }
    }
}
