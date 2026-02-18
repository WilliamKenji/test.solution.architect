using MediatR;

namespace api.financial.transactions.Features.ConsultarLancamentosPorDia
{
    public record ConsultarLancamentosPorDiaQuery(
    DateTime Data,
    int Pagina = 1,
    int TamanhoPagina = 20
) : IRequest<ConsultarLancamentosPorDiaResponse>;

    public record ConsultarLancamentosPorDiaResponse(
        List<LancamentoDto> Itens,
        int PaginaAtual,
        int TamanhoPagina,
        int TotalItens,
        int TotalPaginas
    );

    public record LancamentoDto(
        Guid Id,
        string Tipo,
        decimal Valor,
        DateTime Data,
        string? Descricao
    );
}
