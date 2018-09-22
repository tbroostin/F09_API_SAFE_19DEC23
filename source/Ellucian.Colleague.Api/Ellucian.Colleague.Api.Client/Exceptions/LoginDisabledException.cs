using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Api.Client.Exceptions
{
    public class LoginDisabledException : Exception
    {
        public LoginDisabledException() 
            : base()
        {
        }

        public LoginDisabledException(string message)
            : base(message)
        {
        }
    }
}
