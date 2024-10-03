using System.Windows.Input;

namespace DevToolbox.Wpf.Utils;

internal class KeyboardUtilities
{
    public static bool IsKeyModifyingPopupState(KeyEventArgs e)
    {
        return (((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) && ((e.SystemKey == Key.Down) || (e.SystemKey == Key.Up))) || (e.Key == Key.F4);
    }
}
