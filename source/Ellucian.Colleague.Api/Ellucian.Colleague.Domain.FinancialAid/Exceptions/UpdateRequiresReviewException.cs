using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Exceptions
{
    public class UpdateRequiresReviewException : Exception
    {
        public UpdateRequiresReviewException()
            : base()
        {

        }

        public UpdateRequiresReviewException(string message)
            : base(message)
        {

        }
    }
}
