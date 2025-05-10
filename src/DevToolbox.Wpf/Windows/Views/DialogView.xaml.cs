using DevToolbox.Wpf.Windows.ViewModels;

namespace DevToolbox.Wpf.Windows.Views;

/// <summary>
/// Represents the view for a dialog, hosting the content provided by a <see cref="DialogViewModel"/>.
/// </summary>
public partial class DialogView : BaseDialogView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogView"/> class and sets its data context.
    /// </summary>
    /// <param name="dialogViewModel">
    /// The <see cref="DialogViewModel"/> providing the content and logic for this view.
    /// </param>
    public DialogView(DialogViewModel dialogViewModel) : base(dialogViewModel)
    {
        InitializeComponent();
    }
}