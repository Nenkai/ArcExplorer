//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/ArcExplorer
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

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
