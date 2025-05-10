using System.Threading.Tasks;

namespace DevToolbox.WinUI.Contracts;

/// <summary>
/// Defines methods for handling application activation events.
/// </summary>
public interface IActivationHandler
{
    /// <summary>
    /// Determines whether this handler can process the specified activation arguments.
    /// </summary>
    /// <param name="args">
    /// The activation arguments to evaluate.
    /// </param>
    /// <returns>
    /// <c>true</c> if the handler can handle the given arguments; otherwise, <c>false</c>.
    /// </returns>
    bool CanHandle(object args);

    /// <summary>
    /// Handles the activation logic for the specified arguments asynchronously.
    /// </summary>
    /// <param name="args">
    /// The activation arguments to process.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous handling operation.
    /// </returns>
    Task HandleAsync(object args);
}