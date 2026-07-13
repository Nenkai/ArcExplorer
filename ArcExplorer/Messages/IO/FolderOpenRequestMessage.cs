// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using CommunityToolkit.Mvvm.Messaging.Messages;

using Avalonia.Platform.Storage;

namespace ArcExplorer.Messages.IO;

public class FolderOpenRequestMessage : ValueChangedMessage<FolderOpenResult>
{
    public FolderOpenRequestMessage(FolderOpenResult value) : base(value)
    {

    }
}

public record FolderOpenResult(IReadOnlyList<IStorageFolder> Folders);
