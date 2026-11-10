//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/ArcExplorer
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.Messaging.Messages;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.Messages;

public record ArchivePropertiesRequestedMessage(IGameArchive archive);