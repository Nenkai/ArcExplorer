// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using ArcExplorer.ViewModels.Windows.Properties;

namespace ArcExplorer.Views.Windows;

public partial class PropertiesWindow : Window
{
    public PropertiesWindow() => InitializeComponent();

    public PropertiesWindow(PropertiesWindowViewModel vm)
        : this()
    {
        DataContext = vm;
    }
}