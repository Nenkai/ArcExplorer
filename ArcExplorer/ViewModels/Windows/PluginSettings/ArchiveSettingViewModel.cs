// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using CommunityToolkit.Mvvm.ComponentModel;

using NenTools.ArchiveService.Abstractions;

using System;
using System.Collections.Generic;
using System.Text;

namespace ArcExplorer.ViewModels.Windows.PluginSettings;

public partial class ArchiveSettingViewModel : ViewModelBase
{
    public ArchiveSettingDescriptor Descriptor { get; set; }

    [ObservableProperty]
    private object? _value;

    public ArchiveSettingViewModel(ArchiveSettingDescriptor descriptor)
    {
        Descriptor = descriptor;
        _value = descriptor.DefaultValue;
    }
}
