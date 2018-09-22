using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Data.Base
{
    /// <summary>
    /// Central registry of mappings between "primary views" in Colleague rules, and the classes that
    /// represent those views on the .NET side.
    /// </summary>
    public class RuleAdapterRegistry
    {
        private readonly Dictionary<string, IRuleAdapter> registry = new Dictionary<string, IRuleAdapter>();

        /// <summary>
        /// Registers an adapter for translating from the specified view to a strongly typed .NET rule object.
        /// </summary>
        /// <typeparam name="T">the C# type representing the primary view</typeparam>
        /// <param name="primaryView">the primary view file name from Colleague</param>
        /// <param name="adapter">the adapter to register, must not be null</param>
        public void Register<T>(string primaryView, IRuleAdapter adapter)
        {
            if (adapter == null)
            {
                throw new ArgumentNullException("adapter");
            }
            registry[primaryView] = adapter;
        }

        /// <summary>
        /// Returns an adapter, provided the Envision side primary view name.
        /// </summary>
        /// <param name="primaryView"></param>
        /// <returns></returns>
        public IRuleAdapter Get(string primaryView)
        {
            if (registry.ContainsKey(primaryView))
            {
                return registry[primaryView];
            }
            return new UnsupportedRuleAdapter();
        }

        /// <summary>
        /// Returns an adapter, provided the .NET side type.
        /// </summary>
        /// <param name="contextType"></param>
        /// <returns>the adapter, or null if none found</returns>
        public IRuleAdapter Get(Type contextType)
        {
            foreach (var adapter in registry.Values)
            {
                if (adapter.ContextType == contextType)
                {
                    return adapter;
                }
            }
            return null;
        }
    }
}
