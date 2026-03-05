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
        command.Options.Add(GitOptionDefinitions.OrganizationName.AsOptional());
        command.Options.Add(GitOptionDefinitions.ProjectName.AsOptional());
        command.Options.Add(GitOptionDefinitions.RepositoryName.AsOptional());
        command.Options.Add(GitOptionDefinitions.BranchName.AsOptional());
        command.Options.Add(GitOptionDefinitions.DirectoryName.AsOptional());
        command.Options.Add(GitOptionDefinitions.GitProviderType.AsOptional());
    }

    protected override GitConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.WorkspaceId.Name);
        var workspaceName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;
        options.ConnectionId = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.ConnectionId.Name);
        options.OrganizationName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.OrganizationName.Name);
        options.ProjectName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.ProjectName.Name);
        options.RepositoryName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.RepositoryName.Name);
        options.BranchName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.BranchName.Name);
        options.DirectoryName = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.DirectoryName.Name);
        options.GitProviderType = parseResult.GetValueOrDefault<string>(GitOptionDefinitions.GitProviderType.Name);
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

            if (!string.IsNullOrWhiteSpace(options.OrganizationName) ||
                !string.IsNullOrWhiteSpace(options.ProjectName) ||
                !string.IsNullOrWhiteSpace(options.RepositoryName) ||
                !string.IsNullOrWhiteSpace(options.BranchName))
            {
                request.GitProviderDetails = new GitProviderDetails
                {
                    OrganizationName = options.OrganizationName,
                    ProjectName = options.ProjectName,
                    RepositoryName = options.RepositoryName,
                    BranchName = options.BranchName,
                    DirectoryName = options.DirectoryName ?? "/",
                    GitProviderType = options.GitProviderType ?? "AzureDevOps"
                };
            }

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
    public string? OrganizationName { get; set; }
    public string? ProjectName { get; set; }
    public string? RepositoryName { get; set; }
    public string? BranchName { get; set; }
    public string? DirectoryName { get; set; }
    public string? GitProviderType { get; set; }
}
