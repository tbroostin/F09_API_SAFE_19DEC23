// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Provides a filter capable of decoding action method parameters which have been encoded
    /// using a 'character to literal string' technique to allow certain characters to be accepted
    /// by ASP.NET when they would otherwise be rejected. See <see cref="UrlParameterUtility.DecodeWithSubstitution"/>
    /// for a list of supported character substitution strings.
    /// </summary>
    public class ParameterSubstitutionFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        /// <summary>
        /// Optional list of parameter names to limit this filter too.
        /// </summary>
        public string[] ParameterNames { get; set; }

        /// <summary>
        /// Performs the parameter substitution
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);
            if (actionContext != null && actionContext.ActionArguments != null && actionContext.ActionArguments.Count > 0)
            {
                // loop over any parameters, perform the substitution, and store modified parameters for updating outside of this loop
                Dictionary<string, object> modifiedActionArguments = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> actionArgument in actionContext.ActionArguments)
                {
                    // only applicable to string parameters
                    if (actionArgument.Value != null && actionArgument.Value.GetType() == typeof(string))
                    {
                        // filter can work from a list or apply to all string parameters
                        if (ParameterNames != null && ParameterNames.Count() > 0)
                        {
                            // perform substitution of only those parameters specified by the list
                            if (ParameterNames.Contains(actionArgument.Key))
                            {
                                modifiedActionArguments.Add(actionArgument.Key, UrlParameterUtility.DecodeWithSubstitution(actionArgument.Value as string));
                            }
                        }
                        else
                        {
                            // perform substitution on all parameters
                            modifiedActionArguments.Add(actionArgument.Key, UrlParameterUtility.DecodeWithSubstitution(actionArgument.Value as string));
                        }
                    }
                }

                // update the actionContext's parameters if any were modified above
                if (modifiedActionArguments.Count > 0)
                {
                    foreach (KeyValuePair<string, object> argument in modifiedActionArguments)
                    {
                        actionContext.ActionArguments[argument.Key] = argument.Value;
                    }
                }
            }
        }
    }
}
