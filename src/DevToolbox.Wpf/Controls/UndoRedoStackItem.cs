using System;
using System.Windows;
using DevToolbox.Wpf.Helpers;

namespace DevToolbox.Wpf.Controls;

internal class UndoRedoStackItem : IEquatable<UndoRedoStackItem>
{
    #region Fields/Consts

    private const string _stringTemplate = "Rectangle {{0},{1}}, Zoom {2}";

    #endregion

    #region Properties

    public Rect Rect { get; }

    public double Zoom { get; }

    #endregion

    #region Constructor

    public UndoRedoStackItem(Rect rect, double zoom)
    {
        Rect = rect;
        Zoom = zoom;
    }

    public UndoRedoStackItem(double offsetX, double offsetY, double width, double height, double zoom)
    {
        Rect = new Rect(offsetX, offsetY, width, height);
        Zoom = zoom;
    }

    #endregion

    #region Overrides

    public override string ToString() => string.Concat(_stringTemplate, Rect.X, Rect.X, Zoom);

    #endregion

    #region Methods

    public bool Equals(UndoRedoStackItem? obj) => obj is not null && Zoom.IsWithinOnePercent(obj.Zoom) && Rect.Equals(obj.Rect);

    public override bool Equals(object? obj) => obj is UndoRedoStackItem undoRedoStackItem && Equals(undoRedoStackItem);

    public override int GetHashCode() => base.GetHashCode();

    #endregion
}
