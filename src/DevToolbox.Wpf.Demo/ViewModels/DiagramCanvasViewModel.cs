using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class DiagramCanvasViewModel : DockIBaseItemViewModel
{
    #region Properties

    public ObservableCollection<DiagramLayerViewModel> Items { get; }

    #endregion

    public DiagramCanvasViewModel()
    {
        Items =
        [
            new DiagramLayerViewModel
            {
                Width = 500,
                Height = 500,
                Source = new Image()
                {
                    Source = BitmapFrame.Create(new BitmapImage(new Uri(@"pack://application:,,,/DevToolbox.Wpf.Demo;component/Assets/photo_1.jpg")))
                }
            },
            new DiagramLayerViewModel
            {
                Width = 500,
                Height = 500,
                Source = new Image()
                {
                    Source = BitmapFrame.Create(new BitmapImage(new Uri(@"pack://application:,,,/DevToolbox.Wpf.Demo;component/Assets/photo_2.jpg")))
                }
            },
            new DiagramLayerViewModel
            {
                Width = 300,
                Height = 200,
                Source = new Image()
                {
                    Source = BitmapFrame.Create(CreateBitmapSource(Colors.AliceBlue))
                }
            },
            new DiagramLayerViewModel
            {
                Width = 200,
                Height = 300,
                Source = new Image()
                {
                    Source = BitmapFrame.Create(CreateBitmapSource(Colors.Red))
                }
            }
        ];
    }

    #region Methods

    private static BitmapSource CreateBitmapSource(Color color)
    {
        var width = 128;
        var height = width;
        var stride = width / 8;
        var pixels = new byte[height * stride];
        var colors = new List<Color>() { color };
        var myPalette = new BitmapPalette(colors);

        return BitmapSource.Create(
            width,
            height,
            96,
            96,
            PixelFormats.Indexed1,
            myPalette,
            pixels,
            stride);
    }

    #endregion
}
