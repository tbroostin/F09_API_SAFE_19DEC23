// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Utility;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RegistrationPriorityRepository : BaseColleagueRepository, IRegistrationPriorityRepository
    {
        private readonly string colleagueTimeZone;

        public RegistrationPriorityRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {

            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Retrieve all registration priorities for a student
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All registration priority items for the student.  If none an empty list is returned.</returns>
        public async Task<IEnumerable<RegistrationPriority>> GetAsync(string studentId)
        {
            IEnumerable<RegistrationPriority> priorities = new List<RegistrationPriority>();
            try
            {
                var queryString = "WITH RGPR.STUDENT EQ '" + studentId + "'";
                string[] regPrioritiesIds = await DataReader.SelectAsync("REG.PRIORITIES", queryString);
                regPrioritiesIds = regPrioritiesIds.ToList().Where(p => !string.IsNullOrEmpty(p)).Distinct().ToArray();
                if (regPrioritiesIds != null && regPrioritiesIds.Count() > 0)
                {
                    Collection<RegPriorities> regPriorities =await DataReader.BulkReadRecordAsync<RegPriorities>("REG.PRIORITIES", regPrioritiesIds);
                    // If not all of the ids can be found log an error - but keep going.
                    if (regPrioritiesIds.Count() != regPriorities.Count)
                    {
                        logger.Info("Warning: Unable to retrieve all of the registration priorities for " + studentId);
                    }

                    priorities = BuildRegistrationPriorities(regPriorities.ToList());
                }
            }
            catch (Exception ex)
            {
                logger.Info("Error occurred processing selected reg priorities for student " + studentId + ": " + ex.Message);
            }
            return priorities;
        }

        private IEnumerable<RegistrationPriority> BuildRegistrationPriorities(List<RegPriorities> regPrioritiesData)
        {
            var regPriorities = new List<RegistrationPriority>();
            if (regPrioritiesData != null && regPrioritiesData.Count() > 0)
            {
                foreach (var regPriority in regPrioritiesData)
                {
                    try
                    {
                        DateTimeOffset? start = null;
                        DateTimeOffset? end = null;
                        if (regPriority.RgprStartDate != null)
                        {
                            //Is the time specified?
                            if (regPriority.RgprStartTime != null)
                            {
                                start = regPriority.RgprStartTime.ToPointInTimeDateTimeOffset(regPriority.RgprStartDate, colleagueTimeZone);
                            }
                                //Time was not specified - this will default to 12am on the date
                            else
                            {
                                start = regPriority.RgprStartDate.ToPointInTimeDateTimeOffset(regPriority.RgprStartDate, colleagueTimeZone);
                            }
                        }
                        
                        if (regPriority.RgprEndTime != null && regPriority.RgprEndDate != null)
                        {
                            end = regPriority.RgprEndTime.ToPointInTimeDateTimeOffset(regPriority.RgprEndDate, colleagueTimeZone);
                        }
                        RegistrationPriority registrationPriority = new RegistrationPriority(regPriority.Recordkey, regPriority.RgprStudent, regPriority.RgprTerm, start, end);
                        regPriorities.Add(registrationPriority);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                        var inString = "Registration Priority Id: " + regPriority.Recordkey + ", Student Id: " + regPriority.RgprStudent + ", TermCode: " + regPriority.RgprTerm;
                        LogDataError("Registration Priority", regPriority.Recordkey, regPriority, e, inString);                        
                    }
                }
            }
            return regPriorities;
        }
    }
}
