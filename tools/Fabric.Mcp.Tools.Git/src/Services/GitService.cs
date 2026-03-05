// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Git.Models;

namespace Fabric.Mcp.Tools.Git.Services;

public class GitService(HttpClient httpClient, TokenCredential? credential = null) : IGitService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly TokenCredential _credential = credential ?? new DefaultAzureCredential();
    private const string UserAgentHeaderName = "User-Agent";
    private const string UserAgentHeaderValue = "Fabric Git MCP";

    public async Task<GitStatus> GetStatusAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/git/status";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(response, GitJsonContext.Default.GitStatus, cancellationToken)
            ?? new GitStatus();
    }

    public async Task<GitConnection> GetConnectionAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/git/connection";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(response, GitJsonContext.Default.GitConnection, cancellationToken)
            ?? new GitConnection();
    }

    public async Task ConnectAsync(
        string workspaceId,
        GitConnectRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/git/connect";
        var jsonContent = JsonSerializer.Serialize(request, GitJsonContext.Default.GitConnectRequest);
        using var _ = await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, cancellationToken);
    }

    public async Task DisconnectAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/git/disconnect";
        using var _ = await SendFabricApiRequestAsync(HttpMethod.Post, url, cancellationToken: cancellationToken);
    }

    public async Task<InitializeConnectionResponse> InitializeConnectionAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/git/initializeConnection";
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, "{}", cancellationToken);
        return await JsonSerializer.DeserializeAsync(response, GitJsonContext.Default.InitializeConnectionResponse, cancellationToken)
            ?? new InitializeConnectionResponse();
    }

    public async Task UpdateFromGitAsync(
        string workspaceId,
        GitUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/git/updateFromGit";
        var jsonContent = JsonSerializer.Serialize(request, GitJsonContext.Default.GitUpdateRequest);
        await SendLongRunningOperationAsync(HttpMethod.Post, url, jsonContent, cancellationToken);
    }

    public async Task CommitToGitAsync(
        string workspaceId,
        GitCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{workspaceId}/git/commitToGit";
        var jsonContent = JsonSerializer.Serialize(request, GitJsonContext.Default.GitCommitRequest);
        await SendLongRunningOperationAsync(HttpMethod.Post, url, jsonContent, cancellationToken);
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

    private async Task SendLongRunningOperationAsync(
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

        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            var locationHeader = response.Headers.Location?.ToString();
            if (!string.IsNullOrEmpty(locationHeader))
            {
                await PollOperationAsync(locationHeader, cancellationToken);
            }

            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private async Task PollOperationAsync(
        string operationUrl,
        CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 30;
        const int delayMilliseconds = 2000;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            await Task.Delay(delayMilliseconds, cancellationToken);

            var tokenContext = new TokenRequestContext(FabricEndpoints.FabricScopes);
            var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, operationUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            ApplyUserAgent(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (content.Contains("\"status\":\"Succeeded\"", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("\"status\":\"Completed\"", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (content.Contains("\"status\":\"Failed\"", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Long-running operation failed. Response: {content}");
            }
        }

        throw new TimeoutException(
            $"Long-running operation did not complete within {maxAttempts * delayMilliseconds / 1000} seconds.");
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
