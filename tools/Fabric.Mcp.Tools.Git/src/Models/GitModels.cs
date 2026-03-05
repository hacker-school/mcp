// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Git.Models;

public class GitStatus
{
    [JsonPropertyName("workspaceHead")]
    public string? WorkspaceHead { get; set; }

    [JsonPropertyName("remoteCommitHash")]
    public string? RemoteCommitHash { get; set; }

    [JsonPropertyName("changes")]
    public List<GitChange>? Changes { get; set; }
}

public class GitChange
{
    [JsonPropertyName("remoteChange")]
    public string? RemoteChange { get; set; }

    [JsonPropertyName("workspaceChange")]
    public string? WorkspaceChange { get; set; }

    [JsonPropertyName("conflictType")]
    public string? ConflictType { get; set; }

    [JsonPropertyName("itemMetadata")]
    public GitItemMetadata? ItemMetadata { get; set; }
}

public class GitItemMetadata
{
    [JsonPropertyName("itemIdentifier")]
    public GitItemIdentifier? ItemIdentifier { get; set; }

    [JsonPropertyName("itemType")]
    public string? ItemType { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}

public class GitItemIdentifier
{
    [JsonPropertyName("logicalId")]
    public string? LogicalId { get; set; }

    [JsonPropertyName("objectId")]
    public string? ObjectId { get; set; }
}

public class GitConnection
{
    [JsonPropertyName("gitProviderDetails")]
    public GitProviderDetails? GitProviderDetails { get; set; }

    [JsonPropertyName("gitConnectionState")]
    public string? GitConnectionState { get; set; }

    [JsonPropertyName("gitSyncDetails")]
    public GitSyncDetails? GitSyncDetails { get; set; }
}

public class GitProviderDetails
{
    [JsonPropertyName("organizationName")]
    public string? OrganizationName { get; set; }

    [JsonPropertyName("projectName")]
    public string? ProjectName { get; set; }

    [JsonPropertyName("gitProviderType")]
    public string? GitProviderType { get; set; }

    [JsonPropertyName("repositoryName")]
    public string? RepositoryName { get; set; }

    [JsonPropertyName("branchName")]
    public string? BranchName { get; set; }

    [JsonPropertyName("directoryName")]
    public string? DirectoryName { get; set; }

    [JsonPropertyName("ownerName")]
    public string? OwnerName { get; set; }
}

public class GitSyncDetails
{
    [JsonPropertyName("head")]
    public string? Head { get; set; }

    [JsonPropertyName("lastSyncTime")]
    public string? LastSyncTime { get; set; }
}

public class InitializeConnectionResponse
{
    [JsonPropertyName("remoteCommitHash")]
    public string? RemoteCommitHash { get; set; }

    [JsonPropertyName("workspaceHead")]
    public string? WorkspaceHead { get; set; }

    [JsonPropertyName("requiredAction")]
    public string? RequiredAction { get; set; }
}

public class GitConnectRequest
{
    [JsonPropertyName("gitProviderDetails")]
    public GitProviderDetails? GitProviderDetails { get; set; }

    [JsonPropertyName("myGitCredentials")]
    public GitCredentials? MyGitCredentials { get; set; }
}

public class GitCredentials
{
    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

public class GitUpdateRequest
{
    [JsonPropertyName("remoteCommitHash")]
    public string? RemoteCommitHash { get; set; }

    [JsonPropertyName("workspaceHead")]
    public string? WorkspaceHead { get; set; }

    [JsonPropertyName("conflictResolution")]
    public ConflictResolution? ConflictResolution { get; set; }

    [JsonPropertyName("options")]
    public GitUpdateOptions? Options { get; set; }
}

public class ConflictResolution
{
    [JsonPropertyName("conflictResolutionType")]
    public string? ConflictResolutionType { get; set; }

    [JsonPropertyName("conflictResolutionPolicy")]
    public string? ConflictResolutionPolicy { get; set; }
}

public class GitUpdateOptions
{
    [JsonPropertyName("allowOverrideItems")]
    public bool? AllowOverrideItems { get; set; }
}

public class GitCommitRequest
{
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("workspaceHead")]
    public string? WorkspaceHead { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

public static class FabricEndpoints
{
    public const string FabricApiBaseUrl = "https://api.fabric.microsoft.com/v1";

    public static readonly string[] FabricScopes =
    [
        "https://api.fabric.microsoft.com/.default"
    ];
}
