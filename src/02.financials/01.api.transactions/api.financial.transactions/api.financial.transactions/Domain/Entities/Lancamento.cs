namespace api.financial.transactions.Domain.Entities
{
    public class Lancamento
    {
        public Guid Id { get; private set; }
        public string Tipo { get; private set; } // "credito" ou "debito"
        public decimal Valor { get; private set; }
        public DateTime Data { get; private set; }
        public string? Descricao { get; private set; }

        private Lancamento() { } 

        public Lancamento(string tipo, decimal valor, DateTime data, string? descricao)
        {
            if (valor <= 0) throw new DomainException("Valor deve ser positivo.");
            if (string.IsNullOrWhiteSpace(tipo) || (tipo != "credito" && tipo != "debito"))
                throw new DomainException("Tipo inválido: credito ou debito.");

            Id = Guid.NewGuid();
            Tipo = tipo;
            Valor = valor;
            Data = data;
            Descricao = descricao;
        }
    }

    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
