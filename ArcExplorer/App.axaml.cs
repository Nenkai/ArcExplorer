// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using NenTools.ArchiveService.Abstractions;

using ArcExplorer.Services;
using ArcExplorer.ViewModels;
using ArcExplorer.ViewModels.StatusBar;
using ArcExplorer.Views;
using ArcExplorer.ViewModels.Windows.Plugin;
using ArcExplorer.ViewModels.Windows.Main;
using ArcExplorer.ViewModels.Windows.Main.FolderExplorer;
using ArcExplorer.ViewModels.Windows.Properties;

namespace ArcExplorer;

public partial class App : Application
{
    public new static App? Current => Application.Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<DialogService>();
        services.AddGameArchivePlugins();
        services.AddSingleton<PluginViewWindowViewModel>();
        services.AddSingleton<PropertiesWindowViewModel>();

        services.AddSingleton<TopMenuViewModel>();
        services.AddSingleton<StatusBarViewModel>();
        services.AddSingleton<FileSystemTreeExplorerViewModel>();
        services.AddSingleton<FolderExplorerViewModel>();
        services.AddSingleton<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            services.AddSingleton<IPlatformServicesAccessor>(new DefaultPlatformServiceAccessor(desktop));
            services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow.StorageProvider));
            Services = services.BuildServiceProvider();

            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
            Services.GetRequiredService<DialogService>().SetMainWindow(desktop.MainWindow);
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            Services = services.BuildServiceProvider();

            singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainView { DataContext = Services.GetRequiredService<MainViewModel>() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            services.AddSingleton<IFilesService>(x => new FilesService());
            Services = services.BuildServiceProvider();

            singleViewPlatform.MainView = new MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}