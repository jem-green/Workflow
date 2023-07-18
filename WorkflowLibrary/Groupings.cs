using System;
using System.Text;
using TracerLibrary;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace WorkflowLibrary
{
    public class Groupings
    {
        #region Fields
        #endregion
        #region Methods

        public ArrayList ExtractGroupings(string source, string matchPattern, bool wantInitialMatch)
        {
            ArrayList keyedMatches = new ArrayList();
            int startingElement = 1;
            if (wantInitialMatch)
            {
                startingElement = 0;
            }
            Regex RE = new Regex(matchPattern, RegexOptions.Multiline);
            MatchCollection theMatches = RE.Matches(source);
            foreach (Match m in theMatches)
            {
                Hashtable groupings = new Hashtable();
                for (int counter = startingElement;
                   counter < m.Groups.Count; counter++)
                {
                    // If we had just returned the MatchCollection directly, the
                    // GroupNameFromNumber method would not be available to use
                    groupings.Add(RE.GroupNameFromNumber(counter), m.Groups[counter]);
                }
                keyedMatches.Add(groupings);
            }
            return (keyedMatches);
        }

        public string ReplaceGrouping(string input, ArrayList groupings, ArrayList hierarchy)
        {
            string output = input;

            TraceInternal.TraceVerbose("before=" + input);

            if (groupings != null)
            {
                for (int node = hierarchy.Count - 1; node >= 0; node--)    // Search through the data from child to parent
                {
                    int position = (int)hierarchy[node];
                    if (position > -1)   // Fix for process being missing
                    {
                        ArrayList grouping = (ArrayList)groupings[position];
                        foreach (object data in grouping)
                        {
                            var type = data.GetType();
                            if ( type == typeof(DictionaryEntry))
                            {
                                DictionaryEntry keyvalue = (DictionaryEntry)data;
                                if (output.IndexOf(keyvalue.Key.ToString()) > -1)
                                {
                                    TraceInternal.TraceVerbose("replace=" + "[" + keyvalue.Key.ToString() + "]" + " with=" + keyvalue.Value.ToString());                                    
                                    try
                                    {
                                        output = output.Replace("[" + keyvalue.Key.ToString() + "]", keyvalue.Value.ToString());
                                    }
                                    catch
                                    {
                                        Trace.TraceError("Replace failed");
                                    }
                                }
                            }
                            else
                            {
                                Trace.TraceWarning("Other data type");
                            }
                        }
                    }
                }
            }

            TraceInternal.TraceVerbose("after=" + output);
            return (output);
        }

        public string ReplaceGrouping(string input, ArrayList groupings)
        {
            string output = input;
            //string before = "";

            TraceInternal.TraceVerbose("before=" + input);
            if (groupings != null)
            {
                foreach (object data in groupings)
                {
                    var type = data.GetType();
                    if (type == typeof(DictionaryEntry))
                    {
                        DictionaryEntry keyvalue = (DictionaryEntry)data;
                        if (output.IndexOf(keyvalue.Key.ToString()) > -1)
                        {
                            TraceInternal.TraceVerbose("replace=" + "[" + keyvalue.Key.ToString() + "]" + " with=" + keyvalue.Value.ToString());
                            try
                            {
                                output = output.Replace("[" + keyvalue.Key.ToString() + "]", keyvalue.Value.ToString());
                            }
                            catch
                            {
                                TraceInternal.TraceVerbose("Replace failed");
                            }
                        }
                    }
                    else
                    {
                        Hashtable grouping = (Hashtable)data;
                        foreach (DictionaryEntry keyvalue in grouping)
                        {
                            if (keyvalue.Key.ToString() != "0")
                            {
                                if (output.IndexOf(keyvalue.Key.ToString()) > -1)
                                {
                                    TraceInternal.TraceVerbose("replace=" + "[" + keyvalue.Key.ToString() + "]" + " with=" + keyvalue.Value.ToString());
                                    try
                                    {
                                        output = output.Replace("[" + keyvalue.Key.ToString() + "]", keyvalue.Value.ToString());
                                    }
                                    catch
                                    {
                                        TraceInternal.TraceVerbose("Replace failed");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            TraceInternal.TraceVerbose("after=" + output);
            return (output);
        }
        #endregion
    }
}
