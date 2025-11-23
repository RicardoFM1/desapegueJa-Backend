using BackendDesapegaJa.Interfaces;
using BackendDesapegaJa.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class ExpiracaoPagamentosService : BackgroundService
{
    private readonly IServiceProvider _services;

    
    public ExpiracaoPagamentosService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessarExpiracoes();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task ProcessarExpiracoes()
    {
        using (var scope = _services.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<PagamentoService>();
            var statusRepo = scope.ServiceProvider.GetRequiredService<IStatusDePagamentoRepository>();

          
            var statusExpirado = statusRepo.BuscarPorDescricao("expirado")
                ?? throw new InvalidOperationException("Status 'Expirado' não encontrado.");
            int statusIdExpirado = statusExpirado.id;

           
            var statusPendente = statusRepo.BuscarPorDescricao("pendente")
                 ?? throw new InvalidOperationException("Status 'Pendente' não encontrado.");
            int statusIdPendente = statusPendente.id;

         
            var pagamentosExpirados = service.ListarPagamentosExpirados(); 

            foreach (var pagamento in pagamentosExpirados)
            {
               
                service.AtualizarStatusPagamentoPorReferencia(
                    pagamento.pagamento_uuid!,
                    statusIdExpirado,
                    null
                );
            }
        }
    }
}