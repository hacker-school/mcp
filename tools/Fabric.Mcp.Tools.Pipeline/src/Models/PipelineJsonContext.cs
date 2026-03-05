// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Fabric.Mcp.Tools.Pipeline.Commands.Job;

namespace Fabric.Mcp.Tools.Pipeline.Models;

[JsonSerializable(typeof(PipelineRunResponse))]
[JsonSerializable(typeof(JobInstance))]
[JsonSerializable(typeof(PipelineRunCommand.PipelineRunCommandResult))]
[JsonSerializable(typeof(PipelineGetRunCommand.PipelineGetRunCommandResult))]
[JsonSerializable(typeof(PipelineCancelRunCommand.PipelineCancelRunCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class PipelineJsonContext : JsonSerializerContext;
