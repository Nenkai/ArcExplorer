// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using CommunityToolkit.Mvvm.Messaging.Messages;

using NenTools.ArchiveService.Abstractions;

namespace ArcExplorer.Messages.IO;

public class FileDropRequestMessage(IGameArchiveFile node)
    : ValueChangedMessage<IGameArchiveFile>(node);

