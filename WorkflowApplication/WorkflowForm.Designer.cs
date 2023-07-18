namespace WorkflowApplication
{
    partial class WorkflowForm
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.infoView = new System.Windows.Forms.ListView();
            this.autoButton = new System.Windows.Forms.Button();
            this.processButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.jobLabel = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(12, 12);
            this.cancelButton.Name = "CancelButton";
            this.cancelButton.Size = new System.Drawing.Size(50, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // RefreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(68, 12);
            this.refreshButton.Name = "RefreshButton";
            this.refreshButton.Size = new System.Drawing.Size(53, 23);
            this.refreshButton.TabIndex = 1;
            this.refreshButton.Text = "&Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // infoView
            // 
            this.infoView.GridLines = true;
            this.infoView.Location = new System.Drawing.Point(12, 68);
            this.infoView.Name = "infoView";
            this.infoView.Size = new System.Drawing.Size(280, 360);
            this.infoView.TabIndex = 2;
            this.infoView.UseCompatibleStateImageBehavior = false;
            this.infoView.View = System.Windows.Forms.View.Details;
            this.infoView.SelectedIndexChanged += new System.EventHandler(this.infoView_SelectedIndexChanged);
            // 
            // AutoButton
            // 
            this.autoButton.Location = new System.Drawing.Point(127, 12);
            this.autoButton.Name = "AutoButton";
            this.autoButton.Size = new System.Drawing.Size(51, 23);
            this.autoButton.TabIndex = 3;
            this.autoButton.Text = "&Auto";
            this.autoButton.UseVisualStyleBackColor = true;
            // 
            // ProcessButton
            // 
            this.processButton.Location = new System.Drawing.Point(184, 12);
            this.processButton.Name = "ProcessButton";
            this.processButton.Size = new System.Drawing.Size(56, 23);
            this.processButton.TabIndex = 4;
            this.processButton.Text = "&Process";
            this.processButton.UseVisualStyleBackColor = true;
            this.processButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(246, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(46, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // JobLabel
            // 
            this.jobLabel.AutoSize = true;
            this.jobLabel.Location = new System.Drawing.Point(9, 38);
            this.jobLabel.Name = "JobLabel";
            this.jobLabel.Size = new System.Drawing.Size(25, 13);
            this.jobLabel.TabIndex = 6;
            this.jobLabel.Text = "Info";
            // 
            // CloseButton
            // 
            this.closeButton.Location = new System.Drawing.Point(241, 434);
            this.closeButton.Name = "CloseButton";
            this.closeButton.Size = new System.Drawing.Size(50, 23);
            this.closeButton.TabIndex = 7;
            this.closeButton.Text = "&Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // JobsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 462);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.jobLabel);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.processButton);
            this.Controls.Add(this.autoButton);
            this.Controls.Add(this.infoView);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "JobsForm";
            this.Text = "JobsForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ListView infoView;
        private System.Windows.Forms.Button autoButton;
        private System.Windows.Forms.Button processButton;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label jobLabel;
        private System.Windows.Forms.Button closeButton;
    }
}