using api.financial.transactions.Features.ConsultarLancamentosPorDia;
using api.financial.transactions.Features.RegistrarLancamento;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.financial.transactions.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LancamentosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LancamentosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarLancamentoCommand command)
        {
            try
            {
                var id = await _mediator.Send(command);
                return CreatedAtAction(nameof(Registrar), new { id }, command);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    errors = ex.Errors.Select(e => new { property = e.PropertyName, error = e.ErrorMessage })
                });
            }
        }

        [HttpGet("por-dia")]
        [ProducesResponseType(typeof(ConsultarLancamentosPorDiaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConsultarPorDia(
        [FromQuery] DateTime data,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
        {
            var query = new ConsultarLancamentosPorDiaQuery(data, pagina, tamanhoPagina);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
