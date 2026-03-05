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

public sealed class GitConnectionGetCommand(
    ILogger<GitConnectionGetCommand> logger,
    IGitService gitService) : GlobalCommand<GitConnectionGetOptions>()
{
    private readonly ILogger<GitConnectionGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IGitService _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    public override string Id => "a1b2c3d4-1001-4000-8000-000000000002";
    public override string Name => "get";
    public override string Title => "Get Git Connection";
    public override string Description => "Get the Git connection details for a Fabric workspace, including provider, repository, and branch information.";

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

    protected override GitConnectionGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceId.Name);
        var workspaceName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.Workspace.Name);
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

            var connection = await _gitService.GetConnectionAsync(options.WorkspaceId, cancellationToken);

            _logger.LogInformation(
                "Retrieved Git connection for workspace {WorkspaceId}",
                options.WorkspaceId);

            var result = new GitConnectionGetCommandResult(connection);
            context.Response.Results = ResponseResult.Create(result, GitJsonContext.Default.GitConnectionGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Git connection for workspace {WorkspaceId}. Options: {@Options}",
                options.WorkspaceId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GitConnectionGetCommandResult(GitConnection Connection);
}

public sealed class GitConnectionGetOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
}
