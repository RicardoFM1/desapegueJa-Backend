using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class Pagamentos
    {
        public int id { get; set; }

        [Required(ErrorMessage = "A referência do usuário deve ser preenchida")]
        public int usuario_id { get; set; }

        [Required(ErrorMessage = "A forma de pagamento deve ser preenchida")]
        public int forma_pagamento_id { get; set; }

        [Required(ErrorMessage = "O status deve ser preenchido")]
        public int status_pagamento_id { get; set; }

        [Required(ErrorMessage = "A ordem deve ser preenchida")]
        public int? ordem_id { get; set; }

        public string? observacao { get; set; }
        public int valor { get; set; }

        public string? card_token { get; set; }

        public int? parcelas { get; set; }

        public string? payment_method_id { get; set; }
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }



        public string? pagamento_uuid { get; set; }

        public string? pix_qr_code { get; set; }
        public string? pix_copia_codigo { get; set; }

      
        public string? boleto_url { get; set; }

        
        public DateTime? expiracao { get; set; }

      
        public int? valor_pago { get; set; }
    }


    public class PagamentosUpdateDTO
    {
        public int? usuario_id { get; set; }
        public int? forma_pagamento_id { get; set; }
        public int? status_pagamento_id { get; set; }
        public int? ordem_id { get; set; }
        public string? observacao { get; set; }
        public int? valor { get; set; }

        public string? pix_qr_code { get; set; }
        public string? pix_copia_codigo { get; set; }
        public string? boleto_url { get; set; }
        public DateTime? expiracao { get; set; }
        public int? valor_pago { get; set; }

        public string? card_token { get; set; }

        public string? payment_method_id { get; set; }

        public int? parcelas { get; set; }
        public string? pagamento_uuid { get; set; }
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
    }



    public class MercadoPagoReturn
    {
        public string? PaymentId { get; set; }
        public string? PreferenceId { get; set; }
        public string? Status { get; set; }
        public string? PixQrCodeBase64 { get; set; }
        public string? PixCopiaCola { get; set; }
        public string? BoletoUrl { get; set; }

        public string? card_token { get; set; }
        public int? parcelas { get; set; }
        public DateTime? Expiracao { get; set; }
        public int? AmountPaid { get; set; }
    }

    public class PagamentoRetornoApi
    {
        public string TransacaoIdExterno { get; set; }
        public string PixCopiaCodigo { get; set; }
        public string PixQrCodeBase64 { get; set; }

        public int ValorPago { get; set; }

        public string BoletoURL { get; set; }
        public DateTime Expiracao { get; set; }
    }




    public class MercadoPagoWebhook
    {
        public long id { get; set; }
        public string? type { get; set; }
        public string? action { get; set; }
        public MercadoPagoWebhookData? data { get; set; }
    }

    public class MercadoPagoWebhookData
    {
        public string? id { get; set; }
    }

    public class MercadoPagoPagamento
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("payer")]
        public MercadoPagoPayer Payer { get; set; }
    }

    public class MercadoPagoPayer
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class PaymentPixResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("point_of_interaction")]
        public PointOfInteraction PointOfInteraction { get; set; }
    }

    public class PointOfInteraction
    {
        [JsonProperty("transaction_data")]
        public TransactionData TransactionData { get; set; }
    }

    public class TransactionData
    {
        [JsonProperty("qr_code")]
        public string QrCode { get; set; }

        [JsonProperty("qr_code_base64")]
        public string QrCodeBase64 { get; set; }
    }

    public enum StatusPagamento
    {
        pendente = 1,
        pago = 2,
        rejeitado = 3,
        erro = 4,
        expirado = 5
    }
}
