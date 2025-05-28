using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using TracerLibrary;

namespace WorkflowTray
{
    public class ServiceManager
    {
        

        ServiceController serviceController;
        ServiceControllerStatus serviceStatus = ServiceControllerStatus.Running;
        bool initialised = false;
        string status = "";
        string _serviceName;

        public ServiceManager(string serviceName)
        {
            _serviceName = serviceName;
            serviceController = new ServiceController(_serviceName);
        }

        public string Status
        {
            get
            {
                return (status);
            }
        }

        public string Check()
        {
            string check = "";

            try
            {
                serviceController.Refresh();
                if ((serviceStatus != serviceController.Status) || (initialised == false))
                {
                    switch (serviceController.Status)
                    {
                        case ServiceControllerStatus.ContinuePending:
                            check = "resuming";
                            break;
                        case ServiceControllerStatus.Paused:
                            check = "paused";
                            break;
                        case ServiceControllerStatus.PausePending:
                            check = "pausing";
                            break;
                        case ServiceControllerStatus.Running:
                            check = "started";
                            break;
                        case ServiceControllerStatus.StartPending:
                            check = "starting";
                            break;
                        case ServiceControllerStatus.Stopped:
                            check = "stopped";
                            break;
                        case ServiceControllerStatus.StopPending:
                            check = "stopping";
                            break;
                        default:
                            check = "changing";
                            break;
                    }
                    status = check;
                    serviceStatus = serviceController.Status;
                    initialised = true;
                }
            }
            catch (Exception e)
            {
                TraceInternal.TraceVerbose("Exception=" + e.ToString());
            }
            return (check);
        }

        public void Start()
        {
            try
            {
                serviceController = new ServiceController(_serviceName);
                serviceController.Start();
            }
            catch (Exception e)
            {
                TraceInternal.TraceVerbose("Exception=" + e.ToString());
            }
        }

        public void Stop()
        {
            try
            {
                serviceController.Stop();
            }
            catch (Exception e)
            {
                TraceInternal.TraceVerbose("Exception=" + e.ToString());
            }
        }

        public void Restart()
        {
            try
            {
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                serviceController.Start();
            }
            catch (Exception e)
            {
                TraceInternal.TraceVerbose("Exception=" + e.ToString());
            }
        }

        public void Pause()
        {
            try
            {
                serviceController.Pause();
            }
            catch (Exception e)
            {
                TraceInternal.TraceVerbose("Exception=" + e.ToString());
            }
        }

    }
}
