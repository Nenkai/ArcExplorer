// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using ArcExplorer.Services;
using ArcExplorer.ViewModels.Windows.PluginSettings;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;

using NenTools.ArchiveService.Abstractions;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ArcExplorer.ViewModels.Windows.Plugin;

public partial class PluginViewWindowViewModel : ViewModelBase
{
    private readonly DialogService _dialogService;

    public ObservableCollection<PluginViewModel> Rows { get; } = [];
    public FlatTreeDataGridSource<PluginViewModel> Source { get; }

    public PluginViewWindowViewModel()
    {
        if (Design.IsDesignMode)
        {
            Rows.Add(new PluginViewModel(null) { Name = "Plugin #1" });
            Rows.Add(new PluginViewModel(null) { Name = "Plugin #2" });
            Rows.Add(new PluginViewModel(null) { Name = "Plugin #3" });
        }

        Source = new FlatTreeDataGridSource<PluginViewModel>(Rows);
        SetupColumns();
    }

    public PluginViewWindowViewModel(PluginHostService pluginHostService, DialogService dialogService)
    {
        _dialogService = dialogService;

        foreach (var loadedPlugin in pluginHostService.Plugins)
        {
            IGameArchivePlugin plugin = loadedPlugin.Plugin;
            Rows.Add(new PluginViewModel(plugin)
            {
                Name = plugin.Name,
                Version = plugin.Version,
                Author = plugin.Author,
                Website = plugin.Website,
                SupportedFiles = plugin.SupportedFileTypes is not null ? string.Join(", ", plugin.SupportedFileTypes.Select(e => $"{e.Name} ({e.Extension})")) : string.Empty,
                Path = loadedPlugin.Path,
            });
        }

        Source = new FlatTreeDataGridSource<PluginViewModel>(Rows);
        SetupColumns();
    }

    private void SetupColumns()
    {
        Source.Columns.Add(new TextColumn<PluginViewModel, string>("Name", x => x.Name));
        Source.Columns.Add(new TextColumn<PluginViewModel, string>("Version", x => x.Version));
        Source.Columns.Add(new TextColumn<PluginViewModel, string>("Author", x => x.Author));
        Source.Columns.Add(new TemplateColumn<PluginViewModel>(
            header: "Configuration",
            cellTemplate: new FuncDataTemplate<PluginViewModel>((item, _) =>
            {
                // TODO: Move this away from view model.
                if (item is null) // For some reason this would re-trigger when the window is being closed, with item being null
                    return null;

                var btn = new Button { Content = "Settings" };
                btn.Margin = new Avalonia.Thickness(4);
                btn.CornerRadius = new Avalonia.CornerRadius(4);
                btn.IsEnabled = item.Plugin.Settings.Descriptors.Count > 0;
                btn.Click += async (s, e) =>
                {
                    var vm = new PluginSettingsDialogViewModel(item.Plugin.Settings);
                    var result = await _dialogService.ShowDialogAsync<IReadOnlyDictionary<string, object?>?>(vm);

                    if (result is null)
                        return;

                    foreach (var (key, value) in result)
                        item.Plugin.Settings.SetValue(key, value);
                };
                return btn;
            })
        ));
        Source.Columns.Add(new TextColumn<PluginViewModel, string>("Website", x => x.Website));
        Source.Columns.Add(new TextColumn<PluginViewModel, string>("Supported files", x => x.SupportedFiles));
        Source.Columns.Add(new TextColumn<PluginViewModel, string>("Path", x => x.Path));
    }
}
