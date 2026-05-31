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

        var response = await _aiService.GetResponseAsync(request.Message);
        System.Console.WriteLine($"[ChatController] Response ready.");
        return Ok(response);
    }
}
