// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Web.Http.Exceptions
{
    /// <summary>
    /// Colleague Web API Exception base class
    /// </summary>
    public class ColleagueWebApiException : Exception
    {
        public ColleagueWebApiException(string message) : base(message)
        {
        }

        public ColleagueWebApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ColleagueWebApiException()
        {
        }
    }
}
