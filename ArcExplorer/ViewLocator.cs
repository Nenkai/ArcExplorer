// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using ArcExplorer.ViewModels;

using System;

using StaticViewLocator;

namespace ArcExplorer;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[StaticViewLocator]
public partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var type = data.GetType();
        var func = TryGetFactory(type) ?? TryGetFactoryFromInterfaces(type);

        if (func is not null)
        {
            return func.Invoke();
        }

        var missingView = TryGetMissingView(type) ?? TryGetMissingViewFromInterfaces(type);
        if (missingView is not null)
        {
            return new TextBlock { Text = missingView };
        }

        throw new Exception($"Unable to create view for type: {type}");
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}