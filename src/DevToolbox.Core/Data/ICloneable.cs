namespace DevToolbox.Core.Data;

/// <summary>
/// Defines a mechanism for creating a copy of an object and copying values from another instance.
/// </summary>
public interface ICloneable : System.ICloneable
{
    /// <summary>
    /// Copies the values from the specified source object into the current instance.
    /// </summary>
    /// <param name="source">The source object to copy values from.</param>
    void CopyFrom(object source);
}

/// <summary>
/// Defines a type-safe mechanism for cloning and copying instances of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of object to clone and copy.</typeparam>
public interface ICloneable<T> : ICloneable
{
    /// <summary>
    /// Creates a type-safe clone of the current instance.
    /// </summary>
    /// <returns>A new instance of <typeparamref name="T"/> that is a copy of the current instance.</returns>
    new T Clone();

    /// <summary>
    /// Copies the values from the specified source of type <typeparamref name="T"/> into the current instance.
    /// </summary>
    /// <param name="source">The source object to copy values from.</param>
    void CopyFrom(T source);
}
