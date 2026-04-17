using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using System.Reflection;
using System.Data;
using TracerLibrary;

namespace WorkflowLibrary
{
    public class Evaluator
    {
        #region Fields
        private EvaluatorItem[] _items;
        #endregion
        #region Constructor
        
        public Evaluator(EvaluatorItem[] items)
        {
            ConstructEvaluator(items);
        }

        public Evaluator(Type returnType, string expression, string name)
        {
            EvaluatorItem[] items = { new EvaluatorItem(returnType, expression, name) };
            ConstructEvaluator(items);
        }

        public Evaluator(EvaluatorItem item)
        {
            EvaluatorItem[] items = { item };
            ConstructEvaluator(items);
        }

        #endregion
        #region Methods
        private void ConstructEvaluator(EvaluatorItem[] items)
        {
            _items = items;
        }
        public int EvaluateInt(string name)
        {
            return (int)Evaluate(name);
        }

        public int EvaluateDouble(string name)
        {
            return (int)Evaluate(name);
        }

        public string EvaluateString(string name)
        {
            return (string)Evaluate(name);
        }

        public bool EvaluateBool(string name)
        {
            return (bool)Evaluate(name);
        }

        public object Evaluate(string name)
        {
            try
            {
                // Find the evaluator item by name
                EvaluatorItem item = null;
                foreach (var evalItem in _items)
                {
                    if (evalItem.Name == name)
                    {
                        item = evalItem;
                        break;
                    }
                }

                if (item == null)
                {
                    TraceInternal.TraceVerbose($"Evaluator item not found: {name}");
                    return null;
                }

                // Evaluate the expression using DataTable.Compute
                DataTable table = new DataTable();
                var result = table.Compute(item.Expression, "");

                // Convert to the expected return type
                if (item.ReturnType == typeof(int))
                    return Convert.ToInt32(result);
                else if (item.ReturnType == typeof(double))
                    return Convert.ToDouble(result);
                else if (item.ReturnType == typeof(string))
                    return result?.ToString() ?? "";
                else if (item.ReturnType == typeof(bool))
                {
                    if (result is bool boolResult)
                        return boolResult;
                    if (result is int || result is decimal || result is double || result is long)
                        return Convert.ToDouble(result) != 0;
                    return Convert.ToBoolean(result);
                }
                else
                    return result;
            }
            catch (Exception ex)
            {
                TraceInternal.TraceVerbose($"Expression evaluation failed for '{name}': {ex.Message}");
                return null;
            }
        }

        static public int EvaluateToInteger(string code)
        {
            try
            {
                DataTable table = new DataTable();
                var result = table.Compute(code, "");
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                TraceInternal.TraceVerbose($"Expression evaluation failed: {code} - {ex.Message}");
                return 0;
            }
        }

        static public string EvaluateToString(string code)
        {
            try
            {
                DataTable table = new DataTable();
                var result = table.Compute(code, "");
                return result?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                TraceInternal.TraceVerbose($"Expression evaluation failed: {code} - {ex.Message}");
                return "";
            }
        }

        static public bool EvaluateToBool(string code)
        {
            try
            {
                DataTable table = new DataTable();
                var result = table.Compute(code, "");

                // Handle different return types from comparisons
                if (result is bool boolResult)
                    return boolResult;

                // Handle numeric results (non-zero = true)
                if (result is int || result is decimal || result is double || result is long)
                    return Convert.ToDouble(result) != 0;

                return Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                TraceInternal.TraceVerbose($"Expression evaluation failed: {code} - {ex.Message}");
                return false;
            }
        }

        static public object EvaluateToObject(string code)
        {
            try
            {
                DataTable table = new DataTable();
                return table.Compute(code, "");
            }
            catch (Exception ex)
            {
                TraceInternal.TraceVerbose($"Expression evaluation failed: {code} - {ex.Message}");
                return null;
            }
        }
        #endregion
    }

    public class EvaluatorItem
    {
        #region Constructors
        public Type ReturnType;
        public string Name;
        public string Expression;
        #endregion
        #region Properties 
        public EvaluatorItem(Type returnType, string expression, string name)
        {
            ReturnType = returnType;
            Expression = expression;
            Name = name;
        }
        #endregion
    }
}
