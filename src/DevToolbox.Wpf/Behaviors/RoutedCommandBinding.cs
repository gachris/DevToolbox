﻿using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace DevToolbox.Wpf.Behaviors;

/// <summary>
///  Allows associated a routed command with a non-ordinary command. 
/// </summary>
public class RoutedCommandBinding : Behavior<FrameworkElement>
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the <see cref="Command"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(RoutedCommandBinding), new PropertyMetadata(default(ICommand)));

    #endregion

    #region Properties

    /// <summary> 
    /// The command that should be executed when the RoutedCommand fires. 
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// The command that triggers <see cref="ICommand"/>. 
    /// </summary>
    public ICommand RoutedCommand { get; set; } = default!;

    #endregion

    #region Methods Overrides

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        var binding = new CommandBinding(RoutedCommand, HandleExecuted, HandleCanExecute);
        AssociatedObject.CommandBindings.Add(binding);
    }

    #endregion

    #region Methods

    /// <summary> 
    /// Proxy to the current Command.CanExecute(object).
    /// </summary>
    private void HandleCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Command?.CanExecute(e.Parameter) == true;
        e.Handled = true;
    }

    /// <summary> 
    /// Proxy to the current Command.Execute(object). 
    /// </summary>
    private void HandleExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        Command?.Execute(e.Parameter);
        e.Handled = true;
    }

    #endregion
}