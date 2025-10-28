using BackendDesapegaJa.Entities;

namespace BackendDesapegaJa.Interfaces
{
    public interface IStatusOrdemRepository
    {
        IEnumerable<StatusOrdem> ListarTodos();

        void Adicionar(StatusOrdem status);

        StatusOrdem BuscarPorDescricao(string descricao);

        StatusOrdem BuscarPorId(int? id);

        StatusOrdem Atualizar(int id, StatusOrdemUpdateDTO status);
    }
}
