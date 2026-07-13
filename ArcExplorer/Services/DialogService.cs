// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using ArcExplorer.ViewModels.Windows.Plugin;
using ArcExplorer.ViewModels.Windows.PluginSettings;
using ArcExplorer.ViewModels.Windows.Properties;
using ArcExplorer.Views.Windows;

using Avalonia.Controls;

using AvaloniaDialogs.Views;

using System;
using System.Threading.Tasks;

namespace ArcExplorer.Services;

public class DialogService
{
    private Window _mainWindow;

    public void SetMainWindow(Window window) 
        => _mainWindow = window;

    public async Task ShowDialogAsync(object viewModel)
    {
        ArgumentNullException.ThrowIfNull(_mainWindow);

        var window = ResolveWindow(viewModel);
        window.DataContext = viewModel;
        await window.ShowDialog(_mainWindow);
    }

    public async Task<T?> ShowDialogAsync<T>(object viewModel)
    {
        ArgumentNullException.ThrowIfNull(_mainWindow);

        var window = ResolveWindow(viewModel);
        window.DataContext = viewModel;
        return await window.ShowDialog<T?>(_mainWindow);
    }

    private static Window ResolveWindow(object viewModel) => viewModel switch
    {
        PluginViewWindowViewModel => new PluginViewWindow(),
        PropertiesWindowViewModel => new PropertiesWindow(),
        PluginSettingsDialogViewModel => new PluginSettingsWindow(),
        _ => throw new ArgumentException($"No window registered for {viewModel.GetType().Name}")
    };

    public async Task ShowErrorExceptionDialog(Exception exception)
    {
        SingleActionDialog dialog = new()
        {
            Message = $"An error has occured:\n\n{exception}",
            ButtonText = "OK",
        };
        await dialog.ShowAsync();
    }

    public async Task ShowOKDialog(string message)
    {
        SingleActionDialog dialog = new()
        {
            Message = message,
            ButtonText = "OK",
        };
        await dialog.ShowAsync();
    }
}
