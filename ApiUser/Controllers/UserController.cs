using ApiPostService.Context;
using ApiPostService.Entities;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
	private readonly IProducer<string, string> _producer;

	private readonly PostServiceContext _context;


	public UserController(IProducer<string, string> producer, PostServiceContext context)
	{
		_producer = producer;
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<User>>> GetUser()
	{
		return await _context.Users.ToListAsync();
	}
	
	[HttpPut("{id}")]
	public async Task<IActionResult> PutUser(int id, User user)
	{
		if (user == null || id <= 0)
			return BadRequest();

		_context.Entry(user).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			// Verifique se o registro ainda existe no banco
			var exists = await _context.Users.AnyAsync(e => e.Id == id);

			if (!exists)
			{
				return NotFound("O registro foi excluído por outro usuário.");
			}
			else
			{
				throw; // Outro erro de concorrência (ex: RowVersion)
			}
		}


		//await _context.SaveChangesAsync();

		var integrationEventData = JsonSerializer.Serialize(new
		{
			id = user.Id,
			newname = user.Name
		});

		PublishToMessageQueue(integrationEventData);

		return NoContent();
	}

	[HttpPost]
	public async Task<ActionResult<User>> PostUser(User user)
	{
		if (user is null)
			return BadRequest();


		// Verifique se o registro ainda existe no banco
		var exists = await _context.Users.AnyAsync(e => e.Id == user.Id);

		if (!exists)
		{
			_context.Users.Add(user);
		}else{
			user.Name += " alterado";

			_context.Entry(user).State = EntityState.Modified;
		}

		await _context.SaveChangesAsync();

		var integrationEventData = JsonSerializer.Serialize(new
		{
			id = user.Id,
			name = user.Name
		});

		PublishToMessageQueue(integrationEventData);

		return CreatedAtAction("GetUser", new { id = user.Id }, user);
	}

	private async void PublishToMessageQueue([FromBody] string texto)
	{
		//if (string.IsNullOrEmpty(texto))
		//	return BadRequest();

		var message = new Message<string, string> { Value = texto };

		await _producer.ProduceAsync("meu-topico-teste", message);

	}
}