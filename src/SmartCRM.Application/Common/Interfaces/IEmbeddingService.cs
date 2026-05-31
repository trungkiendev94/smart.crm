using System.Threading;
using System.Threading.Tasks;

namespace SmartCRM.Application.Common.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
