// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Git.Models;

namespace Fabric.Mcp.Tools.Git.Services;

/// <summary>
/// Service interface for Fabric Git integration operations.
/// </summary>
public interface IGitService
{
    Task<GitStatus> GetStatusAsync(string workspaceId, CancellationToken cancellationToken = default);

    Task<GitConnection> GetConnectionAsync(string workspaceId, CancellationToken cancellationToken = default);

    Task ConnectAsync(string workspaceId, GitConnectRequest request, CancellationToken cancellationToken = default);

    Task DisconnectAsync(string workspaceId, CancellationToken cancellationToken = default);

    Task<InitializeConnectionResponse> InitializeConnectionAsync(string workspaceId, CancellationToken cancellationToken = default);

    Task UpdateFromGitAsync(string workspaceId, GitUpdateRequest request, CancellationToken cancellationToken = default);

    Task CommitToGitAsync(string workspaceId, GitCommitRequest request, CancellationToken cancellationToken = default);
}
