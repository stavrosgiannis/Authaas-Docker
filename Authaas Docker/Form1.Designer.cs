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
            progressBar1 = new ProgressBar();
            SuspendLayout();
            // 
            // listBoxLogs
            // 
            listBoxLogs.Dock = DockStyle.Top;
            listBoxLogs.FormattingEnabled = true;
            listBoxLogs.ItemHeight = 15;
            listBoxLogs.Location = new Point(0, 0);
            listBoxLogs.Name = "listBoxLogs";
            listBoxLogs.Size = new Size(831, 349);
            listBoxLogs.TabIndex = 0;
            // 
            // progressBar1
            // 
            progressBar1.Dock = DockStyle.Bottom;
            progressBar1.Location = new Point(0, 434);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(831, 23);
            progressBar1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(831, 457);
            Controls.Add(progressBar1);
            Controls.Add(listBoxLogs);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AuthaaS Docker Installer";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBoxLogs;
        private ProgressBar progressBar1;
    }
}