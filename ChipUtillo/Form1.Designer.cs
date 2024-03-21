namespace ChipUtillo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            fanCurve = new FanCurve();
            btnApplyCurve = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            lblCurrentMode = new Label();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            openToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // fanCurve
            // 
            fanCurve.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            fanCurve.BackColor = Color.Black;
            fanCurve.Location = new Point(201, 12);
            fanCurve.Name = "fanCurve";
            fanCurve.Size = new Size(446, 400);
            fanCurve.TabIndex = 2;
            // 
            // btnApplyCurve
            // 
            btnApplyCurve.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnApplyCurve.Location = new Point(73, 389);
            btnApplyCurve.Name = "btnApplyCurve";
            btnApplyCurve.Size = new Size(122, 23);
            btnApplyCurve.TabIndex = 3;
            btnApplyCurve.Text = "Apply";
            btnApplyCurve.UseVisualStyleBackColor = true;
            btnApplyCurve.Click += btnApplyCurve_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;
            // 
            // lblCurrentMode
            // 
            lblCurrentMode.AutoSize = true;
            lblCurrentMode.Location = new Point(12, 9);
            lblCurrentMode.Name = "lblCurrentMode";
            lblCurrentMode.Size = new Size(111, 15);
            lblCurrentMode.TabIndex = 4;
            lblCurrentMode.Text = "Reading HW data ...";
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Text = "ChipUtillo";
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { openToolStripMenuItem, exitToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(116, 48);
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Image = Properties.Resources.favicon;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(115, 22);
            openToolStripMenuItem.Text = "Open ...";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(115, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(659, 440);
            Controls.Add(lblCurrentMode);
            Controls.Add(btnApplyCurve);
            Controls.Add(fanCurve);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Asus Fans Curve Control - ChipUtillo";
            FormClosing += Form1_FormClosing;
            FormClosed += Form1_FormClosed;
            Resize += Form1_Resize;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private FanCurve fanCurve;
        private Button btnApplyCurve;
        private System.Windows.Forms.Timer timer1;
        private Label lblCurrentMode;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
    }
}
