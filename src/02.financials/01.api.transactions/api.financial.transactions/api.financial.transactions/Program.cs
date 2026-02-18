
using api.financial.transactions.Application.Behaviors;
using api.financial.transactions.Application.Interfaces;
using api.financial.transactions.Infrastructure.Persistence;
using Azure.Messaging.ServiceBus;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace api.financial.transactions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configuração de ambientes múltiplos
            // Carrega appsettings.Local.json quando o ambiente for "Local"
            // Em Dev/Stg/Prd, pode adicionar Key Vault real depois (comentado)
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            // Perfil custom "Local" (carrega appsettings.Local.json)
            if (builder.Environment.IsEnvironment("Local"))
            {
                builder.Configuration.AddJsonFile("appsettings.Local.json", optional: false, reloadOnChange: true);
            }

            // (Opcional - para ambientes reais no Azure, descomente quando quiser)
            // if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Local"))
            // {
            //     var keyVaultName = builder.Environment.IsEnvironment("Staging") ? "kv-stg" : "kv-prd";
            //     var vaultUri = new Uri($"https://{keyVaultName}.vault.azure.net");
            //     builder.Configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());
            // }

            // 2. Adicionar serviços
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lançamentos API", Version = "v1" });
            });

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            builder.Services.AddDbContext<FluxoCaixaDbContext>((sp, options) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

                options.UseSqlServer(connectionString);
            });

            builder.Services.AddSingleton(sp =>
            {
                var connectionString = builder.Configuration.GetValue<string>("ServiceBus:ConnectionString")
                    ?? throw new InvalidOperationException("ServiceBus:ConnectionString não encontrado.");

                return new ServiceBusClient(connectionString);
            });

            builder.Services.AddScoped<ILancamentoRepository, LancamentoRepository>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FluxoCaixaDbContext>();
                try
                {
                    dbContext.Database.Migrate();
                    Console.WriteLine("Migrations aplicadas automaticamente no startup (ambiente Local/Development).");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao aplicar migrations no startup: {ex.Message}");
                }
            }

            app.Run();
        }
    }
}
