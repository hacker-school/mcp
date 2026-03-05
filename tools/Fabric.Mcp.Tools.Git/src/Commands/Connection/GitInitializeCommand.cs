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

public sealed class GitInitializeCommand(
    ILogger<GitInitializeCommand> logger,
    IGitService gitService) : GlobalCommand<GitInitializeOptions>()
{
    private readonly ILogger<GitInitializeCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IGitService _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    public override string Id => "a1b2c3d4-1001-4000-8000-000000000005";
    public override string Name => "initialize";
    public override string Title => "Initialize Git Connection";
    public override string Description => "Initialize the Git connection for a Fabric workspace, returning required action and commit hashes for subsequent sync operations.";

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
    }

    protected override GitInitializeOptions BindOptions(ParseResult parseResult)
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

            var response = await _gitService.InitializeConnectionAsync(options.WorkspaceId, cancellationToken);

            _logger.LogInformation(
                "Initialized Git connection for workspace {WorkspaceId}. Required action: {RequiredAction}",
                options.WorkspaceId, response.RequiredAction);

            var result = new GitInitializeCommandResult(response);
            context.Response.Results = ResponseResult.Create(result, GitJsonContext.Default.GitInitializeCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Git connection for workspace {WorkspaceId}. Options: {@Options}",
                options.WorkspaceId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GitInitializeCommandResult(InitializeConnectionResponse InitializeResponse);
}

public sealed class GitInitializeOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
}
