// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using ArcExplorer.ViewModels.Windows.Plugin;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArcExplorer.Views.Windows;

public partial class PluginViewWindow : Window
{
    public PluginViewWindow() => InitializeComponent();

    public PluginViewWindow(PluginViewWindowViewModel vm)
        : this()
    {
        DataContext = vm;
    }
}