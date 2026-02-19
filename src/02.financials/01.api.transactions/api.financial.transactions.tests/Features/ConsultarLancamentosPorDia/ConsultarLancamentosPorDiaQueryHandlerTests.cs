using api.financial.transactions.Application.Interfaces;
using api.financial.transactions.Domain.Entities;
using api.financial.transactions.Features.ConsultarLancamentosPorDia;
using api.financial.transactions.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace api.financial.transactions.tests.Features.ConsultarLancamentosPorDia
{
    public class ConsultarLancamentosPorDiaQueryHandlerTests
    {
        private readonly Mock<ILancamentoRepository> _repoMock = new();
        private readonly ConsultarLancamentosPorDiaQueryHandler _handler;

        public ConsultarLancamentosPorDiaQueryHandlerTests()
        {
            _handler = new ConsultarLancamentosPorDiaQueryHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Deve_retornar_lancamentos_paginados_corretamente()
        {
            var options = new DbContextOptionsBuilder<FluxoCaixaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new FluxoCaixaDbContext(options);

            var data = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc);

            context.Lancamentos.AddRange(
                new Lancamento("credito", 1000m, data.AddHours(10), "Venda 1"),
                new Lancamento("debito", 300m, data.AddHours(9), "Conta luz"),
                new Lancamento("credito", 500m, data.AddHours(8), "Venda 2")
            );
            await context.SaveChangesAsync();

            var repository = new LancamentoRepository(context);
            var handler = new ConsultarLancamentosPorDiaQueryHandler(repository);

            var query = new ConsultarLancamentosPorDiaQuery(data, 1, 2);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Itens.Should().HaveCount(2);
            result.TotalItens.Should().Be(3);
        }
    }
}
