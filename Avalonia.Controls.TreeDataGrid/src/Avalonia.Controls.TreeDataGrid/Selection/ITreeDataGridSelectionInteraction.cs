using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;

namespace Avalonia.Controls.Selection
{
    /// <summary>
    /// Defines the interaction between a <see cref="TreeDataGrid"/> and an
    /// <see cref="ITreeDataGridSelection"/> model.
    /// </summary>
    public interface ITreeDataGridSelectionInteraction
    {
        event EventHandler? SelectionChanged;

        bool IsCellSelected(int columnIndex, int rowIndex) => false;
        bool IsRowSelected(IRow rowModel) => false;
        bool IsRowSelected(int rowIndex) => false;
        void OnKeyDown(TreeDataGrid sender, KeyEventArgs e) { }
        void OnPreviewKeyDown(TreeDataGrid sender, KeyEventArgs e) { }
        void OnKeyUp(TreeDataGrid sender, KeyEventArgs e) { }
        void OnTextInput(TreeDataGrid sender, TextInputEventArgs e) { }
        void OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e) { }
        void OnPointerMoved(TreeDataGrid sender, PointerEventArgs e) { }
        void OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e) { }
    }
}
