using BackendDesapegaJa.Interfaces;
using BackendDesapegaJa.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ExpiracaoPagamentosService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ExpiracaoPagamentosService> _logger;

    public ExpiracaoPagamentosService(
        IServiceProvider services,
        ILogger<ExpiracaoPagamentosService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiracaoPagamentosService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarExpiracoes();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar expiração de pagamentos.");
                // ⚠️ NÃO relança a exception
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task ProcessarExpiracoes()
    {
        using var scope = _services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<PagamentoService>();
        var statusRepo = scope.ServiceProvider.GetRequiredService<IStatusDePagamentoRepository>();

        var statusExpirado = statusRepo.BuscarPorDescricao("expirado");
        var statusPendente = statusRepo.BuscarPorDescricao("pendente");

        if (statusExpirado == null || statusPendente == null)
        {
            _logger.LogWarning(
                "Status de pagamento não encontrados (expirado ou pendente). Tentará novamente depois.");
            return;
        }

        var pagamentosExpirados = service.ListarPagamentosExpirados();

        foreach (var pagamento in pagamentosExpirados)
        {
            if (string.IsNullOrEmpty(pagamento.pagamento_uuid))
                continue;

            service.AtualizarStatusPagamentoPorReferencia(
                pagamento.pagamento_uuid,
                statusExpirado.id,
                null
            );
        }
    }
}
