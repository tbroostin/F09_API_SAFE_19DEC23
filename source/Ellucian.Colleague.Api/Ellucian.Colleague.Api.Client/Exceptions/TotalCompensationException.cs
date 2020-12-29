/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client.Exceptions
{

    /// <summary>
    /// Represents an Exception that occurred while performing an action on a TotalCompensation Calculation.
    /// </summary>
    public class TotalCompensationException: Exception
    {
        #region Constructors
        public TotalCompensationException() 
            : base()
        {
        }

        public TotalCompensationException(string message)
            : base(message)
        {
        }
        public TotalCompensationException(TotalCompensationExceptionCodes code, Exception ex)
            : base(code.ToString(), ex)
        {

        }

        public TotalCompensationException(TotalCompensationExceptionCodes code)
            : base()
        {

        }
        #endregion
        /// <summary>
        /// Gets or sets the error code associated with this DegreePlanException. Further details can be
        /// relayed in the Message property.
        /// </summary>
        public TotalCompensationExceptionCodes Code { get; set; }
    }
}
