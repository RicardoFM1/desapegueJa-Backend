using BackendDesapegaJa.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json; 

namespace BackendDesapegaJa.Helpers
{
    
    public class PagamentoRetornoApi
    {
        public string? TransacaoIdExterno { get; set; }
        public string? PixCopiaCodigo { get; set; }
        public string? PixQrCodeBase64 { get; set; }
        public string? BoletoUrl { get; set; }
        public DateTime? Expiracao { get; set; }
    }

   
    public class PagSeguroPixResponseDto
    {
        public string? IdTransacaoPagSeguro { get; set; }
        public NestedPagamentos Pagamentos { get; set; }
    }

    
    public class NestedPagamentos
    {
        public NestedPix Pix { get; set; }
    }
    public class NestedPix
    {
        public string Payload { get; set; }
        public string QrCodeBase64 { get; set; }
        public DateTime Expiracao { get; set; }
    }


    public class PagSeguroIntegration
    {
        private readonly HttpClient _httpClient;
        private readonly string _pagSeguroAuthToken;
        private readonly string _webhookToken;

        public PagSeguroIntegration(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;

            _pagSeguroAuthToken = config["PagSeguro:AuthToken"] ?? throw new InvalidOperationException("Token PagSeguro não configurado.");
            _webhookToken = config["PagSeguro:WebhookToken"] ?? throw new InvalidOperationException("Webhook Token não configurado.");

          
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_pagSeguroAuthToken}");
            _httpClient.BaseAddress = new Uri(config["PagSeguro:ApiUrl"]!);
        }

   
        public async Task<PagamentoRetornoApi> CriarCobrancaPixAsync(OrdemDeCompra ordem, Usuario usuario)
        {
          
            decimal valorEmReais = (decimal)ordem.valor_total / 100m;
            int valorEmCentavos = (int)(valorEmReais * 100);

           
            var requestPayload = new
            {
                reference_id = $"ORDEM-{ordem.id}",
                amount = new { value = valorEmCentavos },

                notification_url = "https://elina-unrabbinical-consuelo.ngrok-free.dev/desapega/pagamentos/webhook",

               
                customer = new
                {
                    name = usuario.Nome,
                    email = usuario.Email,
                    tax_id = new
                    {
                        type = "CPF",
                        // O CPF deve ser apenas números!
                        value = usuario.Cpf.Replace(".", "").Replace("-", "").Replace("/", "")
                    }
                }
            };

        
            var response = await _httpClient.PostAsJsonAsync("charges/pix", requestPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro PagSeguro {response.StatusCode}: {errorContent}");

               
                throw new InvalidOperationException($"Erro na comunicação com o PagSeguro ({response.StatusCode}): {errorContent}");
            }

           
            var content = await response.Content.ReadFromJsonAsync<PagSeguroPixResponseDto>()
                ?? throw new InvalidOperationException("Resposta da API PagSeguro Pix inválida.");

            
            return new PagamentoRetornoApi
            {
                TransacaoIdExterno = content.IdTransacaoPagSeguro,
                PixCopiaCodigo = content.Pagamentos?.Pix?.Payload,
                PixQrCodeBase64 = content.Pagamentos?.Pix?.QrCodeBase64,
                Expiracao = content.Pagamentos?.Pix?.Expiracao,
            };
        }


        public async Task<PagamentoRetornoApi> CriarCobrancaBoletoAsync(OrdemDeCompra ordem, Usuario usuario)
        {
            await Task.Delay(1);
            throw new NotImplementedException("A integração de Boleto ainda não foi implementada.");
        }

        
        public bool ValidateWebhookToken(string token)
        {
            return token == _webhookToken;
        }
    }
}