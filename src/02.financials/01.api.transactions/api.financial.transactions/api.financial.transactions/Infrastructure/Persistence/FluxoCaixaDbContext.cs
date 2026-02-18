using api.financial.transactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.financial.transactions.Infrastructure.Persistence
{
    public class FluxoCaixaDbContext : DbContext
    {
        public DbSet<Lancamento> Lancamentos { get; set; }

        public FluxoCaixaDbContext(DbContextOptions<FluxoCaixaDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lancamento>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Tipo).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Valor).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.Descricao).HasMaxLength(500);

                entity.HasIndex(e => new { e.Data, e.Id })
                      .IsDescending()
                      .HasDatabaseName("IX_Lancamentos_Data_Desc");

            });
        }
    }
}
