/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Dependency;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using slf4net;
using System;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BenefitsEnrollmentConfigurationRepository : BaseColleagueRepository, IBenefitsEnrollmentConfigurationRepository
    {
        public BenefitsEnrollmentConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }
        /// <summary>
        /// Builds the BenefitsEnrollmentConfiguration from HRSS.DEFAULTS File
        /// </summary>
        /// <returns>BenefitsEnrollmentConfiguration object</returns>
        public async Task<BenefitsEnrollmentConfiguration> GetBenefitsEnrollmentConfigurationAsync()
        {

            return await GetOrAddToCacheAsync<BenefitsEnrollmentConfiguration>("BenefitsEnrollmentConfiguration",
            async () =>
            {
                return await GetBenefitsEnrollmentConfiguration();
            });
        }

        private async Task<BenefitsEnrollmentConfiguration> GetBenefitsEnrollmentConfiguration()
        {

            var benefitsEnrollmentConfiguration = new BenefitsEnrollmentConfiguration();
            var hrwebDefaults = await DataReader.ReadRecordAsync<DataContracts.HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
            if (hrwebDefaults != null)
            {
                benefitsEnrollmentConfiguration.RelationshipTypes = hrwebDefaults.HrwebLimitRelationTypes;
            }
            else
            {
                logger.Info("Null HrwebDefaults record returned from database");
            }

            var hrssDefaults = await DataReader.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS");
            if (hrwebDefaults != null)
            {
                benefitsEnrollmentConfiguration.IsBenefitsEnrollmentEnabled = !string.IsNullOrEmpty(hrssDefaults.HrssEnableBenefitsEnrlmnt) && hrssDefaults.HrssEnableBenefitsEnrlmnt.Equals("Y", StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                logger.Info("Null HrssDefaults record returned from database");
            }

            return benefitsEnrollmentConfiguration;
        }
    }
}
