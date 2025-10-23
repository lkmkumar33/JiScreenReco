using System.Windows;

namespace JiScreenReco
{
    public partial class FrameExtractionWindow : Window
    {
        public bool ExtractAllFrames { get; private set; }
        public bool ExtractKeyFrames { get; private set; }
        public double TimeInterval { get; private set; }

        public FrameExtractionWindow()
        {
            InitializeComponent();
            rbKeyFrames.IsChecked = true;
        }

        private void Extract_Click(object sender, RoutedEventArgs e)
        {
            if (rbAllFrames.IsChecked == true)
            {
                ExtractAllFrames = true;
            }
            else if (rbKeyFrames.IsChecked == true)
            {
                ExtractKeyFrames = true;
            }
            else if (rbTimeInterval.IsChecked == true)
            {
                if (double.TryParse(txtInterval.Text, out double interval) && interval > 0)
                {
                    TimeInterval = interval;
                }
                else
                {
                    MessageBox.Show("Please enter a valid time interval.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}