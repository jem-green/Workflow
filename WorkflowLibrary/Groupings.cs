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
    public class Replacer 
    {
        #region Fields

        #endregion
        #region Methods

        //public static List<Grouping> ExtractGroupings(string source, string matchPattern, bool wantInitialMatch)
        //{
        //    List<Grouping> keyedMatches = new List<Grouping>();
        //    int startingElement = 1;
        //    if (wantInitialMatch)
        //    {
        //        startingElement = 0;
        //    }
        //    Regex RE = new Regex(matchPattern, RegexOptions.Multiline);
        //    MatchCollection theMatches = RE.Matches(source);
        //    foreach (Match m in theMatches)
        //    {
        //        Hashtable groupings = new Hashtable();
        //        for (int counter = startingElement;
        //           counter < m.Groups.Count; counter++)
        //        {
        //            // If we had just returned the MatchCollection directly, the
        //            // GroupNameFromNumber method would not be available to use
        //            groupings.Add(RE.GroupNameFromNumber(counter), m.Groups[counter]);
        //        }
        //        keyedMatches.Add(groupings);
        //    }
        //    return (keyedMatches);
        //}



        /// <summary>
        /// replace grouping keys in the input string with the corresponding values
        /// from the groupings ArrayList.  The hierarchy ArrayList is used to determine
        /// which groupings to use for replacement, and the order in which to apply them.
        /// The hierarchy should be ordered from parent to child, and should contain the
        /// index of the grouping in the groupings ArrayList.  For example, if hierarchy
        /// contains [0,2], then the method will first apply the replacements from groupings[0],
        /// and then apply the replacements from groupings[2].  This allows for nested groupings,
        /// where a child grouping can reference a key from a parent grouping.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="groupings"></param>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        public static string ReplaceGrouping(string input, List<Grouping> groupings, List<int> hierarchy)
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
                        // groupings -> List<Grouping>

                        Grouping grouping = groupings[position];

                        // grouping -> List<object>
                        // Where each object could be a Dictionary entry or HashTable

                        foreach (KeyValuePair<string, object> keyvalue in grouping)
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
                                    Trace.TraceError("Replace failed");
                                }
                            }
                        }
                    }
                }
            }

            TraceInternal.TraceVerbose("after=" + output);
            return (output);
        }

        public static string ReplaceGrouping(string input, List<object> groupings)
        {
            string output = input;
            //string before = "";

            TraceInternal.TraceVerbose("before=" + input);
            if (groupings != null)
            {
                foreach (object data in groupings)
                {
                    if (data is KeyValuePair<string, object> keyvalue)
                    {
                        if (output.IndexOf(keyvalue.Key) > -1)
                        {
                            TraceInternal.TraceVerbose("replace=" + "[" + keyvalue.Key + "]" + " with=" + (keyvalue.Value != null ? keyvalue.Value.ToString() : "null"));
                            try
                            {
                                output = output.Replace("[" + keyvalue.Key + "]", keyvalue.Value != null ? keyvalue.Value.ToString() : null);
                            }
                            catch
                            {
                                TraceInternal.TraceVerbose("Replace failed");
                            }
                        }
                    }
                    else if (data is List<KeyValuePair<string, object>> grouping)
                    {
                        foreach (var kv in grouping)
                        {
                            if (output.IndexOf(kv.Key) > -1)
                            {
                                TraceInternal.TraceVerbose("replace=" + "[" + kv.Key + "]" + " with=" + (kv.Value != null ? kv.Value.ToString() : "null"));
                                try
                                {
                                    output = output.Replace("[" + kv.Key + "]", kv.Value != null ? kv.Value.ToString() : null);
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
            TraceInternal.TraceVerbose("after=" + output);
            return (output);
        }
        #endregion


    }

}
