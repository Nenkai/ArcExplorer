// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.ViewModels.TreeView;

public class FolderNodeTreeViewItemViewModel : TreeViewItemViewModel
{
    public IGameArchiveFolder Folder { get; set; }
}
