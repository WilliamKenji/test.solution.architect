using api.financial.transactions.Domain.Entities;
using api.financial.transactions.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace api.financial.transactions.tests.Features.ConsultarLancamentosPorDia
{
    public class LancamentoRepositoryTests
    {
        private FluxoCaixaDbContext CreateInMemoryContext(string dbName = "TestDb")
        {
            var options = new DbContextOptionsBuilder<FluxoCaixaDbContext>()
                .UseInMemoryDatabase(dbName + Guid.NewGuid())
                .Options;

            var context = new FluxoCaixaDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task ObterLancamentosPorData_Deve_retornar_apenas_do_dia_especifico()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repo = new LancamentoRepository(context);

            var dataAlvo = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc);

            context.Lancamentos.AddRange(
                new Lancamento("credito", 1000m, dataAlvo.AddHours(10), "Venda"),
                new Lancamento("debito", 500m, dataAlvo.AddHours(8), "Conta"),
                new Lancamento("credito", 200m, dataAlvo.AddDays(1), "Amanhã"),
                new Lancamento("debito", 300m, dataAlvo.AddDays(-1), "Ontem")
            );
            await context.SaveChangesAsync();

            // Act
            var result = repo.ObterLancamentosPorData(dataAlvo);
            var lancamentos = await result.ToListAsync();

            // Assert
            lancamentos.Should().HaveCount(2);
            //lancamentos.Should().AllSatisfy(l => l.Data.Date == dataAlvo.Date);
        }

        [Fact]
        public void ObterLancamentosPorData_Deve_ser_IQueryable_para_permitir_paginacao()
        {
            using var context = CreateInMemoryContext();
            var repo = new LancamentoRepository(context);

            var query = repo.ObterLancamentosPorData(DateTime.UtcNow.Date);

            query.Should().BeAssignableTo<IQueryable<Lancamento>>();
        }
    }
}
