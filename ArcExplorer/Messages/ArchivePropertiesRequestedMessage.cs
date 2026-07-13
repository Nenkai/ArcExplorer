// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.Messaging.Messages;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.Messages;

public record ArchivePropertiesRequestedMessage(IGameArchive archive);