// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;

using ArcExplorer.Services;
using ArcExplorer.ViewModels.TreeView;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using ArcExplorer.ViewModels.Windows.Main;

namespace ArcExplorer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent(); 
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null || App.Current?.Services is null)
            return;

        IStorageProvider? storageProvider = topLevel.StorageProvider;
        if (storageProvider is not null)
            App.Current.Services.GetRequiredService<IFilesService>().SetStorageProvider(storageProvider);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Source is Control control && control.DataContext is TreeViewItemViewModel) // Prevent drag-dropping to itself
            return;

        IEnumerable<IStorageItem>? files = e.DataTransfer.TryGetFiles();
        if (files is null)
            return;

        var vm = (MainViewModel)DataContext!;

        foreach (var file in files)
        {
            vm.LoadFile(file.Path);
        }
    }
}