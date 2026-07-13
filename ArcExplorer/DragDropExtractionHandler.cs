// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;

using ArcExplorer.ViewModels.Windows.FileExtract;
using ArcExplorer.Views.Windows;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer;

public class DragDropExtractionHandler
{
    private CancellationTokenSource _extractWindowCts;
    private int _counter = 1;
    private FileExtractWindow _fileExtractionWindow;
    private Window _owner;
    private bool _windowTriggered = false;

    public delegate void StreamFileToTargetDelegate(VirtualFileDataObject.FileDescriptor file, Stream stream);
    public delegate string FileNameResolverDelegate(IGameArchiveFile node);

    public void PerformDragDrop(Window owner, IGameArchiveEntry node,
        StreamFileToTargetDelegate onStreamFileTarget,
        FileNameResolverDelegate? onFileNameResolving = null)
    {
        _extractWindowCts = new CancellationTokenSource();
        _owner = owner;

        _fileExtractionWindow = new();
        _fileExtractionWindow.Closing += _fileExtractionWindow_OnClosing;
        FileExtractWindowViewModel dialogViewModel = (FileExtractWindowViewModel)_fileExtractionWindow.DataContext!;
        dialogViewModel.CancelAction = OnCancel;

        if (OperatingSystem.IsWindows())
        {
            var vfdo = new VirtualFileDataObject(
                // on start, we init a timer. after 1s, we display it
                // normally we'd use startAction. but this may be called before folder override confirmation. which is problematic
                startAction: null,
                // on end, we close the window if it was opened
                endAction: _ => Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    _extractWindowCts?.Cancel();

                    if (_fileExtractionWindow.IsVisible)
                    {
                        _fileExtractionWindow.Close();
                        _owner.IsEnabled = true;
                    }
                })
            );

            var folderAndFileList = new List<VirtualFileDataObject.FileDescriptor>();
            int numFiles;
            if (node is IGameArchiveFolder folder)
            {
                numFiles = BuildCopyList(folderAndFileList, folder.Name, folder, onStreamFileTarget, isCancelled: () =>
                {
                    TriggerWindow(owner); // Show the window if it hasn't already been shown.
                    return _extractWindowCts.IsCancellationRequested;
                },
                fileNameResolver: onFileNameResolving);
            }
            else if (node is IGameArchiveFile fileNode)
            {
                folderAndFileList.Add(new VirtualFileDataObject.FileDescriptor
                {
                    Name = onFileNameResolving?.Invoke(fileNode) ?? fileNode.Name.Replace('/', '\\'),
                    UserData = fileNode,
                    StreamContents = (f, s) =>
                    {
                        TriggerWindow(owner); // Show the window if it hasn't already been shown.
                        if (!_extractWindowCts.IsCancellationRequested)
                            onStreamFileTarget(f, s);
                    },
                });
                numFiles = 1;
            }
            else
                throw new InvalidDataException("Wrong node?");

            _counter = 0;
            dialogViewModel.ProgressMaximum = numFiles;
            dialogViewModel.ProgressValue = 0;
            dialogViewModel.ProgressMinimum = 0;

            vfdo.SetData(folderAndFileList);

            VirtualFileDataObject.DoDragDrop(vfdo, DragDropEffects.Copy);
        }
        else
            throw new NotSupportedException("Drag-drop to explorer is not supported on this platform yet");
    }

    private void _fileExtractionWindow_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _extractWindowCts.Cancel();
        _owner.IsEnabled = true;
    }

    private void OnCancel()
    {
        _extractWindowCts.Cancel();
        _fileExtractionWindow.Close();
        _owner.IsEnabled = true;
    }

    private void TriggerWindow(Window owner)
    {
        if (_windowTriggered)
            return;

        _windowTriggered = true;

        Task.Delay(TimeSpan.FromSeconds(1), _extractWindowCts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                owner.IsEnabled = false;
                _fileExtractionWindow.Show(owner);
            });
        });
    }

    public void SetProgress(string fileName, string fileSizeString)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            FileExtractWindowViewModel dialogViewModel = (FileExtractWindowViewModel)_fileExtractionWindow.DataContext!;
            dialogViewModel.ProgressValue = ++_counter;
            dialogViewModel.CurrentFile = fileName;
            dialogViewModel.FileSize = fileSizeString;
        });
    }


    public static int BuildCopyList(List<VirtualFileDataObject.FileDescriptor> list, string folderName, IGameArchiveFolder folder,
        StreamFileToTargetDelegate StreamCallback, Func<bool> isCancelled,
        FileNameResolverDelegate? fileNameResolver = null)
    {
        int counter = 0;
        Recurse(list, folderName, folder, ref counter);

        void Recurse(List<VirtualFileDataObject.FileDescriptor> list, string folderName, IGameArchiveFolder folder, ref int counter)
        {
            list.Add(new VirtualFileDataObject.FileDescriptor
            {
                Name = folderName + "\\", // Must end with "\\" apparently
                IsDirectory = true,
            });

            foreach (var child in folder.Children)
            {
                if (child.Value is IGameArchiveFolder subFolder)
                {
                    Recurse(list, $"{folderName}\\{subFolder.Name}", subFolder, ref counter);
                }
                else if (child.Value is IGameArchiveFile file)
                {
                    list.Add(new VirtualFileDataObject.FileDescriptor
                    {
                        Name = $"{folderName}\\{fileNameResolver?.Invoke(file) ?? file.Name.Replace('/', '\\')}",
                        UserData = file,
                        StreamContents = (f, s) =>
                        {
                            if (!isCancelled())
                                StreamCallback(f, s);
                        },
                    });
                    counter++;
                }
            }
        }

        return counter;
    }
}
