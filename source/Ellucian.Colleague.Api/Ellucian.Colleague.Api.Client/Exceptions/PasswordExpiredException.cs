using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Api.Client.Exceptions
{
    public class PasswordExpiredException : Exception
    {
        public PasswordExpiredException() 
            : base()
        {
        }

        public PasswordExpiredException(string message)
            : base(message)
        {
        }
    }
}
