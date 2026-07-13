// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using ArcExplorer.ViewModels.Windows.PluginSettings;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArcExplorer.Views.Windows;

public partial class PluginSettingsWindow : Window
{
    public PluginSettingsWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = DataContext as PluginSettingsDialogViewModel;
        Close(vm.CollectValues());
    }
}