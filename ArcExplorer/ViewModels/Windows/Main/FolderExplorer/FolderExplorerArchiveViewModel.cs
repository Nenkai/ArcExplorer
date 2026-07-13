// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.ViewModels.Windows.Main.FolderExplorer;

public partial class FolderExplorerArchiveViewModel : ViewModelBase
{
    public IGameArchive Archive { get; set; }

    public FolderExplorerArchiveViewModel(IGameArchive archive)
    {
        Archive = archive ?? throw new ArgumentNullException(nameof(archive));
    }
}
