using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace DevToolbox.Wpf.Controls.Automation.Peers;

/// <summary>
/// Provides UI Automation support for the custom DropDown control.
/// Implements <see cref="IInvokeProvider"/> to support the Invoke pattern for opening and closing the DropDown.
/// </summary>
public class DropDownAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
{
    /// <summary>
    /// Gets the DropDown control associated with this AutomationPeer.
    /// </summary>
    private DropDown DropDownControl => (DropDown)Owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownAutomationPeer"/> class with the specified DropDown owner.
    /// </summary>
    /// <param name="owner">The DropDown control that is associated with this automation peer.</param>
    public DropDownAutomationPeer(DropDown owner) : base(owner)
    {
    }

    /// <summary>
    /// Returns the control pattern for the specified pattern interface.
    /// Supports the Invoke pattern for opening and closing the DropDown.
    /// </summary>
    /// <param name="patternInterface">The pattern interface for which the control pattern is requested.</param>
    /// <returns>The <see cref="IInvokeProvider"/> if the requested pattern is Invoke, otherwise the base implementation.</returns>
    public override object GetPattern(PatternInterface patternInterface)
    {
        return patternInterface == PatternInterface.Invoke ? this : base.GetPattern(patternInterface);
    }

    /// <summary>
    /// Invokes the DropDown action, either opening or closing it depending on its current state.
    /// </summary>
    public void Invoke()
    {
        if (DropDownControl != null)
        {
            if (DropDownControl.IsOpen)
            {
                // Close the DropDown
                DropDownControl.IsOpen = false;
            }
            else
            {
                // Open the DropDown
                DropDownControl.IsOpen = true;
            }
        }
    }
}