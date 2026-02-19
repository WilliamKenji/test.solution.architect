using api.financial.consolidated.Application.Interfaces;
using api.financial.consolidated.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.financial.consolidated.Infrastructure.Persistence
{
    public class ConsolidatedRepository : IConsolidatedRepository
    {
        private readonly ConsolidatedDbContext _context;

        public ConsolidatedRepository(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<ConsolidatedDaily?> GetByDateAsync(DateTime date, CancellationToken ct = default)
        {
            var dataUtc = date.Date;

            return await _context.ConsolidatedDailies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Data == dataUtc, ct);
        }
    }
}
