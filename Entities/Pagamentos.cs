using System;
using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class Pagamentos
    {
        public int id { get; set; }

        [Required(ErrorMessage = "A referência do usuário deve ser preenchida")]
        public int usuario_id { get; set; }

        [Required(ErrorMessage = "A referência da forma de pagamento deve ser preenchida")]
        public int forma_pagamento_id { get; set; }

        [Required(ErrorMessage = "A referência do status de pagamento deve ser preenchida")]
        public int status_pagamento_id { get; set; }

        [Required(ErrorMessage = "A referência da ordem deve ser preenchida")]
        public int ordem_id { get; set; }

        public string? observacao { get; set; }
        public int valor { get; set; }

        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }

        public string? pix_qr_code { get; set; }
        public string? pix_copia_codigo { get; set; }
        public string? boleto_url { get; set; }
        public DateTime? expiracao { get; set; }
        public int? valor_pago { get; set; }

        public string? pagamento_uuid { get; set; }


    }

    public class PagamentosUpdateDTO
        {
            public int? usuario_id { get; set; }
            public int? forma_pagamento_id { get; set; }
            public int? status_pagamento_id { get; set; }
            public int? ordem_id { get; set; }
            public string? observacao { get; set; }
            public int? valor { get; set; }
            public DateTime? createdAt { get; set; }
            public DateTime? updatedAt { get; set; }
          
            public string? pix_qr_code { get; set; }
            public string? pix_copia_codigo { get; set; }
            public string? boleto_url { get; set; }
            public DateTime? expiracao { get; set; }
            public int? valor_pago { get; set; }
        public string? pagamento_uuid { get; set; }

    }


}
