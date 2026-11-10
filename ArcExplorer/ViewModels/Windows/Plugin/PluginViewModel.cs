//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/ArcExplorer
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.ViewModels.Windows.Plugin;

public partial class PluginViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _version;

    [ObservableProperty]
    private string? _author;

    [ObservableProperty]
    private string? _website;

    [ObservableProperty]
    private string? _supportedFiles;

    [ObservableProperty]
    private string? _path;

    public IGameArchivePlugin Plugin { get; set; }

    public PluginViewModel(IGameArchivePlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin, nameof(plugin));

        Plugin = plugin;
    }
}
