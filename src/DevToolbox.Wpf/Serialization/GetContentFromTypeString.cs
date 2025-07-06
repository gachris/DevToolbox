using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Serialization;

/// <summary>
/// Defines a method that returns a <see cref="LayoutDockItem"/> instance corresponding to a given type string.
/// </summary>
/// <param name="type">A string identifier representing the type of content to create.</param>
/// <returns>
/// A <see cref="LayoutDockItem"/> that corresponds to the specified <paramref name="type"/> string.
/// </returns>
public delegate LayoutDockItem GetContentFromTypeString(string type);
