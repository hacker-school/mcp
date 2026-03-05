// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Pipeline.Models;

public class PipelineRunResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }
}

public class JobInstance
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }

    [JsonPropertyName("jobType")]
    public string? JobType { get; set; }

    [JsonPropertyName("invokeType")]
    public string? InvokeType { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("startTimeUtc")]
    public string? StartTimeUtc { get; set; }

    [JsonPropertyName("endTimeUtc")]
    public string? EndTimeUtc { get; set; }

    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }
}

public static class FabricEndpoints
{
    public const string FabricApiBaseUrl = "https://api.fabric.microsoft.com/v1";

    public static readonly string[] FabricScopes =
    [
        "https://api.fabric.microsoft.com/.default"
    ];
}
