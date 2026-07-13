// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ArcExplorer.Messages.IO;

public class FileOpenRequestMessage : ValueChangedMessage<FileOpenResult>
{
    public FileOpenRequestMessage(FileOpenResult value) : base(value)
    {

    }
}

public record FileOpenResult(Uri Uri, Stream Stream);
