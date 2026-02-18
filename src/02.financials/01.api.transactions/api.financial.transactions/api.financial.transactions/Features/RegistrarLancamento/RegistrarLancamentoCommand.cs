using MediatR;

namespace api.financial.transactions.Features.RegistrarLancamento
{
    public record RegistrarLancamentoCommand(string Tipo, decimal Valor, DateTime Data, string? Descricao) : IRequest<Guid>;
}
