using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

internal class LassoAdorner : Adorner
{
    #region Fields/Consts

    private Rect _rect;
    private readonly Control _lasso;

    #endregion

    #region Properties

    protected override int VisualChildrenCount => 1;

    #endregion

    #region Constructors

    public LassoAdorner(UIElement adornedElement) : base(adornedElement)
    {
        _lasso = new Control();
        _ = new VisualCollection(this) { _lasso };
        IsHitTestVisible = false;
    }

    #endregion

    #region Overrides 

    protected override Visual GetVisualChild(int index) => _lasso;

    protected override Size ArrangeOverride(Size finalSize)
    {
        _lasso.Arrange(_rect);
        return base.ArrangeOverride(finalSize);
    }

    #endregion

    #region Methods 

    public void Refresh(Rect rect, Transform transform)
    {
        if (rect == Rect.Empty)
            return;

        _rect = rect;
        _lasso.RenderTransform = transform;
        InvalidateArrange();
    }

    public void Refresh(ControlTemplate template) => _lasso.Template = template;

    #endregion
}
