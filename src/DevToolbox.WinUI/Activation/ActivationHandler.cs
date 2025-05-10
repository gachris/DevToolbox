using System.Threading.Tasks;
using DevToolbox.WinUI.Contracts;

namespace DevToolbox.WinUI.Activation;

/// <summary>
/// Provides a base implementation for activation handlers that process
/// specific activation arguments of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">
/// The type of activation arguments this handler can process.
/// </typeparam>
public abstract class ActivationHandler<T> : IActivationHandler where T : class
{
    #region Protected Methods

    /// <summary>
    /// Determines whether this handler can process the given activation arguments.
    /// </summary>
    /// <param name="args">
    /// The activation arguments to evaluate.
    /// </param>
    /// <returns>
    /// <c>true</c> if this handler is capable of handling the arguments; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool CanHandleInternal(T args) => true;

    /// <summary>
    /// Executes the activation handling logic for the given arguments.
    /// </summary>
    /// <param name="args">
    /// The activation arguments to handle.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous handling operation.
    /// </returns>
    protected abstract Task HandleInternalAsync(T args);

    #endregion

    #region IActivationHandler Members

    /// <inheritdoc />
    public bool CanHandle(object args)
    {
        return args is T typedArgs && CanHandleInternal(typedArgs);
    }

    /// <inheritdoc />
    public async Task HandleAsync(object args)
    {
        if (args is T typedArgs)
        {
            await HandleInternalAsync(typedArgs).ConfigureAwait(false);
        }
    }

    #endregion
}
