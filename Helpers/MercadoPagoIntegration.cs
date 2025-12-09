using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BackendDesapegaJa.Helpers
{
    public class MercadoPagoIntegration
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _webhookUrl;
        private readonly IOrdemDeCompraRepository _repoOrdem;
        private readonly IPagamentosRepository _repoPagamentos;

        public MercadoPagoIntegration(IConfiguration config, HttpClient httpClient, IOrdemDeCompraRepository repoOrdem, IPagamentosRepository repoPagamentos)
        {
            _httpClient = httpClient;
            _accessToken = config["MercadoPago:AccessToken"]
                ?? throw new InvalidOperationException("AccessToken do Mercado Pago não configurado.");

            _webhookUrl = config["MercadoPago:WebhookUrl"]
                ?? throw new InvalidOperationException("WebhookUrl do Mercado Pago não configurado.");

            _httpClient.BaseAddress = new Uri("https://api.mercadopago.com/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _repoOrdem = repoOrdem;
            _repoPagamentos = repoPagamentos;
        }

        public async Task<PagamentoRetornoApi> CriarCobrancaPixAsync(OrdemDeCompra ordem, Usuario usuario, string uuidExterno)
        {
            var payload = new
            {
                transaction_amount = (decimal)ordem.valor_total / 100m,
                description = $"ORDEM-{ordem.id}",
                payment_method_id = "pix",
                payer = new
                {
                    email = usuario.Email,
                    first_name = usuario.Nome,
                    identification = new
                    {
                        type = "CPF",
                        number = usuario.Cpf.Replace(".", "").Replace("-", "")
                    }
                },
                notification_url = _webhookUrl,

               
                external_reference = uuidExterno
            };

            var idempotencyKey = Guid.NewGuid().ToString();
            _httpClient.DefaultRequestHeaders.Remove("X-Idempotency-Key");
            _httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey);

            var response = await _httpClient.PostAsJsonAsync("v1/payments", payload);
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _repoPagamentos.DeletarPorUsuarioId(ordem.usuario_id);
                _repoOrdem.DeletarOrdemEmAbertoPorOrdemId(ordem.id);
                throw new Exception($"Erro Mercado Pago: {response.StatusCode} - {jsonString}");
            }


            var paymentResponse = JsonConvert.DeserializeObject<PaymentPixResponse>(jsonString);

            if (paymentResponse == null)
            {
                _repoPagamentos.DeletarPorUsuarioId(ordem.usuario_id);
                _repoOrdem.DeletarOrdemEmAbertoPorOrdemId(ordem.id);
                throw new InvalidOperationException("Resposta do Mercado Pago veio nula ao deserializar.");
            }


            if (paymentResponse.PointOfInteraction?.TransactionData == null)
            {
                Console.WriteLine($"ERRO JSON MP: {jsonString}");
                _repoPagamentos.DeletarPorUsuarioId(ordem.usuario_id);
                _repoOrdem.DeletarOrdemEmAbertoPorOrdemId(ordem.id);
                throw new InvalidOperationException("Mercado Pago não retornou dados do QR Code.");
            }

            var pixInfo = paymentResponse.PointOfInteraction.TransactionData;

            return new PagamentoRetornoApi
            {
                TransacaoIdExterno = paymentResponse.Id.ToString(),
                PixCopiaCodigo = pixInfo.QrCode,
                PixQrCodeBase64 = pixInfo.QrCodeBase64,
                Expiracao = DateTime.UtcNow.AddMinutes(5)
            };
        }


        public async Task<PagamentoRetornoApi> CriarCobrancaBoletoAsync(
    OrdemDeCompra ordem,
    Usuario usuario,
    Enderecos endereco,
    string uuidExterno)
        {
            var nomeCompleto = usuario.Nome.Trim();
            var partesNome = nomeCompleto.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            string firstName = partesNome.Length > 0 ? partesNome[0] : nomeCompleto;
            string lastName = partesNome.Length > 1 ? partesNome[1] : "Sobrenome Ausente";
            var payload = new
            {
                transaction_amount = (decimal)ordem.valor_total / 100m,
                description = $"ORDEM-{ordem.id}",
                payment_method_id = "bolbradesco",
                payer = new
                {
                    email = usuario.Email,
                    first_name = firstName,       
                    last_name = lastName,
                    identification = new
                    {
                        type = "CPF",
                        number = usuario.Cpf.Replace(".", "").Replace("-", "")
                    },
                    address = new
                    {
                        zip_code = endereco.Cep,
                        street_name = endereco.rua,
                        street_number = endereco.numero,
                        neighborhood = endereco.bairro,
                        city = endereco.cidade,
                        federal_unit = endereco.estado 
                    }
                },
                notification_url = _webhookUrl,
                external_reference = uuidExterno
            };

            var idempotencyKey = Guid.NewGuid().ToString();
            _httpClient.DefaultRequestHeaders.Remove("X-Idempotency-Key");
            _httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey);

            var response = await _httpClient.PostAsJsonAsync("v1/payments", payload);
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _repoPagamentos.DeletarPorUsuarioId(ordem.usuario_id);
                _repoOrdem.DeletarOrdemEmAbertoPorOrdemId(ordem.id);
                throw new Exception($"Erro Mercado Pago: {response.StatusCode} - {jsonString}");
            }

            var paymentResponse = JsonConvert.DeserializeObject<dynamic>(jsonString);

            string boletoUrl = paymentResponse?.transaction_details?.external_resource_url;

            return new PagamentoRetornoApi
            {
                BoletoURL = boletoUrl,
                TransacaoIdExterno = paymentResponse.id.ToString(),
                Expiracao = DateTime.UtcNow.AddDays(3)
            };
        }

        public async Task<PagamentoRetornoApi> CriarPagamentoCartaoAsync(
    OrdemDeCompra ordem,
    Usuario usuario,
    string uuidExterno,
    string token,
    int parcelas,
    string paymentMethodId)
        {
            var payload = new
            {
                transaction_amount = (decimal)ordem.valor_total / 100m,
                token = token,
                installments = parcelas,
                payment_method_id = paymentMethodId,
                description = $"ORDEM-{ordem.id}",
                payer = new
                {
                    email = usuario.Email
                },
                notification_url = _webhookUrl,
                external_reference = uuidExterno
            };

            var idempotencyKey = Guid.NewGuid().ToString();
            _httpClient.DefaultRequestHeaders.Remove("X-Idempotency-Key");
            _httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey);

            var response = await _httpClient.PostAsJsonAsync("v1/payments", payload);
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _repoPagamentos.DeletarPorUsuarioId(ordem.usuario_id);
                _repoOrdem.DeletarOrdemEmAbertoPorOrdemId(ordem.id);
                throw new Exception($"Erro Mercado Pago: {response.StatusCode} - {jsonString}");
            }

            var resp = JsonConvert.DeserializeObject<dynamic>(jsonString);

            return new PagamentoRetornoApi
            {
                TransacaoIdExterno = resp.id.ToString(),
                ValorPago = (int)(resp.transaction_amount * 100)
                
            };
        }


        public async Task<MercadoPagoPagamento?> ObterPagamentoPorId(string paymentId)
        {
            var response = await _httpClient.GetAsync($"v1/payments/{paymentId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MercadoPagoPagamento>(json);
        }
    }
}