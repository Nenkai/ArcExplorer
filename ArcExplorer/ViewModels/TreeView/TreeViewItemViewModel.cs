// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Drawing;

using CommunityToolkit.Mvvm.ComponentModel;
using ArcExplorer.ViewModels.Menu;

namespace ArcExplorer.ViewModels.TreeView;

public partial class TreeViewItemViewModel : ObservableObject
{
    private TreeViewItemChildLoadState _loadState = TreeViewItemChildLoadState.Immediate;

    /// <summary>
    /// Whether children are currently unloaded and loading is pending.
    /// </summary>
    public bool HasUnloadedChildren => _loadState == TreeViewItemChildLoadState.Pending;

    [ObservableProperty]
    private Guid _guid = Guid.CreateVersion7();

    [ObservableProperty]
    private string? _caption;

    [ObservableProperty]
    private string _treeViewName = "No Name";

    [ObservableProperty]
    private string? _iconKind;

    [ObservableProperty]
    public Color? _iconColor;

    [ObservableProperty]
    private bool _visible = true;

    [ObservableProperty]
    private bool _isExpanded = false;

    [ObservableProperty]
    private bool _canDrop;

    [ObservableProperty]
    private ICommand? _doubleClickedCommand;

    [ObservableProperty]
    public ObservableCollection<MenuItemViewModel> _contextActions;

    public TreeViewItemViewModel? Parent { get; set; }

    /// <summary>
    /// Sub-tree items.<br/>
    /// <br/>
    /// You should not add directly to this (unless this item is the root).
    /// </summary>
    public ObservableCollection<TreeViewItemViewModel> DisplayedItems { get; set; } = [];

    private Action<TreeViewItemViewModel>? _onLoadChildren;

    partial void OnIsExpandedChanged(bool value)
    {
        if (value && _loadState == TreeViewItemChildLoadState.Pending)
        {
            LoadChildren();
        }
    }

    public void SetOnExpandedPopulateCallback(Action<TreeViewItemViewModel> loadCallback)
    {
        _loadState = TreeViewItemChildLoadState.Pending;
        _onLoadChildren = loadCallback;

        DisplayedItems.Add(new TreeViewItemViewModel());
        OnPropertyChanged(nameof(HasUnloadedChildren));
    }

    private void LoadChildren()
    {
        _loadState = TreeViewItemChildLoadState.Loaded;
        OnPropertyChanged(nameof(HasUnloadedChildren));

        try
        {
            DisplayedItems.Clear();
            _onLoadChildren?.Invoke(this);
        }
        catch
        {
            // Roll back so the node isn't silently stuck empty.
            _loadState = TreeViewItemChildLoadState.Pending;
            OnPropertyChanged(nameof(HasUnloadedChildren));
            throw;
        }
    }
}

public enum TreeViewItemChildLoadState
{
    /// <summary>
    /// Children will be loaded immediately.
    /// </summary>
    Immediate,

    /// <summary>
    /// Children loading is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// Children is loaded.
    /// </summary>
    Loaded
}