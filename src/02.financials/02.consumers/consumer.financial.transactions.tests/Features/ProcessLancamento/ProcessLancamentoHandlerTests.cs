using AutoFixture;
using AutoFixture.AutoMoq;
using consumer.financial.transactions.Application.Interfaces;
using consumer.financial.transactions.Domain.Entities;
using consumer.financial.transactions.Domain.Exceptions;
using consumer.financial.transactions.Features.ProcessLancamento;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace consumer.financial.transactions.tests.Features.ProcessLancamento
{
    public class ProcessLancamentoHandlerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IConsolidatedRepository> _repositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<ILogger<ProcessLancamentoHandler>> _loggerMock;
        private readonly ProcessLancamentoHandler _handler;

        public ProcessLancamentoHandlerTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _repositoryMock = _fixture.Freeze<Mock<IConsolidatedRepository>>();
            _cacheServiceMock = _fixture.Freeze<Mock<ICacheService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<ProcessLancamentoHandler>>>();

            _handler = new ProcessLancamentoHandler(
                _repositoryMock.Object,
                _cacheServiceMock.Object,
                _loggerMock.Object);
        }

        [Theory]
        [InlineData("invalido", 100, "Tipo de lançamento inválido.")]
        [InlineData("credito", -50, "Valor de crédito deve ser positivo.")]
        public async Task Handle_Deve_LancarDomainException_QuandoDadosInvalidos(string tipo, decimal valor, string expectedMessage)
        {
            // Arrange
            var command = _fixture.Create<ProcessLancamentoCommand>() with { Tipo = tipo, Valor = valor };
            var idempotencyKey = $"processed-lancamento:{command.LancamentoId}";
            _cacheServiceMock.Setup(c => c.ExistsAsync(idempotencyKey, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.GetByDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync((ConsolidatedDaily?)null);

            // Act & Assert
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<DomainException>().WithMessage(expectedMessage);
            _repositoryMock.Verify(r => r.UpsertAsync(It.IsAny<ConsolidatedDaily>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
