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

public sealed class GitCommitCommand(
    ILogger<GitCommitCommand> logger,
    IGitService gitService) : GlobalCommand<GitCommitOptions>()
{
    private readonly ILogger<GitCommitCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IGitService _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    public override string Id => "a1b2c3d4-1001-4000-8000-000000000007";
    public override string Name => "commit";
    public override string Title => "Commit Workspace to Git";
    public override string Description => "Commit workspace changes to the connected Git repository. This is a long-running operation.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
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
        command.Options.Add(GitOptionDefinitions.CommitMessage.AsOptional());
        command.Options.Add(GitOptionDefinitions.WorkspaceHead.AsOptional());
    }

    protected override GitCommitOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceIdName);
        var workspaceName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceName);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;
        options.CommitMessage = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.CommitMessageName);
        options.WorkspaceHead = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceHeadName);
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

            var request = new GitCommitRequest
            {
                Mode = "All",
                WorkspaceHead = options.WorkspaceHead,
                Comment = options.CommitMessage
            };

            await _gitService.CommitToGitAsync(options.WorkspaceId, request, cancellationToken);

            _logger.LogInformation(
                "Successfully committed workspace {WorkspaceId} changes to Git",
                options.WorkspaceId);

            var result = new GitCommitCommandResult($"Workspace {options.WorkspaceId} changes committed to Git successfully.");
            context.Response.Results = ResponseResult.Create(result, GitJsonContext.Default.GitCommitCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing workspace {WorkspaceId} to Git. Options: {@Options}",
                options.WorkspaceId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GitCommitCommandResult(string Message);
}

public sealed class GitCommitOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string? CommitMessage { get; set; }
    public string? WorkspaceHead { get; set; }
}
