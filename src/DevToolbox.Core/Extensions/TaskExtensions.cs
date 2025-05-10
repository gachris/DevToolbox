using System.Threading.Tasks;

namespace DevToolbox.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Task"/> objects.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Executes the task without awaiting its completion, safely ignoring its result or exceptions.
    /// </summary>
    /// <param name="task">
    /// The task to execute in a fire-and-forget manner.
    /// </param>
    public static async void FireAndForget(this Task task)
    {
        await task.ConfigureAwait(false);
    }
}
