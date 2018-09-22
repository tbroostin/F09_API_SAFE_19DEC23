using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Exceptions
{
    public class BankingAuthenticationException : Exception
    {
        public string DepositDirectiveId { get; private set; }

        public override string Message
        {
            get
            {
                return string.Format("Authentication denied for deposit directive {0}\n{1}", DepositDirectiveId, base.Message);
            }
        }

        public BankingAuthenticationException(string depositDirectiveId, string message)
            : base(message)
        {
            DepositDirectiveId = depositDirectiveId;
        }

    }
}
