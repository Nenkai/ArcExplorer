//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/ArcExplorer
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

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
