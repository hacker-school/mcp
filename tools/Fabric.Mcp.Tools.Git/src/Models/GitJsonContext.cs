// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Fabric.Mcp.Tools.Git.Commands.Connection;
using Fabric.Mcp.Tools.Git.Commands.Status;
using Fabric.Mcp.Tools.Git.Commands.Sync;

namespace Fabric.Mcp.Tools.Git.Models;

[JsonSerializable(typeof(GitStatus))]
[JsonSerializable(typeof(GitChange))]
[JsonSerializable(typeof(GitItemMetadata))]
[JsonSerializable(typeof(GitItemIdentifier))]
[JsonSerializable(typeof(GitConnection))]
[JsonSerializable(typeof(GitProviderDetails))]
[JsonSerializable(typeof(GitSyncDetails))]
[JsonSerializable(typeof(InitializeConnectionResponse))]
[JsonSerializable(typeof(GitConnectRequest))]
[JsonSerializable(typeof(GitCredentials))]
[JsonSerializable(typeof(GitUpdateRequest))]
[JsonSerializable(typeof(ConflictResolution))]
[JsonSerializable(typeof(GitUpdateOptions))]
[JsonSerializable(typeof(GitCommitRequest))]
[JsonSerializable(typeof(GitStatusCommand.GitStatusCommandResult))]
[JsonSerializable(typeof(GitConnectionGetCommand.GitConnectionGetCommandResult))]
[JsonSerializable(typeof(GitConnectCommand.GitConnectCommandResult))]
[JsonSerializable(typeof(GitDisconnectCommand.GitDisconnectCommandResult))]
[JsonSerializable(typeof(GitInitializeCommand.GitInitializeCommandResult))]
[JsonSerializable(typeof(GitUpdateFromGitCommand.GitUpdateFromGitCommandResult))]
[JsonSerializable(typeof(GitCommitCommand.GitCommitCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class GitJsonContext : JsonSerializerContext;
