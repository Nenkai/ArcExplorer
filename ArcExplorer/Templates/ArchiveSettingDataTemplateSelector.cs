// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using ArcExplorer.ViewModels.Windows.PluginSettings;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ArcExplorer.Templates;

public class SettingDataTemplateSelector : IDataTemplate
{
    public bool Match(object? data) => data is ArchiveSettingViewModel;

    public Control Build(object? data)
    {
        var vm = (ArchiveSettingViewModel)data!;

        if (vm.Descriptor.ValueType == typeof(bool))
        {
            var cb = new CheckBox();
            cb.Bind(ToggleButton.IsCheckedProperty, new Binding("Value"));
            return cb;
        }

        if (vm.Descriptor.ValueType == typeof(int) || vm.Descriptor.ValueType == typeof(long))
        {
            var num = new NumericUpDown();
            num.Bind(NumericUpDown.ValueProperty, new Binding("Value"));
            return num;
        }

        if (vm.Descriptor.ValueType.IsEnum)
        {
            var items = Enum.GetValues(vm.Descriptor.ValueType)
                   .Cast<object>()
                   .Select(v => new EnumDisplayItem(v))
                   .ToList();

            var comboBox = new ComboBox
            {
                ItemsSource = items,
                PlaceholderText = "Select item..."
            };

            comboBox.SelectionChanged += (_, _) =>
            {
                if (comboBox.SelectedItem is EnumDisplayItem selected)
                    vm.Value = selected.Value;
            };

            if (vm.Value is not null)
                comboBox.SelectedItem = items.FirstOrDefault(i => i.Value.Equals(vm.Value));

            return comboBox;
        }

        var tb = new TextBox();
        tb.Bind(TextBox.TextProperty, new Binding("Value"));
        return tb;
    }
}

public class EnumDisplayItem
{
    public object Value { get; }
    public string DisplayName { get; }

    public EnumDisplayItem(object value)
    {
        Value = value;
        var field = value.GetType().GetField(value.ToString()!);
        var attr = field?.GetCustomAttribute<DescriptionAttribute>();
        DisplayName = attr?.Description ?? value.ToString()!;
    }

    public override string ToString() => DisplayName;
}
