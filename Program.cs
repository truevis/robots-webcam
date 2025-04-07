using System;
using System.IO;
using System.Windows.Forms;

namespace WebcamViewer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting application: {ex.Message}\n\nStack trace: {ex.StackTrace}", 
                    "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Log to file for debugging
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
                File.AppendAllText(logPath, 
                    $"[{DateTime.Now}] Error: {ex.Message}\r\n{ex.StackTrace}\r\n\r\n");
            }
        }
    }
} 