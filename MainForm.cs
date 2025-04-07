using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace WebcamViewer
{
    public partial class MainForm : Form
    {
        private VideoCaptureDevice? videoSource;
        private FilterInfoCollection? videoDevices;
        private Point dragStartPoint;
        private bool isDragging = false;
        private readonly Size defaultSize = new Size(320, 240);

        // Window styles to remove border and make it click-through
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        // Constants for dragging the window
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public MainForm()
        {
            InitializeComponent();
            SetupForm();
            InitializeCamera();
        }

        private void SetupForm()
        {
            // Basic form settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.Size = defaultSize;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.ShowInTaskbar = true;
            this.ShowIcon = true;
            this.KeyPreview = true;
            
            // Set the application icon
            LoadApplicationIcon();

            // Set up dragging
            this.videoDisplay.MouseDown += OnMouseDown;
            this.videoDisplay.MouseMove += OnMouseMove;
            this.videoDisplay.MouseUp += OnMouseUp;
            
            // Set up keyboard shortcuts
            this.KeyDown += MainForm_KeyDown;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Location = new Point(
                    this.Location.X + e.X - dragStartPoint.X,
                    this.Location.Y + e.Y - dragStartPoint.Y
                );
            }
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            // Handle Ctrl+Plus for zoom in
            if (e.Control && (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus))
            {
                ResizeWindow(true);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Handle Ctrl+Minus for zoom out
            else if (e.Control && (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus))
            {
                ResizeWindow(false);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Handle Ctrl+0 for reset
            else if (e.Control && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
            {
                this.Size = defaultSize;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ResizeWindow(bool increase)
        {
            // Get current screen dimensions
            Rectangle screenBounds = Screen.FromControl(this).Bounds;
            
            int newWidth = this.Width + (increase ? defaultSize.Width : -defaultSize.Width);
            int newHeight = this.Height + (increase ? defaultSize.Height : -defaultSize.Height);
            
            // Check if new size would be too large for the screen
            if (newWidth > screenBounds.Width || newHeight > screenBounds.Height)
            {
                // Reset to default size
                this.Size = defaultSize;
            }
            // Check if new size would be too small
            else if (newWidth >= defaultSize.Width && newHeight >= defaultSize.Height)
            {
                this.Size = new Size(newWidth, newHeight);
            }
        }

        private void InitializeCamera()
        {
            try
            {
                // Get list of video devices
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (videoDevices.Count == 0)
                {
                    MessageBox.Show("No webcam found! Please connect a webcam and restart the application.", 
                        "No Camera Detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
                
                // Create video source
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                
                // Check if video source was created successfully
                if (videoSource == null)
                {
                    MessageBox.Show("Failed to create video source.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
                
                videoSource.NewFrame += VideoSource_NewFrame;
                
                try
                {
                    // Start the camera
                    videoSource.Start();
                    
                    // Verify camera started
                    if (!videoSource.IsRunning)
                    {
                        MessageBox.Show("Failed to start the camera. The device may be in use by another application.", 
                            "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error starting camera: {ex.Message}", "Camera Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing webcam: {ex.Message}\n\n{ex.StackTrace}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Log error to file
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "camera_error_log.txt");
                File.AppendAllText(logPath, 
                    $"[{DateTime.Now}] Camera error: {ex.Message}\r\n{ex.StackTrace}\r\n\r\n");
                
                this.Close();
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Clone the current frame
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            
            // Update UI on the UI thread
            if (videoDisplay.InvokeRequired)
            {
                videoDisplay.Invoke(new Action(() => {
                    // Dispose previous image to avoid memory leaks
                    videoDisplay.Image?.Dispose();
                    videoDisplay.Image = bitmap;
                }));
            }
            else
            {
                videoDisplay.Image?.Dispose();
                videoDisplay.Image = bitmap;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Unsubscribe from events to prevent memory leaks
                if (videoSource != null)
                {
                    videoSource.NewFrame -= VideoSource_NewFrame;
                }

                // Stop and clean up camera resources
                if (videoSource != null && videoSource.IsRunning)
                {
                    try
                    {
                        videoSource.SignalToStop();
                        // Add timeout to prevent hanging if device doesn't respond
                        System.Threading.Thread.Sleep(500);
                        videoSource.WaitForStop();
                        videoSource = null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error stopping video source: {ex.Message}");
                    }
                }
                
                // Clean up display resources
                if (videoDisplay?.Image != null)
                {
                    try
                    {
                        videoDisplay.Image.Dispose();
                        videoDisplay.Image = null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error disposing image: {ex.Message}");
                    }
                }

                // Dispose components
                if (components != null)
                {
                    components.Dispose();
                }

                // Force garbage collection to release resources
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during form closing: {ex.Message}");
            }
            finally
            {
                base.OnFormClosing(e);
                
                // Ensure application terminates properly
                if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
                {
                    Application.Exit();
                }
            }
        }

        private void LoadApplicationIcon()
        {
            try
            {
                // Try to load icon directly from the file path
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "app-icon.ico");
                if (File.Exists(iconPath))
                {
                    using (FileStream fs = new FileStream(iconPath, FileMode.Open, FileAccess.Read))
                    {
                        this.Icon = new Icon(fs);
                    }
                    Console.WriteLine($"Icon loaded successfully from {iconPath}");
                    return;
                }
                
                // Fallback method - try the current directory
                iconPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "app-icon.ico");
                if (File.Exists(iconPath))
                {
                    using (FileStream fs = new FileStream(iconPath, FileMode.Open, FileAccess.Read))
                    {
                        this.Icon = new Icon(fs);
                    }
                    Console.WriteLine($"Icon loaded successfully from {iconPath}");
                    return;
                }
                
                // Second fallback - try the application directory
                iconPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? "", "images", "app-icon.ico");
                if (File.Exists(iconPath))
                {
                    using (FileStream fs = new FileStream(iconPath, FileMode.Open, FileAccess.Read))
                    {
                        this.Icon = new Icon(fs);
                    }
                    Console.WriteLine($"Icon loaded successfully from {iconPath}");
                    return;
                }
                
                Console.WriteLine("Could not load application icon from any path:");
                Console.WriteLine($"- Tried: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "app-icon.ico")}");
                Console.WriteLine($"- Tried: {Path.Combine(Directory.GetCurrentDirectory(), "images", "app-icon.ico")}");
                Console.WriteLine($"- Tried: {Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? "", "images", "app-icon.ico")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading icon: {ex.Message}");
                // Don't throw the exception - just continue without an icon
            }
        }
    }
} 