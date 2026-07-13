// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ArcExplorer;

public static class Utils
{
    static string[] sizeSuffixes = {
        "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    public static void RunContextMenuFadeIn(ContextMenu contextMenu, TimeSpan? time = null)
    {
        contextMenu.Opacity = 0;
        var animation = new Animation
        {
            Duration = time ?? TimeSpan.FromMilliseconds(150),
            FillMode = FillMode.Forward, // needed, otherwise the opacity will reset to 0... "fill" isn't exactly obvious here
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter(Visual.OpacityProperty, 0.0) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter(Visual.OpacityProperty, 1.0) }
                }
            }
        };
        animation.RunAsync(contextMenu);
    }

    public static string ByteSize(ulong size)
    {
        Debug.Assert(sizeSuffixes.Length > 0);

        const string formatTemplate = "{0}{1:0.#} {2}";

        if (size == 0)
        {
            return string.Format(formatTemplate, null, 0, sizeSuffixes[0]);
        }

        var absSize = Math.Abs((double)size);
        var fpPower = Math.Log(absSize, 1000);
        var intPower = (int)fpPower;
        var iUnit = intPower >= sizeSuffixes.Length
            ? sizeSuffixes.Length - 1
            : intPower;
        var normSize = absSize / Math.Pow(1000, iUnit);

        return string.Format(
            formatTemplate,
            size < 0 ? "-" : null, normSize, sizeSuffixes[iUnit]);
    }
}
