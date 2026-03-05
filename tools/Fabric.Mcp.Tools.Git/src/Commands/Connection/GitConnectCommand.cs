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

public sealed class GitConnectCommand(
    ILogger<GitConnectCommand> logger,
    IGitService gitService) : GlobalCommand<GitConnectOptions>()
{
    private readonly ILogger<GitConnectCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IGitService _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    public override string Id => "a1b2c3d4-1001-4000-8000-000000000003";
    public override string Name => "connect";
    public override string Title => "Connect Workspace to Git";
    public override string Description => "Connect a Fabric workspace to a Git repository using the specified connection credentials.";

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
        command.Options.Add(GitOptionDefinitions.ConnectionId.AsOptional());
    }

    protected override GitConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceIdName);
        var workspaceName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceName);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;
        options.ConnectionId = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.ConnectionIdName);
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

            var request = new GitConnectRequest();

            if (!string.IsNullOrWhiteSpace(options.ConnectionId))
            {
                request.MyGitCredentials = new GitCredentials
                {
                    ConnectionId = options.ConnectionId,
                    Source = "ConfiguredConnection"
                };
            }

            await _gitService.ConnectAsync(options.WorkspaceId, request, cancellationToken);

            _logger.LogInformation(
                "Successfully connected workspace {WorkspaceId} to Git",
                options.WorkspaceId);

            var result = new GitConnectCommandResult($"Workspace {options.WorkspaceId} connected to Git successfully.");
            context.Response.Results = ResponseResult.Create(result, GitJsonContext.Default.GitConnectCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting workspace {WorkspaceId} to Git. Options: {@Options}",
                options.WorkspaceId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GitConnectCommandResult(string Message);
}

public sealed class GitConnectOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
}
