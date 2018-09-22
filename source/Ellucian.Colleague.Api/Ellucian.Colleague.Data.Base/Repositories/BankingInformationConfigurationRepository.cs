/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Banking Information Configruation Repository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BankingInformationConfigurationRepository : BaseColleagueRepository, IBankingInformationConfigurationRepository
    {
        #region FIELDS

        private const string BankingInformationConfigurationCacheKey = "BankingInformationConfiguration";

        #endregion

        #region CONSTRUCTOR(S)
        /// <summary>
        /// Instantiate a new banking information configuration repository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        /// <param name="webRequestHelper"></param>
        public BankingInformationConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {

        }
        #endregion

        #region GET CONFIGURATION
        public async Task<BankingInformationConfiguration> GetBankingInformationConfigurationAsync()
        {
            return await GetOrAddToCacheAsync<BankingInformationConfiguration>(BankingInformationConfigurationCacheKey, async () => await this.BuildConfiguration(), Level1CacheTimeoutValue);
        }

        private async Task<BankingInformationConfiguration> BuildConfiguration()
        {
            var parameters = await DataReader.ReadRecordAsync<BankInfoParms>("CORE.PARMS", "BANK.INFO.PARMS");


            if (parameters == null)
            {
                var message = "Unable to find BankInfoParams record";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var paraNames = new string[3]{
                parameters.BipTermsPara,
                parameters.BipPrMessagePara,
                parameters.BipPrEffectDatePara
            };

            var configuration = new BankingInformationConfiguration();
            var paragraphCollection = await DataReader.BulkReadRecordAsync<DocPara>(paraNames, true);

            if (paragraphCollection == null)
            {
                LogDataError("DocPara", null, paraNames, null, "Null Collection returned from data reader");
                paragraphCollection = new Collection<DocPara>();
            }

            if (!string.IsNullOrWhiteSpace(parameters.BipTermsPara))
            {
                var termsAndConditionsPara = paragraphCollection.FirstOrDefault(p => p.Recordkey == parameters.BipTermsPara);
                if (termsAndConditionsPara == null)
                {
                    var message = "Unable to find referenced document in DocPara from BankInfoParams record code";
                    LogDataError("DocPara", parameters.BipTermsPara, paragraphCollection, null, message);
                }
                else
                {
                    configuration.AddEditAccountTermsAndConditions = termsAndConditionsPara.ParaText;
                }
            }

            if (!string.IsNullOrWhiteSpace(parameters.BipPrMessagePara))
            {
                var messagePara = paragraphCollection.FirstOrDefault(p => p.Recordkey == parameters.BipPrMessagePara);
                if (messagePara == null)
                {
                    var message = "Unable to find referenced document in DocPara from BankInfoParams record code";
                    LogDataError("DocPara", parameters.BipPrMessagePara, null, null, message);
                }
                else
                {
                    configuration.PayrollMessage = messagePara.ParaText;
                }
            }

            if (!string.IsNullOrWhiteSpace(parameters.BipPrEffectDatePara))
            {
                var effectiveDatePara = paragraphCollection.FirstOrDefault(p => p.Recordkey == parameters.BipPrEffectDatePara);
                if (effectiveDatePara == null)
                {
                    var message = "Unable to find referenced document in DocPara from BankInfoParams record code";
                    LogDataError("DocPara", parameters.BipPrEffectDatePara, null, null, message);
                }
                else
                {
                    configuration.PayrollEffectiveDateMessage = effectiveDatePara.ParaText;
                }
            }

            configuration.IsRemainderAccountRequired = parameters.BipDirDepRequired != null && parameters.BipDirDepRequired.Equals("Y", System.StringComparison.CurrentCultureIgnoreCase);
            configuration.UseFederalRoutingDirectory = parameters.BipUseFedRoutingDir != null && parameters.BipUseFedRoutingDir.Equals("Y", System.StringComparison.CurrentCultureIgnoreCase);
            configuration.IsDirectDepositEnabled = parameters.BipPrDepositEnabled != null && parameters.BipPrDepositEnabled.Equals("Y", System.StringComparison.CurrentCultureIgnoreCase);
            configuration.IsPayableDepositEnabled = parameters.BipPayableDepositEnabled != null && parameters.BipPayableDepositEnabled.Equals("Y", System.StringComparison.CurrentCultureIgnoreCase);
            configuration.IsAccountAuthenticationDisabled = parameters.BipAcctAuthDisabled != null && parameters.BipAcctAuthDisabled.Equals("Y", System.StringComparison.CurrentCultureIgnoreCase);

            return configuration;
        }
        #endregion
    }
}
