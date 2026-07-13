// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using System.Drawing;
using Avalonia.Media;

namespace ArcExplorer.Converters;

public class ColorToBrushConverter : IValueConverter
{
    public static readonly VectorIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter,
                                                            CultureInfo culture)
    {
        if (value is null)
            return null;

        if (value is not System.Drawing.Color color)
            return new BindingNotification(new InvalidCastException("Expected a Color for brush conversion"), BindingErrorType.Error);

        return new SolidColorBrush((uint)color.ToArgb());
    }

    public object ConvertBack(object? value, Type targetType,
                                object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
