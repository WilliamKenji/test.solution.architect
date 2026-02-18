using api.financial.transactions.Application.Interfaces;
using api.financial.transactions.Domain.Entities;
using Azure.Messaging.ServiceBus;
using MediatR;

namespace api.financial.transactions.Features.RegistrarLancamento
{
    public class RegistrarLancamentoCommandHandler : IRequestHandler<RegistrarLancamentoCommand, Guid>
    {
        private readonly ILancamentoRepository _repository;
        private readonly ServiceBusClient _serviceBusClient;

        public RegistrarLancamentoCommandHandler(ILancamentoRepository repository, ServiceBusClient serviceBusClient)
        {
            _repository = repository;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Guid> Handle(RegistrarLancamentoCommand request, CancellationToken cancellationToken)
        {
            var lancamento = new Lancamento(request.Tipo, request.Valor, request.Data, request.Descricao);

            await _repository.AddAsync(lancamento, cancellationToken);

            var sender = _serviceBusClient.CreateSender("lancamentos-queue");
            var messageBody = System.Text.Json.JsonSerializer.Serialize(new { Id = lancamento.Id, request.Tipo, request.Valor, request.Data });
            await sender.SendMessageAsync(new ServiceBusMessage(messageBody), cancellationToken);
            await sender.DisposeAsync();

            return lancamento.Id;
        }
    }
}
