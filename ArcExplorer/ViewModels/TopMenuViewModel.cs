// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Styling;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ArcExplorer.Messages;
using ArcExplorer.Messages.IO;
using ArcExplorer.Messages.StatusBar;
using ArcExplorer.Services;
using ArcExplorer.ViewModels.Menu;
using ArcExplorer.ViewModels.Windows.Plugin;
using ArcExplorer.Views.Windows;

namespace ArcExplorer.ViewModels;

public partial class TopMenuViewModel : ObservableObject
{
    private readonly DialogService _dialogService;

    public ObservableCollection<IMenuItemViewModel> MenuItems { get; set; } = [];
    public ObservableCollection<MenuItemViewModel> _themeMenuItems = [];

    private MenuItemViewModel _saveMenuItem;
    private MenuItemViewModel _saveAsMenuItem;

    public TopMenuViewModel(DialogService dialogService)
    {
        _dialogService = dialogService;

        BuildMenu();
    }

    public void BuildMenu()
    {
        MenuItemViewModel fileMenuItem = CreateFileMenu();
        MenuItems.Add(fileMenuItem);

        MenuItemViewModel viewMenuItem = CreateViewMenu();
        MenuItems.Add(viewMenuItem);

        MenuItemViewModel toolsMenuItem = CreateToolsMenu();
        MenuItems.Add(toolsMenuItem);
    }

    private MenuItemViewModel CreateFileMenu()
    {
        var file = new MenuItemViewModel()
        {
            Header = "File",
            Enabled = true,
        };

        file.MenuItems.Add(new MenuItemViewModel()
        {
            Header = "Open File",
            Command = new RelayCommand(OnOpenFileClicked),
            IconKind = "Material.FileFind",
            HotKey = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.O, modifiers: Avalonia.Input.KeyModifiers.Control),
            Enabled = true,
        });
        file.MenuItems.Add(new MenuItemViewModel()
        {
            Header = "Open Folder",
            Command = new RelayCommand(OnOpenFolderClicked),
            IconKind = "Material.Folder",
            HotKey = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.P, modifiers: Avalonia.Input.KeyModifiers.Control),
            Enabled = true,
        });

        file.MenuItems.Add(MenuItemViewModel.Separator);

        file.MenuItems.Add(new MenuItemViewModel()
        {
            Header = "Exit",
            Command = new RelayCommand(OnExit),
            IconKind = "Material.ExitToApp",
            Enabled = true,
        });

        return file;
    }

    private MenuItemViewModel CreateViewMenu()
    {
        var view = new MenuItemViewModel()
        {
            Header = "View",
            Enabled = true,
        };
        view.MenuItems.Add(new MenuItemViewModel()
        {
            Header = "Installed Plugins",
            Command = new RelayCommand(OnInstalledPluginsClicked),
            IconKind = "Material.ToyBrick",
            Enabled = true,
        });

        return view;
    }

    private MenuItemViewModel CreateToolsMenu()
    {
        var toolsMenuItem = new MenuItemViewModel()
        {
            Header = "Tools",
            Enabled = true,
        };

        var themesMenuItem = new MenuItemViewModel()
        {
            Header = "Themes",
            Enabled = true,
            IconKind = "Material.ThemeLightDark",
        };
        toolsMenuItem.MenuItems.Add(themesMenuItem);

        foreach (var style in Enum.GetValues<AppTheme>())
        {
            var themeChangedCommand = new RelayCommand<AppTheme>(OnThemeChanged);
            var themeMenuItem = new MenuItemViewModel
            {
                Header = style == AppTheme.Default ? "System Default" : style.ToString(),
                IconKind = style != AppTheme.Default ? style == AppTheme.Light ? "Material.Brightness5" : "Material.Brightness2"
                                    : null,

                Command = themeChangedCommand,
                Parameter = style,
                Enabled = true,
                Checked = style == AppTheme.Default,
                ToggleType = MenuItemToggleType.Radio,
            };

            themesMenuItem.MenuItems.Add(themeMenuItem);
            _themeMenuItems.Add(themeMenuItem);
        }
        return toolsMenuItem;
    }

    public async void OnOpenFileClicked()
    {
        var filesService = App.Current?.Services?.GetService<IFilesService>();
        if (filesService is null)
            return;

        var file = await filesService.OpenFileAsync("Open file...", filters:
        [
            FilePickerFileTypes.All,
        ]);

        if (file is null)
            return;

        var stream = await file.OpenReadAsync();
        WeakReferenceMessenger.Default.Send(new FileOpenRequestMessage(new FileOpenResult(file.Path, stream)));
    }

    public async void OnInstalledPluginsClicked()
    {
        var vm = App.Current.Services.GetRequiredService<PluginViewWindowViewModel>();
        await _dialogService.ShowDialogAsync(vm);
    }

    public async void OnOpenFolderClicked()
    {
        var filesService = App.Current?.Services?.GetService<IFilesService>();
        if (filesService is null)
            return;

        var folders = await filesService.OpenFolderPickerAsync("Open folder...");
        if (folders is null || folders.Count == 0)
            return;

        WeakReferenceMessenger.Default.Send(new FolderOpenRequestMessage(new FolderOpenResult(folders)));
    }

    public void OnExit()
    {
        if (Application.Current is not null && Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime classic)
        {
            classic.Shutdown();
        }
    }

    public void OnThemeChanged(AppTheme parameter)
    {
        ThemeVariant themeVariant = parameter switch
        {
            AppTheme.Default => ThemeVariant.Default,
            AppTheme.Light => ThemeVariant.Light,
            AppTheme.Dark => ThemeVariant.Dark,
            _ => throw new InvalidOperationException($"Invalid theme {parameter}"),
        };

        if (Application.Current is not null && themeVariant != Application.Current.RequestedThemeVariant)
        {
            Application.Current.RequestedThemeVariant = themeVariant;

            foreach (var i in _themeMenuItems)
                i.Checked = (AppTheme)i.Parameter! == parameter;

            WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(parameter));
        }
    }
}