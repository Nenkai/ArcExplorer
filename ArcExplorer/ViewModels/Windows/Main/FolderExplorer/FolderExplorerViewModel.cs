using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Input.Platform;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using // Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

NenTools.ArchiveService.Abstractions;

using ArcExplorer.Messages;
using ArcExplorer.Messages.StatusBar;
using ArcExplorer.Services;

namespace ArcExplorer.ViewModels.Windows.Main.FolderExplorer;

public partial class FolderExplorerViewModel : ViewModelBase,
    IRecipient<FileSystemFolderNodeLoadedMessage>
{
    private readonly IPlatformServicesAccessor _platformServices;

    private ObservableCollection<FolderExplorerRow> Rows { get; } = [];
    public FlatTreeDataGridSource<FolderExplorerRow> Source { get; }

    private readonly FolderExplorerRow _goToParentRow;
    private IGameArchiveFolder _currentFolder;
    private string _currentPath;

    private bool _columnsSetUp;
    private IReadOnlyDictionary<string, IAttributeMetadata<IGameArchiveFile>> _attributes;

    [ObservableProperty]
    public string _sourceFile;

    [ObservableProperty]
    public string _path;

    [ObservableProperty]
    public string _searchQuery;

    [ObservableProperty]
    private FolderExplorerArchiveViewModel _selectedArchive;
    private bool _ignoreArchiveChange;

    public ObservableCollection<FolderExplorerArchiveViewModel> ArchiveList { get; set; } = [];

    // For designer
    public FolderExplorerViewModel()
    {

    }

    public FolderExplorerViewModel(IPlatformServicesAccessor platformServices)
    {
        _platformServices = platformServices;

        Source = new FlatTreeDataGridSource<FolderExplorerRow>(Rows);
        Source.RowSelection?.SelectionChanged += RowSelection_SelectionChanged;

        WeakReferenceMessenger.Default.Register<FileSystemFolderNodeLoadedMessage>(this);

        _goToParentRow = new FolderExplorerRow()
        {
            Name = "..",
            IconKind = "Bootstrap.FileArrowUpFill",
        };
    }


    public void Receive(FileSystemFolderNodeLoadedMessage message)
    {
        SetFolder(message.Folder);
    }

    /// <summary>
    /// Fired when a selected archive on the archive list has changed.
    /// </summary>
    /// <param name="value"></param>
    partial void OnSelectedArchiveChanged(FolderExplorerArchiveViewModel value)
    {
        if (_ignoreArchiveChange)
            return;

        var root = value.Archive.GetTree().Root;
        SetFolder(root);
    }

    /// <summary>
    /// Fired when navigating to a specific folder in the main bar.
    /// </summary>
    public void OnPathNavigation()
    {
        var tree = _currentFolder.SourceArchive.GetTree();
        var entry = tree.GetByPath(Path);
        if (entry is IGameArchiveFolder folder)
        {
            SetFolder(folder);
        }
        else
        {
            Path = _currentPath;
        }
    }

    public void SetupColumns(IReadOnlyDictionary<string, IAttributeMetadata<IGameArchiveFile>> attributes)
    {
        _attributes = attributes;

        Source.Columns.Clear();

        foreach (var (name, attribute) in attributes)
        {
            if (!attribute.IsPrimary)
                continue;

            switch (attribute.DisplayFormat)
            {
                case AttributeDisplayFormat.FileName:
                    var template = (IDataTemplate)Application.Current!.Resources["FileNameCellTemplate"]!;
                    Source.Columns.Add(new TemplateColumn<FolderExplorerRow>(attribute.DisplayName, template, width: new GridLength(1, GridUnitType.Star)));
                    break;

                case AttributeDisplayFormat.CheckBox:
                    Source.Columns.Add(new CheckBoxColumn<FolderExplorerRow>(attribute.DisplayName, x => GetCheckBoxValue(attribute, x)));
                    break;

                default:
                    Source.Columns.Add(new TextColumn<FolderExplorerRow, string?>(attribute.DisplayName, x => FormatText(attribute, x)));
                    break;
            }
        }
    }

    private void SetFolder(IGameArchiveFolder folder)
    {
        // Invalidate search
        // TODO: User option not to invalidate search?
        SearchQuery = string.Empty; 

        _currentFolder = folder;

        if (!_columnsSetUp)
        {
            IReadOnlyDictionary<string, IAttributeMetadata<IGameArchiveFile>> attributes;
            if (folder.SourceArchive is null) // May be merged root
                attributes = folder.Children.FirstOrDefault().Value.SourceArchive.SourcePlugin.GetFileAttributes();
            else
                attributes = folder.SourceArchive.SourcePlugin.GetFileAttributes();

            SetupColumns(attributes);

            _columnsSetUp = true;
        }

        int displayedCount = PopulateRowsFromFolder(folder, SearchQuery);
        WeakReferenceMessenger.Default.Send(new SetStatusBarTextRequest($"{displayedCount} items", null));

        SourceFile = folder.SourceArchive?.Name ?? "N/A";
        Path = folder.Path;
        _currentPath = Path;

        var selectedArchive = ArchiveList.FirstOrDefault(e => e.Archive == folder.SourceArchive);
        if (selectedArchive is not null)
        {
            _ignoreArchiveChange = true;
            SelectedArchive = selectedArchive;
            _ignoreArchiveChange = false;
        }
    }

    private int PopulateRowsFromFolder(IGameArchiveFolder folder, string? filter)
    {
        Rows.Clear();
        Source.RowSelection?.Clear();

        if (!string.IsNullOrWhiteSpace(filter))
            return DoSearch(folder, filter);

        if (folder.Parent is not null)
        {
            Rows.Add(_goToParentRow);
        }

        int count = 0;
        foreach (IGameArchiveEntry entry in folder.Children.Values)
        {
            if (entry is IGameArchiveFolder)
                Rows.Add(NewFolderRow(entry.Name, entry));
            else
                Rows.Add(NewFileRow(entry.Name, entry));

            count++;
        }

        return count;
    }

    private int DoSearch(IGameArchiveFolder folder, string filter)
    {
        var stack = new Stack<IGameArchiveFolder>();
        stack.Push(folder);
        string? basePath = folder.Path;

        int fileCount = 0;
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            foreach (var entry in current.Children.Values)
            {
                if (entry is IGameArchiveFolder subFolder)
                {
                    ReadOnlySpan<char> relPath = GetRelativePath(basePath, subFolder.Path);
                    if (relPath.Contains(filter!, StringComparison.OrdinalIgnoreCase) || subFolder.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        Rows.Add(NewFolderRow(relPath.ToString(), subFolder));
                        fileCount++;
                    }

                    stack.Push(subFolder);
                }
                else
                {
                    ReadOnlySpan<char> relPath = GetRelativePath(basePath, entry.Path);
                    if (relPath.Contains(filter!, StringComparison.OrdinalIgnoreCase) || entry.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        Rows.Add(NewFileRow(relPath.ToString(), entry));
                        fileCount++;
                    }
                }
            }
        }

        return fileCount;
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (_currentFolder is null)
            return;

        int displayed = PopulateRowsFromFolder(_currentFolder, value);
        WeakReferenceMessenger.Default.Send(new SetStatusBarTextRequest($"{displayed} items", null));
    }

    private static bool GetCheckBoxValue(IAttributeMetadata<IGameArchiveFile> attribute, FolderExplorerRow row)
    {
        if (row.Entry is not IGameArchiveFile fileNode)
            return false;

        return (bool?)attribute.Accessor?.Invoke(fileNode) ?? false;
    }

    private static string? FormatText(IAttributeMetadata<IGameArchiveFile> attribute, FolderExplorerRow row)
    {
        if (row.Entry is not IGameArchiveFile fileNode)
            return string.Empty;

        object? value = attribute.Accessor?.Invoke(fileNode);
        if (value is null)
            return string.Empty;

        return attribute.DisplayFormat switch
        {
            AttributeDisplayFormat.ByteSize => Utils.ByteSize((ulong)value),
            AttributeDisplayFormat.Hex => $"0x{value:X}",
            AttributeDisplayFormat.TextCustomFormatter => attribute.Formatter is not null ? attribute.Formatter(fileNode) : string.Empty,
            _ => value.ToString() ?? string.Empty,
        };
    }

    private void RowSelection_SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<FolderExplorerRow> e)
    {
        ;
    }

    public void OnRowContextMenuOpening(CancelEventArgs cancelEventArgs)
    {
        if (Source.RowSelection is null)
            return;

        FolderExplorerRow? selected = Source.RowSelection.SelectedItem;
        if (selected is null)
            return;

        if (selected == _goToParentRow)
        {
            cancelEventArgs.Cancel = true;
            return;
        }

        selected.ContextActions = [];
        if (selected.Entry is IGameArchiveFile)
        {
            var copyAttribute = new Menu.MenuItemViewModel()
            {
                Header = "Copy Attribute...",
                Enabled = true,
                IconKind = "Bootstrap.ClipboardPlus",
            };

            foreach (var (name, attribute) in _attributes)
            {
                copyAttribute.MenuItems.Add(new Menu.MenuItemViewModel()
                {
                    Header = $"{attribute.DisplayName} ({FormatText(attribute, selected)})",
                    Enabled = selected.Entry is IGameArchiveFile fileNode && attribute.Accessor?.Invoke(fileNode) is not null,
                    Command = new RelayCommand(async () =>
                    {
                        if (selected.Entry is not IGameArchiveFile fileNode)
                            return;

                        object? value = attribute.Accessor?.Invoke(fileNode);
                        if (value is null)
                            return;

                        string? text = FormatText(attribute, selected);

                        if (_platformServices.Clipboard is not null)
                        {
                            await _platformServices.Clipboard.SetTextAsync(text);
                            WeakReferenceMessenger.Default.Send(new SnackbarMessage($"Attribute '{attribute.DisplayName}' copied to clipboard."));
                        }
                    })
                });
            }

            selected.ContextActions.Add(new Menu.MenuItemViewModel()
            {
                Header = "Copy Path",
                IconKind = "Bootstrap.Clipboard",
                Enabled = true,
                Command = new RelayCommand(async () =>
                {
                    if (_platformServices.Clipboard is not null)
                    {
                        await _platformServices.Clipboard.SetTextAsync(selected.Entry?.Path ?? string.Empty);
                        WeakReferenceMessenger.Default.Send(new SnackbarMessage("Path copied to clipboard."));
                    }
                })
            });

            selected.ContextActions.Add(copyAttribute);
        }

        if (selected.ContextActions.Count == 0)
        {
            cancelEventArgs.Cancel = true;
            return;
        }
    }

    [RelayCommand]
    private void OnClearSearchQuery()
    {
        SearchQuery = string.Empty;
    }

    [RelayCommand]
    private void DoubleTapped()
    {
        if (Source.RowSelection is null)
            return;

        FolderExplorerRow? selected = Source.RowSelection.SelectedItem;
        if (selected is null)
            return;

        if (selected == _goToParentRow)
        {
            IGameArchiveFolder? parent = _currentFolder.Parent;
            if (parent is not null)
                SetFolder(parent);
        }
        else if (selected.Entry is IGameArchiveFolder subFolderNode)
        {
            SetFolder(subFolderNode);
        }
    }

    private static FolderExplorerRow NewFileRow(string name, IGameArchiveEntry entry)
    {
        return new FolderExplorerRow()
        {
            Name = name,
            Entry = entry,
            IconKind = "Bootstrap.FileEarmark",
        };
    }

    private static FolderExplorerRow NewFolderRow(string name, IGameArchiveEntry entry)
    {
        return new FolderExplorerRow()
        {
            Name = name,
            Entry = entry,
            IconKind = "Bootstrap.FolderFill",
            IconColor = Color.FromArgb(red: 0xFF, green: 0xB4, blue: 0x00),
        };
    }

    private static ReadOnlySpan<char> GetRelativePath(string? basePath, string entryPath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
            return entryPath.AsSpan().TrimStart(['/', '\\']);

        if (entryPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> rel = entryPath.AsSpan(basePath.Length);
            return rel.TrimStart(['/', '\\']);
        }

        return entryPath;
    }
}