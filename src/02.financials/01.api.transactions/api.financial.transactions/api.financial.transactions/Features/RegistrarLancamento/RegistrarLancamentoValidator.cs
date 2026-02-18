using FluentValidation;

namespace api.financial.transactions.Features.RegistrarLancamento
{
    public class RegistrarLancamentoValidator : AbstractValidator<RegistrarLancamentoCommand>
    {
        public RegistrarLancamentoValidator()
        {
            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("O tipo do lançamento é obrigatório.")
                .Must(t => t == "credito" || t == "debito")
                .WithMessage("O tipo deve ser 'credito' ou 'debito'.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor deve ser maior que zero.")
                .PrecisionScale(18, 2, false).WithMessage("O valor deve ter no máximo 2 casas decimais.");

            RuleFor(x => x.Data.Date)
                .NotEmpty().WithMessage("A data é obrigatória.")
                .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("A data não pode ser futura.");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("A descrição não pode exceder 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Descricao));
        }
    }
}
