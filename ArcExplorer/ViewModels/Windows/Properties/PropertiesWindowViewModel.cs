// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using ArcExplorer.ViewModels.Windows.Plugin;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using CommunityToolkit.Mvvm.ComponentModel;

using NenTools.ArchiveService.Abstractions;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ArcExplorer.ViewModels.Windows.Properties;

public partial class PropertiesWindowViewModel : ViewModelBase
{
    public ObservableCollection<PropertiesAttributeViewModel> Rows { get; } = [];
    public FlatTreeDataGridSource<PropertiesAttributeViewModel> Source { get; private set; }

    public PropertiesWindowViewModel()
    {
        if (Design.IsDesignMode)
        {

            return;
        }

    }

    public void Setup(IGameArchive archive)
    {
        Source = new FlatTreeDataGridSource<PropertiesAttributeViewModel>(Rows);
        Source.Columns.Add(new TextColumn<PropertiesAttributeViewModel, string>("Name", x => x.Name));
        Source.Columns.Add(new TextColumn<PropertiesAttributeViewModel, string>("Value", x => x.Value));

        var attrs = archive.GetAttributes();
        foreach (var attr in attrs)
        {
            var value = archive.AdditionalProperties.GetValueOrDefault(attr.Key);
            Rows.Add(new PropertiesAttributeViewModel()
            {
                Name = attr.Key,
                Value = value?.ToString() ?? string.Empty,
            });
        }
    }
}
