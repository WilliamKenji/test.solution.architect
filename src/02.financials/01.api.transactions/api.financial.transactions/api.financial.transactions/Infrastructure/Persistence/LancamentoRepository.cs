using api.financial.transactions.Application.Interfaces;
using api.financial.transactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.financial.transactions.Infrastructure.Persistence
{
    public class LancamentoRepository : ILancamentoRepository
    {
        private readonly FluxoCaixaDbContext _context;

        public LancamentoRepository(FluxoCaixaDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Lancamento lancamento, CancellationToken ct = default)
        {
            await _context.Lancamentos.AddAsync(lancamento, ct);
            await _context.SaveChangesAsync(ct);
        }

        public IQueryable<Lancamento> ObterLancamentosPorData(DateTime data)
        {
            var dataUtc = DateTime.SpecifyKind(data.Date, DateTimeKind.Utc);

            var inicio = dataUtc;
            var fim = dataUtc.AddDays(1).AddTicks(-1);

            return _context.Lancamentos
                .Where(l => l.Data >= inicio && l.Data <= fim)
                .AsNoTracking();
        }
    }
}
