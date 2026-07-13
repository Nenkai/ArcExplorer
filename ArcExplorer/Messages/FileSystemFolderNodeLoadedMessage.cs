// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.Messages;

public record FileSystemFolderNodeLoadedMessage(IGameArchiveFolder Folder);