using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TracerLibrary;
using System.Threading;

namespace WorkflowApplication
{
    public partial class WorkflowForm : Form
    {
        
        string currentJobID = "";

        public WorkflowForm()
        {
            InitializeComponent();

            infoView.Columns.Add("Job", 30);
            infoView.Columns.Add("ID", 50);
            infoView.Columns.Add("Task", 40);
            infoView.Columns.Add("Item", 40);
            infoView.Columns.Add("Description", 500);

            Program.pipeClient.SendMessage("info");
            Thread.Sleep(100);
            refresh(infoView, Program.info);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {

            Program.pipeClient.SendMessage("info");
            Thread.Sleep(10);
            refresh(infoView, Program.info);

        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (currentJobID == "")
            {
                Program.pipeClient.SendMessage("cancel");
            }
            else
            {
                Program.pipeClient.SendMessage("cancel " + currentJobID.ToString());
            }
            Thread.Sleep(10);
            refresh(infoView, Program.info);
        }

        private string Level(int level)
        {
            string text = "";
            for (int i = 1; i < level; i++)
            {
                text = text + "\t";
            }
            return (text);
        }

        private void refresh(ListView listView, XmlReader xmlReader)
        {
            ListViewItem item = null;
            string element = "";
            int stage = 0;
            string jobID = "";
            string taskID = "";
            string itemID = "";
            //string jobDescription = "";
            //string taskDescription = "";
            string itemState = "";
            string text = "";
            string jobName = "";
            string taskName = "";
            string itemName = "";
            string itemDescription = "";
            bool added = false;

            listView.Items.Clear();
            if (xmlReader == null)
            {
                listView.Refresh();
            }
            else
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                // stuff
                                element = xmlReader.LocalName.ToLower();

                                TraceInternal.TraceVerbose(Level(stage) + "<" + element + ">");
                                switch (element)
                                {
                                    case "jobs":
                                        added = false;
                                        stage = stage + 1;
                                        break;
                                    case "job":
                                        {
                                            while (xmlReader.MoveToNextAttribute())
                                            {
                                                switch (xmlReader.Name.ToLower())
                                                {
                                                    case "id":
                                                        jobID = xmlReader.Value;
                                                        TraceInternal.TraceVerbose(Level(stage) + "id=" + jobID);
                                                        break;
                                                    case "name":
                                                        jobName = xmlReader.Value;
                                                        TraceInternal.TraceVerbose(Level(stage) + "name=" + jobName);
                                                        break;
                                                }
                                            }

                                            break;
                                        }
                                    case "tasks":
                                        stage = stage + 1;
                                        break;
                                    case "task":
                                        {
                                            while (xmlReader.MoveToNextAttribute())
                                            {
                                                switch (xmlReader.Name.ToLower())
                                                {
                                                    case "id":
                                                        taskID = xmlReader.Value;
                                                        TraceInternal.TraceVerbose(Level(stage) + "id=" + taskID);
                                                        break;
                                                    case "name":
                                                        taskName = xmlReader.Value;
                                                        TraceInternal.TraceVerbose(Level(stage) + "name=" + taskName);
                                                        break;
                                                }
                                            }
                                            break;
                                        }
                                    case "items":
                                        stage = stage + 1;
                                        break;
                                    case "item":
                                        {
                                            itemID = "";
                                            itemState = "";
                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            itemID = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(stage) + "id=" + itemID);
                                                            break;
                                                        case "name":
                                                            itemName = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(stage) + "name=" + itemName);
                                                            break;
                                                        case "type":
                                                            TraceInternal.TraceVerbose(Level(stage) + "type=" + xmlReader.Value);
                                                            break;
                                                        case "state":
                                                            itemState = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(stage) + "state=" + xmlReader.Value);
                                                            break;
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        case XmlNodeType.EndElement:
                            {
                                element = xmlReader.LocalName;
                                TraceInternal.TraceVerbose(Level(stage) + "</" + element + ">");
                                switch (element)
                                {
                                    case "jobs":

                                        if (added == false)
                                        {
                                            item = listView.Items.Add(jobID.ToString());
                                            item.SubItems.Add(jobName);
                                        }
                                        stage = stage - 1;
                                        break;
                                    case "tasks":
                                        stage = stage - 1;
                                        break;
                                    case "items":
                                        stage = stage - 1;
                                        break;
                                }
                                break;
                            }
                        case XmlNodeType.Text:
                            {
                                text = xmlReader.Value;
                                TraceInternal.TraceVerbose(Level(stage) + "\t" + text);
                                switch (element)
                                {
                                    case "description":
                                        switch (stage)
                                        {
                                            case 1:
                                                // Job
                                                break;
                                            case 2:
                                                // Task
                                                break;
                                            case 3:
                                                // Item
                                                itemDescription = text;
                                                if (itemState.ToLower() == "active")
                                                {
                                                    added = true;
                                                    item = listView.Items.Add(jobID.ToString());
                                                    item.SubItems.Add(jobName);
                                                    item.SubItems.Add(taskID.ToString());
                                                    item.SubItems.Add(itemID.ToString());
                                                    if (itemName == "")
                                                    {
                                                        item.SubItems.Add(itemDescription);
                                                    }
                                                    else
                                                    {
                                                        item.SubItems.Add(itemName);
                                                    }
                                                    TraceInternal.TraceVerbose(Level(stage) + "Add info:" + jobID.ToString() + "," + taskID.ToString() + "," + itemID.ToString() + "," + itemDescription);
                                                }
                                                break;
                                        }
                                        break;
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            if (currentJobID == "")
            {
                Program.pipeClient.SendMessage("process");
            }
            else
            {
                Program.pipeClient.SendMessage("process " + currentJobID.ToString());
            }
            Thread.Sleep(10);
            refresh(infoView, Program.info);
        }

        private void infoView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.infoView.SelectedIndices.Count > 0)
            {
                currentJobID = this.infoView.Items[this.infoView.SelectedIndices[0]].Text;   
            }
            else
            {
                currentJobID = "";
            }
            this.jobLabel.Text = "Job=" + currentJobID.ToString();
        }

    }
}