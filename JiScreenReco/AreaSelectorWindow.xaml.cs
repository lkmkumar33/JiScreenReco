using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace JiScreenReco
{
    public partial class AreaSelectorWindow : Window
    {
        private Point startPoint;
        private bool isSelecting = false;

        public Rect? SelectedRect { get; private set; }

        public AreaSelectorWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += AreaSelectorWindow_PreviewKeyDown;
        }

        private void AreaSelectorWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SelectedRect = null;
                this.DialogResult = false;
                this.Close();
            }
        }

        private void RootCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(RootCanvas);
            isSelecting = true;
            SelectionRect.Visibility = Visibility.Visible;
            Canvas.SetLeft(SelectionRect, startPoint.X);
            Canvas.SetTop(SelectionRect, startPoint.Y);
            SelectionRect.Width = 0;
            SelectionRect.Height = 0;
        }

        private void RootCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;

                var endPoint = e.GetPosition(RootCanvas);
                var rect = new Rect(
                    Math.Min(startPoint.X, endPoint.X),
                    Math.Min(startPoint.Y, endPoint.Y),
                    Math.Abs(endPoint.X - startPoint.X),
                    Math.Abs(endPoint.Y - startPoint.Y)
                );

                // Validate selection
                if (rect.Width > 50 && rect.Height > 50) // Reasonable minimum size
                {
                    SelectedRect = rect;
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please select a larger area (minimum 50x50 pixels).", "Area Too Small", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SelectedRect = null;
                    this.DialogResult = false;
                }

                this.Close();
            }
        }

        private void RootCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(RootCanvas);

                double x = Math.Min(currentPoint.X, startPoint.X);
                double y = Math.Min(currentPoint.Y, startPoint.Y);
                double width = Math.Abs(currentPoint.X - startPoint.X);
                double height = Math.Abs(currentPoint.Y - startPoint.Y);

                Canvas.SetLeft(SelectionRect, x);
                Canvas.SetTop(SelectionRect, y);
                SelectionRect.Width = width;
                SelectionRect.Height = height;

                // Update instructions with size info
                tbInstructions.Text = $"Selection: {(int)width}x{(int)height} - Release mouse when ready (ESC to cancel)";
            }
        }
    }
}