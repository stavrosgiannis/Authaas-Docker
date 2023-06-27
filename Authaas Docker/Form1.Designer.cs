namespace Authaas_Docker
{
    partial class Form1
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
            listBoxLogs = new ListBox();
            SuspendLayout();
            // 
            // listBoxLogs
            // 
            listBoxLogs.Dock = DockStyle.Top;
            listBoxLogs.FormattingEnabled = true;
            listBoxLogs.ItemHeight = 15;
            listBoxLogs.Items.AddRange(new object[] { "Welcome to Authaas Docker Installer", "----------------------------------------" });
            listBoxLogs.Location = new Point(0, 0);
            listBoxLogs.Name = "listBoxLogs";
            listBoxLogs.Size = new Size(800, 94);
            listBoxLogs.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(listBoxLogs);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBoxLogs;
    }
}