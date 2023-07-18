using TracerLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;


namespace WorkflowLibrary
{
    public class Item : Element, IItem
    {
        #region Fields

        private static int itemId;
        private string application = "";
        private string command = "";
        private Content input;
        private Content error;
        private Content output;
        private Coupler inlet;
        private Coupler outlet;
        private System.Diagnostics.Process proc;   // move to global within class
        private bool received = false;

        #endregion
        #region Constructors

        public Item() : base()
        {
		    itemId = itemId + 1;
            input = new Content();
            output = new Content();
            error = new Content();
            inlet = new Coupler();
            outlet = new Coupler();
            _id = "item_" + itemId.ToString();
        }

        public Item(string id) : base (id)
        {
            input = new Content();
            output = new Content();
            error = new Content();
            inlet = new Coupler();
            outlet = new Coupler();
            _id = id;
            if (_id.StartsWith("item_"))
            {
                if (itemId < Convert.ToInt16(id.Substring(5)))
                {
                    itemId = Convert.ToInt16(id.Substring(5));
                }
            }
        }

        #endregion Constructors
        #region Properites

        public string Application
        {
            get
            {
                return (application);
            }
            set
            {
                application = value;
            }
        }

        public string Command
        {
            get
            {
                return (command);
            }
            set
            {
                command = value;
            }
        }

        public Content Error
        {
            get
            {
                return (error);
            }
            set
            {
                error = value;
            }
        }

        public Content Input
        {
            get
            {
                return (input);
            }
            set
            {
                input = value;
            }
        }

        public Content Output
        {
            get
            {
                return (output);
            }
            set
            {
                output = value;
            }
        }

        public Coupler Inlet
        {
            get
            {
                return (inlet);
            }
            set
            {
                inlet = value;
            }
        }

        public Coupler Outlet
        {
            get
            {
                return (outlet);
            }
            set
            {
                outlet = value;
            }
        }

        #endregion Properites
        #region Methods

