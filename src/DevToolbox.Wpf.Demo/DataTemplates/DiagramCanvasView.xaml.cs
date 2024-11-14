using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Demo.Data;
using DevToolbox.Wpf.Demo.ViewModels;

namespace DevToolbox.Wpf.Demo.Views;

public partial class DiagramCanvasView : ResourceDictionary
{
    private void DiagramCanvas_Drop(object sender, DragEventArgs e)
    {
        var diagramCanvas = (DiagramCanvas)sender;

        if (e.Data.GetData(typeof(DragObject)) is DragObject dragObject && !string.IsNullOrEmpty(dragObject.Xaml))
        {
            var content = XamlReader.Load(XmlReader.Create(new StringReader(dragObject.Xaml)));

            if (content != null)
            {
                var addNewItem = diagramCanvas.Items as IEditableCollectionViewAddNewItem;
                var newModelItem = (DiagramLayerViewModel)addNewItem.AddNew();

                newModelItem.Source = content;
                newModelItem.Width = 60;
                newModelItem.Height = 50;

                var newItem = (DiagramLayer)diagramCanvas.ItemContainerGenerator.ContainerFromItem(newModelItem);
                var position = e.GetPosition(Window.GetWindow(diagramCanvas));

                if (dragObject.DesiredSize.HasValue)
                {
                    var desiredSize = dragObject.DesiredSize.Value;
                    newItem.Width = desiredSize.Width;
                    newItem.Height = desiredSize.Height;

                    Canvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                    Canvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                }
                else
                {
                    Canvas.SetLeft(newItem, Math.Max(0, position.X));
                    Canvas.SetTop(newItem, Math.Max(0, position.Y));
                }

                Panel.SetZIndex(newItem, diagramCanvas.Items.Count);

                diagramCanvas.UnselectAll();
                diagramCanvas.Select(newItem);
                newItem.Focus();
            }

            e.Handled = true;
        }
    }
}