using Azure.Messaging.ServiceBus;
using consumer.financial.transactions.Features.ProcessLancamento;
using MediatR;
using System.Text.Json;

namespace consumer.financial.transactions.Infrastructure.Messaging
{
    public class ServiceBusConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceBusConsumer> _logger;

        private ServiceBusClient? _client;
        private ServiceBusProcessor? _processor;

        public ServiceBusConsumer(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<ServiceBusConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = _configuration.GetValue<string>("ServiceBus:ConnectionString")
                ?? throw new InvalidOperationException("ServiceBus connection string não configurada.");

            var queueName = _configuration.GetValue<string>("ServiceBus:QueueName")
                ?? throw new InvalidOperationException("Nome da queue não configurado.");

            var maxConcurrentCalls = _configuration.GetValue<int>("ServiceBus:MaxConcurrentCalls", 5);

            try
            {
                _client = new ServiceBusClient(connectionString);

                _processor = _client.CreateProcessor(
                    queueName,
                    new ServiceBusProcessorOptions
                    {
                        AutoCompleteMessages = false, 
                        MaxConcurrentCalls = maxConcurrentCalls,
                        MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
                    });

                _processor.ProcessMessageAsync += ProcessMessageHandler;
                _processor.ProcessErrorAsync += ProcessErrorHandler;

                _logger.LogInformation("Iniciando consumer da fila {QueueName} com {MaxConcurrent} chamadas simultâneas", queueName, maxConcurrentCalls);

                await _processor.StartProcessingAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Falha crítica ao inicializar Service Bus consumer");
                throw;
            }
        }

        private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                var body = args.Message.Body.ToString();
                _logger.LogDebug("Mensagem recebida. ID={MessageId}, Body={Body}", args.Message.MessageId, body);

                var payload = JsonSerializer.Deserialize<ProcessLancamentoCommand>(body)
                    ?? throw new JsonException("Falha ao deserializar mensagem do Service Bus");

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(payload, args.CancellationToken);

                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                _logger.LogInformation("Mensagem processada com sucesso. ID={MessageId}", args.Message.MessageId);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Falha ao deserializar mensagem. Enviando para dead-letter.");
                await args.DeadLetterMessageAsync(args.Message, "Deserialização falhou", ex.Message, args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem. Tentativa={DeliveryCount}", args.Message.DeliveryCount);

                if (args.Message.DeliveryCount >= 10)
                {
                    await args.DeadLetterMessageAsync(args.Message, "Máximo de tentativas excedido", ex.Message, args.CancellationToken);
                }
            }
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs args)
        {
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Parando Service Bus consumer...");

            if (_processor != null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
