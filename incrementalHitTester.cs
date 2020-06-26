using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Ink;

// Enum that keeps track of whether StrokeCollectionDemo is in ink mode 
// or select mode.
public enum InkMode
{
  Ink, Select
}

// This control allows the user to input and select ink.  When the
// user selects ink, the lasso remains visible until they erase, or clip
// the selected strokes, or clear the selection.  When the control is
// in selection mode, strokes that are selected turn red.
public class InkSelector : Label
{
  InkMode mode;
  readonly DrawingAttributes inkDA;
  readonly DrawingAttributes selectDA;
  readonly InkPresenter presenter;
  IncrementalLassoHitTester selectionTester;
  readonly StrokeCollection selectedStrokes = new StrokeCollection();

  // StylusPointCollection that collects the stylus points from the stylus events.
  StylusPointCollection stylusPoints;
  // Stroke that represents the lasso.
  Stroke lassoPath;
  readonly DynamicRenderer renderer;

  public InkSelector()
  {
    mode = InkMode.Ink;

    // Use an InkPresenter to display the strokes on the custom control.
    presenter = new InkPresenter();
    this.Content = presenter;

    // In the constructor.
    // Selection drawing attributes use dark gray ink.
    selectDA = new DrawingAttributes
    {
      Color = Colors.DarkGray
    };

    // ink drawing attributes use default attributes
    inkDA = new DrawingAttributes
    {
      Width = 5,
      Height = 5
    };

    inkDA.AttributeChanged += new PropertyDataChangedEventHandler(DrawingAttributesChanged);
    selectDA.AttributeChanged += new PropertyDataChangedEventHandler(DrawingAttributesChanged);

    // Add a DynmaicRenderer to the control so ink appears
    // to "flow" from the tablet pen.
    renderer = new DynamicRenderer
    {
      DrawingAttributes = inkDA
    };
    this.StylusPlugIns.Add(renderer);
    presenter.AttachVisuals(renderer.RootVisual, renderer.DrawingAttributes);
  }

  static InkSelector()
  {
    // Allow ink to be drawn only within the bounds of the control.
    Type owner = typeof(InkSelector);
    ClipToBoundsProperty.OverrideMetadata(owner,
        new FrameworkPropertyMetadata(true));
  }

  // Prepare to collect stylus packets. If Mode is set to Select,  
  // get the IncrementalHitTester from the InkPresenter'newStroke 
  // StrokeCollection and subscribe to its StrokeHitChanged event.
  protected override void OnStylusDown(StylusDownEventArgs e)
  {
    base.OnStylusDown(e);

    Stylus.Capture(this);

    // Create a new StylusPointCollection using the StylusPointDescription
    // from the stylus points in the StylusDownEventArgs.
    stylusPoints = new StylusPointCollection();
    StylusPointCollection eventPoints = e.GetStylusPoints(this, stylusPoints.Description);

    stylusPoints.Add(eventPoints);

    InitializeHitTester(eventPoints);
  }

  protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
  {
    base.OnMouseLeftButtonDown(e);

    Mouse.Capture(this);

    if (e.StylusDevice != null)
    {
      return;
    }

    Point pt = e.GetPosition(this);

    StylusPointCollection collectedPoints = new StylusPointCollection(new Point[] { pt });

    stylusPoints = new StylusPointCollection
    {
      collectedPoints
    };

    InitializeHitTester(collectedPoints);
  }
  private void InitializeHitTester(StylusPointCollection collectedPoints)
  {
    // Deselect any selected strokes.
    foreach (Stroke selectedStroke in selectedStrokes)
    {
      selectedStroke.DrawingAttributes.Color = inkDA.Color;
    }
    selectedStrokes.Clear();

    if (mode == InkMode.Select)
    {
      // Remove the previously drawn lasso, if it exists.
      if (lassoPath != null)
      {
        presenter.Strokes.Remove(lassoPath);
        lassoPath = null;
      }

      selectionTester = presenter.Strokes.GetIncrementalLassoHitTester(80);
      selectionTester.SelectionChanged += new LassoSelectionChangedEventHandler(SelectionTester_SelectionChanged);
      selectionTester.AddPoints(collectedPoints);
    }
  }

  // Collect the stylus packets as the stylus moves.
  protected override void OnStylusMove(StylusEventArgs e)
  {
    if (stylusPoints == null)
    {
      return;
    }

    StylusPointCollection collectedPoints = e.GetStylusPoints(this, stylusPoints.Description);
    stylusPoints.Add(collectedPoints);
    AddPointsToHitTester(collectedPoints);
  }

