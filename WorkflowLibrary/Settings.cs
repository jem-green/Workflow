using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TracerLibrary;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace JobsLibrary
{
    public class Settings
    {
        public string Read(string keyName, string valueName, string defaultValue)
        {
            string value = GetValue("software\\green\\tag\\" + keyName, valueName, defaultValue);
            return (value);
        }

        public bool BuildKeys(String tree)
        {

            TraceInternal.TraceInformation("BuildKeys");

            RegistryKey regKey = null;
            bool create = false;
            string[] keys;

            // Better way is to recurse through the registry and check that the keys actually exist
            // Rather than just checking for the root

            keys = tree.Split('\\');
            regKey = Registry.LocalMachine;
            foreach (string key in keys)
            {
                if (regKey.OpenSubKey(key) == null)
                {
                    try
                    {
                        regKey = regKey.CreateSubKey(key);
                        create = true;
                    }
                    catch
                    {
                        // Probably a security issue
                    }
                }
                else
                {
                    regKey = regKey.OpenSubKey(key, true);     // Make writeable
                }
            }
            return (create);
        }

        public RegistryKey OpenKey(String tree)
        {

            TraceInternal.TraceInformation("OpenKey");

            RegistryKey regKey = null;
            string[] keys;

            keys = tree.Split('\\');
            regKey = Registry.LocalMachine;
            foreach (string key in keys)
            {
                try
                {
                    regKey = regKey.OpenSubKey(key);
                }
                catch
                {
                    //Writeline("Could not open " & tree);
                    regKey = null;
                }
            }
            return (regKey);
        }

        public string GetValue(string tree, string valueName, string defaultValue)
        {

            TraceInternal.TraceInformation("GetValue");

            RegistryKey regKey = null;
            string[] keys;
            string value = "";

            TraceInternal.TraceInformation("Get value '" + valueName + "' from '" + tree + "' or default to '" + defaultValue + "'");

            keys = tree.Split('\\');
            regKey = Registry.LocalMachine;
            foreach (string key in keys)
            {
                try
                {
                    TraceInternal.TraceInformation("Open key '" + key + "'");
                    regKey = regKey.OpenSubKey(key);
                }
                catch
                {
                    TraceInternal.TraceInformation("Cannot open key '" + key + "'");
                    regKey = null;
                }
            }
            try
            {
                value = (string)regKey.GetValue(valueName, defaultValue);
                TraceInternal.TraceInformation("Found key='" + valueName + "' value='" + value + "'");
            }
            catch
            {
                value = defaultValue;
                TraceInternal.TraceInformation("Error value='" + value + "'");
            }
            return (value);
        }

        public bool SetValue(string tree, string valueName, string value)
        {

            TraceInternal.TraceInformation("SetValue");

            RegistryKey regKey = null;
            bool create = false;
            string[] keys;

            keys = tree.Split('\\');
            regKey = Registry.LocalMachine;
            foreach (string key in keys)
            {
                if (regKey.OpenSubKey(key) == null)
                {
                    try
                    {
                        regKey = regKey.CreateSubKey(key);
                        create = true;
                    }
                    catch
                    {
                        TraceInternal.TraceInformation("Could not create key '" + key + "'");
                        create = false;
                    }
                }
                else
                {
                    TraceInternal.TraceInformation("Open key '" + key + "'");
                    regKey = regKey.OpenSubKey(key, true);    // Make writeable
                }
            }
            try
            {
                TraceInternal.TraceInformation("Set " + valueName + "='" + value + "'");
                regKey.SetValue(valueName, value);
            }
            catch
            {
                // nothing
            }
            return (create);

        }
        public Collection<string> GetNames(string tree)
        {

            TraceInternal.TraceInformation("GetNames");

            RegistryKey regKey = null;
            string[] keys;
            Collection<string> names;
            names = new Collection<string>();

            keys = tree.Split('\\');
            regKey = Registry.LocalMachine;

            foreach (string key in keys)
            {
                try
                {
                    TraceInternal.TraceInformation("Open key '" + key + "'");
                    regKey = regKey.OpenSubKey(key);
                }
                catch
                {
                    TraceInternal.TraceInformation("Could not open key '" + key + "'");
                    regKey = null;
                }
            }

            if (regKey == null)
            {
                // Cannot add
                names = null;
            }
            else
            {

                foreach (string name in regKey.GetSubKeyNames())
                {
                    names.Add(name);
                }

            }
            return (names);
        }
        public Collection<string> GetKeys(string tree)
        {

            TraceInternal.TraceInformation("GetKeys");

            RegistryKey regKey = null;
            string[] keys;
            Collection<string> subKeys;
            subKeys = new Collection<string>();

            keys = tree.Split('\\');
            regKey = Registry.LocalMachine;

            foreach (string key in keys)
            {
                try
                {
                    TraceInternal.TraceInformation("Open sub key '" + key + "'");
                    regKey = regKey.OpenSubKey(key);
                }
                catch
                {
                    TraceInternal.TraceInformation("Could not open sub key '" + key + "'");
                }
            }

            if (regKey == null)
            {
                TraceInternal.TraceInformation("Could not find sub key '" + tree + "'");
            }
            else
            {

                foreach (string keyName in regKey.GetSubKeyNames())
                {
                    TraceInternal.TraceInformation("Add sub key '" + keyName + "'");
                    subKeys.Add(keyName);
                }

            }
            return (subKeys);
        }

        public Collection<string> GetValues(string tree)
        {

            RegistryKey regKey = null;
            string[] keys;
            string value = "";
            Collection<string> values;

            TraceInternal.TraceInformation("GetValues");

            values = new Collection<string>();
            keys = tree.Split('\\');
            regKey = Registry.LocalMachine;

            foreach (string key in keys)
            {
                try
                {
                    TraceInternal.TraceInformation("Open sub key '" + key + "'");
                    regKey = regKey.OpenSubKey(key);
                }
                catch
                {
                    TraceInternal.TraceInformation("Could not open sub key '" + key + "'");
                }
            }

            if (regKey == null)
            {
                TraceInternal.TraceInformation("Could not find sub key '" + tree + "'");
            }
            else
            {

                foreach (string name in regKey.GetValueNames())
                {
                    try
                    {
                        TraceInternal.TraceInformation("Get Value for name '" + name + "'");
                        value = (string)regKey.GetValue(name);
                        if (value.EndsWith("\\"))
                        {
                            value = value.Substring(0, value.Length - 1);
                        }
                        TraceInternal.TraceInformation("Add value '" + value + "'");
                        values.Add(value);
                    }
                    catch
                    {
                        TraceInternal.TraceInformation("Could not add value for name '" + name + "'");
                        value = "";
                    }

                }

            }
            return (values);
        }
    }
}
