using Microsoft.AspNetCore.Mvc;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Application.Models;
using System.Threading.Tasks;

namespace SmartCRM.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAiService _aiService;

    public ChatController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        System.Console.WriteLine($"[ChatController] Incoming request: {request.Message}");
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty.");
        }

        try 
        {
            var response = await _aiService.GetResponseAsync(request.Message);
            System.Console.WriteLine($"[ChatController] Response ready. Content length: {response.Reply.Length}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[ChatController] FATAL ERROR: {ex.Message}");
            return StatusCode(500, "Internal Server Error during processing.");
        }
    }
}
