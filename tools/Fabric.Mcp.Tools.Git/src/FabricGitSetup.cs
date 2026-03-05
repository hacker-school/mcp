// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Git.Commands.Connection;
using Fabric.Mcp.Tools.Git.Commands.Status;
using Fabric.Mcp.Tools.Git.Commands.Sync;
using Fabric.Mcp.Tools.Git.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Git;

public class FabricGitSetup : IAreaSetup
{
    public string Name => "git";
    public string Title => "Microsoft Fabric Git Integration";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IGitService, GitService>();
        services.AddHttpClient<GitService>();

        // Register status commands
        services.AddSingleton<GitStatusCommand>();

        // Register connection commands
        services.AddSingleton<GitConnectionGetCommand>();
        services.AddSingleton<GitConnectCommand>();
        services.AddSingleton<GitDisconnectCommand>();
        services.AddSingleton<GitInitializeCommand>();

        // Register sync commands
        services.AddSingleton<GitUpdateFromGitCommand>();
        services.AddSingleton<GitCommitCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricGit = new CommandGroup(Name,
            """
            Microsoft Fabric Git Integration - Manage Git connections and sync for Fabric workspaces.
            Fabric Git integration enables source control for workspace items by connecting workspaces
            to Git repositories. Use this tool when you need to:
            - Check Git sync status for a workspace
            - Connect or disconnect a workspace to/from a Git repository
            - Initialize a Git connection for first-time sync
            - Update workspace items from the connected Git repository
            - Commit workspace changes back to the Git repository
            This tool provides operations for managing the full Git integration lifecycle.
            """);

        var statusGroup = new CommandGroup("status",
            "Status operations - Commands for checking the Git sync status of a workspace.");
        fabricGit.AddSubGroup(statusGroup);

        var connectionGroup = new CommandGroup("connection",
            "Connection operations - Commands for managing Git connections, including connect, disconnect, and initialize.");
        fabricGit.AddSubGroup(connectionGroup);

        var syncGroup = new CommandGroup("sync",
            "Sync operations - Commands for syncing workspace items with Git, including update and commit.");
        fabricGit.AddSubGroup(syncGroup);

        var gitStatusCommand = serviceProvider.GetRequiredService<GitStatusCommand>();
        statusGroup.AddCommand(gitStatusCommand.Name, gitStatusCommand);

        var gitConnectionGetCommand = serviceProvider.GetRequiredService<GitConnectionGetCommand>();
        connectionGroup.AddCommand(gitConnectionGetCommand.Name, gitConnectionGetCommand);

        var gitConnectCommand = serviceProvider.GetRequiredService<GitConnectCommand>();
        connectionGroup.AddCommand(gitConnectCommand.Name, gitConnectCommand);

        var gitDisconnectCommand = serviceProvider.GetRequiredService<GitDisconnectCommand>();
        connectionGroup.AddCommand(gitDisconnectCommand.Name, gitDisconnectCommand);

        var gitInitializeCommand = serviceProvider.GetRequiredService<GitInitializeCommand>();
        connectionGroup.AddCommand(gitInitializeCommand.Name, gitInitializeCommand);

        var gitUpdateFromGitCommand = serviceProvider.GetRequiredService<GitUpdateFromGitCommand>();
        syncGroup.AddCommand(gitUpdateFromGitCommand.Name, gitUpdateFromGitCommand);

        var gitCommitCommand = serviceProvider.GetRequiredService<GitCommitCommand>();
        syncGroup.AddCommand(gitCommitCommand.Name, gitCommitCommand);

        return fabricGit;
    }
}
