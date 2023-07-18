using System;
using System.Diagnostics;
using System.Windows.Forms;
using WorkflowTray.Properties;
using TracerLibrary;

namespace WorkflowTray
{
	/// <summary>
	/// 
	/// </summary>
	class ProcessIcon : IDisposable
	{
        

		/// <summary>
		/// The NotifyIcon object.
		/// </summary>
        /// 
		readonly NotifyIcon notifyIcon;
        Timer processTimer;
        readonly ServiceManager manager;
        readonly ContextMenuStrip menu = new ContextMenuStrip();
        bool isAboutLoaded = false;
        string jobPath = "";
        string jobName = "";

		/// <summary>
		/// Initializes a new instance of the <see cref="ProcessIcon"/> class.
		/// </summary>
		public ProcessIcon()
		{
			// Instantiate the NotifyIcon object.
			notifyIcon = new NotifyIcon();
            manager = new ServiceManager("Workflow");
		}

		/// <summary>
		/// Displays the icon in the system tray.
		/// </summary>
		public void Display()
		{
			// Put the icon in the system tray and allow it react to mouse clicks.			
			notifyIcon.MouseClick += new MouseEventHandler(NotifyIconi_MouseClick);
			notifyIcon.Icon = Resources.WorkflowTray;
			notifyIcon.Text = "Workflow";
			notifyIcon.Visible = true;

            ToolStripMenuItem item;
            ToolStripSeparator sep;

            // About.
            item = new ToolStripMenuItem();
            item.Name = "about";
            item.Text = "About";
            item.Click += new EventHandler(About_Click);
            item.Image = Resources.About;
            item.Enabled = true;
            menu.Items.Add(item);

            // Start.
            item = new ToolStripMenuItem();
            item.Name = "start";
            item.Text = "Start";
            item.Click += new EventHandler(Start_Click);
            item.Image = Resources.Start;
            item.Enabled = true;
            menu.Items.Add(item);

            // Stop.
            item = new ToolStripMenuItem();
            item.Name = "stop";
            item.Text = "Stop";
            item.Click += new EventHandler(Stop_Click);
            item.Image = Resources.Stop;
            item.Enabled = false;
            menu.Items.Add(item);

            // Pause.
            item = new ToolStripMenuItem();
            item.Name = "pause";
            item.Text = "Pause";
            item.Click += new EventHandler(Pause_Click);
            item.Image = Resources.Pause;
            item.Enabled = false;
            menu.Items.Add(item);

            // Restart.
            item = new ToolStripMenuItem();
            item.Name = "restart";
            item.Text = "Restart";
            item.Click += new EventHandler(Restart_Click);
            item.Image = Resources.Restart;
            item.Enabled = false;
            menu.Items.Add(item);

            // Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

            // Exit.
            item = new ToolStripMenuItem();
            item.Name = "exit";
            item.Text = "Exit";
            item.Click += new System.EventHandler(Exit_Click);
            item.Image = Resources.Exit;
            item.Enabled = true;
            menu.Items.Add(item);

			// Attach a context menu.
			notifyIcon.ContextMenuStrip = menu;          
		}

        public void Start(string name,string path)
        {
            // Set the location

            jobName = name;
            jobPath = path;

            // Start a timer to keep track of the service state.

            processTimer = new Timer();
            processTimer.Tick += new EventHandler(TimerEventProcessor);

            // Set the Interval to 5 seconds.
            processTimer.Interval = 5000;
            processTimer.Enabled = true;
        }

        public void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            // check the service status

            Debug.WriteLine("In TimerEventProcessor()");
            string check = manager.Check().ToLower();
            switch (check)
            {
                case "resuming":
                    {
                        Debug.WriteLine("Outresuming");
                        menu.Items["start"].Enabled = false;
                        menu.Items["stop"].Enabled = true;
                        menu.Items["pause"].Enabled = true;
                        menu.Items["restart"].Enabled = true;
                        notifyIcon.Icon = Resources.WorkflowTrayChange;
                        break;
                    }
                case "pausing":
                    {
                        Debug.WriteLine("Outresuming");
                        menu.Items["start"].Enabled = false;
                        menu.Items["stop"].Enabled = true;
                        menu.Items["pause"].Enabled = true;
                        menu.Items["restart"].Enabled = true;
                        notifyIcon.Icon = Resources.WorkflowTrayChange;
                        break;
                    }
                case "starting":
                    {
                        Debug.WriteLine("Outstarting");
                        menu.Items["start"].Enabled = false;
                        menu.Items["stop"].Enabled = true;
                        menu.Items["pause"].Enabled = false;
                        menu.Items["restart"].Enabled = false;
                        notifyIcon.Icon = Resources.WorkflowTrayChange;
                        break;
                    }
                case "started":
                    {
                        Debug.WriteLine("Outrunning");
                        menu.Items["start"].Enabled = false;
                        menu.Items["stop"].Enabled = true;
                        menu.Items["pause"].Enabled = true;
                        menu.Items["restart"].Enabled = true;
                        notifyIcon.Icon = Resources.WorkflowTrayRun;
                        break;
                    }
                case "stopping":
                    {
                        Debug.WriteLine("Outstopping");
                        menu.Items["start"].Enabled = true;
                        menu.Items["stop"].Enabled = false;
                        menu.Items["pause"].Enabled = false;
                        menu.Items["restart"].Enabled = false;
                        notifyIcon.Icon = Resources.WorkflowTrayChange;
                        break;
                    }
                case "stopped":
                    {
                        Debug.WriteLine("Outstopped");
                        menu.Items["start"].Enabled = true;
                        menu.Items["stop"].Enabled = false;
                        menu.Items["pause"].Enabled = false;
                        menu.Items["restart"].Enabled = false;
                        notifyIcon.Icon = Resources.WorkflowTrayStop;
                        break;
                    }
                case "":
                    {
                        break;
                    }
                default:
                    {
                        TraceInternal.TraceVerbose(check);
                        break;
                    }
         
            }
            Debug.WriteLine("Out TimerEventProcessor()");
        }

        /// <summary>
        /// Handles the Click event of the About control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void About_Click(object sender, EventArgs e)
        {
            if (!isAboutLoaded)
            {
                isAboutLoaded = true;
                new AboutBox().ShowDialog();
                isAboutLoaded = false;
            }
        }

        void Start_Click(Object sender, EventArgs e)
        {
            manager.Start();
        }

        void Stop_Click(Object sender, EventArgs e)
        {
            manager.Stop();
        }

        void Restart_Click(Object sender, EventArgs e)
        {
            manager.Restart();
        }

        void Pause_Click(Object sender, EventArgs e)
        {
            manager.Restart();
        }

        /// <summary>
        /// Handles the Exit event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Exit_Click(object sender, EventArgs e)
        {
            // Quit without further ado.
            Application.Exit();
        }

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		public void Dispose()
		{
			// When the application closes, this will remove the icon from the system tray immediately.
			notifyIcon.Dispose();
		}

		/// <summary>
		/// Handles the MouseClick event of the ni control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		void NotifyIconi_MouseClick(object sender, MouseEventArgs e)
		{
            Debug.WriteLine("OutFrom " + sender);
			// Handle mouse button clicks.
			if (e.Button == MouseButtons.Left)
			{
				// Start Windows Explorer.
				Process.Start("explorer", jobPath);
			}
		}
	}
}