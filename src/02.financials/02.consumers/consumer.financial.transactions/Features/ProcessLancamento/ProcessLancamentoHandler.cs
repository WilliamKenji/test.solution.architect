using consumer.financial.transactions.Application.Interfaces;
using consumer.financial.transactions.Domain.Entities;
using consumer.financial.transactions.Domain.Exceptions;
using MediatR;

namespace consumer.financial.transactions.Features.ProcessLancamento
{
    public class ProcessLancamentoHandler : IRequestHandler<ProcessLancamentoCommand>
    {
        private readonly IConsolidatedRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ProcessLancamentoHandler> _logger;

        public ProcessLancamentoHandler(
            IConsolidatedRepository repository,
            ICacheService cacheService,
            ILogger<ProcessLancamentoHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task Handle(ProcessLancamentoCommand request, CancellationToken cancellationToken)
        {

            var dataLancamento = request.Data.Date;
            var idempotencyKey = $"processed-lancamento:{dataLancamento}";

            var consolidado = await _repository.GetByDateAsync(dataLancamento, cancellationToken)
                ?? new ConsolidatedDaily(dataLancamento);

            if (request.Tipo == "credito")
            {
                consolidado.AdicionarCredito(request.Valor);
            }
            else if (request.Tipo == "debito")
            {
                consolidado.AdicionarDebito(request.Valor);
            }
            else
            {
                throw new DomainException("Tipo de lançamento inválido.");
            }

            var consolidadoAtualizado = await _repository.UpsertAsync(consolidado, cancellationToken);

            var cacheKey = $"consolidado:{dataLancamento:yyyy-MM-dd}";
            await _cacheService.SetAsync(cacheKey, consolidadoAtualizado, TimeSpan.FromDays(7), cancellationToken);

            await _cacheService.SetAsync(idempotencyKey, true, TimeSpan.FromDays(30), cancellationToken);

            _logger.LogInformation(
                "Consolidado do dia {Data} atualizado. Crédito: {Credito}, Débito: {Debito}, Saldo: {Saldo}",
                dataLancamento.ToShortDateString(),
                consolidadoAtualizado.TotalCredito,
                consolidadoAtualizado.TotalDebito,
                consolidadoAtualizado.Saldo);
        }
    }
}
