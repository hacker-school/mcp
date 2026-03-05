// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Pipeline.Models;

namespace Fabric.Mcp.Tools.Pipeline.Services;

/// <summary>
/// Service interface for Fabric Pipeline job orchestration operations.
/// </summary>
public interface IPipelineService
{
    Task<PipelineRunResponse> RunAsync(string workspaceId, string itemId, CancellationToken cancellationToken = default);

    Task<JobInstance> GetRunAsync(string workspaceId, string itemId, string jobId, CancellationToken cancellationToken = default);

    Task CancelRunAsync(string workspaceId, string itemId, string jobId, CancellationToken cancellationToken = default);
}
