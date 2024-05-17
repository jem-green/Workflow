using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TracerLibrary;

namespace WorkflowLibrary
{
    public class Serialise : State
    {
        #region Fields

        string _filename = "";
        string _path = "";

        #endregion
        #region Constructors

        public Serialise()
        {
        }

        #endregion
        #region Properties
        public string Filename
        {
            get
            {
                return (_filename);
            }
            set
            {
                _filename = value;
            }
        }

        public string Path
        {
            get
            {
                return (_path);
            }
            set
            {
                _path = value;
            }
        }
        #endregion
        #region Methods
        public int SerializeToXML(Collection<Job> jobs)
        {
            return (SerializeToXML(jobs, _filename, _path));
        }

        public int SerializeToXML(Collection<Job> jobs, string filename, string path)
        {

            XmlWriterSettings tstxmlsettings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true,
                Encoding = System.Text.Encoding.UTF8
            };

            // Open the file
            string fileLocation = System.IO.Path.Combine(path, filename);
            if (System.IO.File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
            FileStream fs = new FileStream(fileLocation, FileMode.Create);
            XmlWriter xmlwOutput; 
            xmlwOutput = XmlWriter.Create(fs, tstxmlsettings);

            // Start the document

            xmlwOutput.WriteStartDocument();

            // ?

            xmlwOutput.WriteStartElement("jobs");

            foreach (Job job in jobs)
            {
                xmlwOutput.WriteStartElement("job");
                xmlwOutput.WriteElementString("description", job.Description.ToString());      // This writes out <description>1</description>
                xmlwOutput.WriteStartElement("tasks");
                foreach (Task task in job)
                {
                    xmlwOutput.WriteStartElement("task");
                    xmlwOutput.WriteElementString("description", task.Description.ToString());      // This writes out <description>1</description>
                    if (task.Next.Length > 0)
                    {
                        xmlwOutput.WriteElementString("next", task.Next.ToString());                // This writes out <next>1</next>
                    }
                    else
                    {
                        xmlwOutput.WriteElementString("next","");                                      // This writes out <next />
                    }

                    if (task.Previous.Length > 0)
                    {

                        xmlwOutput.WriteElementString("previous", task.Previous.ToString());            // This writes out <previous>1</previous>
                    }
                    else
                    {
                        xmlwOutput.WriteElementString("previous","");                                      // This writes out <previous />
                    }
                    xmlwOutput.WriteStartElement("items");
                    foreach (Item item in task)
                    {
                        xmlwOutput.WriteStartElement("item");
                        xmlwOutput.WriteElementString("application", item.Application.ToString());                  // This writes out <application>1</application>
                        xmlwOutput.WriteElementString("command", item.Command.ToString());                      // This writes out <command>1</command>
                        xmlwOutput.WriteElementString("description", item.Description.ToString());              // This writes out <description>1</description>
                        xmlwOutput.WriteElementString("error", item.Error.ToString());                          // This writes out <error>1</error>
                        xmlwOutput.WriteElementString("input", item.Input.ToString());                          // This writes out <input>1</input>
                        xmlwOutput.WriteElementString("output", item.Output.ToString());                        // This writes out <output>1</output>

                        // Also store any item data
                        xmlwOutput.WriteStartElement("itemdata");
                        ArrayList i = item.LocalData;

                        if (i.Count > 0)
                        {
                            
                            foreach (Hashtable grouping in i)
                            {
                                foreach (DictionaryEntry DE in grouping)
                                {
                                    if (DE.Key.ToString() != "0")
                                    {
                                        xmlwOutput.WriteStartElement("data");
                                        xmlwOutput.WriteElementString("key", DE.Key.ToString());
                                        xmlwOutput.WriteElementString("value", DE.Value.ToString());
                                        xmlwOutput.WriteEndElement();
                                    }
                                }
                            }
                        }

                        xmlwOutput.WriteEndElement();

                        xmlwOutput.WriteEndElement();
                    }
                    xmlwOutput.WriteEndElement();
                    xmlwOutput.WriteEndElement();
                }
                xmlwOutput.WriteEndElement();
                xmlwOutput.WriteEndElement();

                //xmlwOutput.WriteElementString("id", job.ID.ToString());     // This writes out <id>1</id>
            }
            xmlwOutput.WriteEndElement();

            // Finish the document

            xmlwOutput.WriteEndDocument();
            xmlwOutput.Flush();
            xmlwOutput.Close();

            return (1);

        }

        public Collection<Object> DeserialiseProcess()
        {
            int level = 1;
            return (DeserialiseFromXML(StageType.Process, this._filename, this._path, level));
        }

        public Collection<Object> DeserialiseProcess(string filename, string path, int level)
        {
            return(DeserialiseFromXML(StageType.Process,filename,path,level));
        }

        public Collection<Object> DeserialiseJob(string filename, string path, int level)
        {
            return (DeserialiseFromXML(StageType.Job, filename, path, level));
        }

        public Collection<Object> DeserialiseTask(string filename, string path, int level)
        {
            return (DeserialiseFromXML(StageType.Task, filename, path, level));
        }

        public Collection<Object> DeserialiseItem(string filename, string path, int level)
        {
            return (DeserialiseFromXML(StageType.Item, filename, path, level));
        }

