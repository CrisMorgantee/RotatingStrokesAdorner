using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RotatingStrokesAdorner
{
  /// <summary>
  /// Interaction logic for NavBar.xaml
  /// </summary>
  public partial class NavBar : Window
  {
    public MainWindow canvas;

    public NavBar()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      canvas.Closed += Canvas_Closed;
    }

    private void Canvas_Closed(object sender, EventArgs e)
    {
      this.Close();
    }    

    /* 
     
     // Raised when Button gains focus.
    // Changes the color of the Button to Blue.
    private void OnGotFocusHandler(object sender, RoutedEventArgs e)
    {
     
      this.badgeFree.BadgeBackground = Brushes.DeepSkyBlue;
      this.badgeFree.BadgeForeground = Brushes.White;
      Button tb = e.Source as Button;
      tb.Background = Brushes.Blue;
    }
    // Raised when Button losses focus. 
    // Changes the color of the Button back to Transparent.
    private void OnLostFocusHandler(object sender, RoutedEventArgs e)
    {
      this.badgeFree.BadgeBackground = Brushes.Black;
      this.badgeFree.BadgeForeground = Brushes.White;
      Button tb = e.Source as Button;
      tb.Background = Brushes.Transparent;
    }
    */

    /* ******************** ToolBar Buttons  ********************  */
    private void Drag_MouseDown(object sender, MouseButtonEventArgs e)
    {
      DragMove();
    }

    private void Free_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.FREE);
    }

    private void Pen_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.INK);
    }

    private void HighLighter_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.HIGHLIGHTER);
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.CLEAR);
    }

    private void ShapeAdd_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.SHAPEADD);
    }

    private void InsertChart_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.INSERTCHART);
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.SELECT);
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.UNDO);
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.REDO);
    }

    private void Line_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.LINE);
    }

    private void Color_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.COLOR);
    }

    private void Slide_Click(object sender, RoutedEventArgs e)
    {
      canvas.SET_TOOL(MainWindow.TOOLS.SLIDE);
    }    
  }
}
