using System;
using System.Collections.Generic;
using System.Text;

namespace WorkflowLibrary
{
    /// <summary>
    /// Object to hold information content description
    /// This will generally apply to input and output
    /// information as this can be varied, probably limited to
    /// string, integer, double.
    /// </summary>
    public class Content : State
    {
        #region Fields
        
        private object _value;
        private DataType _type;
        private DataKind _kind;     // This is not going to work with multiple entries.
        private StageType _stage;

        #endregion
        #region Constructors

        public Content()
        {
            _kind = DataKind.echo;
        }

        public Content(object value)
        {
            _kind = DataKind.echo;
            _value = value;
            Type itemType = _value.GetType();

            if (itemType == typeof(int)) 
            {
                _type = DataType.Integer;
            }
            else if (itemType == typeof(string))
            {
                _type = DataType.String;
            }
            else if (itemType == typeof(double))
            {
                _type = DataType.Double;
            }
        }

        public Content(object value, DataType kind, DataKind type, StageType stage)
        {
            _value = value;
            _type = kind;
            _kind = type;
            _stage = stage;

        }
        #endregion
        #region Properties
        public object Value
        {
            set
            {
                _value = value;
                Type itemType = _value.GetType();

                if (itemType == typeof(int))
                {
                    _type = DataType.Integer;
                }
                else if (itemType == typeof(string))
                {
                    _type = DataType.String;
                }
                else if (itemType == typeof(double))
                {
                    _type = DataType.Double;
                }
            }
            get
            {
                if (_value == null)
                {
                    if (_type == DataType.String)
                    {
                        return ("");
                    }
                    else
                    {
                        return(0);
                    }
                }
                else
                {
                    return _value;
                }
            }
        }            

        public DataType Type
        {
            set
            {
                _type = value;
            }
            get
            {
                return _type;
            }
        }

        public DataKind Kind
        {
            set
            {
                _kind = value;
            }
            get
            {
                return _kind;
            }
        }

        public StageType Stage
        {
            set
            {
                _stage = value;
            }
            get
            {
                return _stage;
            }
        }

        #endregion
        #region Methods
        //public static implicit operator Content(object value)
        //{
        //    return new Content(value);
        //}

        //public static implicit operator object(Content value)
        //{
        //    return value._value;
        //}
        
        #endregion
    }
}
