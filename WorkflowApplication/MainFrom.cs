using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WorkflowLibrary;

namespace WorkflowApplication
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.JobsButton.Enabled = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!Program.pipeClient.Connected)
            {
                Program.pipeClient.PipeName = this.tbPipeName.Text;
                Program.pipeClient.Connect();
            }
            else
            {
                MessageBox.Show("Already connected.");
            }
            // Should recheck that we are connected
            if (Program.pipeClient.Connected)
            {
                btnStart.Enabled = false;
                this.JobsButton.Enabled = true;
            }
            else
            {
                MessageBox.Show("Not connected");
            }            
        }

        private void JobsButton_Click(object sender, EventArgs e)
        {
            Form f = new WorkflowForm();
            //f.Show();
            f.ShowDialog();
        }

        private void MainForm_Close(object sender, EventArgs e)
        {
            Program.pipeClient.Disconnect();
        }
    }
}