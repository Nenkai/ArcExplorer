// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

using CommunityToolkit.Mvvm.Messaging;

using ArcExplorer.Messages;

namespace ArcExplorer.Controls;

public class VectorIconDrawingImage : DrawingImage
{
    public static readonly StyledProperty<IBrush?> BrushProperty =
        AvaloniaProperty.Register<VectorIconDrawingImage, IBrush?>(nameof(Brush), defaultValue: null!);

    private bool _changing;

    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public VectorIconDrawingImage()
    {
        WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, (recipient, message) =>
        {
            UpdateBrushes(message.Theme);
        });
    }

    public void UpdateBrushes(AppTheme appTheme)
    {
        if (Drawing is GeometryDrawing geometryDrawing)
        {
            if (Brush is null)
            {
                if (appTheme == AppTheme.Light)
                    geometryDrawing.Brush = Brushes.Black;
                else
                    geometryDrawing.Brush = Brushes.White;
            }
            else
            {
                // Need to create a new one since drawings may be shared.
                _changing = true;
                Drawing = new GeometryDrawing() { Brush = Brush, Geometry = geometryDrawing.Geometry };
                _changing = false;
            }

            // Important
            RaiseInvalidated(new EventArgs());
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_changing)
            return;

        base.OnPropertyChanged(change);

        if (change.Property == DrawingProperty || change.Property == BrushProperty)
        {
            UpdateBrushes(App.Current?.ActualThemeVariant == ThemeVariant.Dark
                ? AppTheme.Dark
                : AppTheme.Light);
        }
    }
}
