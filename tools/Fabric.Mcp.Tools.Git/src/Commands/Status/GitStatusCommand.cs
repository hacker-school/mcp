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

namespace Fabric.Mcp.Tools.Git.Commands.Status;

public sealed class GitStatusCommand(
    ILogger<GitStatusCommand> logger,
    IGitService gitService) : GlobalCommand<GitStatusOptions>()
{
    private readonly ILogger<GitStatusCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IGitService _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    public override string Id => "a1b2c3d4-1001-4000-8000-000000000001";
    public override string Name => "get";
    public override string Title => "Get Git Status";
    public override string Description => "Get the Git sync status for a Fabric workspace, including pending changes and commit information.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(GitOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(GitOptionDefinitions.WorkspaceId.AsOptional());
    }

    protected override GitStatusOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceIdName);
        var workspaceName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceName);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;
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

            var status = await _gitService.GetStatusAsync(options.WorkspaceId, cancellationToken);

            _logger.LogInformation(
                "Retrieved Git status for workspace {WorkspaceId}",
                options.WorkspaceId);

            var result = new GitStatusCommandResult(status);
            context.Response.Results = ResponseResult.Create(result, GitJsonContext.Default.GitStatusCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Git status for workspace {WorkspaceId}. Options: {@Options}",
                options.WorkspaceId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GitStatusCommandResult(GitStatus Status);
}

public sealed class GitStatusOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
}
