using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TabControlMaterialTest {

  /// <summary>Self made implementation of the staggered panel, the big part is copied form the community toolkit.</summary>
  /// <seealso cref="https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/rel/7.1.0/Microsoft.Toolkit.Uwp.UI.Controls.Primitives/StaggeredPanel/StaggeredPanel.cs"/>
  public class StaggeredPanel : Panel {

    private Double _columnWidth;
    /// <summary>Gets or sets the desired width for each column.</summary>
    /// <remarks>The width of columns can exceed the DesiredColumnWidth if the HorizontalAlignment is set to Stretch.</remarks>
    public Double DesiredColumnWidth {
      get { return (Double)GetValue(DesiredColumnWidthProperty); }
      set { SetValue(DesiredColumnWidthProperty, value); }
    }

    /// <summary>Identifies the <see cref="DesiredColumnWidth"/> dependency property.</summary>
    /// <returns>The identifier for the <see cref="DesiredColumnWidth"/> dependency property.</returns>
    public static readonly DependencyProperty DesiredColumnWidthProperty = DependencyProperty.Register(
        nameof(DesiredColumnWidth),
        typeof(Double),
        typeof(StaggeredPanel),
        new PropertyMetadata(250d, OnDesiredColumnWidthChanged));

    /// <summary>Gets or sets the distance between the border and its child object.</summary>
    /// <returns>
    /// The dimensions of the space between the border and its child as a Thickness value.
    /// Thickness is a structure that stores dimension values using pixel measures.
    /// </returns>
    public Thickness Padding {
      get { return (Thickness)GetValue(PaddingProperty); }
      set { SetValue(PaddingProperty, value); }
    }

    /// <summary>Identifies the Padding dependency property.</summary>
    /// <returns>The identifier for the <see cref="Padding"/> dependency property.</returns>
    public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
        nameof(Padding),
        typeof(Thickness),
        typeof(StaggeredPanel),
        new PropertyMetadata(default(Thickness), OnPaddingChanged));

    /// <summary>Gets or sets the spacing between columns of items.</summary>
    public Double ColumnSpacing {
      get { return (Double)GetValue(ColumnSpacingProperty); }
      set { SetValue(ColumnSpacingProperty, value); }
    }

    /// <summary>Identifies the <see cref="ColumnSpacing"/> dependency property.</summary>
    public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register(
        nameof(ColumnSpacing),
        typeof(Double),
        typeof(StaggeredPanel),
        new PropertyMetadata(0d, OnPaddingChanged));

    /// <summary>
    /// Gets or sets the spacing between rows of items.
    /// </summary>
    public Double RowSpacing {
      get { return (Double)GetValue(RowSpacingProperty); }
      set { SetValue(RowSpacingProperty, value); }
    }

    /// <summary>Identifies the <see cref="RowSpacing"/> dependency property.</summary>
    public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register(
        nameof(RowSpacing),
        typeof(Double),
        typeof(StaggeredPanel),
        new PropertyMetadata(0d, OnPaddingChanged));

    /// <summary>Overrids the default measure overriede.</summary>
    /// <param name="availableSize">The available space.</param>
    /// <returns>The size.</returns>
    protected override Size MeasureOverride(Size availableSize) {
      Double availableWidth = availableSize.Width - Padding.Left - Padding.Right;
      Double availableHeight = availableSize.Height - Padding.Top - Padding.Bottom;

      _columnWidth = Math.Min(DesiredColumnWidth, availableWidth);
      Int32 numColumns = Math.Max(1, (int)Math.Floor(availableWidth / _columnWidth));

      // adjust for column spacing on all columns expect the first
      Double totalWidth = _columnWidth + ((numColumns - 1) * (_columnWidth + ColumnSpacing));
      if (totalWidth > availableWidth) {
        numColumns--;
      }
      else if (Double.IsInfinity(availableWidth)) {
        availableWidth = totalWidth;
      }

      if (HorizontalAlignment == HorizontalAlignment.Stretch) {
        availableWidth = availableWidth - ((numColumns - 1) * ColumnSpacing);
      }

      if (Children.Count == 0) {
        return new Size(0, 0);
      }

      var columnHeights = new Double[numColumns];
      var itemsPerColumn = new Double[numColumns];

      for (Int32 i = 0; i < Children.Count; i++) {
        var columnIndex = GetColumnIndex(columnHeights);

        var child = Children[i];
        child.Measure(new Size(_columnWidth, availableHeight));
        var elementSize = child.DesiredSize;
        columnHeights[columnIndex] += elementSize.Height + (itemsPerColumn[columnIndex] > 0 ? RowSpacing : 0);
        itemsPerColumn[columnIndex]++;
      }

      Double desiredHeight = columnHeights.Max();

      return new Size(availableWidth, desiredHeight);
    }

    /// <summary>Overrides the default arrange override.</summary>
    /// <param name="finalSize">The final size.</param>
    /// <returns>The size.</returns>
    protected override Size ArrangeOverride(Size finalSize) {
      Double horizontalOffset = Padding.Left;
      Double verticalOffset = Padding.Top;
      Int32 numColumns = Math.Max(1, (Int32)Math.Floor(finalSize.Width / _columnWidth));

      // adjust for horizontal spacing on all columns expect the first
      Double totalWidth = _columnWidth + ((numColumns - 1) * (_columnWidth + ColumnSpacing));
      if (totalWidth > finalSize.Width) {
        numColumns--;

        // Need to recalculate the totalWidth for a correct horizontal offset
        totalWidth = _columnWidth + ((numColumns - 1) * (_columnWidth + ColumnSpacing));
      }

      if (HorizontalAlignment == HorizontalAlignment.Right) {
        horizontalOffset += finalSize.Width - totalWidth;
      }
      else if (HorizontalAlignment == HorizontalAlignment.Center) {
        horizontalOffset += (finalSize.Width - totalWidth) / 2;
      }

      var columnHeights = new Double[numColumns];
      var itemsPerColumn = new Double[numColumns];

      for (Int32 i = 0; i < Children.Count; i++) {
        var columnIndex = GetColumnIndex(columnHeights);

        var child = Children[i];
        var elementSize = child.DesiredSize;

        Double elementHeight = elementSize.Height;

        Double itemHorizontalOffset = horizontalOffset + (_columnWidth * columnIndex) + (ColumnSpacing * columnIndex);
        Double itemVerticalOffset = columnHeights[columnIndex] + verticalOffset + (RowSpacing * itemsPerColumn[columnIndex]);

        //Rect bounds = new Rect(itemHorizontalOffset, itemVerticalOffset, _columnWidth, elementHeight);
        Rect bounds = new Rect(itemHorizontalOffset, itemVerticalOffset, elementSize.Width, elementHeight);
        child.Arrange(bounds);

        columnHeights[columnIndex] += elementSize.Height;
        itemsPerColumn[columnIndex]++;
      }

      return base.ArrangeOverride(finalSize);
    }

    private static void OnDesiredColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var panel = (StaggeredPanel)d;
      panel.InvalidateMeasure();
    }

    private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var panel = (StaggeredPanel)d;
      panel.InvalidateMeasure();
    }

    /// <summary>Gets the column index for the given column heights.</summary>
    /// <param name="columnHeights">The heights</param>
    /// <returns>The index of the columns</returns>
    private Int32 GetColumnIndex(Double[] columnHeights) {
      Int32 columnIndex = 0;
      Double height = columnHeights[0];
      for (Int32 j = 1; j < columnHeights.Length; j++) {
        if (columnHeights[j] < height) {
          columnIndex = j;
          height = columnHeights[j];
        }
      }

      return columnIndex;
    }
  }
}