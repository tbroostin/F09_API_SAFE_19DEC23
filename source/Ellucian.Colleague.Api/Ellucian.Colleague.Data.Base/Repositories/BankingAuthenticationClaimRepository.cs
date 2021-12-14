using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BankingAuthenticationClaimRepository : BaseColleagueRepository, IBankingAuthenticationClaimRepository
    {
        private string colleagueTimeZone;

        public BankingAuthenticationClaimRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
        }



        /// <summary>
        /// For the given token, get a BankingAuthenticationToken object.
        /// </summary>
        /// <param name="token">The GUID key of the BankingAuthenticationToken</param>
        /// <returns>A BankingAuthenticationToken object</returns>
        public async Task<BankingAuthenticationToken> Get(Guid token)
        {
            //the GUID token is the key of the BankingAuthClaims table
            var claimsRecord = await DataReader.ReadRecordAsync<BankingAuthClaims>(token.ToString());

            //throw an key not found if the record is null
            if (claimsRecord == null)
            {
                throw new KeyNotFoundException(string.Format("BankingAuthenticationToken id does not exist {0}", token));
            }

            //throw an exception if the expiration date and time don't have value
            if (!claimsRecord.BacExpirationDate.HasValue || !claimsRecord.BacExpirationTime.HasValue)
            {
                LogDataError("BankingAuthClaims", claimsRecord.Recordkey, claimsRecord, null, "expiration date and time are required");
                throw new ApplicationException("Claims record is corrupted. Expiration date and time are required");
            }

            //convert colleague date and time to DateTimeOffset
            var expiration = claimsRecord.BacExpirationTime.ToPointInTimeDateTimeOffset(claimsRecord.BacExpirationDate, colleagueTimeZone);

            //throw an exception if that conversion fails for some unknown reason
            if (!expiration.HasValue)
            {
                throw new ApplicationException(string.Format("Unable to parse the expiration dateTimeOffset from the date and time - {0}, {1}", claimsRecord.BacExpirationDate.Value, claimsRecord.BacExpirationTime.Value));
            }

            //try to parse the token string into a GUID. throw an exception if the parse fails
            Guid parsedToken;
            if (!Guid.TryParse(claimsRecord.Recordkey, out parsedToken))
            {
                LogDataError("BankingAuthClaims", claimsRecord.Recordkey, claimsRecord, null, "Unable to parse the GUID Token");
                throw new ApplicationException(string.Format("Unable to parse the GUID Token {0}", claimsRecord.Recordkey));
            }

            //cleanup the database by deleting records that expired yesterday
            var today = await GetUnidataFormatDateAsync(DateTime.Now);
            var criteria = string.Format("WITH BAC.EXPIRATION.DATE LT '{0}'", today);
            var expiredTokens = await DataReader.SelectAsync("BANKING.AUTH.CLAIMS", criteria);
            if (expiredTokens != null && expiredTokens.Any())
            {

                var request = new DeleteBankingAuthClaimsRequest()
                {
                    RecordIds = expiredTokens.ToList()
                };

                //this should only be executed on the first authentication request of the day
                await transactionInvoker.ExecuteAsync<DeleteBankingAuthClaimsRequest, DeleteBankingAuthClaimsResponse>(request);

            }

            return new BankingAuthenticationToken(expiration.Value, parsedToken);
        }
    }
}
