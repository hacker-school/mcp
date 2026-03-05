// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Pipeline.Models;

namespace Fabric.Mcp.Tools.Pipeline.Services;

public class PipelineService(HttpClient httpClient, TokenCredential? credential = null) : IPipelineService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly TokenCredential _credential = credential ?? new DefaultAzureCredential();
    private const string UserAgentHeaderName = "User-Agent";
    private const string UserAgentHeaderValue = "Fabric Pipeline MCP";

    public async Task<PipelineRunResponse> RunAsync(
        string workspaceId,
        string itemId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/items/{itemId}/jobs/instances?jobType=Pipeline";
        var response = await SendFabricApiRawResponseAsync(HttpMethod.Post, url, cancellationToken: cancellationToken);

        var location = response.Headers.Location?.ToString();
        var jobId = !string.IsNullOrEmpty(location)
            ? location.Split('/').Last()
            : null;

        return new PipelineRunResponse
        {
            Id = jobId,
            Location = location
        };
    }

    public async Task<JobInstance> GetRunAsync(
        string workspaceId,
        string itemId,
        string jobId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/items/{itemId}/jobs/instances/{jobId}";
        var stream = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(stream, PipelineJsonContext.Default.JobInstance, cancellationToken)
            ?? new JobInstance();
    }

    public async Task CancelRunAsync(
        string workspaceId,
        string itemId,
        string jobId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/items/{itemId}/jobs/instances/{jobId}/cancel";
        using var _ = await SendFabricApiRequestAsync(HttpMethod.Post, url, cancellationToken: cancellationToken);
    }

    private async Task<HttpResponseMessage> SendFabricApiRawResponseAsync(
        HttpMethod method,
        string url,
        string? jsonContent = null,
        CancellationToken cancellationToken = default)
    {
        var tokenContext = new TokenRequestContext(FabricEndpoints.FabricScopes);
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        ApplyUserAgent(request);

        if (!string.IsNullOrEmpty(jsonContent))
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return response;
    }

    private async Task<Stream> SendFabricApiRequestAsync(
        HttpMethod method,
        string url,
        string? jsonContent = null,
        CancellationToken cancellationToken = default)
    {
        var tokenContext = new TokenRequestContext(FabricEndpoints.FabricScopes);
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        ApplyUserAgent(request);

        if (!string.IsNullOrEmpty(jsonContent))
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private static void ApplyUserAgent(HttpRequestMessage request)
    {
        if (request.Headers.Contains(UserAgentHeaderName))
        {
            request.Headers.Remove(UserAgentHeaderName);
        }

        request.Headers.TryAddWithoutValidation(UserAgentHeaderName, UserAgentHeaderValue);
    }
}
