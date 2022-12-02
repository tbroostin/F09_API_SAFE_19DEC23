// Copyright 2022 Ellucian Company L.P. and its affiliates.using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Web.Http.Configuration
{
    /// <summary>
    /// Colleague Web API DTO Exception base class
    /// </summary>
    public class ColleagueWebApiDtoException : Exception
    {
        public ColleagueWebApiDtoException(string message) : base(message)
        {
        }

        public ColleagueWebApiDtoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ColleagueWebApiDtoException()

        {
        }
    }
}
