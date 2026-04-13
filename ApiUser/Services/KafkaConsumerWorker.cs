using Confluent.Kafka;

public class KafkaConsumerWorker : BackgroundService
{
	private readonly ConsumerConfig _config;
	private readonly string _topic = "meu-topico-teste";

	public KafkaConsumerWorker()
	{
		_config = new ConsumerConfig
		{
			BootstrapServers = "127.0.0.1:9092",
			BrokerAddressFamily = BrokerAddressFamily.V4, // Força IPv4
			GroupId = "hmarcone-consumer-group",
			AutoOffsetReset = AutoOffsetReset.Earliest
		};
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		return Task.Run(() =>
		{
			using var consumer = new ConsumerBuilder<string, string>(_config).Build();
			consumer.Subscribe(_topic);
			Console.WriteLine($"[KAFKA WORKER] Iniciado. Ouvindo tópico: {_topic}");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var result = consumer.Consume(stoppingToken);
					Console.WriteLine($"\n[MENSAGEM LIDA]: {result.Message.Value}");
				}
				catch (Exception ex) { Console.WriteLine($"Erro: {ex.Message}"); }
			}
		}, stoppingToken);
	}
}