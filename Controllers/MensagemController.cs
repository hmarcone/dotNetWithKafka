using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;

[ApiController]
[Route("api/[controller]")]
public class MensagemController : ControllerBase
{
	private readonly IProducer<string, string> _producer;

	public MensagemController(IProducer<string, string> producer)
	{
		_producer = producer;
	}

	[HttpPost]
	public async Task<IActionResult> Enviar([FromBody] string texto)
	{
		var message = new Message<string, string> { Value = texto };
		await _producer.ProduceAsync("meu-topico-teste", message);
		return Ok($"Mensagem '{texto}' enviada com sucesso!");
	}
}