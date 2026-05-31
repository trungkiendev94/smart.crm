using System.Threading.Tasks;

namespace SmartCRM.Application.Common.Interfaces;

public interface ISystemSettingsService
{
    Task<string> GetSettingAsync(string key, string defaultValue = "");
    Task SaveSettingAsync(string key, string value);
}
