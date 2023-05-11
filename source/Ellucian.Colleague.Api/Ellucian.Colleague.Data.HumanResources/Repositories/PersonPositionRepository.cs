/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonPositionRepository : BaseColleagueRepository, IPersonPositionRepository
    {
        private readonly int bulkReadSize;
        private readonly string emptimeHistoryCacheKey;
        public PersonPositionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
               : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
            emptimeHistoryCacheKey = "EmptimeHistory_";
        }

        /// <summary>
        /// Get PersonPosition objects for the given personIds
        /// </summary>
        /// <param name="personIds">Required: A list of personIds for which to get PersonPositions</param>  
        /// <param name="lookupStartDate">optional lookup start date for look up filtering, 
        /// all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(IEnumerable<string> personIds, DateTime? lookupStartDate = null)
        {
            if (personIds == null)
            {
                throw new ArgumentNullException("personIds");
            }
            if (!personIds.Any())
            {
                throw new ArgumentException("personIds is required to get PersonPositions", "personIds");
            }

            var criteria = "WITH PERPOS.HRP.ID EQ ?";
            if (lookupStartDate.HasValue)
            {
                criteria += " AND (PERPOS.END.DATE GE '" + UniDataFormatter.UnidataFormatDate(lookupStartDate.Value, InternationalParameters.HostShortDateFormat, InternationalParameters.HostDateDelimiter)
                    + "' OR PERPOS.END.DATE EQ '')";
            }
            var perposKeys = await DataReader.SelectAsync("PERPOS", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

            if (perposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!perposKeys.Any())
            {
                logger.Error("No PERPOS keys exist for the given person Ids: " + string.Join(",", personIds));
            }

            //bulkread the records for all the keys
            var perposRecords = new List<Perpos>();
            for (int i = 0; i < perposKeys.Count(); i += bulkReadSize)
            {
                var subList = perposKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perpos>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Perpos records");
                }
                else
                {
                    perposRecords.AddRange(records);
                }
            }

            // Get Employee Records for obtaining MigrationDate
            var employeCriteria = "WITH EMPLOYES.ID EQ ?";
            var employeKeys = await DataReader.SelectAsync("EMPLOYES", employeCriteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
            if (employeKeys == null)
            {
                var message = "Unexpected null returned from EMPLOYES SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!employeKeys.Any())
            {
                logger.Error("No EMPLOYES keys exist for the given person Ids: " + string.Join(",", personIds));
            }

            //bulkread the records for all the keys
            var employeRecords = new List<Employes>();
            for (int i = 0; i < employeKeys.Count(); i += bulkReadSize)
            {
                var subList = employeKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Employes>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Employe records");
                }
                else
                {
                    employeRecords.AddRange(records);
                }
            }

            var personPositionEntities = new List<PersonPosition>();
            foreach (var perposRecord in perposRecords)
            {
                if (perposRecord != null)
                {
                    try
                    {
                        var employeRecord = employeRecords.FirstOrDefault(x => x.Recordkey == perposRecord.PerposHrpId);
                        if (employeRecord != null)
                        {
                            var cacheKey = emptimeHistoryCacheKey + employeRecord.Recordkey;
                            var empTimeHistoryRecords = await GetOrAddToCacheAsync(cacheKey, async () => await GetEmptimeHistoryRecordsAsync(employeRecord.Recordkey), Level1CacheTimeoutValue);

                            var empTimeHistoryRecord = empTimeHistoryRecords
                                 .Where(x => x.EmpthEmployeeId == perposRecord.PerposHrpId && x.EmpthPosition == perposRecord.PerposPositionId)
                                 .OrderByDescending(x => x.EmpthPeriodEndDate)
                                 .FirstOrDefault();

                            personPositionEntities.Add(BuildPersonPosition(perposRecord, employeRecord, empTimeHistoryRecord));
                        }
                    }
                    catch (ColleagueSessionExpiredException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        LogDataError("Perpos", perposRecord.Recordkey, perposRecord, e, e.Message);
                    }
                }
            }

            // Check to see if the person has a Non-Employee Position.
            // We only want to check for Non-Employee Positions for the main user (not subordinates) which is why I use the first element in personIds
            var hrperRecord = await DataReader.ReadRecordAsync<Hrper>(personIds.ElementAt(0));
            if (hrperRecord != null)
            {
                if (!string.IsNullOrWhiteSpace(hrperRecord.HrpNonempPosition))
                {
                    personPositionEntities.Add(new PersonPosition(hrperRecord.Recordkey, hrperRecord.HrpNonempPosition));
                }
            }

            return personPositionEntities;
        }

        /// <summary>
        /// Helper to get EMPTIME_HISTORY records for an employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>List<EmptimeHistory></returns>
        private async Task<IEnumerable<EmptimeHistory>> GetEmptimeHistoryRecordsAsync(string employeeId)
        {
            var empTimeHistoryRecords = new List<EmptimeHistory>();
            var empTimeHistoryCriteria = "EMPTH.EMPLOYEE.ID EQ ?";
            IEnumerable<string> employeeIds = new List<string>() { employeeId };
            var empTimeHistoryKeys = await DataReader.SelectAsync("EMPTIME.HISTORY", empTimeHistoryCriteria, employeeIds.ToArray());

            if (empTimeHistoryKeys != null || empTimeHistoryKeys.Any())
            {
                for (int i = 0; i < empTimeHistoryKeys.Count(); i += bulkReadSize)
                {
                    var subList = empTimeHistoryKeys.Skip(i).Take(bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<EmptimeHistory>(subList.ToArray());
                    if (records == null)
                    {
                        logger.Error("Unexpected null from bulk read of EmptimeHistory records");
                    }
                    else
                    {
                        empTimeHistoryRecords.AddRange(records);
                    }
                }
            }
            return empTimeHistoryRecords;
        }

        /// <summary>
        /// Helper to build PersonPosition objects
        /// </summary>
        /// <param name="perposRecord">the Perpos db record</param>
        /// <returns></returns>
        private PersonPosition BuildPersonPosition(Perpos perposRecord, Employes employeRecord, EmptimeHistory empTimeHistoryRecord)
        {
            if (perposRecord == null)
            {
                throw new ArgumentNullException("perposRecord");
            }

            if (!perposRecord.PerposStartDate.HasValue)
            {
                throw new ArgumentException("Perpos StartDate must have a value!");
            }

            var personPositionEntity = new PersonPosition(perposRecord.Recordkey, perposRecord.PerposHrpId, perposRecord.PerposPositionId, perposRecord.PerposStartDate.Value, perposRecord.PerposFte)
            {
                EndDate = perposRecord.PerposEndDate,
                AlternateSupervisorId = perposRecord.PerposAltSupervisorId,
                SupervisorId = perposRecord.PerposSupervisorHrpId
            };

            // set the MigrationDate
            if (employeRecord != null)
            {
                var index = employeRecord.EmpSsPositionIds.FindIndex(x => x == personPositionEntity.PositionId);
                // only attempt to set the MigrationDate if the position is found in the list (will only appear in the list if the position has been migrated) 
                if (index != -1) personPositionEntity.MigrationDate = employeRecord.EmpSsPpdEndDates[index];
            }

            // set the LastWebTimeEntryPayPeriodEndDate
            if (empTimeHistoryRecord != null)
            {
                personPositionEntity.LastWebTimeEntryPayPeriodEndDate = empTimeHistoryRecord.EmpthPeriodEndDate;
            }

            var workScheduleItems = new List<WorkScheduleItem>();
            if (perposRecord.PerposDfltSundayEntityAssociation != null || !perposRecord.PerposDfltSundayEntityAssociation.Any())
            {
                workScheduleItems.AddRange(perposRecord.PerposDfltSundayEntityAssociation.Select(schedule => new WorkScheduleItem(perposRecord.PerposWorkWeekId, DayOfWeek.Sunday, schedule.PerposDfltSundayUnitsAssocMember.Value, schedule.PerposDfltSundayPrjAssocMember)));
            }

            if (perposRecord.PerposDfltMondayEntityAssociation != null || !perposRecord.PerposDfltMondayEntityAssociation.Any())
            {
                workScheduleItems.AddRange(perposRecord.PerposDfltMondayEntityAssociation.Select(schedule => new WorkScheduleItem(perposRecord.PerposWorkWeekId, DayOfWeek.Monday, schedule.PerposDfltMondayUnitsAssocMember.Value, schedule.PerposDfltMondayPrjAssocMember)));
            }

            if (perposRecord.PerposDfltTuesdayEntityAssociation != null || !perposRecord.PerposDfltTuesdayEntityAssociation.Any())
            {
                workScheduleItems.AddRange(perposRecord.PerposDfltTuesdayEntityAssociation.Select(schedule => new WorkScheduleItem(perposRecord.PerposWorkWeekId, DayOfWeek.Tuesday, schedule.PerposDfltTuesdayUnitsAssocMember.Value, schedule.PerposDfltTuesdayPrjAssocMember)));
            }

            if (perposRecord.PerposDfltWednesdayEntityAssociation != null || !perposRecord.PerposDfltWednesdayEntityAssociation.Any())
            {
                workScheduleItems.AddRange(perposRecord.PerposDfltWednesdayEntityAssociation.Select(schedule => new WorkScheduleItem(perposRecord.PerposWorkWeekId, DayOfWeek.Wednesday, schedule.PerposDfltWednesdayUnitsAssocMember.Value, schedule.PerposDfltWednesdayPrjAssocMember)));
            }

            if (perposRecord.PerposDfltThursdayEntityAssociation != null || !perposRecord.PerposDfltThursdayEntityAssociation.Any())
            {
                workScheduleItems.AddRange(perposRecord.PerposDfltThursdayEntityAssociation.Select(schedule => new WorkScheduleItem(perposRecord.PerposWorkWeekId, DayOfWeek.Thursday, schedule.PerposDfltThursdayUnitsAssocMember.Value, schedule.PerposDfltThursdayPrjAssocMember)));
            }

            if (perposRecord.PerposDfltFridayEntityAssociation != null || !perposRecord.PerposDfltFridayEntityAssociation.Any())
            {
                workScheduleItems.AddRange(perposRecord.PerposDfltFridayEntityAssociation.Select(schedule => new WorkScheduleItem(perposRecord.PerposWorkWeekId, DayOfWeek.Friday, schedule.PerposDfltFridayUnitsAssocMember.Value, schedule.PerposDfltFridayPrjAssocMember)));
            }

            if (perposRecord.PerposDfltSaturdayEntityAssociation != null || !perposRecord.PerposDfltSaturdayEntityAssociation.Any())
            {
                workScheduleItems.AddRange(perposRecord.PerposDfltSaturdayEntityAssociation.Select(schedule => new WorkScheduleItem(perposRecord.PerposWorkWeekId, DayOfWeek.Saturday, schedule.PerposDfltSaturdayUnitsAssocMember.Value, schedule.PerposDfltSaturdayPrjAssocMember)));
            }

            personPositionEntity.WorkScheduleItems = workScheduleItems;

            return personPositionEntity;
        }
    }
}
