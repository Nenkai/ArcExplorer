// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

using Avalonia.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using NenTools.ArchiveService.Abstractions;

using ArcExplorer.Messages;
using ArcExplorer.ViewModels.TreeView;


namespace ArcExplorer.ViewModels.Windows.Main;

public partial class FileSystemTreeExplorerViewModel : ViewModelBase
{
    private readonly Dictionary<Guid, TreeViewItemViewModel> _idToItem = [];
    public ObservableCollection<TreeViewItemViewModel> DisplayedItems { get; set; } = [];

    [ObservableProperty]
    private object? _selectedItem;

    public FileSystemTreeExplorerViewModel()
    {
        if (Design.IsDesignMode)
        {
            DisplayedItems.Add(new FolderNodeTreeViewItemViewModel()
            {
                TreeViewName = "Hello",
                IconKind = "Bootstrap.FolderFill",
                IconColor = Color.FromArgb(red: 0xFF, green: 0xB4, blue: 0x00),
            });
        }
    }

    partial void OnSelectedItemChanged(object? value)
    {
        if (value is FolderNodeTreeViewItemViewModel folderTvi)
            WeakReferenceMessenger.Default.Send(new FileSystemFolderNodeLoadedMessage(folderTvi.Folder));
        else if (value is ArchiveNodeTreeViewItemViewModel archiveTvi && archiveTvi.RootFolder is not null)
            WeakReferenceMessenger.Default.Send(new FileSystemFolderNodeLoadedMessage(archiveTvi.RootFolder));
    }

    /// <summary>
    /// Adds an item with the specified id.
    /// </summary>
    /// <param name="id">Id of the tree view item.</param>
    /// <param name="item">Item to add.</param>
    /// <param name="parentId">Parent item to add to.</param>
    public void AddItem(TreeViewItemViewModel item, Guid? parentId = null)
    {
        if (parentId is not null && _idToItem.TryGetValue((Guid)parentId, out TreeViewItemViewModel? parentItem))
        {
            if (parentItem!.DisplayedItems.Contains(item))
                return;

            parentItem.DisplayedItems.Add(item);
            item.Parent = parentItem;

            _idToItem.Add(item.Guid, item);

            RegisterSubTreeItem(item);
        }
        else
        {
            // Doesn't exist, add the root
            DisplayedItems.Add(item);
            _idToItem.Add(item.Guid, item);

            RegisterSubTreeItem(item);
        }
    }

    public TreeViewItemViewModel? GetItem(Guid guid)
    {
        _idToItem.TryGetValue(guid, out TreeViewItemViewModel? item);
        return item;
    }

    /// <summary>
    /// Removes a node and its children from the tree.
    /// </summary>
    /// <param name="id"></param>
    public void RemoveItem(Guid guid)
    {
        if (_idToItem.TryGetValue(guid, out TreeViewItemViewModel? item))
        {
            _idToItem.Remove(guid);
            UnregisterSubTreeItem(item);

            item.Parent?.DisplayedItems.Remove(item);
            DisplayedItems.Remove(item);
        }
    }

    private void RegisterSubTreeItem(TreeViewItemViewModel parent)
    {
        foreach (TreeViewItemViewModel ivm in parent.DisplayedItems)
        {
            _idToItem.TryAdd(ivm.Guid, ivm);
            RegisterSubTreeItem(ivm);
        }
    }

    private void UnregisterSubTreeItem(TreeViewItemViewModel parent)
    {
        foreach (TreeViewItemViewModel ivm in parent.DisplayedItems)
        {
            _idToItem.Remove(ivm.Guid);
            UnregisterSubTreeItem(ivm);
        }
    }

    public void Clear()
    {
        _idToItem.Clear();
        DisplayedItems.Clear();
    }
}