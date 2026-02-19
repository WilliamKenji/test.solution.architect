using api.financial.consolidated.Domain.Exceptions;

namespace api.financial.consolidated.Domain.Entities
{
    public class ConsolidatedDaily
    {
        public Guid Id { get; set; }
        public DateTime Data { get; set; }           // data do dia (UTC, sem hora)
        public decimal TotalCredito { get; set; }
        public decimal TotalDebito { get; set; }
        public decimal Saldo => TotalCredito - TotalDebito;
        public DateTime UltimaAtualizacao { get; set; }
        public int Versao { get; set; }              // para optimistic concurrency

        protected ConsolidatedDaily() { }

        public ConsolidatedDaily(DateTime data)
        {
            Id = Guid.NewGuid();
            Data = data.Date;
            TotalCredito = 0;
            TotalDebito = 0;
            UltimaAtualizacao = DateTime.UtcNow;
            Versao = 1;
        }

        public void AdicionarCredito(decimal valor)
        {
            if (valor <= 0) throw new DomainException("Valor de crédito deve ser positivo.");
            TotalCredito += valor;
            UltimaAtualizacao = DateTime.UtcNow;
            Versao++;
        }

        public void AdicionarDebito(decimal valor)
        {
            if (valor <= 0) throw new DomainException("Valor de débito deve ser positivo.");
            TotalDebito += valor;
            UltimaAtualizacao = DateTime.UtcNow;
            Versao++;
        }
    }
}
