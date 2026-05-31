using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Domain.Entities;
using SmartCRM.Infrastructure.Persistence;

namespace SmartCRM.Infrastructure.Services;

public sealed class SystemSettingsService : ISystemSettingsService
{
    private readonly SmartCrmDbContext _context;

    public SystemSettingsService(SmartCrmDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetSettingAsync(string key, string defaultValue = "")
    {
        var setting = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key);
        return setting?.Value ?? defaultValue;
    }

    public async Task SaveSettingAsync(string key, string value)
    {
        var existing = await _context.SystemSettings.FirstOrDefaultAsync(x => x.Key == key);
        if (existing != null)
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
        }

        await _context.SaveChangesAsync();
    }
}
