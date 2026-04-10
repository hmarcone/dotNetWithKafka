using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do Produtor (Singleton)
var producerConfig = new ProducerConfig
{
	BootstrapServers = "127.0.0.1:9092",
	BrokerAddressFamily = BrokerAddressFamily.V4
};
builder.Services.AddSingleton<IProducer<string, string>>(
	new ProducerBuilder<string, string>(producerConfig).Build());

// Registro do Consumidor de Fundo
builder.Services.AddHostedService<KafkaConsumerWorker>();

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.Run();