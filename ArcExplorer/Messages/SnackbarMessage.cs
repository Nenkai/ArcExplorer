// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

namespace ArcExplorer.Messages;

public class SnackbarMessage
{
    public string Text { get; set; }
    public TimeSpan Duration { get; }

    public SnackbarMessage(string text, TimeSpan? duration = default)
    {
        Text = text;
        Duration = duration ?? TimeSpan.FromSeconds(3);
    }
}
