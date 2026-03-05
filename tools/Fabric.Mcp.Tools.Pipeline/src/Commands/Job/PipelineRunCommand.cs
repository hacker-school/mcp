// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.Pipeline.Models;
using Fabric.Mcp.Tools.Pipeline.Options;
using Fabric.Mcp.Tools.Pipeline.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.Pipeline.Commands.Job;

public sealed class PipelineRunCommand(
    ILogger<PipelineRunCommand> logger,
    IPipelineService pipelineService) : GlobalCommand<PipelineRunOptions>()
{
    private readonly ILogger<PipelineRunCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

    public override string Id => "b2c3d4e5-2001-4000-8000-000000000001";
    public override string Name => "run";
    public override string Title => "Run Pipeline";
    public override string Description => "Trigger a pipeline execution in a Fabric workspace. Returns the job instance ID for tracking.";

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
        command.Options.Add(PipelineOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(PipelineOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(PipelineOptionDefinitions.Item.AsOptional());
        command.Options.Add(PipelineOptionDefinitions.ItemId.AsOptional());
    }

    protected override PipelineRunOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(PipelineOptionDefinitions.WorkspaceIdName);
        var workspaceName = parseResult.GetValueOrDefault<string>(PipelineOptionDefinitions.WorkspaceName);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;

        var itemId = parseResult.GetValueOrDefault<string>(PipelineOptionDefinitions.ItemIdName);
        var itemName = parseResult.GetValueOrDefault<string>(PipelineOptionDefinitions.ItemName);
        options.ItemId = !string.IsNullOrWhiteSpace(itemId)
            ? itemId!
            : itemName ?? string.Empty;

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

            if (string.IsNullOrWhiteSpace(options.ItemId))
            {
                throw new ArgumentException(
                    "Pipeline item identifier is required. Provide --item or --item-id.",
                    nameof(options.ItemId));
            }

            var response = await _pipelineService.RunAsync(options.WorkspaceId, options.ItemId, cancellationToken);

            _logger.LogInformation(
                "Triggered pipeline run for workspace {WorkspaceId}, item {ItemId}, job {JobId}",
                options.WorkspaceId, options.ItemId, response.Id);

            var result = new PipelineRunCommandResult(response);
            context.Response.Results = ResponseResult.Create(result, PipelineJsonContext.Default.PipelineRunCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running pipeline for workspace {WorkspaceId}, item {ItemId}. Options: {@Options}",
                options.WorkspaceId, options.ItemId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record PipelineRunCommandResult(PipelineRunResponse Run);
}

public sealed class PipelineRunOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
}
