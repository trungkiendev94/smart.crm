using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace SmartCRM.Application.Services;

public class PiiFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context);

        // Simple PII masking: Mask simple credit card numbers or phone numbers in prompt outputs if needed.
        // Actually, PromptRenderFilter intercepts the prompt *before* sending to LLM.
        // We can mask PII in the rendered prompt to avoid sending it to the model.
        if (context.RenderedPrompt != null)
        {
            // Mask CC numbers
            context.RenderedPrompt = Regex.Replace(context.RenderedPrompt, @"\b(?:\d[ -]*?){13,16}\b", "[REDACTED CC]");
        }
    }
}

public class GuardrailFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Intercept function arguments
        foreach (var arg in context.Arguments)
        {
            if (arg.Value is string strValue && strValue.Contains("DROP TABLE", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Blocked malicious intent: SQL Injection attempt detected in tool arguments.");
            }
        }

        await next(context);
        
        // Guardrail on function result
        if (context.Result?.ValueType == typeof(string))
        {
            var resultStr = context.Result.GetValue<string>();
            if (resultStr != null && resultStr.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new FunctionResult(context.Function, "Sensitive information has been hidden.");
            }
        }
    }
}