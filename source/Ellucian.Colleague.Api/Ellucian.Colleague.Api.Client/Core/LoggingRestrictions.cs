// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;


namespace Ellucian.Colleague.Api.Client.Core
{
    /// <summary>
    /// Provides a list of logging flags for restricting client logging.  
    /// </summary>
    [Flags]
    public enum LoggingRestrictions
    {
        /// <summary>
        /// No restrictions.
        /// </summary>
        None = 0,
        /// <summary>
        /// Do not log the contents of the request.
        /// </summary>
        DoNotLogRequestContent = 1, 
        /// <summary>
        /// Do not log the contents of the response.
        /// </summary>
        DoNotLogResponseContent = 2,
    }
}
