using System.Threading.Tasks;

namespace DevToolbox.WinUI.Contracts;

/// <summary>
/// Defines a service responsible for application activation procedures.
/// </summary>
public interface IActivationService
{
    /// <summary>
    /// Activates the application using the specified activation arguments.
    /// </summary>
    /// <param name="activationArgs">
    /// The arguments provided for activation (e.g., launch parameters, file activation data).
    /// </param>
    /// <returns>
    /// A task that completes when the activation process is finished.
    /// </returns>
    Task ActivateAsync(object activationArgs);
}