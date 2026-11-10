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

namespace ArcExplorer.Messages.StatusBar;

public class SetStatusBarTextRequest : RequestMessage<bool>
{
    public string? LeftStatus { get; set; }
    public string? Message { get; set; }

    public SetStatusBarTextRequest(string? leftStatus = null, string? text = null)
    {
        LeftStatus = leftStatus;
        Message = text;
    }
}
