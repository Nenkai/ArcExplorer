// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using Avalonia.Controls;
using Avalonia.Platform.Storage;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArcExplorer.Services;

public class FilesService : IFilesService
{
    private IStorageProvider? _storageProvider;

    public FilesService()
    {

    }

    public FilesService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public IStorageProvider GetStorageProvider()
    {
        return _storageProvider;
    }

    public async Task<IStorageFile?> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType>? filters = null)
    {
        if (_storageProvider is null)
            throw new ArgumentNullException("Storage provider is null. It was not initialized.");

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = title,
            FileTypeFilter = filters,
            AllowMultiple = false
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(string title, string? suggestedFileName = null, Uri? suggestedStartLocation = null)
    {
        if (_storageProvider is null)
            throw new ArgumentNullException("Storage provider is null. It was not initialized.");

        IStorageFolder? startLocation = null;
        if (suggestedStartLocation is not null)
        {
            startLocation = await _storageProvider.TryGetFolderFromPathAsync(suggestedStartLocation);
            startLocation ??= await _storageProvider.TryGetFolderFromPathAsync(new Uri(suggestedStartLocation, ".")); // Try parent
        }

        var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = title,
            AllowMultiple = false,
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = startLocation,
        });

        return folders;
    }

    public async Task<IStorageFile?> SaveFileAsync(string title, IReadOnlyList<FilePickerFileType>? filters = null, string? suggestedFileName = null, Uri? suggestedStartLocation = null)
    {
        if (_storageProvider is null)
            throw new ArgumentNullException("Storage provider is null. It was not initialized.");

        IStorageFolder? startLocation = null;
        if (suggestedStartLocation is not null)
        {
            startLocation = await _storageProvider.TryGetFolderFromPathAsync(suggestedStartLocation);
            startLocation ??= await _storageProvider.TryGetFolderFromPathAsync(new Uri(suggestedStartLocation, ".")); // Try parent
        }

        return await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = startLocation,
        });
    }
}