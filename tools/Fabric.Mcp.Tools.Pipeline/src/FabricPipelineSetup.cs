// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Pipeline.Commands.Job;
using Fabric.Mcp.Tools.Pipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Pipeline;

public class FabricPipelineSetup : IAreaSetup
{
    public string Name => "pipeline";
    public string Title => "Microsoft Fabric Pipeline";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPipelineService, PipelineService>();
        services.AddHttpClient<PipelineService>();

        // Register job commands
        services.AddSingleton<PipelineRunCommand>();
        services.AddSingleton<PipelineGetRunCommand>();
        services.AddSingleton<PipelineCancelRunCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricPipeline = new CommandGroup(Name,
            """
            Microsoft Fabric Pipeline - Manage pipeline job orchestration for Fabric workspaces.
            Fabric pipelines enable data movement and transformation workflows. Use this tool
            when you need to:
            - Trigger a pipeline execution
            - Check the status of a running or completed pipeline job
            - Cancel a running pipeline job
            This tool provides operations for managing the pipeline job lifecycle.
            """);

        var jobGroup = new CommandGroup("job",
            "Job operations - Commands for running, monitoring, and cancelling pipeline jobs.");
        fabricPipeline.AddSubGroup(jobGroup);

        var pipelineRunCommand = serviceProvider.GetRequiredService<PipelineRunCommand>();
        jobGroup.AddCommand(pipelineRunCommand.Name, pipelineRunCommand);

        var pipelineGetRunCommand = serviceProvider.GetRequiredService<PipelineGetRunCommand>();
        jobGroup.AddCommand(pipelineGetRunCommand.Name, pipelineGetRunCommand);

        var pipelineCancelRunCommand = serviceProvider.GetRequiredService<PipelineCancelRunCommand>();
        jobGroup.AddCommand(pipelineCancelRunCommand.Name, pipelineCancelRunCommand);

        return fabricPipeline;
    }
}
