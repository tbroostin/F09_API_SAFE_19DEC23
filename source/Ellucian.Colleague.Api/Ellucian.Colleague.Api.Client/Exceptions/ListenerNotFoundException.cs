using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Api.Client.Exceptions
{
    public class ListenerNotFoundException : Exception
    {
        public ListenerNotFoundException()
            : base()
        {
        }

        public ListenerNotFoundException(string message)
            : base(message)
        {
        }
    }
}
