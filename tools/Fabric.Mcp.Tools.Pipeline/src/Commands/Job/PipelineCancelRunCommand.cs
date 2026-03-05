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

public sealed class PipelineCancelRunCommand(
    ILogger<PipelineCancelRunCommand> logger,
    IPipelineService pipelineService) : GlobalCommand<PipelineCancelRunOptions>()
{
    private readonly ILogger<PipelineCancelRunCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

    public override string Id => "b2c3d4e5-2001-4000-8000-000000000003";
    public override string Name => "cancel-run";
    public override string Title => "Cancel Pipeline Run";
    public override string Description => "Cancel a running pipeline job instance. This is a destructive operation that stops execution.";

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
        command.Options.Add(PipelineOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(PipelineOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(PipelineOptionDefinitions.Item.AsOptional());
        command.Options.Add(PipelineOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(PipelineOptionDefinitions.JobId.AsOptional());
    }

    protected override PipelineCancelRunOptions BindOptions(ParseResult parseResult)
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

        options.JobId = parseResult.GetValueOrDefault<string>(PipelineOptionDefinitions.JobIdName) ?? string.Empty;

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

            if (string.IsNullOrWhiteSpace(options.JobId))
            {
                throw new ArgumentException(
                    "Job instance ID is required. Provide --job-id.",
                    nameof(options.JobId));
            }

            await _pipelineService.CancelRunAsync(
                options.WorkspaceId, options.ItemId, options.JobId, cancellationToken);

            _logger.LogInformation(
                "Cancelled pipeline run for workspace {WorkspaceId}, item {ItemId}, job {JobId}",
                options.WorkspaceId, options.ItemId, options.JobId);

            var result = new PipelineCancelRunCommandResult(
                $"Pipeline job {options.JobId} cancelled successfully.");
            context.Response.Results = ResponseResult.Create(result, PipelineJsonContext.Default.PipelineCancelRunCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling pipeline run for workspace {WorkspaceId}, item {ItemId}, job {JobId}. Options: {@Options}",
                options.WorkspaceId, options.ItemId, options.JobId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record PipelineCancelRunCommandResult(string Message);
}

public sealed class PipelineCancelRunOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string JobId { get; set; } = string.Empty;
}
