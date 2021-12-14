// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using System.Threading.Tasks;
using System.Text;
using System.CodeDom.Compiler;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using System.Reflection;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Services
{

    public static class CodeBuilderSupport
    {
        /// <summary>
        /// Build a unique cache key from a provided prefix/name and a list of criteria arguments
        /// </summary>
        /// <param name="prefix">Cache Key Prefix</param>
        /// <param name="arguments">List of arguments that effect the result set being cached</param>
        /// <returns>Cache Key string</returns>
        public static string BuildCacheKey(string prefix, params object[] arguments)
        {
            // Define the argument types and how to their values into a string
            var stringConversionFunctions = new Dictionary<Type, Func<object, string>> {
                { typeof(string), (object t) => { return t as string; } },
                { typeof(int), (object t) => { return t.ToString(); } },
                { typeof(float), (object t) => { return t.ToString(); } },
                { typeof(double), (object t) => { return t.ToString(); } },
                { typeof(DateTime), (object t) => { return t.ToString(); } },
                { typeof(DateTimeOffset), (object t) => { return t.ToString(); } },
                { typeof(bool), (object t) => { return t.ToString(); } },
                { typeof(List<string>), (object t) => { return string.Join(",", t as List<string>); } },
                { typeof(List<int>), (object t) => { return string.Join<int>(",", t as List<int>); } },
                { typeof(List<float>), (object t) => { return string.Join<float>(",", t as List<float>); } },
                { typeof(List<double>), (object t) => { return string.Join<double>(",", t as List<double>); } },
                { typeof(List<DateTime>), (object t) => { return string.Join<DateTime>(",", t as List<DateTime>); } },
                { typeof(List<DateTimeOffset>), (object t) => { return string.Join<DateTimeOffset>(",", t as List<DateTimeOffset>); } },
                { typeof(List<bool>), (object t) => { return string.Join<bool>(",", t as List<bool>); } },
                { typeof(string[]), (object t) => { return string.Join(",", t as string[]); } },
                { typeof(int[]), (object t) => { return string.Join<int>(",", t as int[]); } },
                { typeof(float[]), (object t) => { return string.Join<float>(",", t as float[]); } },
                { typeof(double[]), (object t) => { return string.Join<double>(",", t as double[]); } },
                { typeof(DateTime[]), (object t) => { return string.Join<DateTime>(",", t as DateTime[]); } },
                { typeof(DateTimeOffset[]), (object t) => { return string.Join<DateTimeOffset>(",", t as DateTimeOffset[]); } },
                { typeof(bool[]), (object t) => { return string.Join<bool>(",", t as bool[]); } },
                { typeof(List<Tuple<string, string>>), (object t) => {
                    List<Tuple<string,string>>list = t as List<Tuple<string, string>>;
                    return string.Join(",", list.Select(x => string.Format("{0}{1}", x.Item1, x.Item2)));
                } },
                { typeof(Dictionary<string,string>), (object t) => {
                    Dictionary<string,string> list = t as Dictionary<string, string>;
                    return string.Join(",", list.Select(x => string.Format("{0}{1}", x.Key, x.Value)));
                } },
            };

            // Build a list of argument strings
            List<string> args = new List<string>();
            if (arguments != null && arguments.Any())
            {
                foreach (var arg in arguments)
                {
                    Func<object, string> convertArgValueToString;
                    string argValue = "";
                    if (arg != null)
                    {
                        // Each non-null argument needs to be converted into a string consisting of its data
                        // Start by getting a conversion function that matches the argument's data type
                        if (stringConversionFunctions.TryGetValue(arg.GetType(), out convertArgValueToString))
                        {
                            // If we have a conversion function, use it to convert the data value(s) into a single string
                            argValue = convertArgValueToString(arg);
                        }
                        else
                        {
                            // If we throw an Argument Exception, developers making use of this function will get an error if they use it
                            // with arguments made up of unsupported data types - which can then be added to stringConversionFunctions
                            throw new ArgumentException(string.Format("CodeBuilderSupport.BuildCacheKey must be updated to support argument type {0}", arg.GetType().ToString()));
                        }
                    }
                    args.Add(argValue);
                }
            }

            // Join all the arguments into a single string which will identify the criteria of a specific resultset
            string argumentString = string.Join(";", args);
            // If the criteria string isn't empty, we can now get a unique hash code for the criteria string
            string hashCode = (string.IsNullOrEmpty(argumentString)) ? "" : argumentString.GetHashCode().ToString();
            // We can now finalize the cache key using the provided cache prefix and the hash code of the criteria
            string cacheKey = string.Concat(prefix, ":", hashCode);

            return cacheKey;
        }

        public static async Task<CodeBuilderObject> CodeBuilderAsync(
            BaseCachingRepository rep,
            Func<string, bool> ContainsKeyFunc,
            Func<string, Func<object>, double?, object> GetOrAddToCacheFunc,
            IColleagueTransactionInvoker transactionInvoker,
            IColleagueDataReader dataReader,
            string cacheName,
            bool bypassCache,
            double? cacheTimeout,
            CodeBuilderObject codeBuilderObject)
        {
            if (bypassCache && ContainsKeyFunc(rep.BuildFullCacheKey(cacheName)))
            {
                var codeCacheName = "sourceCode:" + codeBuilderObject.SourceCode.Trim(' ');
                var sourceCode = GetOrAddToCacheFunc(
                codeCacheName,
                () => {
                    rep.ClearCache(new List<string> { cacheName });
                    return codeBuilderObject.SourceCode;
                },
                cacheTimeout);
            }

            if (codeBuilderObject == null)
            {
                return null;
            }

            string codeSource = codeBuilderObject.SourceCode;
            object[] inputData = new object[] { codeBuilderObject, transactionInvoker, dataReader, rep, GetOrAddToCacheFunc, cacheTimeout, bypassCache };

            var instanceObject = GetOrAddToCacheFunc(
                cacheName,
                () => {
                    return BuildCompileCode(codeBuilderObject);
                },
                cacheTimeout);

            if (instanceObject == null)
            {
                return null;
            }

            Type t = instanceObject.GetType();
            MethodInfo mi = t.GetMethod("EvalCode");

            try
            {
                CodeBuilderObject results = mi.Invoke(instanceObject, inputData) as CodeBuilderObject;

                return results;
            }
            catch (Exception ex)
            {
                RepositoryException exception = new RepositoryException();
                exception.AddError(new RepositoryError("Global.Internal.Error", string.Format("Invokation Errors in CSharp Code Hook '{0}'.", ex.Message)));
                throw exception;
            }
        }

        public static string GetAssemblyFullPathByName(string name)
        {
            var referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyLocation = referencedAssemblies.Where(assembly => assembly.GetName().Name == name).Select(assembly => assembly.Location).FirstOrDefault();

            return assemblyLocation;
        }

        private static object BuildCompileCode(CodeBuilderObject codeBuilderObject)
        {
            string codeSource = codeBuilderObject.SourceCode;

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            CompilerParameters cp = new CompilerParameters();
            cp.GenerateInMemory = true;
            cp.TreatWarningsAsErrors = false;

            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");

            StringBuilder sb = new StringBuilder();
            sb.Append("using System;\n");
            sb.Append("using System.Text;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append("using System.Linq;\n");
            sb.Append("using System.Threading.Tasks;\n");
            sb.Append("using System.Collections.ObjectModel;\n");
            string assemblyPath = GetAssemblyFullPathByName("slf4net");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using slf4net;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Microsoft.Practices.EnterpriseLibrary");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Microsoft.Practices.EnterpriseLibrary.Common;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Colleague.Configuration");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Colleague.Configuration;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Colleague.Domain");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Colleague.Domain;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Microsoft.Practices.EnterpriseLibrary.Common.Utility");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Microsoft.Practices.EnterpriseLibrary.Common.Utility;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Data.Colleague");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Data.Colleague;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Dmi.Client");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Dmi.Client;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Dmi.Runtime");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Dmi.Runtime;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Colleague.Data.Base");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                //sb.Append("using Ellucian.Colleague.Data.Base;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Colleague.Data.Base");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Colleague.Data.Base.DataContracts;\n");
                sb.Append("using Ellucian.Colleague.Data.Base.Transactions;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Colleague.Domain.Base");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Colleague.Domain.Base.Entities;\n");
                sb.Append("using Ellucian.Colleague.Domain.Base.Exceptions;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Colleague.Domain.Base.Exceptions");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
            }
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Data.Colleague;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Data.Colleague.Repositories");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Data.Colleague.Repositories;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Data.Colleague.DataContracts");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Data.Colleague.DataContracts;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Web.Cache");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Web.Cache;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Web.Dependency");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                //sb.Append("using Ellucian.Web.Dependency;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Web.License");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Web.License;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Web.Security");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Web.Security;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Web.Utility");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Web.Utility;\n");
            }
            assemblyPath = GetAssemblyFullPathByName("Ellucian.Web.Http.Configuration");
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                cp.ReferencedAssemblies.Add(assemblyPath);
                sb.Append("using Ellucian.Web.Http.Configuration;\n");
            }

            sb.Append("namespace Ellucian.Colleague.Domain.Base.Services{ \n");
            sb.Append("public class CSCodeEvaler{ \n");
            sb.Append("char _VM = Convert.ToChar(DynamicArray.VM); \n");
            sb.Append("char _SM = Convert.ToChar(DynamicArray.SM); \n");
            sb.Append("char _TM = Convert.ToChar(DynamicArray.TM); \n");
            sb.Append("char _XM = Convert.ToChar(250); \n");
            sb.Append("public CodeBuilderObject EvalCode(CodeBuilderObject inputData, IColleagueTransactionInvoker transactionInvoker, IColleagueDataReader dataReader, BaseCachingRepository rep, Func<string, Func<object>, double?, object> GetOrAddToCacheFunc, double? cacheTimeOut, bool bypassCache){\n");
            sb.Append("var outputData = new CodeBuilderObject();\n");
            sb.Append("try{");
            sb.Append(codeSource + " \n");
            sb.Append("}catch (Exception ex){");
            sb.Append("outputData.ErrorFlag = true;");
            sb.Append("outputData.ErrorMessages.Add(ex.Message);}");
            sb.Append("return outputData;");
            sb.Append("} \n");
            sb.Append("} \n");
            sb.Append("}\n");

            CompilerResults compilerResults = provider.CompileAssemblyFromSource(cp, sb.ToString());
            if (compilerResults.Errors.Count > 0)
            {
                RepositoryException exception = new RepositoryException();
                foreach (var error in compilerResults.Errors)
                {
                    exception.AddError(new RepositoryError("Global.Internal.Error", string.Format("Compile Errors in CSharp Code Hook '{0}'.", error)));
                }
                throw exception;
            }
            Assembly assembly = compilerResults.CompiledAssembly;
            object instanceObject = assembly.CreateInstance("Ellucian.Colleague.Domain.Base.Services.CSCodeEvaler");

            return instanceObject;
        }
    }
}
