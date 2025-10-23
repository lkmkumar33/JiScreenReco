using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace JiScreenReco
{
    public partial class MainWindow : Window
    {
        private string outputPath;
        private Process recordingProcess;
        private bool isFullScreen = true;
        private Rect? selectedArea;
        private DispatcherTimer timer;
        private DispatcherTimer countdownTimer;
        private TimeSpan recordingTime;
        private bool isRecording = false;
        private int countdownSeconds = 3;
        private string ffmpegPath;

        // Quality settings
        private int frameRate = 30;
        private int crfValue = 18;
        private string preset = "medium";
        private string pixelFormat = "yuv420p";

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            btnStop.IsEnabled = false;
            btnScreenshot.IsEnabled = false;

            // Recording timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            // Countdown timer
            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.Tick += CountdownTimer_Tick;

            // Enable keyboard shortcuts
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            // Find FFmpeg on startup
            FindFfmpeg();
            UpdateStatusDisplay();
        }

        private void FindFfmpeg()
        {
            ffmpegPath = FindFfmpegPath();
            if (ffmpegPath != null)
            {
                btnScreenshot.IsEnabled = true;
                UpdateStatusDisplay();
            }
            else
            {
                statusText.Text = "FFmpeg NOT found";
                btnStart.IsEnabled = false;
                btnScreenshot.IsEnabled = false;
                ShowFfmpegError();
            }
        }

        private string FindFfmpegPath()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "ffmpeg";
                    process.StartInfo.Arguments = "-version";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(2000);

                    if (!process.HasExited)
                        process.Kill();

                    if (output.Contains("ffmpeg version"))
                    {
                        return "ffmpeg";
                    }
                }
            }
            catch
            {
                // Continue to check other locations
            }

            string[] possiblePaths = {
                "ffmpeg.exe",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"),
                Path.Combine(Environment.CurrentDirectory, "ffmpeg.exe"),
                @"C:\ffmpeg\bin\ffmpeg.exe"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
            }

            return null;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F9:
                    if (btnStart.IsEnabled)
                    {
                        StartRecordingWithCountdown();
                        e.Handled = true;
                    }
                    break;

                case Key.F10:
                    if (btnStop.IsEnabled)
                    {
                        StopRecording();
                        e.Handled = true;
                    }
                    break;

                case Key.F11:
                    if (btnScreenshot.IsEnabled)
                    {
                        ExtractFramesFromVideo();
                        e.Handled = true;
                    }
                    break;

                case Key.R when (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control:
                    if (btnStart.IsEnabled)
                    {
                        StartRecordingWithCountdown();
                        e.Handled = true;
                    }
                    break;

                case Key.S when (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control:
                    if (btnStop.IsEnabled)
                    {
                        StopRecording();
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    if (isRecording)
                    {
                        StopRecording();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void BtnScreenMode_Click(object sender, RoutedEventArgs e)
        {
            ShowScreenSelectionMenu();
        }

        private void ShowScreenSelectionMenu()
        {
            var contextMenu = new ContextMenu();

            var fullScreenItem = new MenuItem
            {
                Header = "🖵 Full Screen",
                FontWeight = isFullScreen ? FontWeights.Bold : FontWeights.Normal
            };
            fullScreenItem.Click += (s, e) =>
            {
                isFullScreen = true;
                selectedArea = null;
                UpdateStatusDisplay();
            };

            var selectAreaItem = new MenuItem
            {
                Header = "📏 Select Area",
                FontWeight = (!isFullScreen) ? FontWeights.Bold : FontWeights.Normal
            };
            selectAreaItem.Click += BtnSelectArea_Click;

            // Quality settings submenu
            var qualityMenu = new MenuItem { Header = "⚙ Quality Settings" };

            var fpsMenu = new MenuItem { Header = $"Frame Rate: {frameRate}fps" };
            fpsMenu.Items.Add(CreateQualityMenuItem("24 fps", () => { frameRate = 24; }));
            fpsMenu.Items.Add(CreateQualityMenuItem("30 fps", () => { frameRate = 30; }));
            fpsMenu.Items.Add(CreateQualityMenuItem("60 fps", () => { frameRate = 60; }));

            var qualityLevelMenu = new MenuItem { Header = $"Quality: CRF {crfValue}" };
            qualityLevelMenu.Items.Add(CreateQualityMenuItem("High Quality (CRF 18)", () => { crfValue = 18; }));
            qualityLevelMenu.Items.Add(CreateQualityMenuItem("Good Quality (CRF 23)", () => { crfValue = 23; }));
            qualityLevelMenu.Items.Add(CreateQualityMenuItem("Balanced (CRF 25)", () => { crfValue = 25; }));

            var presetMenu = new MenuItem { Header = $"Preset: {preset}" };
            presetMenu.Items.Add(CreateQualityMenuItem("Fastest (ultrafast)", () => { preset = "ultrafast"; }));
            presetMenu.Items.Add(CreateQualityMenuItem("Fast (fast)", () => { preset = "fast"; }));
            presetMenu.Items.Add(CreateQualityMenuItem("Balanced (medium)", () => { preset = "medium"; }));
            presetMenu.Items.Add(CreateQualityMenuItem("Better (slow)", () => { preset = "slow"; }));

            qualityMenu.Items.Add(fpsMenu);
            qualityMenu.Items.Add(qualityLevelMenu);
            qualityMenu.Items.Add(presetMenu);

            contextMenu.Items.Add(fullScreenItem);
            contextMenu.Items.Add(selectAreaItem);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(qualityMenu);

            if (!isFullScreen && selectedArea.HasValue)
            {
                var areaInfo = new MenuItem
                {
                    Header = $"Area: {(int)selectedArea.Value.Width}x{(int)selectedArea.Value.Height}",
                    IsEnabled = false,
                    FontSize = 10
                };
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(areaInfo);
            }

            contextMenu.PlacementTarget = btnScreenMode;
            contextMenu.IsOpen = true;
        }

        private MenuItem CreateQualityMenuItem(string header, Action action)
        {
            var menuItem = new MenuItem { Header = header };
            menuItem.Click += (s, e) => action();
            return menuItem;
        }

        private void UpdateStatusDisplay()
        {
            if (isRecording) return;

            if (isFullScreen)
            {
                statusText.Text = ffmpegPath != null ? "Ready - Full Screen" : "FFmpeg not found";
            }
            else if (selectedArea.HasValue)
            {
                statusText.Text = $"Ready - Area: {(int)selectedArea.Value.Width}x{(int)selectedArea.Value.Height}";
            }
            else
            {
                isFullScreen = true;
                statusText.Text = ffmpegPath != null ? "Ready - Full Screen" : "FFmpeg not found";
            }
        }

        private void StartRecordingWithCountdown()
        {
            if (!btnStart.IsEnabled) return;

            if (ffmpegPath == null)
            {
                FindFfmpeg();
                if (ffmpegPath == null)
                {
                    ShowFfmpegError();
                    return;
                }
            }

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = false;
            btnScreenMode.IsEnabled = false;
            btnScreenshot.IsEnabled = false;

            countdownSeconds = 3;
            countdownBorder.Visibility = Visibility.Visible;
            countdownText.Text = $"{countdownSeconds}";
            statusText.Text = "Get ready...";

            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            countdownSeconds--;

            if (countdownSeconds > 0)
            {
                countdownText.Text = $"{countdownSeconds}";
                System.Media.SystemSounds.Beep.Play();
            }
            else
            {
                countdownTimer.Stop();
                countdownBorder.Visibility = Visibility.Collapsed;
                countdownText.Text = "";

                _ = StartRecording();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isRecording)
            {
                recordingTime = recordingTime.Add(TimeSpan.FromSeconds(1));

                if (isFullScreen)
                {
                    statusText.Text = $"Recording: {recordingTime:hh\\:mm\\:ss}";
                }
                else
                {
                    statusText.Text = $"Recording Area: {recordingTime:hh\\:mm\\:ss}";
                }
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            StartRecordingWithCountdown();
        }

        private async Task StartRecording()
        {
            try
            {
                statusText.Text = "Starting...";

                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "JiScreenReco");
                Directory.CreateDirectory(folder);

                string fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
                outputPath = Path.Combine(folder, fileName);

                string ffmpegArgs = BuildFfmpegCommand();
                if (ffmpegArgs == null)
                {
                    ResetButtons();
                    return;
                }

                await StartFfmpegProcess(ffmpegArgs);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetButtons();
                statusText.Text = "Failed to start";
            }
        }

        private string BuildFfmpegCommand()
        {
            string baseCommand;

            if (isFullScreen)
            {
                baseCommand = $"-y -f gdigrab -framerate {frameRate} -i desktop";
            }
            else if (selectedArea.HasValue)
            {
                var rect = selectedArea.Value;

                if (rect.Width <= 10 || rect.Height <= 10)
                {
                    MessageBox.Show("Area too small", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                baseCommand = $"-y -f gdigrab -framerate {frameRate} -offset_x {(int)rect.X} -offset_y {(int)rect.Y} -video_size {(int)rect.Width}x{(int)rect.Height} -i desktop";
            }
            else
            {
                isFullScreen = true;
                UpdateStatusDisplay();
                baseCommand = $"-y -f gdigrab -framerate {frameRate} -i desktop";
            }

            string qualityParams = $"-c:v libx264 -preset {preset} -crf {crfValue} -pix_fmt {pixelFormat}";
            string advancedParams = "-profile:v high -level 4.1 -movflags +faststart";
            string keyframeParams = $"-g {frameRate} -keyint_min {frameRate}";

            return $"{baseCommand} {qualityParams} {advancedParams} {keyframeParams} \"{outputPath}\"";
        }

        private async Task StartFfmpegProcess(string ffmpegArgs)
        {
            try
            {
                await Task.Run(() =>
                {
                    recordingProcess = new Process();
                    recordingProcess.StartInfo.FileName = ffmpegPath;
                    recordingProcess.StartInfo.Arguments = ffmpegArgs;
                    recordingProcess.StartInfo.UseShellExecute = false;
                    recordingProcess.StartInfo.CreateNoWindow = true;
                    recordingProcess.StartInfo.RedirectStandardInput = true;
                    recordingProcess.StartInfo.RedirectStandardError = true;
                    recordingProcess.EnableRaisingEvents = true;

                    recordingProcess.Exited += RecordingProcess_Exited;
                    recordingProcess.ErrorDataReceived += RecordingProcess_ErrorDataReceived;

                    recordingProcess.Start();
                    recordingProcess.BeginErrorReadLine();

                    System.Threading.Thread.Sleep(2000);

                    if (recordingProcess.HasExited)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            statusText.Text = "FFmpeg failed";
                            ResetButtons();
                        });
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        isRecording = true;
                        recordingTime = TimeSpan.Zero;
                        timer.Start();
                        statusText.Text = "Recording: 00:00:00";
                        btnStop.IsEnabled = true;
                    });
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"FFmpeg error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetButtons();
                    statusText.Text = "Error";
                });
            }
        }

        private void RecordingProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.WriteLine($"FFmpeg: {e.Data}");
            }
        }

        private void RecordingProcess_Exited(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                isRecording = false;
                timer.Stop();

                if (recordingProcess != null)
                {
                    if (recordingProcess.ExitCode == 0 || recordingProcess.ExitCode == 255)
                    {
                        statusText.Text = $"Saved: {Path.GetFileName(outputPath)}";
                    }
                    else
                    {
                        statusText.Text = "Failed";
                    }

                    recordingProcess.Dispose();
                    recordingProcess = null;
                }

                ResetButtons();
            });
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void StopRecording()
        {
            try
            {
                if (recordingProcess != null && !recordingProcess.HasExited)
                {
                    statusText.Text = "Stopping...";
                    recordingProcess.StandardInput.Write("q");
                    recordingProcess.StandardInput.Close();

                    if (!recordingProcess.WaitForExit(3000))
                    {
                        recordingProcess.Kill();
                        statusText.Text = "Stopped";
                    }
                }
                else
                {
                    ResetButtons();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Stop error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetButtons();
                statusText.Text = "Error";
            }
        }

        private void ResetButtons()
        {
            btnStart.IsEnabled = ffmpegPath != null;
            btnStop.IsEnabled = false;
            btnScreenMode.IsEnabled = true;
            btnScreenshot.IsEnabled = ffmpegPath != null;
            isRecording = false;
            UpdateStatusDisplay();
        }

        private void ShowFfmpegError()
        {
            MessageBox.Show(
                "FFmpeg not found!\n\nCopy ffmpeg.exe to app folder or add to PATH.",
                "FFmpeg Required",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void BtnSelectArea_Click(object sender, RoutedEventArgs e)
        {
            SelectRecordingArea();
        }

        private void SelectRecordingArea()
        {
            try
            {
                this.WindowState = WindowState.Minimized;

                var delayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                delayTimer.Tick += (s, args) =>
                {
                    delayTimer.Stop();

                    var selector = new AreaSelectorWindow();
                    var result = selector.ShowDialog();

                    this.WindowState = WindowState.Normal;
                    this.Activate();

                    if (result == true && selector.SelectedRect.HasValue)
                    {
                        isFullScreen = false;
                        selectedArea = selector.SelectedRect.Value;
                        UpdateStatusDisplay();
                    }
                    else
                    {
                        UpdateStatusDisplay();
                    }
                };
                delayTimer.Start();
            }
            catch (Exception ex)
            {
                this.WindowState = WindowState.Normal;
                UpdateStatusDisplay();
            }
        }

        private void BtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            ExtractFramesFromVideo();
        }

        private void ExtractFramesFromVideo()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Video Files (*.mp4;*.avi;*.mov;*.mkv)|*.mp4;*.avi;*.mov;*.mkv|All Files (*.*)|*.*",
                    Title = "Select Video File to Extract Frames"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string videoPath = openFileDialog.FileName;

                    // Simple extraction - extract keyframes
                    string outputFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        "JiScreenReco_Screenshots",
                        Path.GetFileNameWithoutExtension(videoPath) + "_frames"
                    );

                    Directory.CreateDirectory(outputFolder);

                    string ffmpegArgs = $"-i \"{videoPath}\" -vf \"select=eq(pict_type\\,I)\" -vsync 0 \"{Path.Combine(outputFolder, "frame_%04d.png")}\"";

                    _ = ExtractFramesAsync(ffmpegArgs, outputFolder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExtractFramesAsync(string ffmpegArgs, string outputFolder)
        {
            try
            {
                statusText.Text = "Extracting frames...";
                btnScreenshot.IsEnabled = false;

                await Task.Run(() =>
                {
                    using (var extractProcess = new Process())
                    {
                        extractProcess.StartInfo.FileName = ffmpegPath;
                        extractProcess.StartInfo.Arguments = ffmpegArgs;
                        extractProcess.StartInfo.UseShellExecute = false;
                        extractProcess.StartInfo.CreateNoWindow = true;
                        extractProcess.StartInfo.RedirectStandardOutput = true;
                        extractProcess.StartInfo.RedirectStandardError = true;

                        extractProcess.Start();
                        extractProcess.WaitForExit();

                        Dispatcher.Invoke(() =>
                        {
                            if (extractProcess.ExitCode == 0)
                            {
                                int frameCount = Directory.GetFiles(outputFolder, "*.png").Length;
                                statusText.Text = $"Extracted {frameCount} frames";
                                MessageBox.Show($"Successfully extracted {frameCount} high-quality frames!\n\nLocation: {outputFolder}",
                                              "Extraction Complete",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Information);

                                Process.Start("explorer.exe", outputFolder);
                            }
                            else
                            {
                                statusText.Text = "Frame extraction failed";
                                MessageBox.Show("Failed to extract frames. Please check the video file.",
                                              "Extraction Failed",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                            }

                            btnScreenshot.IsEnabled = true;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error extracting frames: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    statusText.Text = "Extraction error";
                    btnScreenshot.IsEnabled = true;
                });
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (recordingProcess != null && !recordingProcess.HasExited)
            {
                recordingProcess.Kill();
                recordingProcess.Dispose();
            }

            countdownTimer?.Stop();
            timer?.Stop();

            base.OnClosed(e);
        }
    }
}