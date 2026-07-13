// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.ViewModels.TreeView;

public class ArchiveNodeTreeViewItemViewModel : TreeViewItemViewModel
{
    public IGameArchive Archive { get; set; }
    public IGameArchiveFolder RootFolder { get; set; }
}
