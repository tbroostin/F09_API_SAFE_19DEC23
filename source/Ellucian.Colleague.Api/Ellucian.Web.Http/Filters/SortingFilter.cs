// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Http.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Ellucian.Web.Http.Extensions;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Linq.Expressions;
using Ellucian.Web.Http.Utilities;
using Ellucian.Web.Http.Exceptions;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using slf4net;


namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Action filter for Sorting
    /// </summary>
    public class SortingFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        private readonly ILogger logger;
        public ILogger Logger { get { return logger; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortingFilter"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public SortingFilter(ILogger logger)
        {
            this.logger = logger;
        }
        
        /// <summary>
        /// Sorts the list for all the sorting requests
        /// </summary>
        /// <param name="context">context</param>
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            bool isEnumerable = false;
            try
            {
                //Check for Response object NOT NULL
                if (context.Response != null)
                {
                    if (context.Response.Content != null && context.Response.Content is ObjectContent)
                    {
                        var objectContent = context.Response.Content as ObjectContent;
                        if (objectContent != null)
                        {
                            if (objectContent.ObjectType.IsGenericType) //Skip strings
                            {
                                //Check if ObjectContent is IEnumerable OR Implements IEnumerable
                                isEnumerable = objectContent.ObjectType.Name == "IEnumerable`1" || objectContent.ObjectType.GetInterface("IEnumerable`1") != null;

                                //Proceed with sorting only of return type is IEnumerable
                                if (isEnumerable)
                                {
                                    IEnumerable<object> model = null;
                                    context.Response.TryGetContentValue(out model);
                                    if (model != null)
                                    {
                                        List<Sorting> sortParams = context.Request.GetSortingParameters();
                                        //Check for Sort Expressions NOT NULL
                                        if ((sortParams != null) && (sortParams.Count > 0) && model.Count()>0)
                                        {
                                            model = SortCollection(model, sortParams);
                                            context.Response.Content = new ObjectContent<IEnumerable<dynamic>>(model, new JsonMediaTypeFormatter());

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                context.Response.SetErrorResponse(ex.Message);
            }
            base.OnActionExecuted(context);

        }

        /// <summary>
        /// Sorts the collection of objects in the list
        /// </summary>
        /// <param name="model">List to be sorted</param>
        /// <param name="sortParams">Sorting parameters</param>
        /// <returns></returns>
        public IEnumerable<object> SortCollection(IEnumerable<object> model, List<Sorting> sortParams)
        {
            IOrderedQueryable<object> sortedModel = null;
            try
            {
                Type modelType = model.FirstOrDefault().GetType();

                //Loop through the sort expressions for multiple value sort
                for (int i = 0; i < sortParams.Count; i++)
                {
                    if (sortParams[i].SortOrder.Equals(SortingOrder.Asc))
                    {
                        sortedModel = (i == 0) ? model.AsQueryable().OrderBy(ExpressionHelper.BuildSortExpression(modelType, sortParams[i].SortByField))
                          : sortedModel.ThenBy(ExpressionHelper.BuildSortExpression(modelType, sortParams[i].SortByField));
                    }
                    else
                    {
                        sortedModel = (i == 0) ? model.AsQueryable().OrderByDescending(ExpressionHelper.BuildSortExpression(modelType, sortParams[i].SortByField))
                                 : sortedModel.ThenByDescending(ExpressionHelper.BuildSortExpression(modelType, sortParams[i].SortByField));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return sortedModel;
        }
    }
}
