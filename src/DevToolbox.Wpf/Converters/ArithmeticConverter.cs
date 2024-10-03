using System;
using System.Globalization;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Specifies the arithmetic operations supported by the <see cref="ArithmeticConverter"/>.
/// </summary>
public enum ArithmeticOperator
{
    /// <summary>
    /// Addition operation.
    /// </summary>
    Addition,

    /// <summary>
    /// Division operation.
    /// </summary>
    Division,

    /// <summary>
    /// Multiplication operation.
    /// </summary>
    Multiplication,

    /// <summary>
    /// Subtraction operation.
    /// </summary>
    Subtraction,
}

/// <summary>
/// A value converter that performs arithmetic operations on a double value based on the specified operator and operand.
/// </summary>
public class ArithmeticConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the operand used in the arithmetic operation.
    /// </summary>
    public double Operand { get; set; }

    /// <summary>
    /// Gets or sets the arithmetic operator to be used for conversion.
    /// </summary>
    public ArithmeticOperator Operator { get; set; }

    /// <summary>
    /// Converts a value by applying the specified arithmetic operation.
    /// </summary>
    /// <param name="value">The input value to convert, expected to be of type <see cref="double"/>.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">An optional parameter that can be used in the conversion.</param>
    /// <param name="language">The culture information for the conversion.</param>
    /// <returns>The result of the arithmetic operation as a double.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo language) => Operator switch
    {
        ArithmeticOperator.Addition => (double)value + Operand,
        ArithmeticOperator.Division => (double)value / Operand,
        ArithmeticOperator.Multiplication => (double)value * Operand,
        ArithmeticOperator.Subtraction => (double)value - Operand,
        _ => (object)((double)value + Operand), // Default to Addition if the operator is not recognized
    };

    /// <summary>
    /// Converts a value back to its original form. This method is not implemented.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type for the conversion.</param>
    /// <param name="parameter">An optional parameter that can be used in the conversion.</param>
    /// <param name="language">The culture information for the conversion.</param>
    /// <returns>Throws <see cref="NotImplementedException"/> since this operation is not supported.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        => throw new NotImplementedException();
}
