using consumer.financial.transactions.Domain.Entities;

namespace consumer.financial.transactions.Application.Interfaces
{
    public interface IConsolidatedRepository
    {
        Task<ConsolidatedDaily?> GetByDateAsync(DateTime date, CancellationToken ct = default);

        Task<ConsolidatedDaily> UpsertAsync(ConsolidatedDaily consolidated, CancellationToken ct = default);
    }
}
