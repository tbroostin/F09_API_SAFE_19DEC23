using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IAccountingStringRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AccountingStringRepository : BaseColleagueRepository, IAccountingStringRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public AccountingStringRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using Level 1 Cache Timeout Value for data that changes rarely.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Check if a single accounting code and project number are valid and return it
        /// </summary>
        /// <param name="accountingString">AccountingString domain entity</param>
        /// <returns>Single AccountingString if valid</returns>
        public async Task<Domain.ColleagueFinance.Entities.AccountingString> GetValidAccountingString(Domain.ColleagueFinance.Entities.AccountingString accountingString)
        {
            if (accountingString == null)
            {
                throw new ArgumentNullException("accountingString");
            }

            ValidateGlStringRequest validateGlStringRequest = new ValidateGlStringRequest()
            {
                GlAcctId = accountingString.AccountString,
                GlDate = accountingString.ValidOn,
                ProjectId = accountingString.ProjectNumber
            };

            var validateGlStringResponse =
                await transactionInvoker.ExecuteAsync<ValidateGlStringRequest, ValidateGlStringResponse>(
                        validateGlStringRequest);

            if (validateGlStringResponse.ErrorMessages.Any())
            {
                var sb = new StringBuilder();

                validateGlStringResponse.ErrorMessages.ForEach(s =>
                {
                    sb.Append(s);
                    sb.AppendLine();
                });

                logger.Error(sb.ToString());
                throw new RepositoryException(sb.ToString());
            }

            var returnEntity = accountingString;
            returnEntity.Description = validateGlStringResponse.Description;

            return returnEntity;

        }
    }
}