// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

using System;
using System.Collections.Generic;
using System.Text;

namespace ArcExplorer.Services;

public interface IPlatformServicesAccessor
{
    IStorageProvider? StorageProvider { get; }
    IClipboard? Clipboard { get; }
}

class DefaultPlatformServiceAccessor : IPlatformServicesAccessor
{
    readonly IClassicDesktopStyleApplicationLifetime _desktop;

    public DefaultPlatformServiceAccessor(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _desktop = desktop;
    }

    public IStorageProvider? StorageProvider => _desktop.MainWindow?.StorageProvider;
    public IClipboard? Clipboard => _desktop.MainWindow?.Clipboard;
}
