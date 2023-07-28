// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
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
            RegistrationConfiguration result = new RegistrationConfiguration(false, false, false, 0);
            RegistrationConfiguration configuration = await GetOrAddToCacheAsync<RegistrationConfiguration>("RegistrationConfiguration",
              async () =>
              {
                  Ellucian.Colleague.Data.Student.DataContracts.RegDefaults regDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.RegDefaults>("ST.PARMS", "REG.DEFAULTS");
                  Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                  Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2 stwebDefaults2 = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2>("ST.PARMS", "STWEB.DEFAULTS.2");
                  if (regDefaults == null)
                  {
                      var errorMessage = "Unable to access registration defaults from ST.PARMS. REG.DEFAULTS. Default values will be assumed for purpose of building registration configuration in API." + Environment.NewLine
                      + "You can build a REG.DEFAULTS record by accessing the RGPD form in Colleague UI.";
                      logger.Info(errorMessage);
                      return result;
                  }
                  else
                  {
                      //Seat Service is only used when the flag is Y or y.
                      var seatServiceIsEnabled = !string.IsNullOrEmpty(regDefaults.RgdSeatServiceEnabled) && regDefaults.RgdSeatServiceEnabled.ToUpper() == "Y" ? true : false;

                      //Add Authorization is required only if the flag is Y or y.
                      bool requireAddAuthorization = (!string.IsNullOrEmpty(regDefaults.RgdRequireAddAuthFlag) && regDefaults.RgdRequireAddAuthFlag.ToUpper() == "Y");
                      bool exceedAddAuthCapacity = (!string.IsNullOrEmpty(regDefaults.RgdAddAuthExceedCapacity) && regDefaults.RgdAddAuthExceedCapacity.ToUpper() == "Y");
                      bool bypassAddAuthWaitlist = (!string.IsNullOrEmpty(regDefaults.RgdBypassAddAuthWaitlist) && regDefaults.RgdBypassAddAuthWaitlist.ToUpper() == "Y");
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
                          result = new RegistrationConfiguration(requireAddAuthorization, exceedAddAuthCapacity, bypassAddAuthWaitlist, offsetDays, seatServiceIsEnabled: seatServiceIsEnabled);
                      }
                      else
                      {
                          var quickRegistrationIsEnabled = !string.IsNullOrEmpty(stwebDefaults.StwebEnableQuickReg) && stwebDefaults.StwebEnableQuickReg.ToUpper() == "Y" ? true : false;
                          result = new RegistrationConfiguration(requireAddAuthorization, exceedAddAuthCapacity, bypassAddAuthWaitlist, offsetDays, quickRegistrationIsEnabled, seatServiceIsEnabled: seatServiceIsEnabled);
                          result.PromptForDropReason = !string.IsNullOrEmpty(stwebDefaults.StwebDropRsnPromptFlag) && stwebDefaults.StwebDropRsnPromptFlag.ToUpper() == "Y" ? true : false;
                          result.RequireDropReason = !string.IsNullOrEmpty(stwebDefaults.StwebDropRsnRequiredFlag) && stwebDefaults.StwebDropRsnRequiredFlag.ToUpper() == "Y" ? true : false;
                          result.ShowBooksOnPrintedSchedules = !string.IsNullOrEmpty(stwebDefaults.StwebShowBksOnSchedPrt) && stwebDefaults.StwebShowBksOnSchedPrt.ToUpper() == "Y" ? true : false;
                          result.ShowCommentsOnPrintedSchedules = !string.IsNullOrEmpty(stwebDefaults.StwebShowCmntOnSchedPrt) && stwebDefaults.StwebShowCmntOnSchedPrt.ToUpper() == "Y" ? true : false;
                          result.AddDefaultTermsToDegreePlan = !string.IsNullOrEmpty(stwebDefaults.StwebAddDfltTermsToDp) && stwebDefaults.StwebAddDfltTermsToDp.ToUpper() == "N" ? false : true;
                          result.AllowFacultyAddAuthFromWaitlist = (!string.IsNullOrEmpty(regDefaults.RgdAllowAddAuthWaitlist) && regDefaults.RgdAllowAddAuthWaitlist.ToUpper() == "Y");
                          if (stwebDefaults.StwebQuickRegTerms != null)
                          {
                              foreach (var termCode in stwebDefaults.StwebQuickRegTerms)
                              {
                                  try
                                  {
                                      result.AddQuickRegistrationTerm(termCode);
                                  }
                                  catch (Ellucian.Data.Colleague.Exceptions.ColleagueSessionExpiredException)
                                  {
                                      throw;
                                  }
                                  catch (Exception ex)
                                  {
                                      logger.Debug(ex, string.Format("Unable to add termCode '{0}' to list of quick registration terms.", termCode));
                                  }
                              }
                          }
                      }
                      if (stwebDefaults2 == null)
                      {
                          var errorMessage = "Unable to access registration defaults from ST.PARMS. STWEB.DEFAULTS.2";
                          logger.Info(errorMessage);
                          result.AlwaysPromptUsersForIntentToWithdrawWhenDropping = false;
                          result.CensusDateNumberForPromptingIntentToWithdraw = null;
                      }
                      else
                      {
                          result.AlwaysPromptUsersForIntentToWithdrawWhenDropping = !string.IsNullOrEmpty(stwebDefaults2.Stweb2IntToWdrlAlways) && stwebDefaults2.Stweb2IntToWdrlAlways.ToUpper() == "Y" ? true : false;
                          result.CensusDateNumberForPromptingIntentToWithdraw = stwebDefaults2.Stweb2IntToWdrlCensus;
                      }
                  }
                  return result;
              });
            return configuration;
        }

        /// <summary>
        /// Retrieves course section availability information configuration
        /// </summary>
        /// <returns><see cref="SectionAvailabilityInformationConfiguration"/></returns>
        public async Task<SectionAvailabilityInformationConfiguration> GetSectionAvailabilityInformationConfigurationAsync()
        {
            SectionAvailabilityInformationConfiguration result = new SectionAvailabilityInformationConfiguration(false, false);
            SectionAvailabilityInformationConfiguration configuration = await GetOrAddToCacheAsync<SectionAvailabilityInformationConfiguration>("SectionAvailabilityInformationConfiguration",
              async () =>
              {
                  Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                  if (stwebDefaults == null)
                  {
                      var errorMessage = "Unable to access section availability information settings from ST.PARMS. STWEB.DEFAULTS.";
                      logger.Info(errorMessage);
                  }
                  else
                  {
                      bool showNegativeSeatCounts = !string.IsNullOrEmpty(stwebDefaults.StwebShowNegativeSeats) && stwebDefaults.StwebShowNegativeSeats.ToUpper() == "Y" ? true : false;
                      bool includeSeatsTakenInAvailabilityInformation = !string.IsNullOrEmpty(stwebDefaults.StwebUseAtcwFormat) && stwebDefaults.StwebUseAtcwFormat.ToUpper() == "Y" ? true : false;
                      result = new SectionAvailabilityInformationConfiguration(showNegativeSeatCounts, includeSeatsTakenInAvailabilityInformation);
                  }
                  return result;
              });
            return configuration;
        }
    }
}
