using api.financial.transactions.Features.RegistrarLancamento;
using FluentValidation.TestHelper;

namespace api.financial.transactions.tests.Features.RegistrarLancamento
{
    public class RegistrarLancamentoValidatorTests
    {
        private readonly RegistrarLancamentoValidator _validator = new();

        [Fact]
        public void Validator_Deve_Rejeitar_Tipo_Invalido()
        {
            var command = new RegistrarLancamentoCommand("invalido", 100m, DateTime.UtcNow, "teste");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Tipo)
                  .WithErrorMessage("O tipo deve ser 'credito' ou 'debito'.");
        }

        [Fact]
        public void Validator_Deve_Rejeitar_Valor_Negativo()
        {
            var command = new RegistrarLancamentoCommand("credito", -10m, DateTime.UtcNow, "teste");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Valor)
                  .WithErrorMessage("O valor deve ser maior que zero.");
        }

        [Fact]
        public void Validator_Deve_Rejeitar_Data_Futura()
        {
            var command = new RegistrarLancamentoCommand("credito", 100m, DateTime.UtcNow.AddDays(1), "teste");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Data.Date)
                  .WithErrorMessage("A data não pode ser futura.");
        }

        [Fact]
        public void Validator_Deve_Passar_Com_Dados_Validos()
        {
            var command = new RegistrarLancamentoCommand("debito", 50.75m, DateTime.UtcNow, "Pagamento fornecedor");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
