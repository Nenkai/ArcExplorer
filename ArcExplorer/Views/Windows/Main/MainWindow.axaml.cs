// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System.Reflection;

using ArcExplorer.Messages;

using Avalonia.Controls;

using CommunityToolkit.Mvvm.Messaging;

namespace ArcExplorer.Views;

public partial class MainWindow : Window, 
    IRecipient<SnackbarMessage>
{
    public MainWindow()
    {
        InitializeComponent();

        string versionString = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(3) ?? "unknown version";
        this.Title += $" (v{versionString})";

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(SnackbarMessage message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Snackbar.Show(message.Text, message.Duration);
        });
    }
}