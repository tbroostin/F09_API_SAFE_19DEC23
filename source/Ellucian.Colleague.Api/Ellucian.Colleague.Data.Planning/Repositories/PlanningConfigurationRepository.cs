// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Planning.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PlanningConfigurationRepository : BaseColleagueRepository, IPlanningConfigurationRepository
    {
        public PlanningConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Retrieves Student Planning configuration data
        /// </summary>
        /// <returns>A <see cref="PlanningConfiguration">Student Planning Configuration</see> object</returns>
        public async Task<PlanningConfiguration> GetPlanningConfigurationAsync()
        {
            PlanningConfiguration planningConfiguration =await GetOrAddToCacheAsync<PlanningConfiguration>("PlanningConfiguration",
                async () =>
                {
                    var planningConfig = new PlanningConfiguration();
                    Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                    if (stwebDefaults == null)
                    {
                        var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    planningConfig.DefaultCurriculumTrack = stwebDefaults.StwebDefaultCtk;
                    planningConfig.ShowAdvisementCompleteWorkflow = (!string.IsNullOrEmpty(stwebDefaults.StwebShowAdviseComplete) && stwebDefaults.StwebShowAdviseComplete.ToUpperInvariant() == "Y");
                    planningConfig.AdviseByOfficeFlag = (!string.IsNullOrEmpty(stwebDefaults.StwebAdviseByOfficeFlag) && stwebDefaults.StwebAdviseByOfficeFlag.ToUpperInvariant() == "Y");
                    if (stwebDefaults.StwebCatalogYearPolicy == "1")
                    {
                        planningConfig.DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear;
                    }
                    else
                    {
                        planningConfig.DefaultCatalogPolicy = CatalogPolicy.CurrentCatalogYear;
                    }
                    planningConfig.OpenOfficeAdvisors = stwebDefaults.StwebOpenOfficeAdvisors;
                    return planningConfig;
                });
            return planningConfiguration;
        }
    }
}
