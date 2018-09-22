// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Extensions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Web.Http.ModelBinding
{
    /// <summary>
    /// Model binder for Paging model
    /// </summary>
    public class PagingBinder : IModelBinder
    {
        /// <summary>
        /// Binds the limit and offset values from querystring to Paging class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext context, ModelBindingContext bindingContext)
        {
            PagingFilter filters = (PagingFilter)context.ActionDescriptor.GetFilters().FirstOrDefault(x=>x.GetType() == typeof(PagingFilter));
            if (filters != null)
            {
                Paging setPagingParams = context.Request.GetPagingParameters(filters.DefaultLimit);
                if (setPagingParams != null)
                {
                    setPagingParams.DefaultLimit = filters.DefaultLimit;
                }
                bindingContext.Model = setPagingParams;
            }
            return true;
        }
    }
}
