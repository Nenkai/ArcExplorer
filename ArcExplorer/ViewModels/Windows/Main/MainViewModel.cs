using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Platform.Storage;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;

using NenTools.ArchiveService.Abstractions;

using ArcExplorer.Messages;
using ArcExplorer.Messages.IO;
using ArcExplorer.Services;
using ArcExplorer.ViewModels.StatusBar;
using ArcExplorer.ViewModels.TreeView;
using ArcExplorer.ViewModels.Windows.Main.FolderExplorer;
using ArcExplorer.ViewModels.Windows.PluginSettings;
using ArcExplorer.ViewModels.Windows.Properties;

namespace ArcExplorer.ViewModels.Windows.Main;

public partial class MainViewModel : ViewModelBase,
    IRecipient<FileOpenRequestMessage>,
    IRecipient<FolderOpenRequestMessage>,
    IRecipient<FileExtractRequestMessage>
{
    public TopMenuViewModel TopMenu { get; }
    public StatusBarViewModel StatusBar { get; }
    public FileSystemTreeExplorerViewModel FileSystemTreeExplorer { get; }
    public FolderExplorerViewModel FolderExplorer { get; }

    private DialogService _dialogService;
    private PluginHostService _pluginHostService;

    // For previewing.
    public MainViewModel()
    {
        TopMenu = new TopMenuViewModel(null);
        StatusBar = new StatusBarViewModel();
        FileSystemTreeExplorer = new FileSystemTreeExplorerViewModel();
        FolderExplorer = new FolderExplorerViewModel(null);
    }

    // Main ctor.
    public MainViewModel(TopMenuViewModel topMenuViewModel,
        StatusBarViewModel statusBarViewModel,
        FileSystemTreeExplorerViewModel fileExplorerViewModel,
        FolderExplorerViewModel folderExplorer,
        DialogService dialogService,
        PluginHostService pluginHostService)
    {
        TopMenu = topMenuViewModel;
        StatusBar = statusBarViewModel;
        FileSystemTreeExplorer = fileExplorerViewModel;
        FolderExplorer = folderExplorer;
        // Copyright (c) 2026 Nenkai
        // SPDX-License-Identifier: MIT


        _dialogService = dialogService;
        _pluginHostService = pluginHostService;

        WeakReferenceMessenger.Default.Register<FileOpenRequestMessage>(this);
        WeakReferenceMessenger.Default.Register<FolderOpenRequestMessage>(this);
        WeakReferenceMessenger.Default.Register<FileExtractRequestMessage>(this);
    }

    public async void Receive(FileOpenRequestMessage message)
    {
        await LoadFile(message.Value.Uri, defaultExpanded: true);
    }

    public async void Receive(FolderOpenRequestMessage message)
    {
        string? path = message.Value.Folders[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
            return;

        foreach (LoadedPlugin loadedPlugin in _pluginHostService.Plugins)
        {
            IGameArchivePlugin? plugin = loadedPlugin.Plugin;

            try
            {
                IReadOnlyList<IGameArchive>? archives = await Task.Run(() =>
                    plugin.LoadArchivesFromFolder(path, new ArchiveLoadParameters()
                    {
                        OnSettingsRequired = OnSettingsRequiredCallback,
                    }));

                if (archives is null)
                    return;

                if (archives.Count > 0)
                {
                    IFileSystemTree tree = plugin.GetMergedTree();
                    var archiveTvi = new ArchiveNodeTreeViewItemViewModel()
                    {
                        TreeViewName = "Merged",
                        Archive = archives[0],
                        IconKind = "Bootstrap.FileEarmarkZipFill",
                        IsExpanded = true,
                        RootFolder = tree.Root,
                    };
                    FileSystemTreeExplorer.AddItem(archiveTvi, null);

                    foreach (var archive in archives)
                        FolderExplorer.ArchiveList.Add(new FolderExplorerArchiveViewModel(archive));

                    PopulateFolderChildren(tree.Root, archiveTvi.Guid);
                }

                WeakReferenceMessenger.Default.Send(new SnackbarMessage($"{archives.Count} archives loaded."));
                break;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorExceptionDialog(ex);
                return;
            }
        }
    }

    private bool OnSettingsRequiredCallback(IPluginSettings settings)
    {
        var result = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var vm = new PluginSettingsDialogViewModel(settings, requiredOnly: true);
            return await _dialogService.ShowDialogAsync<IReadOnlyDictionary<string, object?>?>(vm);
        }).GetAwaiter().GetResult();

        if (result is null)
            return false;

        foreach (var (key, value) in result)
            settings.SetValue(key, value);

        return true;
    }

    public async void Receive(FileExtractRequestMessage message)
    {
        var node = message.Value.Node;
        if (node is IGameArchiveFile file)
        {
            try
            {
                file.SourceArchive.ExtractFile(file, message.Value.OutputStream);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorExceptionDialog(ex);
                return;
            }

            WeakReferenceMessenger.Default.Send(new SnackbarMessage($"'{file.Name}' extracted."));
        }
    }

    public async Task LoadFile(Uri fileResult, bool defaultExpanded = false)
    {
        string fileName = fileResult.LocalPath;

        foreach (LoadedPlugin loadedPlugin in _pluginHostService.Plugins)
        {
            if (loadedPlugin.Plugin.IsSupported(fileName))
            {
                try
                {
                    IGameArchive? archive = await Task.Run(() =>
                        loadedPlugin.Plugin.OpenArchive(fileName, new ArchiveLoadParameters()
                        {
                            OnSettingsRequired = OnSettingsRequiredCallback,
                        }));

                    if (archive is null)
                        return;

                    IFileSystemTree tree = archive.GetTree();

                    var archiveTvi = new ArchiveNodeTreeViewItemViewModel()
                    {
                        TreeViewName = archive.Name,
                        Archive = archive,
                        IconKind = "Bootstrap.FileEarmarkZipFill",
                        IsExpanded = defaultExpanded,
                        Caption = archive.Description,
                        RootFolder = tree.Root,
                        ContextActions =
                        [
                            new Menu.MenuItemViewModel()
                            {
                                Header = "Properties",
                                Enabled = true,
                                Command = new AsyncRelayCommand<IGameArchive>(OnProperties!),
                                Parameter = archive,
                            }
                        ]
                    };
                    FileSystemTreeExplorer.AddItem(archiveTvi, null);
                    FolderExplorer.ArchiveList.Add(new FolderExplorerArchiveViewModel(archive));

                    PopulateFolderChildren(tree.Root, archiveTvi.Guid);

                    WeakReferenceMessenger.Default.Send(new SnackbarMessage($"Archive loaded."));
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorExceptionDialog(ex);
                    return;
                }
            }
        }
    }

    private async Task OnProperties(IGameArchive archive)
    {
        if (archive.AdditionalProperties.Count == 0)
        {
            await _dialogService.ShowOKDialog("No properties defined for this archive.");
            return;
        }

        var vm = App.Current.Services.GetRequiredService<PropertiesWindowViewModel>();
        vm.Setup(archive);

        await _dialogService.ShowDialogAsync<PropertiesWindowViewModel>(vm);
    }

    private void PopulateFolderChildren(IGameArchiveFolder folder, Guid? parentGuid = null)
    {
        foreach (KeyValuePair<string, IGameArchiveEntry> node in folder.Children)
        {
            if (node.Value is IGameArchiveFolder fileSysFolderNode)
            {
                // Sub-Folder
                var subFolder = new FolderNodeTreeViewItemViewModel()
                {
                    TreeViewName = node.Key,
                    Folder = fileSysFolderNode,
                    IconKind = "Bootstrap.FolderFill",
                    IconColor = Color.FromArgb(red: 0xFF, green: 0xB4, blue: 0x00),
                };
                FileSystemTreeExplorer.AddItem(subFolder, parentGuid);

                if (fileSysFolderNode.Children.Any(e => e.Value is IGameArchiveFolder))
                {
                    subFolder.SetOnExpandedPopulateCallback((item) =>
                    {
                        var f = ((FolderNodeTreeViewItemViewModel)item);
                        PopulateFolderChildren(f.Folder, f.Guid);
                    });
                }
            }
            /*
            else
            {
                // File
                FileSystemTreeExplorer.AddItem(new TreeViewItemViewModel() 
                { 
                    TreeViewName = node.Key,
                    IconKind = "Bootstrap.FileEarmark",
                    ContextActions = new ObservableCollection<MenuItemViewModel>()
                    {
                        new()
                        { 
                            Header = "Extract File",
                        }
                    }
                }, folderGuid);
            }
            */
        }
    }
}
