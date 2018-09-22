//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Definition of the Loan request period
    /// </summary>
    [Serializable]
    public class LoanRequestPeriod
    {
        private readonly string _code;
        private readonly int _loanAmount;

        /// <summary>
        /// Loan period code
        /// </summary>
        public string Code { get { return _code; } }

        /// <summary>
        /// Loan period amount
        /// </summary>
        public int LoanAmount { get { return _loanAmount; } }

        /// <summary>
        /// LoanRequestPeriod constructor
        /// </summary>
        /// <param name="loanPeriodCode">loan period code</param>
        /// <param name="loanPeriodAmount"> loan period amount</param>
        public LoanRequestPeriod(string loanPeriodCode, int loanPeriodAmount)
        {
            if(string.IsNullOrEmpty(loanPeriodCode)){
                throw new ArgumentNullException("loanPeriodCode cannot be null.");
            }

            if (loanPeriodAmount < 0)
            {
                throw new ArgumentException("loanPeriodAmount must be greater or equal to 0.");
            }

            _code = loanPeriodCode;
            _loanAmount = loanPeriodAmount;

        }
    }
}
