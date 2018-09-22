using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IBankingAuthenticationClaimRepository
    {
        Task<BankingAuthenticationToken> Get(Guid token);

       // Task Delete(Guid token);


    }
}
