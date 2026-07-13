// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NenTools.ArchiveService.Abstractions;
using NenTools.ArchiveService.Implementations;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

namespace ArcExplorer.Services;

public class PluginHostService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public List<LoadedPlugin> Plugins { get; } = [];

    public PluginHostService(ILoggerFactory loggerFactory, ILogger<PluginHostService> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public void LoadPlugins(string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory))
        {
            _logger.LogWarning("No plugins directory. Not loading any plugins.");
            return;
        }

        foreach (var pluginDir in Directory.EnumerateDirectories(pluginDirectory))
        {
            foreach (var file in Directory.GetFiles(pluginDir, "*.dll"))
            {
                TryLoadPlugin(file);
            }
        }
    }

    public bool TryLoadPlugin(string pluginAssemblyPath)
    {
        try
        {
            var context = new PluginLoadContext(pluginAssemblyPath);
            var assembly = context.LoadFromAssemblyName(
                new AssemblyName(Path.GetFileNameWithoutExtension(pluginAssemblyPath)));

            var pluginTypes = assembly
                .GetTypes()
                .Where(t => typeof(IGameArchivePlugin).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                .ToList();

            if (pluginTypes.Count == 0)
            {
                context.Unload();
                return false;
            }

            foreach (var type in pluginTypes)
            {
                var plugin = (IGameArchivePlugin)Activator.CreateInstance(type)!;
                plugin.SetLoggerFactory(_loggerFactory);
                plugin.Initialize();

                Plugins.Add(new LoadedPlugin(context, pluginAssemblyPath, plugin));

                _logger.LogInformation("Loaded: {name} {version}", plugin.Name, plugin.Version);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load '{path}'", pluginAssemblyPath);
            return false;
        }
    }
}

public record LoadedPlugin(AssemblyLoadContext Context, string Path, IGameArchivePlugin Plugin);

public static class PluginLoadServiceExtensions
{
    public static IServiceCollection AddGameArchivePlugins(this IServiceCollection services)
    {
        // TODO: Actually create the logger factory elsewhere
        var factory = LoggerFactory.Create(b => { });
        var logger = factory.CreateLogger<PluginHostService>();

        var host = new PluginHostService(factory, logger);
        services.AddSingleton(host);

        host.LoadPlugins("Plugins");
        return services;
    }
}