        public Collection<Object> DeserialiseFromXML(StageType dataType, string filename, string path, int level)
        {
            // The deserialise needs to accept a type definition and return an appropiate
            // collection containing these object and sub-objects. This will enable a check on 
            // poorly structured XML.

            string current = "";
            //switch (dataType)
            //{
            //    case StageType.Process:
            //        {
            //            current = "?";
            //            break;
            //        }
            //    case StageType.Job:
            //        {
            //            current = "process";
            //            break;
            //        }
            //    case StageType.Task:
            //        {
            //            current = "job";
            //            break;
            //        }
            //    case StageType.Item:
            //        {
            //            current = "task";
            //            break;
            //        }
            //}

            Collection<Object> container = new Collection<Object>();
            try
            {
                // Point to the file

                string fileLocation = System.IO.Path.Combine(path, filename);
                try
                {
                    FileStream fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    // Pass the parameters in

                    XmlReaderSettings xmlSettings = new XmlReaderSettings
                    {

                        // Enable <!ENTITY to be expanded
                        // <!ENTITY chap1 SYSTEM "chap1.xml">
                        // &chap1;

                        DtdProcessing = DtdProcessing.Ignore
                    };

                    // Open the file and pass in the settings

                    try
                    {

                        XmlReader xmlReader = XmlReader.Create(fs, xmlSettings);

                        string version = "1.0"; // Introduce a version number for compatibility although not recommended as new xml features should be ignored by older versions

                        string element = "";
                        bool empty = false;

                        Process process = null;
                        Job job = null;
                        Task task = null;
                        Item item = null;
                        Event @event = null;
                        Link link = null;
                        Decision decision = null;
                        Pipe pipe = null;
                        Node node = null;
                        Connector connector = null;

                        Stack<string> stack = new Stack<string>();

                        string text = "";
                        string key = "";
                        object value = null;
                        string id = "";
                        bool processEnabled = true;
                        bool jobEnabled = true;
                        bool taskEnabled = true;
                        bool eventEnabled = true;
                        bool itemEnabled = true;
                        bool linkEnabled = true;
                        bool decisionEnabled = true;
                        bool connectorEnabled = true;
                        string name = "";
                        //string current = "";    // Used to flag what level we are at
                        string href = "";
                        DataType type = DataType.String;
                        DataKind kind = DataKind.echo;
                        StageType stage = StageType.Process;
                        ItemType itemType = ItemType.Process;

                        while (xmlReader.Read())
                        {
                            switch (xmlReader.NodeType)
                            {
                                #region Element

                                case XmlNodeType.Element:
                                    // stuff
                                    element = xmlReader.LocalName.ToLower();


                                    if (!xmlReader.IsEmptyElement)
                                    {
                                        TraceInternal.TraceVerbose(Level(level) + "<" + element + ">");
                                        level = level + 1;
                                    }
                                    else
                                    {
                                        TraceInternal.TraceVerbose(Level(level) + "<" + element + "/>");
                                    }
                                    switch (element)
                                    {
                                        case "version":
                                            break;
                                        case "process":
                                            #region Process
                                            stack.Push(current);
                                            current = "process";
                                            stage = StageType.Process;
                                            id = "";
                                            name = "";
                                            processEnabled = true;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "href":
                                                            href = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "href=" + href);
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        processEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        processEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + processEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                process = new Process(id)
                                                {
                                                    Name = name,
                                                    Enabled = processEnabled
                                                };
                                            }
                                            else
                                            {
                                                process = new Process
                                                {
                                                    Name = name,
                                                    Enabled = processEnabled
                                                };
                                            }
                                            break;
                                            #endregion
                                        case "subprocess":
                                            #region SubProcess
                                            stack.Push(current);
                                            current = "subprocess";
                                            stage = StageType.Process;
                                            id = "";
                                            name = "";
                                            processEnabled = true;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "href":
                                                            href = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "href=" + href);
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        processEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        processEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + processEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                job = new Job(id)
                                                {
                                                    Name = name,
                                                    Enabled = jobEnabled
                                                };
                                            }
                                            else
                                            {
                                                job = new Job
                                                {
                                                    Name = name,
                                                    Enabled = jobEnabled
                                                };
                                            }
                                            break;
                                            #endregion
                                        case "jobs":
                                            break;
                                        case "job":
                                            #region Job
                                            stack.Push(current);
                                            current = "job";
                                            stage = StageType.Job;
                                            id = "";
                                            name = "";
                                            jobEnabled = true;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        jobEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        jobEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + jobEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                job = new Job(id)
                                                {
                                                    Name = name,
                                                    Enabled = jobEnabled
                                                };
                                            }
                                            else
                                            {
                                                job = new Job
                                                {
                                                    Name = name,
                                                    Enabled = jobEnabled
                                                };
                                            }

                                            break;
                                            #endregion
                                        case "subjob":
                                            #region SubJob
                                            stack.Push(current);
                                            current = "subjob";
                                            stage = StageType.Job;
                                            id = "";
                                            name = "";
                                            jobEnabled = true;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "href":
                                                            href = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "href=" + href);
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        jobEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        jobEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + jobEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                job = new Job(id)
                                                {
                                                    Name = name,
                                                    Enabled = jobEnabled
                                                };
                                            }
                                            else
                                            {
                                                job = new Job
                                                {
                                                    Name = name,
                                                    Enabled = jobEnabled
                                                };
                                            }

