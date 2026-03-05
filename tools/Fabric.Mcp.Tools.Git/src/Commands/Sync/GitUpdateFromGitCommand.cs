// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.Git.Models;
using Fabric.Mcp.Tools.Git.Options;
using Fabric.Mcp.Tools.Git.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.Git.Commands.Sync;

public sealed class GitUpdateFromGitCommand(
    ILogger<GitUpdateFromGitCommand> logger,
    IGitService gitService) : GlobalCommand<GitUpdateFromGitOptions>()
{
    private readonly ILogger<GitUpdateFromGitCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IGitService _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    public override string Id => "a1b2c3d4-1001-4000-8000-000000000006";
    public override string Name => "update";
    public override string Title => "Update Workspace from Git";
    public override string Description => "Update a Fabric workspace from its connected Git repository. This is a destructive long-running operation that may override workspace items.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(GitOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(GitOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(GitOptionDefinitions.RemoteCommitHash.AsOptional());
        command.Options.Add(GitOptionDefinitions.WorkspaceHead.AsOptional());
        command.Options.Add(GitOptionDefinitions.AllowOverride.AsOptional());
    }

    protected override GitUpdateFromGitOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceIdName);
        var workspaceName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceName);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;
        options.RemoteCommitHash = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.RemoteCommitHashName);
        options.WorkspaceHead = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceHeadName);
        options.AllowOverride = parseResult.GetValueOrDefault<bool>(GitOptionDefinitions.AllowOverrideName);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            if (string.IsNullOrWhiteSpace(options.WorkspaceId))
            {
                throw new ArgumentException(
                    "Workspace identifier is required. Provide --workspace or --workspace-id.",
                    nameof(options.WorkspaceId));
            }

            var request = new GitUpdateRequest
            {
                RemoteCommitHash = options.RemoteCommitHash,
                WorkspaceHead = options.WorkspaceHead,
                ConflictResolution = new ConflictResolution
                {
                    ConflictResolutionType = "Workspace",
                    ConflictResolutionPolicy = "PreferRemote"
                },
                Options = new GitUpdateOptions
                {
                    AllowOverrideItems = options.AllowOverride
                }
            };

            await _gitService.UpdateFromGitAsync(options.WorkspaceId, request, cancellationToken);

            _logger.LogInformation(
                "Successfully updated workspace {WorkspaceId} from Git",
                options.WorkspaceId);

            var result = new GitUpdateFromGitCommandResult($"Workspace {options.WorkspaceId} updated from Git successfully.");
            context.Response.Results = ResponseResult.Create(result, GitJsonContext.Default.GitUpdateFromGitCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workspace {WorkspaceId} from Git. Options: {@Options}",
                options.WorkspaceId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GitUpdateFromGitCommandResult(string Message);
}

public sealed class GitUpdateFromGitOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string? RemoteCommitHash { get; set; }
    public string? WorkspaceHead { get; set; }
    public bool AllowOverride { get; set; }
}
