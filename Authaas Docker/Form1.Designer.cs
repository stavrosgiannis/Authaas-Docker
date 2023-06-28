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
            components = new System.ComponentModel.Container();
            listBoxLogs = new ListBox();
            progressBar1 = new ProgressBar();
            button1 = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
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
            // button1
            // 
            button1.Location = new Point(744, 405);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(831, 457);
            Controls.Add(button1);
            Controls.Add(progressBar1);
            Controls.Add(listBoxLogs);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AuthaaS Docker Installer";
            Load += Form1_Load;
            Shown += Form1_Shown;
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBoxLogs;
        private ProgressBar progressBar1;
        private Button button1;
        private System.Windows.Forms.Timer timer1;
    }
}