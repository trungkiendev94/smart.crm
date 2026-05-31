using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace SmartCRM.Application.Services;

public class ToolTrackingFilter : IFunctionInvocationFilter
{
    public List<string> ExecutedTools { get; } = new();

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        ExecutedTools.Add(context.Function.Name);
        Console.WriteLine($"[ToolTracker] Logic: Executing {context.Function.Name}");
        await next(context);
    }
}
