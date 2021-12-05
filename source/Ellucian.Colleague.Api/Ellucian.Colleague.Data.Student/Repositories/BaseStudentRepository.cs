// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BaseStudentRepository : BaseApiRepository
    {
        public BaseStudentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Retrieves the configuration information needed for registration processing asynchronously.
        /// </summary>
        public async Task<RegistrationConfiguration> GetRegistrationConfigurationAsync()
        {
            RegistrationConfiguration result = new RegistrationConfiguration(false, 0);
            RegistrationConfiguration configuration = await GetOrAddToCacheAsync<RegistrationConfiguration>("RegistrationConfiguration",
              async () =>
              {
                  Ellucian.Colleague.Data.Student.DataContracts.RegDefaults regDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.RegDefaults>("ST.PARMS", "REG.DEFAULTS");
                  Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                  if (regDefaults == null)
                  {
                      var errorMessage = "Unable to access registration defaults from ST.PARMS. REG.DEFAULTS. Default values will be assumed for purpose of building registration configuration in API." + Environment.NewLine
                      + "You can build a REG.DEFAULTS record by accessing the RGPD form in Colleague UI.";
                      logger.Info(errorMessage);
                      return result;
                  }
                  else
                  {
                      //Add Authorization is required only if the flag is Y or y.
                      bool requireAddAuthorization = (!string.IsNullOrEmpty(regDefaults.RgdRequireAddAuthFlag) && regDefaults.RgdRequireAddAuthFlag.ToUpper() == "Y");
                      int offsetDays = 0;
                      // Offset days are only applicable if require add authorization is true.
                      if (requireAddAuthorization)
                      {
                          offsetDays = regDefaults.RgdAddAuthStartOffset.HasValue ? regDefaults.RgdAddAuthStartOffset.Value : 0;
                      }
                      if (stwebDefaults == null)
                      {
                          var errorMessage = "Unable to access registration defaults from ST.PARMS. STWEB.DEFAULTS.";
                          logger.Info(errorMessage);
                          result = new RegistrationConfiguration(requireAddAuthorization, offsetDays);
                      }
                      else
                      {
                          var quickRegistrationIsEnabled = !string.IsNullOrEmpty(stwebDefaults.StwebEnableQuickReg) && stwebDefaults.StwebEnableQuickReg.ToUpper() == "Y" ? true : false;
                          result = new RegistrationConfiguration(requireAddAuthorization, offsetDays, quickRegistrationIsEnabled);
                          result.PromptForDropReason = !string.IsNullOrEmpty(stwebDefaults.StwebDropRsnPromptFlag) && stwebDefaults.StwebDropRsnPromptFlag.ToUpper() == "Y" ? true : false;
                          result.RequireDropReason = !string.IsNullOrEmpty(stwebDefaults.StwebDropRsnRequiredFlag) && stwebDefaults.StwebDropRsnRequiredFlag.ToUpper() == "Y" ? true : false;
                          result.ShowBooksOnPrintedSchedules = !string.IsNullOrEmpty(stwebDefaults.StwebShowBksOnSchedPrt) && stwebDefaults.StwebShowBksOnSchedPrt.ToUpper() == "Y" ? true : false;
                          result.ShowCommentsOnPrintedSchedules = !string.IsNullOrEmpty(stwebDefaults.StwebShowCmntOnSchedPrt) && stwebDefaults.StwebShowCmntOnSchedPrt.ToUpper() == "Y" ? true : false;
                          result.AddDefaultTermsToDegreePlan = !string.IsNullOrEmpty(stwebDefaults.StwebAddDfltTermsToDp) && stwebDefaults.StwebAddDfltTermsToDp.ToUpper() == "N" ? false : true;
                          if (stwebDefaults.StwebQuickRegTerms != null)
                          {
                              foreach (var termCode in stwebDefaults.StwebQuickRegTerms)
                              {
                                  try
                                  {
                                      result.AddQuickRegistrationTerm(termCode);
                                  }
                                  catch (Exception ex)
                                  {
                                      logger.Debug(ex, string.Format("Unable to add termCode '{0}' to list of quick registration terms.", termCode));
                                  }
                              }
                          }
                      }
                  }
                  return result;
              });
            return configuration;
        }
    }
}
