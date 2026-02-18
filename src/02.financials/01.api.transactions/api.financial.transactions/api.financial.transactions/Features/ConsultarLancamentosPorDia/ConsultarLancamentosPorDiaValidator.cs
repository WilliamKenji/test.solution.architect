using FluentValidation;

namespace api.financial.transactions.Features.ConsultarLancamentosPorDia
{
    public class ConsultarLancamentosPorDiaValidator : AbstractValidator<ConsultarLancamentosPorDiaQuery>
    {
        public ConsultarLancamentosPorDiaValidator()
        {
            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("A data é obrigatória.")
                .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("A data não pode ser futura.");

            RuleFor(x => x.Pagina)
                .GreaterThanOrEqualTo(1).WithMessage("Página deve ser maior ou igual a 1.");

            RuleFor(x => x.TamanhoPagina)
                .GreaterThanOrEqualTo(5).WithMessage("Tamanho da página deve ser no mínimo 5.")
                .LessThanOrEqualTo(100).WithMessage("Tamanho da página deve ser no máximo 100.");
        }
    }
}
