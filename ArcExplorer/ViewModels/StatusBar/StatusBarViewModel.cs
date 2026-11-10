//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/ArcExplorer
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using ArcExplorer.Messages.StatusBar;

namespace ArcExplorer.ViewModels.StatusBar;

public partial class StatusBarViewModel : ObservableObject,
    IRecipient<SetStatusBarTextRequest>
{
    [ObservableProperty]
    public string? _leftStatus = "0 items";

    [ObservableProperty]
    public string? _message = string.Empty;

    public StatusBarViewModel()
    {
        WeakReferenceMessenger.Default.Register<SetStatusBarTextRequest>(this);
    }

    public void Receive(SetStatusBarTextRequest message)
    {
        if (message.LeftStatus is not null)
            LeftStatus = message.LeftStatus;

        if (message.Message is not null)
            Message = message.Message;

        message.Reply(true);
    }
}