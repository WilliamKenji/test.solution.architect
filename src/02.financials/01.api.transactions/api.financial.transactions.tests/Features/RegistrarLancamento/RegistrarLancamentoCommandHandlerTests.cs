using api.financial.transactions.Application.Interfaces;
using api.financial.transactions.Domain.Entities;
using api.financial.transactions.Features.RegistrarLancamento;
using AutoFixture;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Moq;

namespace api.financial.transactions.tests.Features.RegistrarLancamento
{
    public class RegistrarLancamentoCommandHandlerTests
    {
        private readonly Mock<ILancamentoRepository> _repositoryMock;
        private readonly Mock<ServiceBusClient> _serviceBusClientMock;
        private readonly Fixture _fixture;

        public RegistrarLancamentoCommandHandlerTests()
        {
            _repositoryMock = new Mock<ILancamentoRepository>();
            _serviceBusClientMock = new Mock<ServiceBusClient>();
            _fixture = new Fixture();
            // Sem customização problemática — o construtor da entidade já garante consistência
        }

        [Fact]
        public async Task Handle_Deve_CriarLancamento_GravarNoBanco_E_PublicarEvento_QuandoComandoValido()
        {
            // Arrange
            var command = new RegistrarLancamentoCommand(
                Tipo: "credito",
                Valor: 1500.75m,
                Data: DateTime.UtcNow.AddDays(-1),
                Descricao: "Venda de produto X"
            );

            Lancamento? lancamentoCapturado = null;

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()))
                .Callback<Lancamento, CancellationToken>((l, _) => lancamentoCapturado = l)
                .Returns(Task.CompletedTask);

            var senderMock = new Mock<ServiceBusSender>();
            _serviceBusClientMock
                .Setup(c => c.CreateSender("lancamentos-queue"))
                .Returns(senderMock.Object);

            senderMock
                .Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new RegistrarLancamentoCommandHandler(
                _repositoryMock.Object,
                _serviceBusClientMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().Be(lancamentoCapturado!.Id);

            lancamentoCapturado.Tipo.Should().Be(command.Tipo);
            lancamentoCapturado.Valor.Should().Be(command.Valor);
            lancamentoCapturado.Data.Should().Be(command.Data);
            lancamentoCapturado.Descricao.Should().Be(command.Descricao);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Once);
            senderMock.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deve_PropagarDomainException_QuandoValorInvalido()
        {
            var command = new RegistrarLancamentoCommand("debito", -50m, DateTime.UtcNow, null);

            var handler = new RegistrarLancamentoCommandHandler(
                _repositoryMock.Object,
                _serviceBusClientMock.Object);

            await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Deve_PropagarDomainException_QuandoTipoInvalido()
        {
            var command = new RegistrarLancamentoCommand("invalido", 100m, DateTime.UtcNow, null);

            var handler = new RegistrarLancamentoCommandHandler(
                _repositoryMock.Object,
                _serviceBusClientMock.Object);

            await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
