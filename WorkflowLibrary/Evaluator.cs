using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using System.Reflection;

namespace JobsLib
{
    public class Evaluator
    {
        #region Fields

        const string staticMethodName = "__foo";
        object _Compiled = null;

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
            ICodeCompiler comp = (new CSharpCodeProvider().CreateCompiler());
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("system.dll");
            cp.ReferencedAssemblies.Add("system.data.dll");
            cp.ReferencedAssemblies.Add("system.xml.dll");
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;

            StringBuilder code = new StringBuilder();
            code.Append("using System; \n");
            code.Append("using System.Data; \n");
            code.Append("using System.Data.SqlClient; \n");
            code.Append("using System.Data.OleDb; \n");
            code.Append("using System.Xml; \n");
            code.Append("namespace JobsLib { \n");
            code.Append("  public class _Evaluator { \n");
            foreach (EvaluatorItem item in items)
            {
                code.AppendFormat("    public {0} {1}() ", item.ReturnType.Name, item.Name);
                code.Append("{ ");
                code.AppendFormat("      return ({0}); ", item.Expression);
                code.Append("}\n");
            }
            code.Append("} }");

            CompilerResults cr = comp.CompileAssemblyFromSource(cp, code.ToString());
            if (cr.Errors.HasErrors)
            {
                StringBuilder error = new StringBuilder();
                error.Append("Error Compiling Expression: ");
                foreach (CompilerError err in cr.Errors)
                {
                    error.AppendFormat("{0}\n", err.ErrorText);
                }
                throw new Exception("Error Compiling Expression: " + error.ToString());
            }
            Assembly a = cr.CompiledAssembly;
            _Compiled = a.CreateInstance("JobsLib._Evaluator");
        }
        public int EvaluateInt(string name)
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
            MethodInfo mi = _Compiled.GetType().GetMethod(name);
            return mi.Invoke(_Compiled, null);
        }

        static public int EvaluateToInteger(string code)
        {
            Evaluator eval = new Evaluator(typeof(int), code, staticMethodName);
            return (int)eval.Evaluate(staticMethodName);
        }

        static public string EvaluateToString(string code)
        {
            Evaluator eval = new Evaluator(typeof(string), code, staticMethodName);
            return (string)eval.Evaluate(staticMethodName);
        }

        static public bool EvaluateToBool(string code)
        {
            Evaluator eval = new Evaluator(typeof(bool), code, staticMethodName);
            return (bool)eval.Evaluate(staticMethodName);
        }

        static public object EvaluateToObject(string code)
        {
            Evaluator eval = new Evaluator(typeof(object), code, staticMethodName);
            return eval.Evaluate(staticMethodName);
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
