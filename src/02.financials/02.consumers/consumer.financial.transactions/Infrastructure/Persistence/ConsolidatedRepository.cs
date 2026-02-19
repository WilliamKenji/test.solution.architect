using consumer.financial.transactions.Application.Interfaces;
using consumer.financial.transactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace consumer.financial.transactions.Infrastructure.Persistence
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

        public async Task<ConsolidatedDaily> UpsertAsync(ConsolidatedDaily consolidated, CancellationToken ct = default)
        {
            var existing = await _context.ConsolidatedDailies
                .FirstOrDefaultAsync(c => c.Data == consolidated.Data, ct);

            if (existing == null)
            {
                await _context.ConsolidatedDailies.AddAsync(consolidated, ct);
                await _context.SaveChangesAsync(ct);
                return consolidated;
            }
            else
            {
                existing.TotalCredito = consolidated.TotalCredito;
                existing.TotalDebito = consolidated.TotalDebito;
                existing.Versao = consolidated.Versao;

                _context.ConsolidatedDailies.Update(existing);
                await _context.SaveChangesAsync(ct);

                return existing;
            }
        }
    }
}
