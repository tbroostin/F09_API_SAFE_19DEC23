/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
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

        public PersonPositionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
               : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get PersonPosition objects for the given personIds
        /// </summary>
        /// <param name="personIds">Required: A list of personIds for which to get PersonPositions</param>       
        /// <returns></returns>
        public async Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(IEnumerable<string> personIds)
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
               var perposKeys = await DataReader.SelectAsync("PERPOS", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
            if (perposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!perposKeys.Any())
            {
                logger.Info("No PERPOS keys exist for the given person Ids: " + string.Join(",", personIds));
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

               if (!perposKeys.Any())
               {
                    logger.Info("No EMPLOYES keys exist for the given person Ids: " + string.Join(",", personIds));
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

               // Get EMPTIME_HISTORY records for 
               var empTimeHistoryCriteria = "EMPTH.EMPLOYEE.ID EQ ?";
               var empTimeHistoryKeys = await DataReader.SelectAsync("EMPTIME.HISTORY", empTimeHistoryCriteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
               if (empTimeHistoryKeys == null)
               {
                    var message = "Unexpected null returned from EMPTIME.HISTORY SelectAsyc";
                    logger.Error(message);
                    throw new ApplicationException(message);
               }

               if (!perposKeys.Any())
               {
                    logger.Info("No EMPTIME.HISTORY keys exist for the given person Ids: " + string.Join(",", personIds));
               }

               //bulkread the records for all the keys
               var empTimeHistoryRecords = new List<EmptimeHistory>();
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

            //build the PersonPosition objects
            var personPositionEntities = new List<PersonPosition>();
            foreach (var perposRecord in perposRecords)
            {
                if (perposRecord != null)
                {
                    try
                    {
                              // select the correct employeRecord
                              var employeRecord = employeRecords.FirstOrDefault(x => x.Recordkey == perposRecord.PerposHrpId);

                              // attempt to get the empTimeHistory records for this person & position
                              var empTimeHistoryRecord = empTimeHistoryRecords
                                   .Where(x => x.EmpthEmployeeId == perposRecord.PerposHrpId && x.EmpthPosition == perposRecord.PerposPositionId)
                                   .OrderByDescending(x => x.EmpthPeriodEndDate)
                                   .FirstOrDefault();

                              // build the entity
                              personPositionEntities.Add(BuildPersonPosition(perposRecord, employeRecord, empTimeHistoryRecord));
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

            var personPositionEntity = new PersonPosition(perposRecord.Recordkey, perposRecord.PerposHrpId, perposRecord.PerposPositionId, perposRecord.PerposStartDate.Value)
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

            return personPositionEntity;
        }
    }
}