  protected override void OnMouseMove(MouseEventArgs e)
  {

    base.OnMouseMove(e);

    if (e.StylusDevice != null)
    {
      return;
    }

    if (e.LeftButton == MouseButtonState.Released)
    {
      return;
    }

    stylusPoints ??= new StylusPointCollection();

    Point pt = e.GetPosition(this);

    StylusPointCollection collectedPoints = new StylusPointCollection(new Point[] { pt });

    stylusPoints.Add(collectedPoints);

    AddPointsToHitTester(collectedPoints);
  }

  private void AddPointsToHitTester(StylusPointCollection collectedPoints)
  {

    if (mode == InkMode.Select &&
        selectionTester != null &&
        selectionTester.IsValid)
    {
      // When the control is selecting strokes, add the
      // stylus packetList to selectionTester.
      selectionTester.AddPoints(collectedPoints);
    }
  }

  // When the user lifts the stylus, create a Stroke from the
  // collected stylus points and add it to the InkPresenter.
  // When the control is selecting strokes, add the
  // point data to the IncrementalHitTester.
  protected override void OnStylusUp(StylusEventArgs e)
  {
    stylusPoints ??= new StylusPointCollection();
    StylusPointCollection collectedPoints = e.GetStylusPoints(this, stylusPoints.Description);

    stylusPoints.Add(collectedPoints);
    AddPointsToHitTester(collectedPoints);
    AddStrokeToPresenter();
    stylusPoints = null;

    Stylus.Capture(null);
  }

  protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
  {

    base.OnMouseLeftButtonUp(e);

    if (e.StylusDevice != null) return;

    if (stylusPoints == null) stylusPoints = new StylusPointCollection();

    Point pt = e.GetPosition(this);

    StylusPointCollection collectedPoints = new StylusPointCollection(new Point[] { pt });

    stylusPoints.Add(collectedPoints);
    AddPointsToHitTester(collectedPoints);
    AddStrokeToPresenter();

    stylusPoints = null;

    Mouse.Capture(null);
  }

  private void AddStrokeToPresenter()
  {
    Stroke newStroke = new Stroke(stylusPoints);

    if (mode == InkMode.Ink)
    {
      // Add the stroke to the InkPresenter.
      newStroke.DrawingAttributes = inkDA.Clone();
      presenter.Strokes.Add(newStroke);
    }

    if (mode == InkMode.Select && lassoPath == null)
    {
      // Add the lasso to the InkPresenter and add the packetList
      // to selectionTester.
      lassoPath = newStroke;
      lassoPath.DrawingAttributes = selectDA.Clone();
      presenter.Strokes.Add(lassoPath);
      selectionTester.SelectionChanged -= new LassoSelectionChangedEventHandler(SelectionTester_SelectionChanged);
      selectionTester.EndHitTesting();
    }
  }

  void SelectionTester_SelectionChanged(object sender,
      LassoSelectionChangedEventArgs args)
  {
    // Change the color of all selected strokes to red.
    foreach (Stroke selectedStroke in args.SelectedStrokes)
    {
      selectedStroke.DrawingAttributes.Color = Colors.Red;
      selectedStrokes.Add(selectedStroke);
    }

    // Change the color of all unselected strokes to 
    // their original color.
    foreach (Stroke unselectedStroke in args.DeselectedStrokes)
    {
      unselectedStroke.DrawingAttributes.Color = inkDA.Color;
      selectedStrokes.Remove(unselectedStroke);
    }
  }

  // Property to indicate whether the user is inputting or
  // selecting ink.  
  public InkMode Mode
  {
    get
    {
      return mode;
    }

    set
    {
      mode = value;

      // Set the DrawingAttributes of the DynamicRenderer
      if (mode == InkMode.Ink)
      {
        renderer.DrawingAttributes = inkDA;
      }
      else
      {
        renderer.DrawingAttributes = selectDA;
      }

      // Reattach the visual of the DynamicRenderer to the InkPresenter.
      presenter.DetachVisuals(renderer.RootVisual);
      presenter.AttachVisuals(renderer.RootVisual, renderer.DrawingAttributes);
    }
  }
  void DrawingAttributesChanged(object sender, PropertyDataChangedEventArgs e)
  {
    // Reattach the visual of the DynamicRenderer to the InkPresenter 
    // whenever the DrawingAttributes change.
    presenter.DetachVisuals(renderer.RootVisual);
    presenter.AttachVisuals(renderer.RootVisual, renderer.DrawingAttributes);
  }

  // Property to allow the user to change the pen's DrawingAttributes.
  public DrawingAttributes InkDrawingAttributes
  {
    get
    {
      return inkDA;
    }
  }

  // Property to allow the user to change the Selector'newStroke DrawingAttributes.
  public DrawingAttributes SelectDrawingAttributes
  {
    get
    {
      return selectDA;
    }
  }
}