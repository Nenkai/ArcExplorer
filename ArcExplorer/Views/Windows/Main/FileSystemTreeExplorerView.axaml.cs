// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using CommunityToolkit.Mvvm.Messaging;

using ArcExplorer.Messages.IO;
using ArcExplorer.ViewModels.TreeView;
using NenTools.ArchiveService.Abstractions;


namespace ArcExplorer.Views;

public partial class FileSystemTreeExplorerView : UserControl
{
    private Point? _dragStart;
    private bool _dragging;
    private PointerPressedEventArgs _dragPointerPressedEvent;
    private DragDropExtractionHandler _dragDropHandler = new();

    public FileSystemTreeExplorerView()
    {
        InitializeComponent();
    }

    private void StackPanel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control control)
            return;

        var dataContext = control.DataContext;
        if (dataContext is not TreeViewItemViewModel item)
            return;

        if (e.ClickCount == 2)
        {
            if (item.DoubleClickedCommand?.CanExecute(null) == true)
                item.DoubleClickedCommand?.Execute(null);
        }
        else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (item is not FolderNodeTreeViewItemViewModel)
                return; // Only allow dragging folders out.

            _dragStart = e.GetPosition(this);
            _dragPointerPressedEvent = e;
        }
    }

    private void StackPanel_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragStart = null;
    }

    private async void StackPanel_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragStart is null || _dragging)
            return;

        var pos = e.GetPosition(this);
        var delta = pos - _dragStart.Value;

        // Windows's drag detection is about 4 pixels, we follow that I guess
        if (Math.Abs(delta.X) < 4 && Math.Abs(delta.Y) < 4)
            return;

        if (e.Source is not Control control)
            return;

        if (control.DataContext is not FolderNodeTreeViewItemViewModel item)
            return;

        _dragging = true;
        _dragStart = null;

        Window owner = (Window)TopLevel.GetTopLevel(control)!;
        _dragDropHandler = new DragDropExtractionHandler();
        _dragDropHandler.PerformDragDrop(owner, item.Folder, OnStreamFileToTargetCallback, (IGameArchiveFile file) =>
        {
            string name = file.Name.Replace('/', '\\');
            if (name.EndsWith(".tex"))
                name = Path.ChangeExtension(name, ".dds");

            return name;
        });

        _dragging = false;
    }

    private void OnStreamFileToTargetCallback(VirtualFileDataObject.FileDescriptor descriptor, Stream stream)
    {
        if (descriptor.TryGetUserData(out IGameArchiveFile fileSystemFileNode))
        {
            _dragDropHandler.SetProgress(fileSystemFileNode.Path, Utils.ByteSize(fileSystemFileNode.Size));
            WeakReferenceMessenger.Default.Send(new FileExtractRequestMessage(new FileExtractRequest(fileSystemFileNode, stream)));
        }
    }
}