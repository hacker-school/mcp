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

namespace Fabric.Mcp.Tools.Git.Commands.Connection;

public sealed class GitDisconnectCommand(
    ILogger<GitDisconnectCommand> logger,
    IGitService gitService) : GlobalCommand<GitDisconnectOptions>()
{
    private readonly ILogger<GitDisconnectCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IGitService _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    public override string Id => "a1b2c3d4-1001-4000-8000-000000000004";
    public override string Name => "disconnect";
    public override string Title => "Disconnect Workspace from Git";
    public override string Description => "Disconnect a Fabric workspace from its connected Git repository. This is a destructive operation.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
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
    }

    protected override GitDisconnectOptions BindOptions(ParseResult parseResult)
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

            await _gitService.DisconnectAsync(options.WorkspaceId, cancellationToken);

            _logger.LogInformation(
                "Successfully disconnected workspace {WorkspaceId} from Git",
                options.WorkspaceId);

            var result = new GitDisconnectCommandResult($"Workspace {options.WorkspaceId} disconnected from Git successfully.");
            context.Response.Results = ResponseResult.Create(result, GitJsonContext.Default.GitDisconnectCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting workspace {WorkspaceId} from Git. Options: {@Options}",
                options.WorkspaceId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GitDisconnectCommandResult(string Message);
}

public sealed class GitDisconnectOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
}
