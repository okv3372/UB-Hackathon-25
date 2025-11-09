using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

namespace SmartStudy.API.SemanticKernel;

public class SemanticKernelService
{
    private readonly Kernel _kernel;

    public SemanticKernelService(IOptions<ModelSettings> options)
    {
        var cfg = options.Value ?? throw new ArgumentNullException(nameof(options));

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: cfg.ModelName,
            endpoint: cfg.Endpoint,
            apiKey: cfg.ApiKey,
            apiVersion: "2025-01-01-preview"
        );

        _kernel = builder.Build();
    }

    public async Task<string> PromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
        return result.GetValue<string>() ?? string.Empty;
    }
}
