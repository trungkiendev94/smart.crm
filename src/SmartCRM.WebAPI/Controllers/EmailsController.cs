using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCRM.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace SmartCRM.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailsController : ControllerBase
{
    private readonly SmartCrmDbContext _dbContext;

    public EmailsController(SmartCrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmailHistory()
    {
        var history = await _dbContext.SentEmails
            .OrderByDescending(e => e.SentAt)
            .Take(50)
            .ToListAsync();

        return Ok(history);
    }
}
