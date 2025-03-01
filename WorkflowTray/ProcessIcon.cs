using System;
using System.Diagnostics;
using System.Windows.Forms;
using TracerLibrary;
using WorkflowTray.Properties;

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
		readonly NotifyIcon _notifyIcon;
        Timer _processTimer;
        readonly ServiceManager _manager;
        readonly ContextMenuStrip _menu = new ContextMenuStrip();
        bool _isAboutLoaded = false;
        string _workflowPath = "";
        string _workflowName = "";

		/// <summary>
		/// Initializes a new instance of the <see cref="ProcessIcon"/> class.
		/// </summary>
		public ProcessIcon()
		{
			// Instantiate the NotifyIcon object.
			_notifyIcon = new NotifyIcon();
            _manager = new ServiceManager("Workflow");
		}

		/// <summary>
		/// Displays the icon in the system tray.
		/// </summary>
		public void Display()
		{
			// Put the icon in the system tray and allow it react to mouse clicks.			
			_notifyIcon.MouseClick += new MouseEventHandler(NotifyIconi_MouseClick);
			_notifyIcon.Icon = Resources.WorkflowTray;
			_notifyIcon.Text = "Workflow";
			_notifyIcon.Visible = true;

            ToolStripMenuItem item;
            ToolStripSeparator sep;

            // About.
            item = new ToolStripMenuItem();
            item.Name = "about";
            item.Text = "About";
            item.Click += new EventHandler(About_Click);
            item.Image = Resources.About;
            item.Enabled = true;
            _menu.Items.Add(item);

            // Start.
            item = new ToolStripMenuItem();
            item.Name = "start";
            item.Text = "Start";
            item.Click += new EventHandler(Start_Click);
            item.Image = Resources.Start;
            item.Enabled = true;
            _menu.Items.Add(item);

            // Stop.
            item = new ToolStripMenuItem();
            item.Name = "stop";
            item.Text = "Stop";
            item.Click += new EventHandler(Stop_Click);
            item.Image = Resources.Stop;
            item.Enabled = false;
            _menu.Items.Add(item);

            // Pause.
            item = new ToolStripMenuItem();
            item.Name = "pause";
            item.Text = "Pause";
            item.Click += new EventHandler(Pause_Click);
            item.Image = Resources.Pause;
            item.Enabled = false;
            _menu.Items.Add(item);

            // Restart.
            item = new ToolStripMenuItem();
            item.Name = "restart";
            item.Text = "Restart";
            item.Click += new EventHandler(Restart_Click);
            item.Image = Resources.Restart;
            item.Enabled = false;
            _menu.Items.Add(item);

            // Separator.
            sep = new ToolStripSeparator();
            _menu.Items.Add(sep);

            // Exit.
            item = new ToolStripMenuItem();
            item.Name = "exit";
            item.Text = "Exit";
            item.Click += new System.EventHandler(Exit_Click);
            item.Image = Resources.Exit;
            item.Enabled = true;
            _menu.Items.Add(item);

			// Attach a context menu.
			_notifyIcon.ContextMenuStrip = _menu;          
		}

        public void Start(string name,string path)
        {
            // Set the location

            _workflowName = name;
            _workflowPath = path;

            // Start a timer to keep track of the service state.

            _processTimer = new Timer();
            _processTimer.Tick += new EventHandler(TimerEventProcessor);

            // Set the Interval to 5 seconds.
            _processTimer.Interval = 5000;
            _processTimer.Enabled = true;
        }

        public void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            // check the service status

            Debug.WriteLine("In TimerEventProcessor()");
            string check = _manager.Check().ToLower();
            switch (check)
            {
                case "resuming":
                    {
                        Debug.WriteLine("Outresuming");
                        _menu.Items["start"].Enabled = false;
                        _menu.Items["stop"].Enabled = true;
                        _menu.Items["pause"].Enabled = true;
                        _menu.Items["restart"].Enabled = true;
                        _notifyIcon.Icon = Resources.WorkflowTrayChange;
                        break;
                    }
                case "pausing":
                    {
                        Debug.WriteLine("Outresuming");
                        _menu.Items["start"].Enabled = false;
                        _menu.Items["stop"].Enabled = true;
                        _menu.Items["pause"].Enabled = true;
                        _menu.Items["restart"].Enabled = true;
                        _notifyIcon.Icon = Resources.WorkflowTrayChange;
                        break;
                    }
                case "starting":
                    {
                        Debug.WriteLine("Outstarting");
                        _menu.Items["start"].Enabled = false;
                        _menu.Items["stop"].Enabled = true;
                        _menu.Items["pause"].Enabled = false;
                        _menu.Items["restart"].Enabled = false;
                        _notifyIcon.Icon = Resources.WorkflowTrayChange;
                        break;
                    }
                case "started":
                    {
                        Debug.WriteLine("Outrunning");
                        _menu.Items["start"].Enabled = false;
                        _menu.Items["stop"].Enabled = true;
                        _menu.Items["pause"].Enabled = true;
                        _menu.Items["restart"].Enabled = true;
                        _notifyIcon.Icon = Resources.WorkflowTrayRun;
                        break;
                    }
                case "stopping":
                    {
                        Debug.WriteLine("Outstopping");
                        _menu.Items["start"].Enabled = true;
                        _menu.Items["stop"].Enabled = false;
                        _menu.Items["pause"].Enabled = false;
                        _menu.Items["restart"].Enabled = false;
                        _notifyIcon.Icon = Resources.WorkflowTrayChange;
                        break;
                    }
                case "stopped":
                    {
                        Debug.WriteLine("Outstopped");
                        _menu.Items["start"].Enabled = true;
                        _menu.Items["stop"].Enabled = false;
                        _menu.Items["pause"].Enabled = false;
                        _menu.Items["restart"].Enabled = false;
                        _notifyIcon.Icon = Resources.WorkflowTrayStop;
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
            if (!_isAboutLoaded)
            {
                _isAboutLoaded = true;
                new AboutBox().ShowDialog();
                _isAboutLoaded = false;
            }
        }

        void Start_Click(Object sender, EventArgs e)
        {
            _manager.Start();
        }

        void Stop_Click(Object sender, EventArgs e)
        {
            _manager.Stop();
        }

        void Restart_Click(Object sender, EventArgs e)
        {
            _manager.Restart();
        }

        void Pause_Click(Object sender, EventArgs e)
        {
            _manager.Restart();
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
			_notifyIcon.Dispose();
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
				Process.Start("explorer", _workflowPath);
			}
		}
	}
}