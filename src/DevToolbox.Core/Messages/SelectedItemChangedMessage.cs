namespace DevToolbox.Core.Messages;

/// <summary>
/// Represents a message indicating that the selected item has changed.
/// </summary>
/// <typeparam name="T">The type of the selected item.</typeparam>
public class SelectedItemChangedMessage<T>
{
    /// <summary>
    /// Gets the item that has been selected.
    /// </summary>
    public T? Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectedItemChangedMessage{T}"/> class.
    /// </summary>
    public SelectedItemChangedMessage()
    {
        Item = default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectedItemChangedMessage{T}"/> class
    /// with the specified selected item.
    /// </summary>
    /// <param name="item">The item that was selected. Can be <c>null</c>.</param>
    public SelectedItemChangedMessage(T? item)
    {
        Item = item;
    }
}
