using FluentValidation;

namespace api.financial.consolidated.Features.ConsultarConsolidado
{
    public class ConsultarConsolidadoValidator : AbstractValidator<ConsultarConsolidadoQuery>
    {
        public ConsultarConsolidadoValidator()
        {
            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("Data é obrigatória.")
                .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Data não pode ser futura.");
        }
    }
}
