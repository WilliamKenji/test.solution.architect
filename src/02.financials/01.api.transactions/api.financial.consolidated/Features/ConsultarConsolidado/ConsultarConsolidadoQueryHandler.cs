using api.financial.consolidated.Application.Interfaces;
using api.financial.consolidated.Domain.Entities;
using MediatR;

namespace api.financial.consolidated.Features.ConsultarConsolidado
{
    public class ConsultarConsolidadoQueryHandler : IRequestHandler<ConsultarConsolidadoQuery, ConsolidadoResponse?>
    {
        private readonly IConsolidatedRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ConsultarConsolidadoQueryHandler> _logger;

        public ConsultarConsolidadoQueryHandler(
            IConsolidatedRepository repository,
            ICacheService cacheService,
            ILogger<ConsultarConsolidadoQueryHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ConsolidadoResponse?> Handle(ConsultarConsolidadoQuery request, CancellationToken cancellationToken)
        {
            var data = request.Data.Date;
            var cacheKey = $"consolidado:{data:yyyy-MM-dd}";

            // Cache-aside: first cache
            var cached = await _cacheService.GetAsync<ConsolidatedDaily>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogInformation("Consolidado de {Data} servido do cache.", data.ToShortDateString());
                return new ConsolidadoResponse(cached.Data, cached.TotalCredito, cached.TotalDebito, cached.Saldo, cached.UltimaAtualizacao);
            }

            // Fallback DB
            var fromDb = await _repository.GetByDateAsync(data, cancellationToken);
            if (fromDb == null)
            {
                _logger.LogWarning("Consolidado de {Data} não encontrado.", data.ToShortDateString());
                return null;
            }

            // Atualiza cache
            await _cacheService.SetAsync(cacheKey, fromDb, TimeSpan.FromDays(7), cancellationToken);

            return new ConsolidadoResponse(fromDb.Data, fromDb.TotalCredito, fromDb.TotalDebito, fromDb.Saldo, fromDb.UltimaAtualizacao);
        }
    }
}
