using MediatR;

namespace api.financial.consolidated.Features.ConsultarConsolidado
{
    public record ConsultarConsolidadoQuery(DateTime Data) : IRequest<ConsolidadoResponse?>;

    public record ConsolidadoResponse(
        DateTime Data,
        decimal TotalCredito,
        decimal TotalDebito,
        decimal Saldo,
        DateTime UltimaAtualizacao);
}
