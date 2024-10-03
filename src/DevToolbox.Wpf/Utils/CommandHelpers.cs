using System.Security;
using System.Windows;
using System.Windows.Input;

namespace DevToolbox.Wpf.Utils;

internal static class CommandHelpers
{
    internal static bool CanExecuteCommandSource(ICommandSource commandSource)
    {
        var command = commandSource.Command;
        if (command != null)
        {
            var parameter = commandSource.CommandParameter;
            var target = commandSource.CommandTarget;

            if (command is RoutedCommand routed)
            {
                target ??= commandSource as IInputElement;
                return routed.CanExecute(parameter, target);
            }
            else
                return command.CanExecute(parameter);
        }

        return false;
    }

    /// <summary>
    /// Executes the command on the given command source.
    /// </summary>
    /// <SecurityNote>
    /// Critical - calls critical function (ExecuteCommandSource)
    /// TreatAsSafe - always passes in false for userInitiated, which is safe
    /// </SecurityNote>
    [SecurityCritical, SecuritySafeCritical]
    internal static void ExecuteCommandSource(ICommandSource commandSource) => CriticalExecuteCommandSource(commandSource);

    /// <summary>
    /// Executes the command on the given command source.
    /// </summary>
    /// <SecurityNote>
    /// Critical - sets the user initiated bit on a command, which is used
    /// for security purposes later. It is important to validate 
    /// the callers of this, and the implementation to make sure
    /// that we only call MarkAsUserInitiated in the correct cases.
    /// </SecurityNote>
    [SecurityCritical]
    internal static void CriticalExecuteCommandSource(ICommandSource commandSource)
    {
        var command = commandSource.Command;
        if (command != null)
        {
            var parameter = commandSource.CommandParameter;
            var target = commandSource.CommandTarget;

            if (command is RoutedCommand routed)
            {
                if (target == null)
                    target = commandSource as IInputElement;
                if (routed.CanExecute(parameter, target))
                    routed.Execute(parameter, target);
            }
            else if (command.CanExecute(parameter))
                command.Execute(parameter);
        }
    }
}
