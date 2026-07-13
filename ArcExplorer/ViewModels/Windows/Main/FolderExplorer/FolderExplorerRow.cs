// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using ArcExplorer.ViewModels.Menu;

using CommunityToolkit.Mvvm.ComponentModel;

using NenTools.ArchiveService.Abstractions;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;

namespace ArcExplorer.ViewModels.Windows.Main.FolderExplorer;

public partial class FolderExplorerRow : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string? _iconKind;

    [ObservableProperty]
    private Color? _iconColor;

    [ObservableProperty]
    public ObservableCollection<MenuItemViewModel>? _contextActions;

    public IGameArchiveEntry? Entry { get; set; }

}
