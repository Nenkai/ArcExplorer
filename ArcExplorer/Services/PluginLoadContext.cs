//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/ArcExplorer
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Linq;

namespace ArcExplorer.Services;

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // For shared assemblies (like CommunityToolkit), defer to the host.
        // Only load privately if the host doesn't have it.
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            var existing = Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            if (existing != null)
                return existing;

            return LoadFromAssemblyPath(assemblyPath);
        }

        return null; // Fall back to host/default context
    }
}
