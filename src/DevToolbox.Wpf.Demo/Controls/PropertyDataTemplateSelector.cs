using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Demo.ViewModels;

namespace DevToolbox.Wpf.Demo.Controls;

public class PropertyDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? CheckBoxDataTemplate { get; set; }

    public DataTemplate? CornerRadiusDataTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is Property property)
        {
            return property.ControlType switch
            {
                ControlType.CheckBox => CheckBoxDataTemplate,
                ControlType.CornerRadius => CornerRadiusDataTemplate,
                _ => null,
            };
        }
        return null;
    }
}