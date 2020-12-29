using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Planning.Exceptions
{
    public class NotAnAdvisorException : System.Exception
    {
        public NotAnAdvisorException()
        {

        }

        public NotAnAdvisorException(string message)
            : base(message)
        {

        }
    }
}
