using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;

namespace WorkflowService
{
    [RunInstaller(true)]
    public partial class WorkflowInstaller : Installer
    {
        public WorkflowInstaller()
        {
            InitializeComponent();
        }
    }
}