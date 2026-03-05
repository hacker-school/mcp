// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Fabric.Mcp.Tools.Pipeline.Options;

public static class PipelineOptionDefinitions
{
    // Workspace options
    public const string WorkspaceName = "workspace";
    public static readonly Option<string> Workspace = new($"--{WorkspaceName}")
    {
        Description = "The name or ID of the Microsoft Fabric workspace.",
        Required = true
    };

    public const string WorkspaceIdName = "workspace-id";
    public static readonly Option<string> WorkspaceId = new($"--{WorkspaceIdName}")
    {
        Description = "The ID of the Microsoft Fabric workspace.",
        Required = true
    };

    // Pipeline item options
    public const string ItemName = "item";
    public static readonly Option<string> Item = new($"--{ItemName}")
    {
        Description = "The name or ID of the pipeline item.",
        Required = true
    };

    public const string ItemIdName = "item-id";
    public static readonly Option<string> ItemId = new($"--{ItemIdName}")
    {
        Description = "The ID of the pipeline item.",
        Required = true
    };

    // Job instance options
    public const string JobIdName = "job-id";
    public static readonly Option<string> JobId = new($"--{JobIdName}")
    {
        Description = "The job instance ID.",
        Required = true
    };
}
