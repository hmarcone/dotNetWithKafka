using ApiPostService.Context;
using ApiPostService.Entities;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class MensagemController : ControllerBase
{
	private readonly IProducer<string, string> _producer;

	private readonly PostServiceContext _context;


	public MensagemController(IProducer<string, string> producer, PostServiceContext context)
	{
		_producer = producer;
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Post>>> GetPost()
	{
		return await _context.Posts.Include(x => x.User).ToListAsync();
	}

	[HttpPost]
	public async Task<IActionResult> Enviar([FromBody] string texto)
	{

		if (string.IsNullOrEmpty(texto))
			return BadRequest();

		var user = new User { Id = 2, Name = "hfreitas" };
		var post = new Post
		{
			PostId = 2,
			User = user,
			Content = texto,
			Title = $@"Mensagem do User {user.Id}"
		};

		_context.Posts.Add(post);

		await _context.SaveChangesAsync();


		var message = new Message<string, string> { Value = texto };

		await _producer.ProduceAsync("meu-topico-teste", message);

		return CreatedAtAction("GetPost", new { id = post.PostId }, post);

		//return Ok($"Mensagem '{texto}' enviada com sucesso!");
	}
}