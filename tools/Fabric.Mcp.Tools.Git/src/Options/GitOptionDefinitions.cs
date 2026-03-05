// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Fabric.Mcp.Tools.Git.Options;

public static class GitOptionDefinitions
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
        Description = "The ID (GUID) of the Microsoft Fabric workspace.",
        Required = true
    };

    // Git commit options
    public const string CommitMessageName = "commit-message";
    public static readonly Option<string> CommitMessage = new($"--{CommitMessageName}")
    {
        Description = "The commit message to use when committing workspace changes to Git.",
        Required = false
    };

    // Git sync options
    public const string AllowOverrideName = "allow-override";
    public static readonly Option<bool> AllowOverride = new($"--{AllowOverrideName}")
    {
        Description = "Whether to allow overriding workspace items during update from Git.",
        Required = false
    };

    public const string RemoteCommitHashName = "remote-commit-hash";
    public static readonly Option<string> RemoteCommitHash = new($"--{RemoteCommitHashName}")
    {
        Description = "The remote Git commit hash to update from or use as reference.",
        Required = false
    };

    public const string WorkspaceHeadName = "workspace-head";
    public static readonly Option<string> WorkspaceHead = new($"--{WorkspaceHeadName}")
    {
        Description = "The workspace head commit hash for conflict detection.",
        Required = false
    };

    // Connection options
    public const string ConnectionIdName = "connection-id";
    public static readonly Option<string> ConnectionId = new($"--{ConnectionIdName}")
    {
        Description = "The connection ID for Git credentials.",
        Required = false
    };

    // Git provider detail options
    public const string OrganizationNameName = "organization-name";
    public static readonly Option<string> OrganizationName = new($"--{OrganizationNameName}")
    {
        Description = "The Git organization name (e.g. Azure DevOps organization).",
        Required = false
    };

    public const string ProjectNameName = "project-name";
    public static readonly Option<string> ProjectName = new($"--{ProjectNameName}")
    {
        Description = "The Git project name.",
        Required = false
    };

    public const string RepositoryNameName = "repository-name";
    public static readonly Option<string> RepositoryName = new($"--{RepositoryNameName}")
    {
        Description = "The Git repository name.",
        Required = false
    };

    public const string BranchNameName = "branch-name";
    public static readonly Option<string> BranchName = new($"--{BranchNameName}")
    {
        Description = "The Git branch name to connect to.",
        Required = false
    };

    public const string DirectoryNameName = "directory-name";
    public static readonly Option<string> DirectoryName = new($"--{DirectoryNameName}")
    {
        Description = "The directory in the Git repository (defaults to '/').",
        Required = false
    };

    public const string GitProviderTypeName = "git-provider-type";
    public static readonly Option<string> GitProviderType = new($"--{GitProviderTypeName}")
    {
        Description = "The Git provider type (defaults to 'AzureDevOps').",
        Required = false
    };
}
