namespace WorkflowApplication
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            Program.pipeClient.Disconnect();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblReceived = new System.Windows.Forms.Label();
            this.tbReceived = new System.Windows.Forms.TextBox();
            this.lblSend = new System.Windows.Forms.Label();
            this.tbSend = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblPipeName = new System.Windows.Forms.Label();
            this.tbPipeName = new System.Windows.Forms.TextBox();
            this.JobsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblReceived
            // 
            this.lblReceived.AutoSize = true;
            this.lblReceived.Location = new System.Drawing.Point(9, 202);
            this.lblReceived.Name = "lblReceived";
            this.lblReceived.Size = new System.Drawing.Size(107, 13);
            this.lblReceived.TabIndex = 15;
            this.lblReceived.Text = "Received Messages:";
            // 
            // tbReceived
            // 
            this.tbReceived.Location = new System.Drawing.Point(12, 223);
            this.tbReceived.Multiline = true;
            this.tbReceived.Name = "tbReceived";
            this.tbReceived.ReadOnly = true;
            this.tbReceived.Size = new System.Drawing.Size(298, 74);
            this.tbReceived.TabIndex = 14;
            // 
            // lblSend
            // 
            this.lblSend.Location = new System.Drawing.Point(0, 0);
            this.lblSend.Name = "lblSend";
            this.lblSend.Size = new System.Drawing.Size(100, 23);
            this.lblSend.TabIndex = 18;
            // 
            // tbSend
            // 
            this.tbSend.Location = new System.Drawing.Point(12, 84);
            this.tbSend.Multiline = true;
            this.tbSend.Name = "tbSend";
            this.tbSend.Size = new System.Drawing.Size(298, 86);
            this.tbSend.TabIndex = 11;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(235, 23);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 10;
            this.btnStart.Text = "Connect";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblPipeName
            // 
            this.lblPipeName.AutoSize = true;
            this.lblPipeName.Location = new System.Drawing.Point(12, 9);
            this.lblPipeName.Name = "lblPipeName";
            this.lblPipeName.Size = new System.Drawing.Size(62, 13);
            this.lblPipeName.TabIndex = 9;
            this.lblPipeName.Text = "Pipe Name:";
            // 
            // tbPipeName
            // 
            this.tbPipeName.Location = new System.Drawing.Point(12, 25);
            this.tbPipeName.Name = "tbPipeName";
            this.tbPipeName.Size = new System.Drawing.Size(217, 20);
            this.tbPipeName.TabIndex = 8;
            this.tbPipeName.Text = "\\\\.\\pipe\\Jobs";
            // 
            // JobsButton
            // 
            this.JobsButton.Location = new System.Drawing.Point(15, 176);
            this.JobsButton.Name = "JobsButton";
            this.JobsButton.Size = new System.Drawing.Size(75, 23);
            this.JobsButton.TabIndex = 16;
            this.JobsButton.Text = "Jobs";
            this.JobsButton.UseVisualStyleBackColor = true;
            this.JobsButton.Click += new System.EventHandler(this.JobsButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 314);
            this.Controls.Add(this.JobsButton);
            this.Controls.Add(this.lblReceived);
            this.Controls.Add(this.tbReceived);
            this.Controls.Add(this.lblSend);
            this.Controls.Add(this.tbSend);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblPipeName);
            this.Controls.Add(this.tbPipeName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.Text = "JobsApp";
            //this.Close += new System.EventHandler(this.MainForm_Close);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblReceived;
        private System.Windows.Forms.TextBox tbReceived;
        private System.Windows.Forms.Label lblSend;
        private System.Windows.Forms.TextBox tbSend;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblPipeName;
        private System.Windows.Forms.TextBox tbPipeName;
        private System.Windows.Forms.Button JobsButton;

    }
}

