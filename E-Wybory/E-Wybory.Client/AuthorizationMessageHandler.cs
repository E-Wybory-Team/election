using System.Net.Http.Headers;
using Microsoft.JSInterop;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public AuthorizationMessageHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string token = null;

        try
        {
            token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"localStorage access failed: {ex.Message}");
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
