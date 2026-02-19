
using api.financial.consolidated.Application.Behaviors;
using api.financial.consolidated.Application.Interfaces;
using api.financial.consolidated.Infrastructure.Caching;
using api.financial.consolidated.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace api.financial.consolidated
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

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = "";
            });

            builder.Services.AddDbContext<ConsolidatedDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("ConsolidatedDb"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null)
                ));

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddScoped<IConsolidatedRepository, ConsolidatedRepository>();
            builder.Services.AddSingleton<ICacheService, RedisCacheService>();


            var app = builder.Build();

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
