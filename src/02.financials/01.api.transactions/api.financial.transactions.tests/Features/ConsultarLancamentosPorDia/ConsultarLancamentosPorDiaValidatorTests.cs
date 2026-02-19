using api.financial.transactions.Features.ConsultarLancamentosPorDia;
using FluentValidation.TestHelper;

namespace api.financial.transactions.tests.Features.ConsultarLancamentosPorDia
{
    public class ConsultarLancamentosPorDiaValidatorTests
    {
        private readonly ConsultarLancamentosPorDiaValidator _validator = new();

        [Theory]
        [InlineData("2026-02-18", 1, 20, true)]   // válido
        [InlineData("2026-02-19", 1, 20, false)]  // data futura
        [InlineData(null, 1, 20, false)]  // data nula
        [InlineData("2026-02-18", 0, 20, false)]  // página inválida
        [InlineData("2026-02-18", 1, 4, false)]  // página muito pequena
        [InlineData("2026-02-18", 1, 101, false)] // página muito grande
        public void Deve_validar_corretamente_parametros(
            string? dataStr, int pagina, int tamanhoPagina, bool deveSerValido)
        {
            var data = dataStr != null ? DateTime.Parse(dataStr) : default(DateTime?);

            var query = new ConsultarLancamentosPorDiaQuery(
                data ?? DateTime.UtcNow.AddDays(1),
                pagina,
                tamanhoPagina);

            var result = _validator.TestValidate(query);

            if (deveSerValido)
                result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Data_futura_deve_falhar()
        {
            var query = new ConsultarLancamentosPorDiaQuery(
                DateTime.UtcNow.AddDays(1).Date,
                1, 20);

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.Data)
                  .WithErrorMessage("A data não pode ser futura.");
        }
    }
}
