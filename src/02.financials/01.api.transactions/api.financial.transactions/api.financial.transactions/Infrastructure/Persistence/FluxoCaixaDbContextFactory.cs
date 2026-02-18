using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace api.financial.transactions.Infrastructure.Persistence
{
    public class FluxoCaixaDbContextFactory : IDesignTimeDbContextFactory<FluxoCaixaDbContext>
    {
        public FluxoCaixaDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Server=localhost,1433;Database=FluxoCaixaDB;User Id=sa;Password=pwdSuper@ssw0rd123!;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<FluxoCaixaDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FluxoCaixaDbContext(optionsBuilder.Options);
        }
    }
}
