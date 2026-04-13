using ApiPostService.Context;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Sqlite");
builder.Services.AddDbContext<PostServiceContext>(options =>
		options.UseSqlite(connectionString));


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

CreateDatabase(app);

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

//InjectDataBase(app.Services);

app.MapControllers();
app.Run();

static void CreateDatabase(WebApplication app)
{
	var serviceScope = app.Services.CreateScope();
	var dataContext = serviceScope.ServiceProvider.GetService<PostServiceContext>();
	dataContext?.Database.EnsureCreated();
}

static void InjectDataBase(IServiceProvider serviceProvider)
{
		using (var scope = serviceProvider.CreateScope())
		{
			var scopedServiceProvider = scope.ServiceProvider;
			var dbContext = scopedServiceProvider.
							GetRequiredService<PostServiceContext>();
	};

}
