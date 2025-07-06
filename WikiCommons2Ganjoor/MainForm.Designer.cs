namespace WikiCommons2Ganjoor
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
            menuStrip1 = new MenuStrip();
            readWikiToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            txtEmail = new ToolStripTextBox();
            txtPassword = new ToolStripTextBox();
            btnLogin = new ToolStripMenuItem();
            dataGridView1 = new DataGridView();
            statusStrip1 = new StatusStrip();
            labelStatus = new ToolStripStatusLabel();
            btnUpdate = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(32, 32);
            menuStrip1.Items.AddRange(new ToolStripItem[] { readWikiToolStripMenuItem, saveToolStripMenuItem, txtEmail, txtPassword, btnLogin, btnUpdate });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1542, 43);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // readWikiToolStripMenuItem
            // 
            readWikiToolStripMenuItem.Name = "readWikiToolStripMenuItem";
            readWikiToolStripMenuItem.Size = new Size(139, 39);
            readWikiToolStripMenuItem.Text = "Read Wiki";
            readWikiToolStripMenuItem.Click += readWikiToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(137, 39);
            saveToolStripMenuItem.Text = "Save Wiki";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // txtEmail
            // 
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(200, 39);
            txtEmail.Text = "test@test.com";
            txtEmail.ToolTipText = "User Email";
            // 
            // txtPassword
            // 
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(200, 39);
            txtPassword.Text = "1234567910";
            txtPassword.ToolTipText = "Password";
            // 
            // btnLogin
            // 
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(93, 39);
            btnLogin.Text = "Login";
            btnLogin.Click += btnLogin_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 43);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 82;
            dataGridView1.Size = new Size(1542, 699);
            dataGridView1.TabIndex = 1;
            dataGridView1.RowHeaderMouseDoubleClick += dataGridView1_RowHeaderMouseDoubleClick;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(32, 32);
            statusStrip1.Items.AddRange(new ToolStripItem[] { labelStatus });
            statusStrip1.Location = new Point(0, 700);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1542, 42);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // labelStatus
            // 
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(78, 32);
            labelStatus.Text = "Ready";
            // 
            // btnUpdate
            // 
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(111, 39);
            btnUpdate.Text = "Update";
            btnUpdate.Click += btnUpdate_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1542, 742);
            Controls.Add(statusStrip1);
            Controls.Add(dataGridView1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "WikiCommons 2 Ganjoor";
            Load += MainForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem readWikiToolStripMenuItem;
        private DataGridView dataGridView1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel labelStatus;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripTextBox txtEmail;
        private ToolStripTextBox txtPassword;
        private ToolStripMenuItem btnLogin;
        private ToolStripMenuItem btnUpdate;
    }
}
