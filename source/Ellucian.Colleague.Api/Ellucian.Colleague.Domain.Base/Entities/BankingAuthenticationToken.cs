using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class BankingAuthenticationToken
    {
        /// <summary>
        /// The exact Date and Time that this token expires. After expiration, consumers must reauthorize.
        /// </summary>
        public DateTimeOffset ExpirationDateTimeOffset { get; private set; }

        /// <summary>
        /// The token that will accompany a request to update, create or delete banking deposit directives. 
        /// </summary>
        public Guid Token { get; private set; }

        /// <summary>
        /// Construct a BankingAuthenticationToken with the given expiration date and time and the GUID token
        /// </summary>
        /// <param name="expiration"></param>
        /// <param name="token"></param>
        public BankingAuthenticationToken(DateTimeOffset expiration, Guid token)
        {
            ExpirationDateTimeOffset = expiration;
            Token = token;
        }
    }
}
