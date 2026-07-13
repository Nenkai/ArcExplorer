// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

using Avalonia.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArcExplorer.ViewModels.Windows.FileExtract;

public partial class FileExtractWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private double _progressMaximum;

    [ObservableProperty]
    private double _progressMinimum;

    [ObservableProperty]
    private string _currentFile;

    [ObservableProperty]
    private string _fileSize;

    [RelayCommand]
    private void Cancel() => CancelAction?.Invoke();

    public Action CancelAction { get; set; }

    public FileExtractWindowViewModel()
    {
        if (Design.IsDesignMode)
        {
            CurrentFile = "hello/world";
            FileSize = "3.141 MB";
        }
    }
}
