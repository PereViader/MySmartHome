using Microsoft.Extensions.Options;

namespace PereViader.MusicCaster.Authentication;

public class ApiKeyEndpointFilterOptions
{
    public required string Key { get; init; }
}

public class ApiKeyEndpointFilter(IOptions<ApiKeyEndpointFilterOptions> configuration) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Query.TryGetValue("key", out var extractedApiKey))
        {
            return Results.Unauthorized();
        }

        if (!configuration.Value.Key.Equals(extractedApiKey))
        {
            return Results.Unauthorized();
        }
        
        return await next(context);
    }
}