using consumer.financial.transactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace consumer.financial.transactions.Infrastructure.Persistence
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
                      .HasColumnType("date")
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
                      .IsConcurrencyToken()
                      .IsRequired();
            });

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
