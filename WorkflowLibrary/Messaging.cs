using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using TracerLibrary;
using System.Diagnostics;

namespace WorkflowLibrary
{
    public static class Messaging
    {

        public static string Decoder(Collection<Object> data, string message)
        {
            Debug.WriteLine("In Decoder()");
            string[] messageParts;
            string response = "";
            messageParts = message.Split(' ');

            TraceInternal.TraceVerbose("Message=" + messageParts[0]);

            switch (messageParts[0].ToLower())
            {
                case "load":
                    {
                        TraceInternal.TraceVerbose("Loading");
                        if (messageParts[1].Length > 0)
                        {
                            string href = messageParts[1];
                            string path = messageParts[2];
                            Serialise s = new Serialise();

                            Collection<object> subprocess = s.DeserialiseJob(href, path, 1);
                            foreach (object o in subprocess)
                            {
                                if (o.GetType() == typeof(Job))
                                {
                                    Job j = (Job)o;
                                    data.Add(j);
                                }
                            }

                            response = "Loaded";
                        }
                        break;
                    }
                case "cancel":
                    {
                        TraceInternal.TraceVerbose("Cancelling");
                        foreach (Object item in data)
                        {
                            if (messageParts[1].Length > 0)
                            {
                                if (item.GetType() == typeof(Job))
                                {                                    
                                    Job job = (Job)item;
                                    if (job.ID.ToString() == messageParts[1])
                                    {
                                        job.Cancel();
                                    }
                                }
                                else if (item.GetType() == typeof(WorkflowLibrary.Event))
                                {
                                    Event @event = (Event)item;
                                    if (@event.ID.ToString() == messageParts[1])
                                    {
                                        @event.Cancel();
                                    }
                                }
                            }
                            else
                            {
                                if (item.GetType() == typeof(Job))
                                {
                                    Job job = (Job)item;
                                    job.Cancel();
                                }
                                else if (item.GetType() == typeof(WorkflowLibrary.Event))
                                {
                                    Event @event = (Event)item;
                                    @event.Cancel();
                                }
                            }
                        }
                        TraceInternal.TraceVerbose("Canceled");
                        response = "";
                        break;
                    }
                case "process":
                    {
                        TraceInternal.TraceVerbose("Processing");
                        foreach (Object item in data)
                        {
                            if (messageParts[1].Length > 0)
                            {
                                if (item.GetType() == typeof(Job))
                                {
                                    Job job = (Job)item;
                                    if (job.ID.ToString() == messageParts[1])
                                    {
                                        job.Activate();
                                    }
                                }
                                else if (item.GetType() == typeof(WorkflowLibrary.Event))
                                {
                                    Event @event = (Event)item;
                                    if (@event.ID.ToString() == messageParts[1])
                                    {
                                        @event.Activate();
                                    }
                                }
                            }
                            else
                            {
                                if (item.GetType() == typeof(Job))
                                {
                                    Job job = (Job)item;
                                    job.Activate();
                                }
                                else if (item.GetType() == typeof(WorkflowLibrary.Event))
                                {
                                    Event @event = (Event)item;
                                    @event.Activate();
                                }
                            }
                        }
                        TraceInternal.TraceVerbose("Processed");
                        response = "";
                        break;
                    }
                case "info":
                    {
                        TraceInternal.TraceVerbose("Get Info");

                        response = response + "<jobs>";
                        foreach (Object obj in data)
                        {
                            if (obj.GetType() == typeof(Job))
                            {
                                Job job = (Job)obj;

                                response = response + "<job id=\"" + job.ID + "\" name=\"" + job.Name + "\"><description>" + job.Description + "</description>";
                                response = response + "<tasks>";
                                foreach (Task task in job)
                                {
                                    response = response + "<task id=\"" + task.ID + "\" name=\"" + task.Name + "\"><description>" + task.Description + "</description>";
                                    response = response + "<items>";
                                    foreach (Item item in task)
                                    {
                                        if (item.State == Item.StateType.Active)
                                        {
                                            response = response + "<item id=\"" + item.ID + "\" name=\"" + item.Name + "\" state=\"active\"><description>" + item.Description + "</description></item>";
                                        }
                                        else
                                        {
                                            response = response + "<item id=\"" + item.ID + "\" name=\"" + item.Name + "\" state=\"inactive\"><description>" + item.Description + "</description></item>";
                                        }
                                    }
                                    response = response + "</items>";
                                    response = response + "</task>";
                                }
                                response = response + "</tasks>";
                                response = response + "</job>";
                            }
                            else if (obj.GetType() == typeof(WorkflowLibrary.Event))
                            {
                                response = response + "<event>";
                                response = response + "</event>";
                            }
                        }
                        response = response + "</jobs>";
                        response = response + "\0";             // Add a terminator

                        TraceInternal.TraceVerbose("Info Got");
                        
                        break;
                    }

            }
            Debug.WriteLine("Out Decoder()");
            return (response);
        }
    }
}
