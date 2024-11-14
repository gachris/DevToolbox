using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using DevToolbox.Wpf.Demo.Data;

namespace DevToolbox.Wpf.Demo.Views;

public partial class FlowChartStencilsView : ResourceDictionary
{
    private Point? dragStartPoint = null;

    private void Path_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var toolboxItem = sender as ContentControl;
        dragStartPoint = new Point?(e.GetPosition(toolboxItem));
    }

    private void Path_MouseMove(object sender, MouseEventArgs e)
    {
        var path = (Path)sender;

        if (e.LeftButton != MouseButtonState.Pressed)
            dragStartPoint = null;

        if (dragStartPoint.HasValue)
        { 
            var clone = new Path
            {
                Data = path.Data?.Clone(),
                Stroke = path.Stroke,
                StrokeThickness = path.StrokeThickness,
                Fill = path.Fill,
                RenderTransform = path.RenderTransform,
                Stretch = path.Stretch
            };

            var xamlString = XamlWriter.Save(clone);
            var dataObject = new DragObject
            {
                Xaml = xamlString,
                DesiredSize = new Size(60, 50)
            };

            DragDrop.DoDragDrop(path, dataObject, DragDropEffects.Copy);

            e.Handled = true;
        }
    }
}