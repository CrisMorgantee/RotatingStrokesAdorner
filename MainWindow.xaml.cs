using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RotatingStrokesAdorner
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  ///     

  public partial class MainWindow : Window
  {

    public enum TOOLS { FREE, INK, HIGHLIGHTER, CLEAR, SHAPEADD, INSERTCHART, SELECT, UNDO, REDO, LINE, COLOR, SLIDE };
    public TOOLS ActiveTool;

    private readonly TransformGroup transformGroup;
    private readonly TranslateTransform translation;
    private readonly ScaleTransform scale;
    private readonly RotateTransform rotation;

    NavBar navBar;
    readonly Dictionary<int, Stroke> strokeList;
    readonly DrawingAttributes drawingAttributes = new DrawingAttributes();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      navBar = new NavBar
      {
        canvas = this
      };
      navBar.Closed += NavBar_Closed;
      navBar.Show();
    }

    private void NavBar_Closed(object sender, EventArgs e)
    {
      this.Close();
    }

    public MainWindow()
    {
      InitializeComponent();

      ActiveTool = TOOLS.FREE;

      /* ******************** Multi Touch ******************** */
      strokeList = new Dictionary<int, Stroke>();
      this.WindowState = WindowState.Maximized;
      WhiteBoard.EditingMode = InkCanvasEditingMode.None;
      WhiteBoard.TouchDown += WhiteBoard_TouchDown;
      WhiteBoard.TouchMove += WhiteBoard_TouchMove;
      WhiteBoard.TouchUp += WhiteBoard_TouchUp;

      /* ******************** Rotating Strokes Adorner ******************** */
      transformGroup = new TransformGroup();

      translation = new TranslateTransform(0, 0);
      scale = new ScaleTransform(1, 1);
      rotation = new RotateTransform(0);

      transformGroup.Children.Add(rotation);
      transformGroup.Children.Add(scale);
      transformGroup.Children.Add(translation);

      shape.RenderTransform = transformGroup;
      shape.Fill = Brushes.Transparent;
      shape.Stroke = Brushes.Gray;
    }

    /* ******************** Multi Touch - Touch Down ******************** */
    private void WhiteBoard_TouchDown(object sender, TouchEventArgs e)
    {
      drawingAttributes.FitToCurve = false;
      var touchPoint = e.GetTouchPoint(this);
      var point = touchPoint.Position;
      Stroke newStroke = new Stroke(new StylusPointCollection(new List<Point> { point }), drawingAttributes);

      if (ActiveTool == TOOLS.INK)
      {
        if (!strokeList.ContainsKey(touchPoint.TouchDevice.Id))
        {
          strokeList.Add(touchPoint.TouchDevice.Id, newStroke);
          WhiteBoard.Strokes.Add(newStroke);
        }
      }
    }
    public bool CheckTouchesOver()
    {
      return shape.AreAnyTouchesOver;
    }

    /* ******************** Multi Touch - Touch move ******************** */
    private void WhiteBoard_TouchMove(object sender, TouchEventArgs e)
    {
      var touchPoint = e.GetTouchPoint(this);
      var point = touchPoint.Position;

      if (strokeList.ContainsKey(touchPoint.TouchDevice.Id))
      {
        var stroke = strokeList[touchPoint.TouchDevice.Id];
        bool nearByPoint = (bool)CheckPointNearBy(stroke, point);
        if (!nearByPoint)
        {
          stroke.StylusPoints.Add(new StylusPoint(point.X, point.Y));
        }
      }
    }
    private object CheckPointNearBy(Stroke stroke, Point point)
    {
      return stroke.StylusPoints.Any(p => (Math.Abs(p.X - point.X) <= 1) && (Math.Abs(p.Y - point.Y) <= 1));
    }

    /* ******************** Multi Touch - Touch Up ******************** */
    private void WhiteBoard_TouchUp(object sender, TouchEventArgs e)
    {
      drawingAttributes.FitToCurve = true;
      var touchPoint = e.GetTouchPoint(this);
      strokeList.Remove(touchPoint.TouchDevice.Id);
    }

    /* ******************** Rotating Strokes Adorner ******************** */
    private void Rectangle_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      // the center never changes in this sample, although we always compute it.
      Point center = new Point(
           shape.RenderSize.Width / 2.0,
           shape.RenderSize.Height / 2.0
           );

      // apply the rotation at the center of the rectangle if it has changed
      rotation.CenterX = center.X;
      rotation.CenterY = center.Y;
      rotation.Angle += e.DeltaManipulation.Rotation;

      // Scale is always uniform, by definition, so the x and y will always have the same magnitude
      scale.CenterX = center.X;
      scale.CenterY = center.Y;
      scale.ScaleX *= e.DeltaManipulation.Scale.X;
      scale.ScaleY *= e.DeltaManipulation.Scale.Y;

      // apply translation
      translation.X += e.DeltaManipulation.Translation.X;
      translation.Y += e.DeltaManipulation.Translation.Y;

    }

    private void Rectangle_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
    {
      e.ManipulationContainer = this;
    }

    /* ******************** Set Tool ******************** */

    public void SET_TOOL(TOOLS selectedTool)
    {
      if (selectedTool == TOOLS.FREE)
      {
        ActiveTool = TOOLS.FREE;
        shape.IsManipulationEnabled = true;
        WhiteBoard.EditingMode = InkCanvasEditingMode.None;
      }
      else
      if (selectedTool == TOOLS.INK)
      {
        ActiveTool = TOOLS.INK;
        WhiteBoard.EditingMode = InkCanvasEditingMode.None;
        shape.IsManipulationEnabled = false;
      }
      else
      if (selectedTool == TOOLS.HIGHLIGHTER)
      {
        ActiveTool = TOOLS.HIGHLIGHTER;
      }
      else
      if (selectedTool == TOOLS.CLEAR)
      {
        WhiteBoard.Strokes.Clear();
        return;
      }
      else
      if (selectedTool == TOOLS.SHAPEADD)
      {
        ActiveTool = TOOLS.SHAPEADD;        
      }
      else
      if (selectedTool == TOOLS.INSERTCHART)
      {
        ActiveTool = TOOLS.INSERTCHART;
      }
      else
      if (selectedTool == TOOLS.SELECT)
      {
        ActiveTool = TOOLS.SELECT;
        WhiteBoard.EditingMode = InkCanvasEditingMode.Select;
      }
      else
      if (selectedTool == TOOLS.UNDO)
      {
        ActiveTool = TOOLS.UNDO;
      }
      else
      if (selectedTool == TOOLS.REDO)
      {
        ActiveTool = TOOLS.REDO;
      }
      else
      if (selectedTool == TOOLS.LINE)
      {
        ActiveTool = TOOLS.LINE;
      }
      else
      if (selectedTool == TOOLS.COLOR)
      {
        ActiveTool = TOOLS.COLOR;
      }
      else
      if (selectedTool == TOOLS.SLIDE)
      {
        ActiveTool = TOOLS.SLIDE;
      }
    } 

    private void SliderRotate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      rotation.Angle = sliderRotate.Value;
    }
  }
}


