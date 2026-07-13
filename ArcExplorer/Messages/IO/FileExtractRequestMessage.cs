// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using CommunityToolkit.Mvvm.Messaging.Messages;

using NenTools.ArchiveService.Abstractions;

using System.IO;

namespace ArcExplorer.Messages.IO;

public class FileExtractRequestMessage(FileExtractRequest request)
    : ValueChangedMessage<FileExtractRequest>(request);

public record FileExtractRequest(IGameArchiveEntry Node, Stream OutputStream);
