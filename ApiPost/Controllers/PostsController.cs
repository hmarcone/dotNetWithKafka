using ApiPostService.Context;
using ApiPostService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Confluent.Kafka;

namespace ApiPostService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly PostServiceContext _context;
	
    private readonly IProducer<string, string> _producer;

	public PostsController(PostServiceContext context, IProducer<string, string> producer)
	{
		_context = context;
		_producer = producer;
	}

	[HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPost()
    {
        return await _context.Posts.Include(x => x.User).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Post>> PostPost(Post post)
    {
        if (post is null)
            return BadRequest();

		var exists = await _context.Posts.AnyAsync(e => e.PostId == post.PostId);

		if (!exists)
		{
			_context.Posts.Add(post);
		}
		else
		{
			post.Title += " alterado";

			_context.Entry(post).State = EntityState.Modified;
		}


		await _context.SaveChangesAsync();


		var integrationEventData = JsonSerializer.Serialize(new
		{
			postId = post.PostId,
            title = post.Title,
			message = post.Content
		});

		Console.WriteLine(integrationEventData);

        PublishToMessageQueue(integrationEventData);

        return CreatedAtAction("GetPost", new { id = post.PostId }, post);
    }

	private async void PublishToMessageQueue([FromBody] string texto)
	{
        //ToDo: verificar depois
		//if (string.IsNullOrEmpty(texto))
		//	return BadRequest();

		var message = new Message<string, string> { Value = texto };

		await _producer.ProduceAsync("meu-topico-teste", message);

	}

}
