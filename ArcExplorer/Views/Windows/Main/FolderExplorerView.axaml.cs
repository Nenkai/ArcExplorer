// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using CommunityToolkit.Mvvm.Messaging;

using NenTools.ArchiveService.Abstractions;

using ArcExplorer.Messages.IO;
using ArcExplorer.ViewModels.Windows.Main.FolderExplorer;

namespace ArcExplorer;

public partial class FolderExplorerView : UserControl
{
    private Point? _dragStart;
    private bool _dragging;
    private PointerPressedEventArgs _dragPointerPressedEvent;
    private DragDropExtractionHandler _dragDropHandler = new();

    public FolderExplorerView()
    {
        InitializeComponent();
        AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerReleasedEvent, StackPanel_PointerReleased, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerMovedEvent, StackPanel_PointerMoved, RoutingStrategies.Tunnel);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Control? source = e.Source as Control;
        if (e.ClickCount == 2 && source?.DataContext is FolderExplorerRow)
        {
            e.Handled = true;

            var vm = DataContext as FolderExplorerViewModel;
            vm?.DoubleTappedCommand.Execute(null);
            return;
        }
        else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
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

        if (control.DataContext is not FolderExplorerRow row || row.Entry is null)
            return;

        _dragging = true;
        _dragStart = null;

        IGameArchiveEntry node = row.Entry;

        Window owner = (Window)TopLevel.GetTopLevel(control)!;
        _dragDropHandler = new DragDropExtractionHandler();
        _dragDropHandler.PerformDragDrop(owner, node, OnStreamFileToTargetCallback, node =>
        {
            string name = node.Name.Replace('/', '\\');

            // TODO FIXME HACK HELP: Move this!!!!
            if (name.EndsWith(".tex"))
                name = Path.ChangeExtension(".tex", ".dds");

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

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var vm = (FolderExplorerViewModel)DataContext!;
            vm.OnPathNavigation();
        }
    }

    private void ContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (sender is ContextMenu menu)
            Utils.RunContextMenuFadeIn(menu);
        
        ((FolderExplorerViewModel)DataContext!).OnRowContextMenuOpening(e);
    }

    
}