        public override bool AddData(string key, object value)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] key=" + key + " value=" + value);
                DictionaryEntry item = new DictionaryEntry(key,value);
                _localData.Add(item);
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add local data: key=" + key + " value=" + value);
                add = true;
            }
            catch { }
            return (add);
        }

        public override bool RemoveData(string key)
        {
            bool remove = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] key=" + key);
                _localData.Remove(key);
                TraceInternal.TraceVerbose("[" + _sessionId + "] Remove local data: key=" + key);
                remove = true;
            }
            catch { }
            return (remove);
        }

        public void Start()
        {
            Start(0);
        }

        public void Start(int activityIndex)
        {
            Start(activityIndex, Id.UniqueCode());
        }

        public void Start(string sessionId)
        {
            Start(0, Id.UniqueCode());
        }

        public void Start(int activityIndex, string sessionId)
        {
            Debug.WriteLine("[" + sessionId + "] In Start() " + _id + "(" + _name + ")");
            cancel = false;
            terminate = false;
            int process = 0;

            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            do
            {
                TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));

                if ((this._state == StateType.Ready) || (this._state == (int)StateType.None))
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Process Message");

                    process = this.Perform();
                }
                Thread.Sleep(1000);
            } while (terminate == false);
            _state = StateType.Closed;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + sessionId + "] Out Start() " + _id +"(" + _name + ")");
        }

        public int Perform()
        {
            return (Perform(0));
        }

        public int Perform(string sessionId)
        {
            return (Perform(0, sessionId));
        }

        public int Perform(int index)
        {
            return (Perform(index, Id.UniqueCode()));
        }

        public int Perform(int index, string sessionId)
        {
            Debug.WriteLine("[" + sessionId + "] In Perform() " + _id + "(" + _name + ")");
            
            this._sessionId = sessionId;
            int process = 0;    
            cancel = false;
            received = false;   // 8/2/2015 JPG fix early trigger of received status as process spead has increased
                                // Assumption is that data has always been received
            
            Groupings replace = new Groupings();
                                                                                                                                                                    
            // Create a process to handle the activity 

            proc = new System.Diagnostics.Process();

            // experimentally can set the processor afinity 

            //long affinityMask = 1;
            //proc.ProcessorAffinity = (IntPtr)affinityMask;
            //proc.PriorityClass = ProcessPriorityClass.BelowNormal;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = replace.ReplaceGrouping(application, _data, _hierarchy),
                Arguments = replace.ReplaceGrouping(command, _data, _hierarchy),
                CreateNoWindow = false,
                UseShellExecute = false
            };

            // Trap Standard output based on the output data type

            if (output.Kind != DataKind.none)
            {
                TraceInternal.TraceVerbose("[" + sessionId + "] Redirect Standard Output");
                startInfo.RedirectStandardOutput = true;
                proc.OutputDataReceived += new DataReceivedEventHandler(OutputReceived);
            }
            else
            {
                // 08/02/201 JPG If no output expected then need to force throught the recevied status
                received = true;
            }

            // Trap Standard error

            TraceInternal.TraceVerbose("[" + sessionId + "] Redirect Standard Error");
            startInfo.RedirectStandardError = true;
            proc.ErrorDataReceived += new DataReceivedEventHandler(ErrorReceived);

            // Trap Standard input based on the data type

            if (input.Kind != DataKind.none)
            {
                TraceInternal.TraceVerbose("[" + sessionId + "] Redirect Standard Input");
                startInfo.RedirectStandardInput = true;
            }


            TraceInternal.TraceVerbose("[" + sessionId + "] Shell \"" + startInfo.FileName + "\" " + startInfo.Arguments); // 23/2/2013 JPG Add quotes to aid testing

            // Enable exit event to be raised

            proc.EnableRaisingEvents = true;
            proc.StartInfo = startInfo;

            try
            {            
                proc.Start();
                if (output.Kind != DataKind.none)
                {    
                    proc.BeginOutputReadLine();
                }
                proc.BeginErrorReadLine();

                TraceInternal.TraceVerbose("[" + sessionId + "] Shell id=" + proc.Id);

                _state = StateType.Active;
                TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));

                // Need to send the input after we have started the process
     
                if ((string)input.Value != "")
                {
                    // Check if the data is coming from a pipe

                    if (input.Kind == DataKind.pipe)
                    {
                        // Register with the connector for events

                        inlet.Message += new Coupler.MessageHandler(InputReceived);
                        
                    }
                    else
                    {

                        string inputData = replace.ReplaceGrouping((string)input.Value, _data, _hierarchy);
                        using (System.IO.StreamWriter inputStream = proc.StandardInput)
                        {
                            inputStream.WriteLine(input.Value);
                            //inputStream.WriteLine((char)26);   // Need to terminate the write ^Z !!
                        }
                        TraceInternal.TraceVerbose("[" + sessionId + "] Sent=" + input.Value);
                    }
                }

                // Wait for any data returned and the process has exited
                // Wait for the process to exist and spoof any  return data as none expected.

                do
                {
                    Thread.Sleep(1000);
                    if (proc.HasExited == true)
                    {
                        TraceInternal.TraceVerbose("[" + sessionId + "] exited (" + received.ToString() + ")");
                    }
                }
                while (!(((proc.HasExited == true) && (received == true)) || (cancel == true) || (terminate == true)));

                if (((cancel == true) || (terminate == true)) && (proc.HasExited == false))
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] killing id=" + proc.Id);
                    proc.Kill();
                    TraceInternal.TraceVerbose("[" + sessionId + "] killed");
                }
                
                process = proc.ExitCode;

                if (process == 0)
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Ok (" + process + ")");
                }
                else
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Error (" + process + ")");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("[" + sessionId + "] Exception=" + e.ToString());
                process = 1;
                TraceInternal.TraceVerbose("[" + sessionId + "] Error (" + process + ")");
            }

            Thread.Sleep(1000);
            proc.Dispose();
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + sessionId + "] Out Perform() " + _id + "(" + process + ")");
            return (process);
        }

        private void InputReceived(Payload payload)
        {
            TraceInternal.TraceVerbose("[" + _sessionId + "] payload=" + payload.Content);
            Decode(payload.Content);
        }

        private void OutputReceived(object sendingProcess, DataReceivedEventArgs outputData)
        {
            Decode(outputData.Data);
        }

        private void ErrorReceived(object sendingProcess, DataReceivedEventArgs errorData)
        {
            if (errorData != null)
            {
                if (errorData.Data != null)
                {
                    if (errorData.Data.Length > 0)
                    {
                        Trace.TraceError("[" + _sessionId + "] Error=" + errorData.Data);
                    }
                }
            }
        }

        private void Decode(string buffer)
        {
            // need to decode a variety of outputData
            // if output is single keyword with no equal
            // if output is blank then do nothing
            // if output is equals only then use the output data which is comma separated
            //      key=value, key1=value1, key2=value2
            //
            // one of the odd issues here is that the event gets raised late, so oddly
            // the item object is beginning to reset before the data arrvies.
            // tried to set a received flag to prevent this happening but this then
            // causes problems as some shelled out process dont return data if their
            // timeout has occured.


            string receivedData = buffer;
            DictionaryEntry item;
            received = false;   // 8/2/2015 JPG fix early trigger of received status as process spead has increased
                                // Assumption is that data has always been received

            TraceInternal.TraceVerbose("[" + _sessionId + "] Reset received status");

            if (receivedData != null)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] buffer=" + receivedData);
                 
                try
                {
                    if (receivedData.Length > 0)
                    {
                        string key = "";
                        object value = "";

                        switch (output.Kind)
                        {
                            // 12/04/2014 JPG change to new attribute

                            case DataKind.multiple:
                            {
                                // The case where multiple key value pairs are retunred.

                                TraceInternal.TraceVerbose("[" + _sessionId + "] Multiple output='" + receivedData + "'");
                                bool passed = false;
                                Dictionary<string, object> keyValues = KVP.KvpDecode(receivedData, ref passed);

                                foreach (KeyValuePair<string,object> keyValue in keyValues)
                                {
                                    key = keyValue.Key;
                                    value = keyValue.Value;
                                    item = new DictionaryEntry(key, value);
                                    TraceInternal.TraceVerbose("Data: key=" + key + " value=" + value);

                                        try
                                        {
                                            TraceInternal.TraceVerbose("stage=" + (int)output.Stage + " hierarchy[stage]=" + _hierarchy[(int)output.Stage]);
                                            ArrayList stageData = (ArrayList)_data[(int)_hierarchy[(int)output.Stage]];   // Get the data for the appropriate stage

                                            int count = 0;
                                            DictionaryEntry existing;
                                            do
                                            {
                                                existing = (DictionaryEntry)stageData[count];
                                                if ((string)existing.Key == key)
                                                {
                                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Remove stage data: key=" + key);
                                                    stageData.RemoveAt(count);
                                                    break;
                                                }
                                                else
                                                {
                                                    count = count + 1;
                                                }
                                            }
                                            while (count < stageData.Count);
                                            stageData.Add(item);
                                            TraceInternal.TraceVerbose("[" + _sessionId + "] Add stage data: key=" + key);
                                        }
                                        catch (Exception e)
                                        {
                                            Trace.TraceError("[" + _sessionId + "] Failed to add stage data: key=" + key);
                                            TraceInternal.TraceVerbose("Exception=" + e.ToString());
                                        }
                                }
                                received = true;    // Finished processing
                                break;
                            }
                            case DataKind.single:
                                {
                                    // The case where single key value pairs are returned.

                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Single output=" + buffer);

                                    string keyValue = receivedData;
                                    string[] part = keyValue.Split('=');
                                    key = "";
                                    value = "";
                                    if (part.Length == 2)
                                    {
                                        key = part[0].Trim('"');
                                        value = part[1].Trim('"');
                                        item = new DictionaryEntry(key, value);
                                        TraceInternal.TraceVerbose("Data: key=" + key + " value=" + value);


                                        try
                                        {
                                            TraceInternal.TraceVerbose("stage=" + (int)output.Stage + " hierarchy[stage]=" + _hierarchy[(int)output.Stage]);
                                            ArrayList stageData = (ArrayList)_data[(int)_hierarchy[(int)output.Stage]];   // Get the data for the appropriate stage

                                            int count = 0;
                                            DictionaryEntry existing;
                                            do
                                            {
                                                existing = (DictionaryEntry)stageData[count];
                                                if ((string)existing.Key == key)
                                                {
                                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Remove stage data: key=" + key);
                                                    stageData.RemoveAt(count);
                                                    break;
                                                }
                                                else
                                                {
                                                    count = count + 1;
                                                }
                                            }
                                            while (count < stageData.Count);
                                            stageData.Add(item);
                                            TraceInternal.TraceVerbose("[" + _sessionId + "] Add stage data: key=" + key);

                                        }
                                        catch (Exception e)
                                        {
                                            Trace.TraceError("[" + _sessionId + "] Failed to add stage data: key=" + key);
                                            TraceInternal.TraceVerbose("Exception=" + e.ToString());
                                        }
                                    }
                                    received = true;    // Finished processing
                                    break;
                                }
                            case DataKind.echo:
                                {
                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Echo output='" + buffer + "'");

                                    // Legacy check on data required even though echo

                                    key = (string)output.Value;
                                    if (key.Length == 0)
                                    {
                                        received = true;
                                    }
                                    break;
                                }
                            case DataKind.none:
                                {
                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Ignore output='" + buffer + "'");
                                    received = true;    // Finished processing
                                    break;
                                }
                            case DataKind.pipe:
                                {
                                    outlet.Fill(receivedData);
                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Piped output='" + receivedData + "'");
                                    received = true;    // Finished processing
                                    break;
                                }
                            case DataKind.raw:
                                {
                                    TraceInternal.TraceVerbose("[" + _sessionId + "] value output='" + buffer + "'");
                                    key = (string)output.Value;
                                    value = receivedData;
                                    item = new DictionaryEntry(key, value);
                                    TraceInternal.TraceVerbose("Data: key=" + key + " value=" + value);

                                    try
                                    {
                                        TraceInternal.TraceVerbose("stage=" + (int)output.Stage + " hierarchy[stage]=" + _hierarchy[(int)output.Stage]);
                                        ArrayList stageData = (ArrayList)_data[(int)_hierarchy[(int)output.Stage]];   // Get the data for the appropriate stage

                                        if (stageData.Count > 0)
                                        {
                                            int count = 0;
                                            DictionaryEntry existing;
                                            do
                                            {
                                                existing = (DictionaryEntry)stageData[count];
                                                if ((string)existing.Key == key)
                                                {
                                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Remove stage data: key=" + key);
                                                    stageData.RemoveAt(count);
                                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Update stage data: key=" + key);
                                                    value = existing.Value.ToString() + "\r\n" + receivedData;
                                                    item = new DictionaryEntry(key, value);
                                                    break;
                                                }
                                                else
                                                {
                                                    count = count + 1;
                                                }
                                            }
                                            while (count < stageData.Count);
                                        }
                                        stageData.Add(item);
                                        TraceInternal.TraceVerbose("[" + _sessionId + "] Add stage data: key=" + key);
                                    }

                                    catch (Exception e)
                                    {
                                        Trace.TraceError("[" + _sessionId + "] Failed to update stage data: key=" + key);
                                        TraceInternal.TraceVerbose("[" + _sessionId + "] Exception=" + e.ToString());
                                    }
                                    received = true;    // Finished processing
                                    break;
                                }
                            case DataKind.value:
                            {
                                TraceInternal.TraceVerbose("[" + _sessionId + "] value output='" + buffer + "'");

                                key = (string)output.Value;
                                value = receivedData;
                                item = new DictionaryEntry(key,value);
                                TraceInternal.TraceVerbose("Data: key=" + key + " value=" + value);

                                try
                                {
                                    TraceInternal.TraceVerbose("stage=" + (int)output.Stage + " hierarchy[stage]=" + _hierarchy[(int)output.Stage]);
                                    ArrayList stageData = (ArrayList)_data[(int)_hierarchy[(int)output.Stage]];   // Get the data for the appropriate stage

                                    if (stageData.Count>0)
                                    {
                                        int count = 0;
                                        DictionaryEntry existing;
                                        do
                                        {
                                            existing = (DictionaryEntry)stageData[count];
                                            if ((string)existing.Key == key)
                                            {
                                                TraceInternal.TraceVerbose("[" + _sessionId + "] Remove stage data: key=" + key);
                                                stageData.RemoveAt(count);
                                            }
                                            else
                                            {
                                                count = count + 1;
                                            }
                                        }
                                        while (count < stageData.Count);
                                    }
                                    stageData.Add(item);
                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Add stage data: key=" + key);
                                }
                                catch (Exception e)
                                {
                                    Trace.TraceError("[" + _sessionId + "] Failed to add stage data: key=" + key);
                                    TraceInternal.TraceVerbose("[" + _sessionId + "] Exception=" + e.ToString());
                                }
                                received = true;    // Finished processing
                                break;
                            }
                        }
                    }
                    else
                    {
                        if ((string)output.Value == "")
                        {
                            received = true;    // Dummy the received status if no output is expected
                        } 
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("[" + _sessionId + "] Cannot add data Item: " + _id + " " + e.Message);
                }
            }
            else
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Force received status");
                received = true;
            }
        }

        public void Update(ref ArrayList data, ArrayList parentHierarchy)
        {
            Debug.WriteLine("[" + _sessionId + "] In Update() " + _id + "(" + _name + ")");

            tempData = (ArrayList)_localData.Clone();                // Preserve the localData and clone.
            int dataID = data.Add(tempData);                        // add the tempData pointer to the data array list.
            TraceInternal.TraceVerbose("[" + _sessionId + "] Add local data to global data as dataID=" + dataID);
            _hierarchy = (ArrayList)parentHierarchy.Clone();         // Copy the parent hierarchy.
            _hierarchy.Insert((int)StageType.Item, dataID);          // Add the tempData reference to the end.
            TraceInternal.TraceVerbose("[" + _sessionId + "] Add dataId to hierachy ");

            for (int i = 0; i < _hierarchy.Count; i++)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] hierarchy(" + i.ToString() + ")=" + _hierarchy[i].ToString());
            }

            this._data = data;
            Debug.WriteLine("[" + _sessionId + "] Out Update() " + _id + "(" + _name + ")");
        }

        public void Cancel()
        {
            Debug.WriteLine("[" + _sessionId + "] In Cancel() " + _id + "(" + _name + ")");
            _state = StateType.Withdrawn;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            this.cancel = true;
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + _sessionId + "] Out Cancel() " + _id + "(" + _name + ")");
        }

        public void Terminate()
        {         
            Debug.WriteLine("[" + _sessionId + "] In Terminate() " + _id + "(" + _name + ")");
            _state = StateType.Terminating;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            this.terminate = true;
            _state = StateType.Terminated;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + _sessionId + "] Out Terminate() " + _id + "(" + _name + ")");
        }

        public void Activate()
        {
            Debug.WriteLine("[" + _sessionId + "] In Activate() " + _id + "(" + _name + ")");
            _state= StateType.Inactive;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            this.cancel = false;
            _state= StateType.Ready;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + _sessionId + "] Out Activate() " + _id + "(" + _name + ")");
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion Methods
      }
}
