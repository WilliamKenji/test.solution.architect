using api.financial.consolidated.Features.ConsultarConsolidado;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.financial.consolidated.Controllers
{
    [ApiController]
    [Route("api/consolidado")]
    public class ConsolidadoController : ControllerBase
    {
        private readonly ISender _mediator;

        public ConsolidadoController(ISender mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime data)
        {
            var result = await _mediator.Send(new ConsultarConsolidadoQuery(data));
            return result == null ? NotFound() : Ok(result);
        }
    }
}
