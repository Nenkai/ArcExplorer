// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Avalonia.Controls;

using CommunityToolkit.Mvvm.ComponentModel;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.ViewModels.Windows.PluginSettings;

public partial class PluginSettingsDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _requiredOnly;

    public ObservableCollection<ArchiveSettingViewModel> Settings { get; } = [];

    public PluginSettingsDialogViewModel()
    {
        if (Design.IsDesignMode)
        {
            Settings.Add(new ArchiveSettingViewModel(new ArchiveSettingDescriptor()
            {
                Name = "ExampleString",
                DisplayName = "Example String",
                ValueType = typeof(string)
            }));
        }
    }

    public PluginSettingsDialogViewModel(IPluginSettings pluginSettings, bool requiredOnly = false)
    {
        ArgumentNullException.ThrowIfNull(pluginSettings, nameof(pluginSettings));

        _requiredOnly = requiredOnly;

        foreach (var descriptor in pluginSettings.Descriptors)
        {
            if (!_requiredOnly || _requiredOnly && descriptor.IsRequired)
            {
                var setting = new ArchiveSettingViewModel(descriptor);
                if (pluginSettings.Values?.TryGetValue(descriptor.Name, out object? value) == true)
                    setting.Value = value;

                Settings.Add(setting);
            }
        }
    }

    public IReadOnlyDictionary<string, object?> CollectValues()
        => Settings.ToDictionary(s => s.Descriptor.Name, s => s.Value);
}
