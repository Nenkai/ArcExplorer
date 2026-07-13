// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;


namespace ArcExplorer.ViewModels.Windows.Properties;

public partial class PropertiesAttributeViewModel : ViewModelBase
{
    public required string Name { get; set; }
    public string? Value { get; set; }
}
