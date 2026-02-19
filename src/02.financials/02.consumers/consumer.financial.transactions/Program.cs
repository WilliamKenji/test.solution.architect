using consumer.financial.transactions.Application.Behaviors;
using consumer.financial.transactions.Application.Interfaces;
using consumer.financial.transactions.Infrastructure.Caching;
using consumer.financial.transactions.Infrastructure.Messaging;
using consumer.financial.transactions.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace consumer.financial.transactions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

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

            
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = "consolidated:";
            });

            builder.Services.AddDbContext<ConsolidatedDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConsolidatedDb")));

            builder.Services.AddHostedService<DatabaseInitializer>();
            builder.Services.AddHostedService<ServiceBusConsumer>();

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); 
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = "";
            });

            builder.Services.AddScoped<IConsolidatedRepository, ConsolidatedRepository>();
            builder.Services.AddSingleton<ICacheService, RedisCacheService>();


            var host = builder.Build();

            host.Run();
        }
    }
}