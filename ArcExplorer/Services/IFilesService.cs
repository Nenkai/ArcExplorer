// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using Avalonia.Platform.Storage;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArcExplorer.Services;

public interface IFilesService
{
    public IStorageProvider GetStorageProvider();
    public void SetStorageProvider(IStorageProvider storageProvider);
    public Task<IStorageFile?> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType>? filters = null);
    public Task<IStorageFile?> SaveFileAsync(string title, IReadOnlyList<FilePickerFileType>? filters = null, string? suggestedFileName = null, Uri? suggestedStartLocation = null);
    Task<IReadOnlyList<IStorageFolder>?> OpenFolderPickerAsync(string title, string? suggestedFileName = null, Uri? suggestedStartLocation = null);
}
