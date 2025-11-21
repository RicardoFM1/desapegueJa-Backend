using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IPagamentosRepository
    {
        IEnumerable<Pagamentos> ListarTodos();
        Pagamentos BuscarPorId(int id);

        Pagamentos? BuscarPorUsuarioId(int usuarioId);
        void Adicionar(Pagamentos pagamento);
        Pagamentos Atualizar(int usuarioId, PagamentosUpdateDTO pagamento);

        void DeletarPorUsuarioId(int usuarioId);

        Pagamentos? BuscarPorUUID(string uuid);

        public interface IPagSeguroIntegration
        {
            Task<PagamentoRetornoApi> CriarCobrancaPixAsync(int ordemId, decimal valorTotal, int usuarioId);
            Task<PagamentoRetornoApi> CriarCobrancaBoletoAsync(int ordemId, decimal valorTotal, int usuarioId);
            bool ValidateWebhookToken(string token);
        }


    }
}
