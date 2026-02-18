using api.financial.transactions.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.financial.transactions.Features.ConsultarLancamentosPorDia
{
    public class ConsultarLancamentosPorDiaQueryHandler : IRequestHandler<ConsultarLancamentosPorDiaQuery, ConsultarLancamentosPorDiaResponse>
    {
        private readonly ILancamentoRepository _repository;

        public ConsultarLancamentosPorDiaQueryHandler(ILancamentoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ConsultarLancamentosPorDiaResponse> Handle(
            ConsultarLancamentosPorDiaQuery request,
            CancellationToken cancellationToken)
        {
            var query = _repository.ObterLancamentosPorData(request.Data.Date);

            var totalItens = await query.CountAsync(cancellationToken);

            var itens = await query
                .OrderByDescending(l => l.Data)
                .ThenByDescending(l => l.Id)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(l => new LancamentoDto(
                    l.Id,
                    l.Tipo,
                    l.Valor,
                    l.Data,
                    l.Descricao))
                .ToListAsync(cancellationToken);

            var totalPaginas = (int)Math.Ceiling(totalItens / (double)request.TamanhoPagina);

            return new ConsultarLancamentosPorDiaResponse(
                itens,
                request.Pagina,
                request.TamanhoPagina,
                totalItens,
                totalPaginas);
        }
    }
}
