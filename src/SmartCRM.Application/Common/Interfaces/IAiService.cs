using System.Threading.Tasks;
using SmartCRM.Application.Models;

namespace SmartCRM.Application.Common.Interfaces;

public interface IAiService
{
    Task<ChatResponse> GetResponseAsync(string prompt);
    Task<string> ExecuteAgentTaskAsync(string taskDescription);
}
