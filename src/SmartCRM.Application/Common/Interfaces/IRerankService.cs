using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCRM.Application.Common.Interfaces;

public interface IRerankService
{
    /// <summary>
    /// Reranks the retrieved documents based on their relevance to the query.
    /// </summary>
    /// <param name="query">The original user query.</param>
    /// <param name="documents">The list of retrieved documents (chunks).</param>
    /// <param name="topK">The number of top documents to return.</param>
    /// <returns>A list of reranked and filtered documents.</returns>
    Task<List<string>> RerankAsync(string query, List<string> documents, int topK = 3);
}