                                            break;
                                            #endregion
                                        case "events":
                                            break;
                                        case "event":
                                            #region Event
                                            stack.Push(current);
                                            current = "event";
                                            stage = StageType.Job;
                                            id = "";
                                            name = "";
                                            eventEnabled = true;
                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);

                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        eventEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        eventEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + eventEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                @event = new Event(id)
                                                {
                                                    Name = name,
                                                    Enabled = eventEnabled
                                                };
                                            }
                                            else
                                            {
                                                @event = new Event
                                                {
                                                    Name = name,
                                                    Enabled = eventEnabled
                                                };
                                            }
                                            break;
                                            #endregion
                                        case "tasks":
                                            break;
                                        case "task":
                                            #region Task
                                            stack.Push(current);
                                            current = "task";
                                            stage = StageType.Task;
                                            id = "";
                                            name = "";
                                            taskEnabled = true;
                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);

                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        taskEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        taskEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + taskEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                task = new Task(id)
                                                {
                                                    Name = name,
                                                    Enabled = taskEnabled
                                                };
                                            }
                                            else
                                            {
                                                task = new Task
                                                {
                                                    Name = name,
                                                    Enabled = taskEnabled
                                                };
                                            }
                                            break;
                                            #endregion
                                        case "subtask":
                                            #region SubTask
                                            stack.Push(current);
                                            current = "subtask";
                                            stage = StageType.Task;
                                            id = "";
                                            name = "";
                                            taskEnabled = true;
                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);

                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "href":
                                                            href = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "href=" + href);
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        taskEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        taskEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + taskEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                task = new Task(id)
                                                {
                                                    Name = name,
                                                    Enabled = taskEnabled
                                                };
                                            }
                                            else
                                            {
                                                task = new Task
                                                {
                                                    Name = name,
                                                    Enabled = taskEnabled
                                                };
                                            }
                                            break;
                                            #endregion
                                        case "items":
                                            break;
                                        case "item":
                                            #region Item
                                            stack.Push(current);
                                            current = "item";
                                            stage = StageType.Item;
                                            id = "";
                                            name = "";
                                            itemEnabled = true;
                                            itemType = 0;   // Default to process
                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "type":
                                                            TraceInternal.TraceVerbose(Level(level) + "type=" + xmlReader.Value);
                                                            if (xmlReader.Value == "process")
                                                            {
                                                                itemType = ItemType.Process;
                                                            }
                                                            if (xmlReader.Value == "sql")
                                                            {
                                                                itemType = ItemType.sql;
                                                            }
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        itemEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        itemEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + itemEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                item = new Item(id)
                                                {
                                                    Name = name,
                                                    Enabled = itemEnabled
                                                };
                                            }
                                            else
                                            {
                                                item = new Item
                                                {
                                                    Name = name,
                                                    Enabled = itemEnabled
                                                };
                                            }

                                            break;
                                            #endregion
                                        case "links":
                                            break;
                                        case "link":
                                            #region Link
                                            stack.Push(current);
                                            current = "link";
                                            stage = StageType.Job;
                                            id = "";
                                            name = "";
                                            linkEnabled = true;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                        case "enabled":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "false":
                                                                case "no":
                                                                    {
                                                                        linkEnabled = false;
                                                                        break;
                                                                    }
                                                                case "true":
                                                                case "yes":
                                                                default:
                                                                    {
                                                                        linkEnabled = true;
                                                                        break;
                                                                    }

                                                            }
                                                            TraceInternal.TraceVerbose(Level(level) + "enabled=" + linkEnabled.ToString());
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                link = new Link(id)
                                                {
                                                    Name = name,
                                                    Enabled = linkEnabled
                                                };
                                            }
                                            else
                                            {
                                                link = new Link
                                                {
                                                    Name = name,
                                                    Enabled = linkEnabled
                                                };
                                            }
                                            break;
                                            #endregion
                                        case "gataways":
                                            break;
                                        case "decision":
                                            #region Decision
                                            stack.Push(current);
                                            current = "decision";
                                            stage = StageType.Job;
                                            id = "";
                                            name = "";
                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                decision = new Decision(id)
                                                {
                                                    Name = name,
                                                    Enabled = decisionEnabled
                                                };
                                            }
                                            else
                                            {
                                                decision = new Decision();
                                                link.Name = name;
                                                decision.Enabled = decisionEnabled;
                                            }
                                            break;
                                        #endregion
                                        case "connector":
                                            #region Connector
                                            {
                                                stack.Push(current);
                                                current = "connector";
                                                stage = StageType.Job;
                                                id = "";
                                                name = "";
                                                if (xmlReader.HasAttributes == true)
                                                {
                                                    while (xmlReader.MoveToNextAttribute())
                                                    {
                                                        switch (xmlReader.Name.ToLower())
                                                        {
                                                            case "id":
                                                                id = xmlReader.Value;
                                                                TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                                break;
                                                            case "name":
                                                                name = xmlReader.Value;
                                                                TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                                break;
                                                        }
                                                    }
                                                }

                                                if (id.Length > 0)
                                                {
                                                    connector = new Connector(id)
                                                    {
                                                        Name = name,
                                                        Enabled = connectorEnabled
                                                    };
                                                }
                                                else
                                                {
                                                    connector = new Connector
                                                    {
                                                        Name = name,
                                                        Enabled = connectorEnabled
                                                    };
                                                }
                                                break;
                                            }
                                        #endregion
                                        case "jobdata":
                                            break;
                                        case "taskdata":
                                            break;
                                        case "itemdata":
                                            break;
                                        case "decisiondata":
                                            break;
                                        case "eventdata":
                                            break;
                                        case "data":
                                            #region data
                                            // Default to string data

                                            type = DataType.String;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "type":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "string":
                                                                    type = DataType.String;
                                                                    break;
                                                                case "double":
                                                                    type = DataType.Double;
                                                                    break;
                                                                case "integer":
                                                                    type = DataType.Integer;
                                                                    break;
                                                            }
                                                            break;
                                                    }
                                                }
                                            }
                                            break;
                                            #endregion
                                        case "error":
                                            #region error
                                            kind = DataKind.echo;   // Defaults to echo output data
                                            type = DataType.String; // Defaults to string
                                            stage = StageType.Job;  // Defaults to job
                                            value = "";
                                            empty = xmlReader.IsEmptyElement;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "stage":
                                                            TraceInternal.TraceVerbose(Level(level) + xmlReader.Name.ToLower() + "=" + xmlReader.Value.ToLower());
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "process":
                                                                    stage = StageType.Process;
                                                                    break;
                                                                case "job":
                                                                    stage = StageType.Job;
                                                                    break;
                                                                case "task":
                                                                    stage = StageType.Task;
                                                                    break;
                                                                case "item":
                                                                    stage = StageType.Item;
                                                                    break;
                                                            }
                                                            break;
                                                        case "kind":
                                                            TraceInternal.TraceVerbose(Level(level) + xmlReader.Name.ToLower() + "=" + xmlReader.Value.ToLower());
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "none":
                                                                    kind = DataKind.none;
                                                                    break;
                                                                case "echo":
                                                                    kind = DataKind.echo;
                                                                    break;
                                                                case "single":
                                                                    kind = DataKind.single;
                                                                    break;
                                                                case "multiple":
                                                                    kind = DataKind.multiple;
                                                                    break;
                                                                case "value":
                                                                    kind = DataKind.value;
                                                                    break;
                                                                case "pipe":
                                                                    kind = DataKind.pipe;
                                                                    break;
                                                                case "raw":
                                                                    kind = DataKind.raw;
                                                                    break;
                                                            }
                                                            break;
                                                        case "type":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "string":
                                                                    type = DataType.String;
                                                                    break;
                                                                case "double":
                                                                    type = DataType.Double;
                                                                    break;
                                                                case "integer":
                                                                    type = DataType.Integer;
                                                                    break;
                                                            }
                                                            break;
                                                    }
                                                }
                                            }
                                            if (empty == true)
                                            {
                                                item.Error.Stage = stage;
                                                item.Error.Type = type;
                                                item.Error.Kind = kind;
                                            }
                                            break;
                                            #endregion
                                        case "input":
                                            #region Input
                                            kind = DataKind.none;   // Defaults to no input data
                                            type = DataType.String; // Defaults to string
                                            stage = StageType.Job;  // Defaults to job
                                            value = "";
                                            empty = xmlReader.IsEmptyElement;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "kind":
                                                            TraceInternal.TraceVerbose(Level(level) + xmlReader.Name.ToLower() + "=" + xmlReader.Value.ToLower());
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "none":
                                                                    kind = DataKind.none;
                                                                    break;
                                                                case "echo":
                                                                    kind = DataKind.echo;
                                                                    break;
                                                                case "single":
                                                                    kind = DataKind.single;
                                                                    break;
                                                                case "multiple":
                                                                    kind = DataKind.multiple;
                                                                    break;
                                                                case "value":
                                                                    kind = DataKind.value;
                                                                    break;
                                                                case "pipe":
                                                                    kind = DataKind.pipe;
                                                                    break;
                                                                case "raw":
                                                                    kind = DataKind.raw;
                                                                    break;
                                                            }
                                                            break;
                                                        case "type":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "string":
                                                                    type = DataType.String;
                                                                    break;
                                                                case "double":
                                                                    type = DataType.Double;
                                                                    break;
                                                                case "integer":
                                                                    type = DataType.Integer;
                                                                    break;
                                                            }
                                                            break;
                                                    }
                                                }
                                            }
                                            if (empty == true)
                                            {
                                                item.Input.Stage = stage;
                                                item.Input.Type = type;
                                                item.Input.Kind = kind;
                                            }
                                            break;
                                            #endregion
                                        case "output":
                                            #region Output
                                            // The output stage is set by the process stage
                                            // it can be overridden specifically to set it to a stage

                                            kind = DataKind.echo;   // Defaults to echo output data
                                            type = DataType.String; // Defaults to string
                                            stage = StageType.Job;  // Defaults to job
                                            value = "";             // Defaults to empty string
                                            empty = xmlReader.IsEmptyElement;

                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "stage":
                                                            TraceInternal.TraceVerbose(Level(level) + xmlReader.Name.ToLower() + "=" + xmlReader.Value.ToLower());
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "process":
                                                                    stage = StageType.Process;
                                                                    break;
                                                                case "job":
                                                                    stage = StageType.Job;
                                                                    break;
                                                                case "task":
                                                                    stage = StageType.Task;
                                                                    break;
                                                                case "item":
                                                                    stage = StageType.Item;
                                                                    break;
                                                            }
                                                            break;
                                                        case "kind":
                                                            TraceInternal.TraceVerbose(Level(level) + xmlReader.Name.ToLower() + "=" + xmlReader.Value.ToLower());
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "none":
                                                                    kind = DataKind.none;
                                                                    break;
                                                                case "echo":
                                                                    kind = DataKind.echo;
                                                                    break;
                                                                case "single":
                                                                    kind = DataKind.single;
                                                                    break;
                                                                case "multiple":
                                                                    kind = DataKind.multiple;
                                                                    break;
                                                                case "value":
                                                                    kind = DataKind.value;
                                                                    break;
                                                                case "pipe":
                                                                    kind = DataKind.pipe;
                                                                    break;
                                                                case "raw":
                                                                    kind = DataKind.raw;
                                                                    break;
                                                            }
                                                            break;
                                                        case "type":
                                                            switch (xmlReader.Value.ToLower())
                                                            {
                                                                case "string":
                                                                    type = DataType.String;
                                                                    break;
                                                                case "double":
                                                                    type = DataType.Double;
                                                                    break;
                                                                case "integer":
                                                                    type = DataType.Integer;
                                                                    break;
                                                            }
                                                            break;
                                                    }
                                                }
                                            }
                                            if (empty == true)
                                            {
                                                item.Output.Stage = stage;
                                                item.Output.Type = type;
                                                item.Output.Kind = kind;
                                            }
                                            break;
                                            #endregion
                                        case "pipes":
                                            break;
                                        case "pipe":
                                            #region Pipe
                                            stack.Push(current);
                                            current = "pipe";
                                            stage = StageType.Item;
                                            id = "";
                                            name = "";
                                            if (xmlReader.HasAttributes == true)
                                            {
                                                while (xmlReader.MoveToNextAttribute())
                                                {
                                                    switch (xmlReader.Name.ToLower())
                                                    {
                                                        case "id":
                                                            id = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "id=" + id);
                                                            break;
                                                        case "name":
                                                            name = xmlReader.Value;
                                                            TraceInternal.TraceVerbose(Level(level) + "name=" + name);
                                                            break;
                                                    }
                                                }
                                            }

                                            if (id.Length > 0)
                                            {
                                                pipe = new Pipe(id)
                                                {
                                                    Name = name
                                                };
                                                //link.Enabled = linkEnabled;
                                            }
                                            else
                                            {
                                                pipe = new Pipe
                                                {
                                                    Name = name
                                                };
                                                //link.Enabled = linkEnabled;
                                            }
                                            break;
                                        #endregion
                                        default:
                                            {
                                                stack.Push(current);
                                                current = element;
                                                break;
                                            }
                                    }
                                    break;

                                #endregion
                                #region EndElement

                                case XmlNodeType.EndElement:
                                    element = xmlReader.LocalName;
                                    level = level - 1;
                                    //TraceInternal.TraceVerbose(Level(level) + "</" + element + ">");
                                    switch (element)
                                    {
                                        case "process":
                                            #region Process
                                            {
                                                current = stack.Pop();
                                                if (process.Enabled == true)    // only add enabled jobs at the moments
                                                {
                                                    container.Add(process);
                                                    TraceInternal.TraceVerbose(Level(level) + "Add Process() '" + process.ID + "'(" + process.Name + ")");
                                                }
                                                else
                                                {
                                                    TraceInternal.TraceVerbose(Level(level) + "Disabled Process() '" + process.ID + "'(" + process.Name + ")");
                                                }
                                                break;
                                            }
                                        #endregion Process
                                        case "subprocess":
                                            #region subprocess
                                            {
                                                current = stack.Pop();
                                                if (process.Enabled == true)    // only add enabled jobs at the moments
                                                {
                                                    Serialise s = new Serialise();
                                                    Collection<object> subprocess = s.DeserialiseJob(href, path, level+1);
                                                    foreach (object o in subprocess)
                                                    {
                                                        if (o.GetType() == typeof(Job))
                                                        {
                                                            Job j = (Job)o;
                                                            process.Add(j);
                                                        }
                                                    }
                                                    container.Add(process);
                                                    TraceInternal.TraceVerbose(Level(level) + "Add SubProcess() '" + process.ID + "'(" + process.Name + ")");
                                                }
                                                else
                                                {
                                                    TraceInternal.TraceVerbose(Level(level) + "Disabled SubProcess() '" + process.ID + "'(" + process.Name + ")");
                                                }
                                                break;
                                            }
                                            #endregion
                                        case "jobs":
                                            #region Jobs
                                            {
                                                break;
                                            }
                                            #endregion
                                        case "job":
                                            #region Job
                                            current = stack.Pop();
                                            if (job.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                if (current == "process")
                                                {
                                                    process.Add(job);
                                                }
                                                else
                                                {
                                                    container.Add(job);
                                                }
                                                TraceInternal.TraceVerbose(Level(level) + "Add Job() '" + job.ID + "'(" + job.Name + ")");
                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Job() '" + job.ID + "'(" + job.Name + ")");
                                            }
                                            break;
                                            #endregion
                                        case "subjob":
                                            #region SubJob
                                            current = stack.Pop();
                                            if (job.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                Serialise s = new Serialise();
                                                Collection<object> subjob = s.DeserialiseTask(href, path, level+1);
                                                foreach (object o in subjob)
                                                {
                                                    if (o.GetType() == typeof(Task))
                                                    {
                                                        Task t = (Task)o;
                                                        job.Add(t);
                                                    }
                                                    else if (o.GetType() == typeof(KeyValuePair<string,object>))
                                                    {
                                                        KeyValuePair<string, object> kvp = (KeyValuePair<string, object>)o;
                                                        job.AddData(kvp.Key, kvp.Value);
                                                    }
                                                }

                                                if (current == "process")
                                                {
                                                    container.Add(job);
                                                }
                                                else
                                                {
                                                    container.Add(job);
                                                }

                                                TraceInternal.TraceVerbose(Level(level) + "Add Subjob() '" + job.ID + "'(" + job.Name + ")");
                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Subjob() '" + job.ID + "'(" + job.Name + ")");
                                            }
                                            break;
                                        #endregion
                                        case "tasks":
                                            #region Tasks
                                            {
                                                break;
                                            }
                                            #endregion
                                        case "task":
                                            #region Task
                                            current = stack.Pop();
                                            if (task.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                if (current == "job")
                                                {
                                                    job.Add(task);
                                                }
                                                else if (current == "event")
                                                {
                                                    @event.Add(task);
                                                }
                                                else
                                                {
                                                    container.Add(task);
                                                }
                                                TraceInternal.TraceVerbose(Level(level) + "Add Task() '" + task.ID + "'(" + task.Name + ")");
                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Task() '" + task.ID + "'(" + task.Name + ")");
                                            }
                                            break;
                                            #endregion
                                        case "subtask":
                                            #region SubTask
                                            current = stack.Pop();
                                            if (task.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                Serialise s = new Serialise();
                                                Collection<object> subtask = s.DeserialiseItem(href, path, level+1);
                                                foreach (object o in subtask)
                                                {
                                                    if (o.GetType() == typeof(Item))
                                                    {
                                                        Item i = (Item)o;
                                                        task.Add(i);
                                                    }
                                                    else if (o.GetType() == typeof(KeyValuePair<string, object>))
                                                    {
                                                        KeyValuePair<string, object> kvp = (KeyValuePair<string, object>)o;
                                                        task.AddData(kvp.Key, kvp.Value);
                                                    }
                                                }

                                                if (current == "job")
                                                {
                                                    job.Add(task);
                                                }
                                                else
                                                {
                                                    container.Add(task);
                                                }

                                                TraceInternal.TraceVerbose(Level(level) + "Add Subtask() '" + task.ID + "'(" + task.Name + ")");

                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Subtask() '" + task.ID + "'(" + task.Name + ")");
                                            }
                                            break;
                                        #endregion
                                        case "events":
                                            #region Events
                                            {
                                                break;
                                            }
                                            #endregion
                                        case "event":
                                            #region Event
                                            current = stack.Pop();
                                            if (@event.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                if (current == "process")
                                                {
                                                    process.Add(@event);
                                                }
                                                else
                                                {
                                                    container.Add(@event);
                                                }
                                                TraceInternal.TraceVerbose(Level(level) + "Add Event() '" + @event.ID + "'(" + @event.Name + ")");
                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Job() '" + @event.ID + "'(" + @event.Name + ")");
                                            }
                                            break;
                                            #endregion
                                        case "links":
                                            #region Links
                                            {
                                                break;
                                            }
                                        #endregion
                                        case "link":
                                            #region Link
                                            current = stack.Pop();
                                            if (link.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                if (current == "process")
                                                {
                                                    process.Add(link);
                                                }
                                                else
                                                {
                                                    container.Add(link);
                                                }
                                                TraceInternal.TraceVerbose(Level(level) + "Add Link() '" + link.ID + "'(" + link.Name + ")");
                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Link() '" + link.ID + "'(" + link.Name + ")");
                                            }
                                            break;
                                        #endregion
                                        case "items":
                                            break;
                                        case "item":
                                            #region Item
                                            current = stack.Pop();
                                            if (current == "task")    // Depends on the parent
                                            {
                                                task.Add(item);
                                            }
                                            else if (current == "event")
                                            {
                                                @event.Add(item);
                                            }
                                            else
                                            {
                                                container.Add(item);
                                            }
                                            break;
                                        #endregion
                                        case "decisions":
                                            #region Decisions
                                            {
                                                break;
                                            }
                                        #endregion
                                        case "decision":
                                            #region Decision
                                            current = stack.Pop();
                                            if (decision.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                if (current == "process")
                                                {
                                                    process.Add(decision);
                                                }
                                                else
                                                {
                                                    container.Add(decision);
                                                }
                                                TraceInternal.TraceVerbose(Level(level) + "Add Decision() '" + decision.ID + "'(" + decision.Name + ")");
                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Decision() '" + decision.ID + "'(" + decision.Name + ")");
                                            }
                                            break;
                                            #endregion
                                        case "connector":
                                            #region Connector
                                            current = stack.Pop();
                                            if (connector.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                if (current == "process")
                                                {
                                                    process.Add(connector);
                                                }
                                                else
                                                {
                                                    container.Add(connector);
                                                }
                                                TraceInternal.TraceVerbose(Level(level) + "Add Connector() '" + connector.ID + "'(" + connector.Name + ")");
                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Connector() '" + connector.ID + "'(" + connector.Name + ")");
                                            }
                                            break;
                                        #endregion
                                        case "itemdata":
                                            break;
                                        case "jobdata":
                                            break;
                                        case "taskdata":
                                            break;
                                        case "decisiondata":
                                            break;
                                        case "eventdata":
                                            break;
                                        case "processdata":
                                            break;
                                        case "data":
                                            switch (current)
                                            {
                                                case "":
                                                    {
                                                        KeyValuePair<string, object> kvp = new KeyValuePair<string, object>(key,value);
                                                        container.Add(kvp);
                                                        break;
                                                    }
                                                case "process":
                                                    {
                                                        process.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                                case "job":
                                                    {
                                                        job.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                                case "subjob":
                                                    {
                                                        job.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                                case "task":
                                                    {
                                                        task.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                                case "subtask":
                                                    {
                                                        task.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                                case "item":
                                                    {
                                                        item.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                                case "decision":
                                                    {
                                                        decision.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                                case "event":
                                                    {
                                                        @event.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                                case "connector":
                                                    {
                                                        connector.AddData(key, value);
                                                        key = "";
                                                        value = "";
                                                        break;
                                                    }
                                            }
                                            break;
                                        case "input":
                                            item.Input.Stage = stage;
                                            item.Input.Type = type;
                                            item.Input.Kind = kind;
                                            item.Input.Value = value;
                                            break;

                                        case "output":
                                            item.Output.Stage = stage;
                                            item.Output.Type = type;
                                            item.Output.Kind = kind;
                                            item.Output.Value = value;
                                            break;

                                        case "error":
                                            item.Error.Stage = stage;
                                            item.Error.Type = type;
                                            item.Error.Kind = kind;
                                            item.Error.Value = value;
                                            break;

                                        case "pipes":
                                            break;
                                        case "pipe":
                                            current = stack.Pop();
                                            if (pipe.Enabled == true)    // only add enabled jobs at the moments
                                            {
                                                container.Add(pipe);
                                                TraceInternal.TraceVerbose(Level(level) + "Add Pipe() '" + pipe.ID + "'(" + pipe.Name + ")");
                                            }
                                            else
                                            {
                                                TraceInternal.TraceVerbose(Level(level) + "Disabled Pipe() '" + pipe.ID + "'(" + pipe.Name + ")");
                                            }
                                            break;
                                    }
                                    TraceInternal.TraceVerbose(Level(level) + "</" + element + ">");
                                    break;

                                #endregion
                                #region Text

                                case XmlNodeType.Text:

                                    text = xmlReader.Value;
                                    text = text.Replace("\t", "");
                                    text = text.Replace("\n", "");
                                    TraceInternal.TraceVerbose(Level(level) + text);
                                    switch (element)
                                    {

                                        case "description":
                                            switch (current)
                                            {
                                                case "job":
                                                    job.Description = text;
                                                    break;
                                                case "subjob":
                                                    job.Description = text;
                                                    break;
                                                case "task":
                                                    task.Description = text;
                                                    break;
                                                case "subtask":
                                                    task.Description = text;
                                                    break;
                                                case "item":
                                                    item.Description = text;
                                                    break;
                                                case "event":
                                                    @event.Description = text;
                                                    break;
                                                case "link":
                                                    link.Description = text;
                                                    break;
                                                case "pipe":
                                                    pipe.Description = text;
                                                    break;
                                            }
                                            break;
                                        case "next":
                                            task.Next = text;
                                            break;
                                        case "previous":
                                            task.Previous = text;
                                            break;
                                        case "application":
                                            item.Application = text;
                                            break;
                                        case "command":
                                            item.Command = text;
                                            break;
                                        case "error":
                                            switch (type)
                                            {
                                                case DataType.Double:
                                                    {
                                                        value = Convert.ToDouble(text);
                                                    }
                                                    break;
                                                case DataType.Integer:
                                                    {
                                                        value = Convert.ToInt32(text);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        value = text;
                                                    }
                                                    break;
                                            }
                                            break;
                                        case "input":
                                            switch (type)
                                            {
                                                case DataType.Double:
                                                    {
                                                        value = Convert.ToDouble(text);
                                                    }
                                                    break;
                                                case DataType.Integer:
                                                    {
                                                        value = Convert.ToInt32(text);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        value = text;
                                                    }
                                                    break;
                                            }
                                            break;
                                        case "output":

                                            if ((kind == DataKind.none) || (kind == DataKind.echo))
                                            {
                                                if (text == "=")
                                                {
                                                    kind = DataKind.multiple;
                                                }
                                                else if (text.Length > 0)
                                                {
                                                    kind = DataKind.value;
                                                }
                                            }

                                            switch (type)
                                            {
                                                case DataType.Double:
                                                    {
                                                        value = Convert.ToDouble(text);
                                                    }
                                                    break;
                                                case DataType.Integer:
                                                    {
                                                        value = Convert.ToInt32(text);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        value = text;
                                                    }
                                                    break;
                                            }
                                            break;
                                        case "key":
                                            key = text;
                                            break;
                                        case "value":
                                            switch (type)
                                            {
                                                case DataType.Double:
                                                    {
                                                        value = Convert.ToDouble(text);
                                                    }
                                                    break;
                                                case DataType.Integer:
                                                    {
                                                        value = Convert.ToInt32(text);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        value = text;
                                                    }
                                                    break;
                                            }

                                            break;
                                        case "throw":
                                            node = new Node(text);
                                            switch (current)
                                            {
                                                case "job":
                                                    job.AddThrow(node);
                                                    break;
                                                case "subjob":
                                                    job.AddThrow(node);
                                                    break;
                                                case "event":
                                                    @event.AddThrow(node);
                                                    break;
                                            }
                                            break;
                                        case "catch":
                                            node = new Node(text);
                                            switch (current)
                                            {
                                                case "job":
                                                    job.AddCatch(node);
                                                    break;
                                                case "subjob":
                                                    job.AddThrow(node);
                                                    break;
                                                case "event":
                                                    @event.AddCatch(node);
                                                    break;
                                            }
                                            break;
                                        case "from":
                                            link.From = text;
                                            break;
                                        case "to":
                                            link.To = text;
                                            break;
                                        case "expression":
                                            link.Expression = text;
                                            break;
                                        case "start":
                                            break;
                                        case "end":
                                            break;
                                        case "version":
                                            {
                                                version = text;
                                                break;
                                            }

                                    }
                                    break;
                                #endregion
                                #region Entity
                                case XmlNodeType.Entity:
                                    break;
                                #endregion
                                case XmlNodeType.EndEntity:
                                    break;
                                case XmlNodeType.Whitespace:
                                    break;
                                case XmlNodeType.Comment:
                                    break;
                                case XmlNodeType.Attribute:
                                    break;
                                default:
                                    TraceInternal.TraceVerbose(xmlReader.NodeType.ToString());
                                    break;

                            }
                        }

                        xmlReader.Close();  // Force the close
                        xmlReader = null;
                    }
                    catch (Exception ex)
                    {
                        TraceInternal.TraceVerbose("XML Error " + ex.Message);
                    }
                    fs.Close();
                    fs.Dispose();   // Force the dispose as it was getting left open

                }
                catch (Exception ex)
                {
                    TraceInternal.TraceVerbose("File Error " + ex.Message);
                }

            }
            catch (Exception e)
            {
                TraceInternal.TraceVerbose("Other Error " + e.Message);
            }
            
            return (container);
        }

        public static Collection<object> LinkObjects(Collection<object> container)
        {
                
            // Link the objects correctly based on the throw/catch rules

            foreach (object i in container)
            {
                if (i.GetType() == typeof(Job))
                {
                    Job j = (Job)i;

                    // Check the throw

                    foreach (Node node in j.Throw)
                    {
                        string linkID = node.Id;
                        foreach (object obj in container)
                        {
                            if (obj.GetType() == typeof(Link))
                            {
                                // Check if the links names match
                                Link l = (Link)obj;
                                if (linkID == l.ID)
                                {
                                    node.Link = l;
                                    TraceInternal.TraceVerbose("Link link() '" + l.ID + "'(" + l.Name + ") to job() '" + j.ID + "'(" + j.Name + ")");
                                }
                            }
                        }
                    }

                    // Check the catch

                    foreach (Node node in j.Catch)
                    {
                        string linkID = node.Id;
                        foreach (object obj in container)
                        {
                            if (obj.GetType() == typeof(Link))
                            {
                                // Check if the links names match
                                Link l = (Link)obj;
                                if (linkID == l.ID)
                                {
                                    node.Link = l;
                                    TraceInternal.TraceVerbose("Link link() '" + l.ID + "'(" + l.Name + ") to job() '" + j.ID + "'(" + j.Name + ")");
                                }
                            }
                        }
                    }
                }
                else if (i.GetType() == typeof(Event))
                {
                    Event e = (Event)i;
                    // Check the throw
                    try
                    {
                        foreach (Node node in e.Throw)
                        {
                            // Only use the link names
                            if (node.GetType() == typeof(string))
                            {
                                string linkID = node.Id;
                                foreach (object obj in container)
                                {
                                    if (obj.GetType() == typeof(Link))
                                    {
                                        // Check if the links names match
                                        Link l = (Link)obj;
                                        if (linkID == l.ID)
                                        {
                                            node.Link = l;
                                            TraceInternal.TraceVerbose("Link link() '" + l.ID + "'(" + l.Name + ") to event() '" + e.ID + "'(" + e.Name + ")");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    // Check the catch
                    try
                    {
                        foreach (Node node in e.Catch)
                        {
                            string linkID = node.Id;
                            foreach (object obj in container)
                            {
                                if (obj.GetType() == typeof(Link))
                                {
                                    // Check if the links names match
                                    Link l = (Link)obj;
                                    if (linkID == l.ID)
                                    {
                                        node.Link = l;
                                        TraceInternal.TraceVerbose("Link link() '" + l.ID + "'(" + l.Name + ") to event() '" + e.ID + "'(" + e.Name + ")");
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            return (container);
        }

        public static Collection<object> JoinPipes(Collection<object> container)
        {

            // Link the pipes to the item connectors

            foreach (object j in container)
            {
                if (j.GetType() == typeof(Job))
                {
                    foreach (object t in (Job)j)
                    {
                        if (t.GetType() == typeof(Task))
                        {
                            foreach (object i in (Task)t)
                            {
                                if (i.GetType() == typeof(Item))
                                {
                                    Item item = (Item)i;

                                    if (item.Input.Kind == DataKind.pipe)
                                    {
                                        // Now check the input connetor with any pipe starts
                                        
                                        foreach (object p in container)
                                        {
                                            if (p.GetType() == typeof(Pipe))
                                            {
                                                Pipe pipe = (Pipe)p;
                                                if ((string)item.Input.Value == pipe.ID)
                                                {
                                                    item.Inlet.Join(pipe);
                                                    TraceInternal.TraceVerbose("Join pipe() '" + pipe.ID + "'(" + pipe.Name + ") to item() '" + item.ID + "'(" + item.Name + ")");
                                                }
                                            }
                                        }
                                    }

                                    if (item.Output.Kind == DataKind.pipe)
                                    {
                                        // Now check the output connetor with any pipe ends
                                        
                                        foreach (object p in container)
                                        {
                                            if (p.GetType() == typeof(Pipe))
                                            {
                                                Pipe pipe = (Pipe)p;
                                                if ((string)item.Output.Value == pipe.ID)
                                                {
                                                    pipe.Join(item.Outlet);
                                                    TraceInternal.TraceVerbose("Join item() '" + item.ID + "'(" + item.Name + ") to pipe() '" + pipe.ID + "'(" + pipe.Name + ")");
                                                }
                                            }
                                        }
                                    }
                                }
                            }                    
                        }
                    }
                }
            }


            return (container);
        }

        private static string Level(int level)
        {
            string text = "";
            for (int i=1; i < level; i++)
            {
                text = text + "  ";
            }
            return (text);
        }

        #endregion
    }
}
