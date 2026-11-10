//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/ArcExplorer
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;


namespace ArcExplorer.ViewModels.Windows.Properties;

public partial class PropertiesAttributeViewModel : ViewModelBase
{
    public required string Name { get; set; }
    public string? Value { get; set; }
}
