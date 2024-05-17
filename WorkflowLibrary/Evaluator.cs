using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using System.Reflection;

namespace WorkflowLibrary
{
    public class Evaluator
    {
        #region Fields

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
            return ("");
        }

        static public int EvaluateToInteger(string code)
        {
            return ((int)0);
        }

        static public string EvaluateToString(string code)
        {
            return ((string)"");
        }

        static public bool EvaluateToBool(string code)
        {
            return ((bool)true);
        }

        static public object EvaluateToObject(string code)
        {
            return (true);
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
