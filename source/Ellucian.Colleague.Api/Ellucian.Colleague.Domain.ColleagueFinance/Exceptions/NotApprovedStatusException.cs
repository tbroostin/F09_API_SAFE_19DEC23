// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Exceptions
{
    public class NotApprovedStatusException : Exception
    {
        public NotApprovedStatusException()
            : base()
        {

        }

        public NotApprovedStatusException(string message)
            : base(message)
        {

        }
    }
}
