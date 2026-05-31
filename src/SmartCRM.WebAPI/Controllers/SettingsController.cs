using Microsoft.AspNetCore.Mvc;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Domain.Entities;
using System.Threading.Tasks;

namespace SmartCRM.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;

    public SettingsController(ISystemSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetSetting(string key)
    {
        var value = await _settingsService.GetSettingAsync(key);
        return Ok(new { key, value });
    }

    [HttpPost]
    public async Task<IActionResult> SaveSetting([FromBody] SystemSetting request)
    {
        await _settingsService.SaveSettingAsync(request.Key, request.Value);
        return Ok(new { message = "Setting saved successfully." });
    }
}
