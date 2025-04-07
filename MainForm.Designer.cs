namespace WebcamViewer
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // videoDisplay
            this.videoDisplay = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.videoDisplay)).BeginInit();
            this.videoDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.videoDisplay.BackColor = System.Drawing.Color.Black;
            this.videoDisplay.Location = new System.Drawing.Point(0, 0);
            this.videoDisplay.Name = "videoDisplay";
            this.videoDisplay.TabIndex = 0;
            this.videoDisplay.TabStop = false;
            
            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 240);
            this.Controls.Add(this.videoDisplay);
            this.Name = "MainForm";
            this.Text = "Webcam Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.videoDisplay)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox videoDisplay;
    }
} 