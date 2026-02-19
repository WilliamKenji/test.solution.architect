using api.financial.transactions.Domain.Entities;
using api.financial.transactions.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace api.financial.transactions.tests.Infrastructure.Persistence
{
    public class LancamentoRepositoryTests
    {
        private FluxoCaixaDbContext CreateInMemoryContext(string dbName = "TestDb")
        {
            var options = new DbContextOptionsBuilder<FluxoCaixaDbContext>()
                .UseInMemoryDatabase(databaseName: dbName) 
                .Options;

            var context = new FluxoCaixaDbContext(options);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task AddAsync_Deve_AdicionarLancamento_QuandoEntidadeValida()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new LancamentoRepository(context);

            var lancamento = new Lancamento(
                tipo: "credito",
                valor: 1000.50m,
                data: DateTime.UtcNow,
                descricao: "Teste de depósito"
            );

            // Act
            await repository.AddAsync(lancamento);

            // Assert
            var salvo = await context.Lancamentos.FindAsync(lancamento.Id);
            salvo.Should().NotBeNull();
            salvo.Tipo.Should().Be("credito");
            salvo.Valor.Should().Be(1000.50m);
            salvo.Descricao.Should().Be("Teste de depósito");
        }

        [Fact]
        public async Task AddAsync_Deve_PersistirApenasUmRegistro_QuandoChamadoUmaVez()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new LancamentoRepository(context);

            var lancamento = new Lancamento("debito", 250.00m, DateTime.UtcNow, null);

            // Act
            await repository.AddAsync(lancamento);

            // Assert
            var count = await context.Lancamentos.CountAsync();
            count.Should().Be(1);
        }

        [Fact]
        public async Task AddAsync_Deve_LancarExcecao_QuandoLancamentoInvalido()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new LancamentoRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(async () =>
            {
                var lancamentoInvalido = new Lancamento("credito", -100m, DateTime.UtcNow, null);
                await repository.AddAsync(lancamentoInvalido);
            });
        }
    }
}
