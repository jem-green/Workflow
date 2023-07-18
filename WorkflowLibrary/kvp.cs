using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using TracerLibrary;

namespace WorkflowLibrary
{

    /// <summary>
    /// 
    /// </summary>
    public class KVP
    {
        #region Fields

        public enum Token
        {
            None = 0,
            Error = 1,
            EndOfInput = 2,
            Comma = 3,
            String = 4,
            Number = 5,
            Equal = 6,
            Char = 7
        }

        private const int BUILDER_CAPACITY = 2000;

        static bool escaped = false;

        #endregion
        #region Methods

        /// <summary>
        /// Parses the string key values into a value
        /// </summary>
        /// <param name="keyValues">A KVP string.</param>
        /// <returns>A dictionary</returns>
        public static object KvpDecode(string keyValues)
        {
            bool success = true;
            return KvpDecode(keyValues, ref success);
        }

        public static Dictionary<string, object> KvpDecode(string keyValues, ref bool success)
        {
            bool escaped = false;
            return KvpDecode(keyValues, ref success, escaped);
        }

        /// <summary>
        /// Parses the string key values into a value; and fills 'success' with the successfullness of the parse.
        /// </summary>
        /// <param name="keyValues">A key value pair string.</param>
        /// <param name="success">Successful parse?</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static Dictionary<string,object> KvpDecode(string keyValues, ref bool success, bool escaped)
        {
            success = true;
            if (keyValues != null)
            {
                Dictionary<string, object> value;
                char[] charArray = keyValues.ToCharArray();
                int index = 0;
                //object value = ParseValue(charArray, ref index, ref success);
                value = ParseObject(charArray, ref index, ref success);
                return value;
            }
            else
            {
                return null;
            }
        }

        #endregion
        #region Protected
        protected static Dictionary<string, object> ParseObject(char[] keyValues, ref int index, ref bool success)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            KVP.Token token;

            bool done = false;
            while (!done)
            {
                token = LookAhead(keyValues, index);

                if (token == KVP.Token.Comma)
                {
                    NextToken(keyValues, ref index);
                }
                else if (token == KVP.Token.EndOfInput)
                {
                    return (dictionary);
                }
                else if ((token == KVP.Token.String) || (token == KVP.Token.Char))
                {

                    // key
                    string name = ParseKey(keyValues, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    // =
                    token = NextToken(keyValues, ref index);
                    if (token != KVP.Token.Equal)
                    {
                        success = false;
                        return null;
                    }

                    // value
                    object value = ParseValue(keyValues, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    dictionary[name] = value;
                }
                else
                {
                    success = false;
                    return null;
                }
            }

            return dictionary;
        }

        protected static string ParseKey(char[] keyValues, ref int index, ref bool success)
        {
            switch (LookAhead(keyValues, index))
            {
                case KVP.Token.String:
                    return ParseString(keyValues, ref index, ref success);
                case KVP.Token.Char:
                    return ParseChars(keyValues, ref index, ref success);
                case KVP.Token.None:
                    break;
            }
            success = false;
            return null;
        }

        protected static object ParseValue(char[] keyValues, ref int index, ref bool success)
        {
            switch (LookAhead(keyValues, index))
            {
                case KVP.Token.String:
                    return ParseString(keyValues, ref index, ref success);
                case KVP.Token.Number:
                    return ParseNumber(keyValues, ref index, ref success);
                case KVP.Token.None:
                    break;
            }

            success = false;
            return null;
        }

        protected static string ParseString(char[] keyValues, ref int index, ref bool success)
        {
            StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
            char c;

            EatWhitespace(keyValues, ref index);

            // "
            c = keyValues[index++]; // skip over the "

            bool complete = false;
            while (!complete)
            {

                if (index == keyValues.Length)
                {
                    break;
                }

                c = keyValues[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if ((c == '\\') && (escaped == true))
                {

                    if (index == keyValues.Length)
                    {
                        break;
                    }
                    c = keyValues[index++];
                    if (c == '"')
                    {
                        s.Append('"');
                    }
                    else if (c == '\\')
                    {
                        s.Append('\\');
                    }
                    else if (c == '/')
                    {
                        s.Append('/');
                    }
                    else if (c == 'b')
                    {
                        s.Append('\b');
                    }
                    else if (c == 'f')
                    {
                        s.Append('\f');
                    }
                    else if (c == 'n')
                    {
                        s.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        s.Append('\r');
                    }
                    else if (c == 't')
                    {
                        s.Append('\t');
                    }
                    else if (c == 'u')
                    {
                        int remainingLength = keyValues.Length - index;
                        if (remainingLength >= 4)
                        {
                            // parse the 32 bit hex into an integer codepoint
                            if (!(success = UInt32.TryParse(new string(keyValues, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint codePoint)))
                            {
                                return "";
                            }
                            // convert the integer codepoint to a unicode char and add to string
                            s.Append(Char.ConvertFromUtf32((int)codePoint));
                            // skip 4 chars
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                else
                {
                    s.Append(c);
                }

            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return s.ToString();
        }

        protected static string ParseChars(char[] keyValues, ref int index, ref bool success)
        {
            StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
            char c;

            EatWhitespace(keyValues, ref index);

            bool complete = false;
            while (!complete)
            {

                if (index == keyValues.Length)
                {
                    break;
                }

                c = keyValues[index++];
                if (c == '=')
                {
                    index = index - 1;  // fix delimiter issue
                    complete = true;
                    break;
                }         
                else
                {
                    s.Append(c);
                }
            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return s.ToString();
        }

        protected static double ParseNumber(char[] keyValues, ref int index, ref bool success)
        {
            EatWhitespace(keyValues, ref index);

            int lastIndex = GetLastIndexOfNumber(keyValues, index);
            int charLength = (lastIndex - index) + 1;

            success = Double.TryParse(new string(keyValues, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out double number);

            index = lastIndex + 1;
            return number;
        }

        protected static int GetLastIndexOfNumber(char[] keyValues, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < keyValues.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(keyValues[lastIndex]) == -1)
                {
                    break;
                }
            }
            return lastIndex - 1;
        }

        protected static void EatWhitespace(char[] keyValues, ref int index)
        {
            for (; index < keyValues.Length; index++)
            {
                if (" \t\n\r".IndexOf(keyValues[index]) == -1)
                {
                    break;
                }
            }
        }

        protected static KVP.Token LookAhead(char[] keyValues, int index)
        {
            int saveIndex = index;
            return NextToken(keyValues, ref saveIndex);
        }

        protected static KVP.Token NextToken(char[] keyValues, ref int index)
        {
            EatWhitespace(keyValues, ref index);

            if (index == keyValues.Length)
            {
                return KVP.Token.EndOfInput;
            }

            char c = keyValues[index];
            index++;
            switch (c)
            {
                case ',':
                    {
                        return (KVP.Token.Comma);
                    }
                case '=':
                    {
                        return (KVP.Token.Equal);
                    }
                case '"':
                    {
                        return (KVP.Token.String);
                    }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    {
                        return (KVP.Token.Number);
                    }
                default:
                    {
                        if (((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')))
                        {
                            return (KVP.Token.Char);
                        }
                        break;
                    }
            }
            index--;
 
            return KVP.Token.None;
        }
        #endregion
    }
}
