using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IStatusOrdemRepository
    {
        IEnumerable<StatusOrdem> ListarTodos(string? status = null);

        void Adicionar(StatusOrdem status);

        StatusOrdem BuscarPorDescricao(string descricao, string? status = null);

        StatusOrdem BuscarPorId(int? id, string? status = null);

        StatusOrdem Atualizar(int id, StatusOrdemUpdateDTO status, string? statusquery = null);
    }
}
