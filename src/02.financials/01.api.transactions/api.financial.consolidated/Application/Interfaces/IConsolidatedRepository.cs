using api.financial.consolidated.Domain.Entities;

namespace api.financial.consolidated.Application.Interfaces
{
    public interface IConsolidatedRepository
    {
        Task<ConsolidatedDaily?> GetByDateAsync(DateTime date, CancellationToken ct = default);
    }
}
