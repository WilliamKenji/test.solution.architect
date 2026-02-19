using FluentValidation;

namespace consumer.financial.transactions.Features.ProcessLancamento
{
    public class ProcessLancamentoValidator : AbstractValidator<ProcessLancamentoCommand>
    {
        public ProcessLancamentoValidator()
        {
            RuleFor(x => x.LancamentoId)
                .NotEmpty().WithMessage("ID do lançamento é obrigatório.");

            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("Tipo é obrigatório.")
                .Must(t => t == "credito" || t == "debito")
                .WithMessage("Tipo deve ser 'credito' ou 'debito'.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser positivo.")
                .PrecisionScale(18, 2, false).WithMessage("Valor deve ter no máximo 2 casas decimais.");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("Data é obrigatória.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data não pode ser futura.");
        }
    }
}
