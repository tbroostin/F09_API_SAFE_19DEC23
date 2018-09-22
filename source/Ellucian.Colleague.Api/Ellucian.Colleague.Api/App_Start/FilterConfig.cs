// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Web.Http.Filters;
using System.Web.Mvc;
using Ellucian.Web.Http.Filters;
using slf4net;


namespace Ellucian.Colleague.Api
{
    /// <summary>
    /// Provides global API filter configuration helpers.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Registers the global API filters.
        /// </summary>
        /// <param name="filters"></param>
        public static void RegisterGlobalFilters(HttpFilterCollection filters)
        {
            filters.Add(new LoggingExceptionFilter(DependencyResolver.Current.GetService<ILogger>()));
            //OnActionExecuted filters run in reverse order
            filters.Add(new PagingFilter());
            filters.Add(new SortingFilter(DependencyResolver.Current.GetService<ILogger>()));
            filters.Add(new FilteringFilter(DependencyResolver.Current.GetService<ILogger>()));
            filters.Add(new CustomMediaTypeAttributeFilter());
        }
    }
}