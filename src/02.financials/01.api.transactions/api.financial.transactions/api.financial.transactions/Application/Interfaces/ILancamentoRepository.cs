using api.financial.transactions.Domain.Entities;

namespace api.financial.transactions.Application.Interfaces
{
    public interface ILancamentoRepository
    {
        Task AddAsync(Lancamento lancamento, CancellationToken ct = default);
        IQueryable<Lancamento> ObterLancamentosPorData(DateTime data);
    }
}
