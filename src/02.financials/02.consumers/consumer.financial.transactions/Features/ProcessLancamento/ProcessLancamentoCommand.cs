using MediatR;

namespace consumer.financial.transactions.Features.ProcessLancamento
{
    public record ProcessLancamentoCommand(
    Guid LancamentoId,
    string Tipo,
    decimal Valor,
    DateTime Data
) : IRequest;
}
