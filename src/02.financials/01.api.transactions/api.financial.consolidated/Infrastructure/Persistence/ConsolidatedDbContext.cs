using api.financial.consolidated.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.financial.consolidated.Infrastructure.Persistence
{
    public class ConsolidatedDbContext : DbContext
    {
        public DbSet<ConsolidatedDaily> ConsolidatedDailies { get; set; }

        public ConsolidatedDbContext(DbContextOptions<ConsolidatedDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsolidatedDaily>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Data)
                      .IsUnique()
                      .HasDatabaseName("IX_ConsolidatedDaily_Data_Unique");

                entity.Property(e => e.Data)
                      .HasColumnType("date")  // armazena apenas a data (sem hora)
                      .IsRequired();

                entity.Property(e => e.TotalCredito)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(e => e.TotalDebito)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(e => e.UltimaAtualizacao)
                      .IsRequired();

                entity.Property(e => e.Versao)
                      .IsConcurrencyToken()  // otimistic concurrency
                      .IsRequired();
            });

            // Evita rastreamento automático em consultas de leitura pesadas
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<ConsolidatedDaily>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UltimaAtualizacao = DateTime.UtcNow;
                }
            }
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<ConsolidatedDaily>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UltimaAtualizacao = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
