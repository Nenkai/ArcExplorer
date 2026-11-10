//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/ArcExplorer
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using CommunityToolkit.Mvvm.Messaging.Messages;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.Messages.IO;

public class FileDropRequestMessage(IGameArchiveFile node)
    : ValueChangedMessage<IGameArchiveFile>(node);

