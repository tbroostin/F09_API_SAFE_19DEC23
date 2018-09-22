/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class SupervisorsRepository : BaseColleagueRepository, ISupervisorsRepository
    {
        private readonly int bulkReadSize;

        public SupervisorsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }


        /// <summary>
        /// Gets direct and position supervisors
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetSuperviseesBySupervisorAsync(string supervisorId)
        {
            if (string.IsNullOrWhiteSpace(supervisorId))
            {
                throw new ArgumentNullException("supervisorId");
            }

            var superviseeIds = new List<string>();


            //select PERPOS records that specify this supervisor Id as the supervisor
            var superviseePerposKeys = await getDirectlySupervisedPerposIds(supervisorId);

            //next, get the supervisor's list of positions
            var supervisorPositionIds = await getPositionIdsOfSupervisor(supervisorId);

            // 2. now get positions where the supervisor's position is specified as the supervisorPosition; 
            var superviseePositionIds = await getPositionIdsSupervisedByPositionIds(supervisorPositionIds);

            // 3.  get the people from perpos who have those positions of the supervisor assignment is empty
            var additionalSuperviseePerposKeys = await getPerposIdsByPositionIdsWithoutDirectSupervisor(superviseePositionIds);

            var allSuperviseePerposKeys = superviseePerposKeys.Concat(additionalSuperviseePerposKeys).ToArray();

            var superviseePerposRecords = await getPerposRecords(allSuperviseePerposKeys);

            foreach (var superviseePerposRecord in superviseePerposRecords)
            {
                if (string.IsNullOrWhiteSpace(superviseePerposRecord.PerposHrpId))
                {
                    LogDataError("PerposHrpId", superviseePerposRecord.Recordkey, superviseePerposRecord);
                }
                else
                {
                    superviseeIds.Add(superviseePerposRecord.PerposHrpId);
                }
            }

            // return the complete list
            return superviseeIds.Distinct().ToList();
        }

        /// <summary>
        /// Gets direct and position supervisors for a supervisee
        /// </summary>
        /// <param name="superviseeId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetSupervisorsBySuperviseeAsync(string superviseeId)
        {
            if (string.IsNullOrWhiteSpace(superviseeId))
            {
                throw new ArgumentNullException("superviseeId");
            }

            var allSupervisorIds = new List<string>();
            var positionsIdsWithoutDirectSupervisor = new List<string>();

            // get all the PERPOS ids for this supervisee
            var allSuperviseeIdPerposIds = await getSuperviseePerposIds(superviseeId);

            if (allSuperviseeIdPerposIds.Any())
            {
                // get all the PERPOS records for this supervisee
                var superviseePerposRecords = await getPerposRecords(allSuperviseeIdPerposIds);

                foreach (var superviseePerposRecord in superviseePerposRecords)
                {
                    if (!string.IsNullOrWhiteSpace(superviseePerposRecord.PerposSupervisorHrpId))
                    {
                        // this PERPOS has a direct supervisor, so add them to the list of directs
                        allSupervisorIds.Add(superviseePerposRecord.PerposSupervisorHrpId);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(superviseePerposRecord.PerposPositionId))
                        {
                            // this PERPOS does not have a direct supervisor, so add this position id to a list for use later
                            positionsIdsWithoutDirectSupervisor.Add(superviseePerposRecord.PerposPositionId);
                        }
                        else
                        {
                            LogDataError("PERPOS", superviseePerposRecord.Recordkey, superviseePerposRecord, null, "PERPOS does not have position ID specified.");
                        }
                    }
                }
            }

            if (positionsIdsWithoutDirectSupervisor.Any())
            {
                // get all the position ids that supervise these position ids
                var supervisorPositionIds = await getSupervisorPositionIdsForPositions(positionsIdsWithoutDirectSupervisor);

                if (supervisorPositionIds.Any())
                {
                    // get all person ids in these supervisor level positions
                    var positionLevelSupervisorDict = await GetSupervisorIdsForPositionsAsync(supervisorPositionIds);

                    allSupervisorIds.AddRange(positionLevelSupervisorDict.SelectMany(i => i.Value));

                }
            }

            if (!allSupervisorIds.Any())
            {
                logger.Error("No supervisors returned for person.");
            }

            return allSupervisorIds.Distinct().ToList();
        }

        /// <summary>
        /// Get the ids of the Perpos records that have a PositionId equal to one of the given position ids 
        /// </summary>
        /// <param name="positionIds"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getPerposIdsByPositionIds(IEnumerable<string> positionIds)
        {
            if (!positionIds.Any())
            {
                throw new ArgumentNullException("positionIds");
            }

            var today = await GetUnidataFormatDateAsync(DateTime.Now);
            var perposCriteria = string.Format("WITH PERPOS.END.DATE GE '{0}'", today);
            perposCriteria = string.Concat(perposCriteria, " OR WITH PERPOS.END.DATE EQ ''");
            perposCriteria = string.Concat(perposCriteria, " AND WITH PERPOS.POSITION.INDEX EQ ?");
            perposCriteria = string.Concat(perposCriteria, " BY PERPOS.NAME");
            var perposValues = positionIds.Distinct().Select(id => string.Format("\"{0}\"", id));

            var perposKeys = await DataReader.SelectAsync("PERPOS", perposCriteria, perposValues.ToArray());

            if (perposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                perposKeys = new string[0];
            }

            if (!perposKeys.Any())
            {
                logger.Info(string.Format("No PERPOS keys exist for positions: {0}", string.Join(", ", positionIds.Distinct())));
            }

            return perposKeys;
        }

        /// <summary>
        /// Gets all PERPOS records for an employee 
        /// </summary>
        /// <param name="superviseeId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getSuperviseePerposIds(string superviseeId)
        {
            // select PERPOS records for this superviseeId
            var superviseePerposCriteria = string.Format("WITH PERPOS.HRP.ID EQ {0}", superviseeId);
            var superviseePerposKeys = await DataReader.SelectAsync("PERPOS", superviseePerposCriteria);

            if (superviseePerposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                superviseePerposKeys = new string[0];
            }

            if (!superviseePerposKeys.Any())
            {
                logger.Info(string.Format("No PERPOS keys exist for employee {0}", superviseeId));
            }
            return superviseePerposKeys;
        }

        /// <summary>
        /// Get PerPosRecords for the given ids
        /// </summary>
        /// <param name="perposIds"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Perpos>> getPerposRecords(IEnumerable<string> perposIds)
        {
            var distinctPerposIds = perposIds.Distinct();
            var returnList = new List<Perpos>();
            for (int i = 0; i < distinctPerposIds.Count(); i += bulkReadSize)
            {
                var subList = distinctPerposIds.Skip(i).Take(bulkReadSize);
                var perposRecords = await DataReader.BulkReadRecordAsync<Perpos>(subList.ToArray());
                if (perposRecords == null)
                {
                    logger.Error("Unexpected null from bulk read of Perpos records");
                }
                else
                {
                    returnList.AddRange(perposRecords);
                }
            }
            return returnList;
        }

        /// <summary>
        /// Get the ids of the PerPos records that have a SupervisorId equal to the given supervisor id
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getDirectlySupervisedPerposIds(string supervisorId)
        {
            //select PERPOS records that specify this supervisor Id as the supervisor
            var superviseePerposCriteria = string.Format("WITH INDEX.PERPOS.SUPERVISOR EQ {0}", supervisorId); // note: supervisorId is indexed, but alternate is not

            var superviseePerposKeys = await DataReader.SelectAsync("PERPOS", superviseePerposCriteria);

            if (superviseePerposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                superviseePerposKeys = new string[0];
            }
            if (!superviseePerposKeys.Any())
            {
                logger.Info(string.Format("No PERPOS keys exist where {0} is supervisor", supervisorId));
            }
            return superviseePerposKeys;
        }

        /// <summary>
        /// Get the ids of the Position records for the given supervisor id
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getPositionIdsOfSupervisor(string supervisorId)
        {
            var supervisorPerposCriteria = string.Format("WITH PERPOS.HRP.ID EQ {0}", supervisorId);

            var supervisorPersonPositions = await DataReader.BulkReadRecordAsync<Perpos>(supervisorPerposCriteria);

            if (supervisorPersonPositions == null)
            {
                logger.Error("Unexpected null from bulk read of Perpos records");
                supervisorPersonPositions = new Collection<Perpos>();
            }

            var supervisorPositionIds = new List<string>();

            foreach (var position in supervisorPersonPositions)
            {
                if (string.IsNullOrWhiteSpace(position.PerposPositionId))
                {
                    LogDataError("PerposPositionId", position.Recordkey, position);
                }
                else
                {
                    supervisorPositionIds.Add(position.PerposPositionId);
                }
            }

            // Select non employee position IDs from HRPER file and add them to the supervisorPositionIds list
            var hrperRecord = await DataReader.ReadRecordAsync<Hrper>(supervisorId);
            if (hrperRecord != null)
            {
                if (!string.IsNullOrWhiteSpace(hrperRecord.HrpNonempPosition))
                {
                    supervisorPositionIds.Add(hrperRecord.HrpNonempPosition);
                }
            }

            return supervisorPositionIds;
        }

        /// <summary>
        /// Get the Ids of the Position records that have a Supervisor Position Id equal to one of the given supervisorPositionIds
        /// </summary>
        /// <param name="supervisorPositionIds"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getPositionIdsSupervisedByPositionIds(IEnumerable<string> supervisorPositionIds)
        {

            var supervisedPositionsCriteria = "WITH INDEX.POS.SUPER EQ ?";
            var criteriaValues = supervisorPositionIds.Distinct().Select(id => string.Format("\"{0}\"", id));

            var superviseePositionKeys = await DataReader.SelectAsync("POSITION", supervisedPositionsCriteria, criteriaValues.ToArray());

            if (superviseePositionKeys == null)
            {
                var message = "Unexpected null returned from POSITION SelectAsyc";
                logger.Error(message);
                superviseePositionKeys = new string[0];
            }

            if (!superviseePositionKeys.Any())
            {
                logger.Info(string.Format("No POSITION keys exist for position supervisors: {0}", string.Join(", ", supervisorPositionIds.Distinct())));

                // this supervisor has no position subordinates so we can return our current list
                return new List<string>();
            }

            var superviseePositionRecords = await DataReader.BulkReadRecordAsync<DataContracts.Position>(superviseePositionKeys.ToArray());

            if (superviseePositionRecords == null)
            {
                var message = string.Format(
                    "Unexpected null or empty from bulk read async of Position records for position keys: {0}",
                    string.Join(", ", superviseePositionKeys.Distinct())
                    );
                logger.Error(message);
                return new List<string>();
            }

            var superviseePositionIds = new List<string>();

            foreach (var superviseePositionRecord in superviseePositionRecords)
            {
                if (string.IsNullOrWhiteSpace(superviseePositionRecord.Recordkey))
                {
                    LogDataError("Position", "no record key!", superviseePositionRecord);
                }
                else
                {
                    superviseePositionIds.Add(superviseePositionRecord.Recordkey);
                }
            }

            return superviseePositionIds;
        }

        /// <summary>
        /// Get the ids of the Perpos records that have a PositionId equal to one of the given position ids AND that
        /// aren't already assigned to a specific supervisor.
        /// </summary>
        /// <param name="positionIds"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getPerposIdsByPositionIdsWithoutDirectSupervisor(IEnumerable<string> positionIds)
        {
            var perposCriteria = "WITH PERPOS.POSITION.INDEX EQ ? AND WITH INDEX.PERPOS.SUPERVISOR EQ ''";
            var perposValues = positionIds.Distinct().Select(id => string.Format("\"{0}\"", id));

            var perposKeys = await DataReader.SelectAsync("PERPOS", perposCriteria, perposValues.ToArray());

            if (perposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                perposKeys = new string[0];
            }

            if (!perposKeys.Any())
            {
                logger.Info(string.Format("No PERPOS keys exist for supervisees: {0}", string.Join(", ", positionIds.Distinct())));
            }

            return perposKeys;
        }

        /// <summary>
        /// Get the Ids of the supervisor positions records for the given supervisee position id
        /// </summary>
        /// <param name="positionIds"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getSupervisorPositionIdsForPositions(IEnumerable<string> positionIds)
        {
            var positionRecords = await DataReader.BulkReadRecordAsync<DataContracts.Position>(positionIds.ToArray());

            if (positionRecords == null)
            {
                var message = string.Format(
                    "Unexpected null or empty from bulk read async of Position records for position keys: {0}",
                    string.Join(", ", positionIds.Distinct())
                    );
                logger.Error(message);
                return new List<string>();
            }

            var supervisorPositionIds = new List<string>();

            foreach (var positionRecord in positionRecords)
            {
                if (!string.IsNullOrWhiteSpace(positionRecord.PosSupervisorPosId))
                {
                    supervisorPositionIds.Add(positionRecord.PosSupervisorPosId);
                }
            }

            return supervisorPositionIds;
        }

        /// <summary>
        /// Dictionary where the keys are the positionIds you passed in, and the values are the PERSON ids
        /// who hold those positions
        /// </summary>
        /// <param name="positionIds"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<string>>> GetSupervisorIdsForPositionsAsync(IEnumerable<string> positionIds)
        {
            if (positionIds == null || !positionIds.Any())
            {
                throw new ArgumentNullException("positionIds");
            }
            var positionIdDictionary = positionIds.Distinct().ToDictionary(p => p, p => new List<string>());


            // get all the supervisor perpos ids assigned to these positionIds
            var positionLevelSupervisorPerposIds = await getPerposIdsByPositionIds(positionIds);
            if (positionLevelSupervisorPerposIds.Any())
            {
                // get all the supervisor perpos records
                var positionLeavelSupervisorPerposRecords = await getPerposRecords(positionLevelSupervisorPerposIds);
                foreach (var perpos in positionLeavelSupervisorPerposRecords)
                {
                    if (positionIdDictionary.ContainsKey(perpos.PerposPositionId))
                    {
                        if (!positionIdDictionary[perpos.PerposPositionId].Contains(perpos.PerposHrpId))
                        {
                            positionIdDictionary[perpos.PerposPositionId].Add(perpos.PerposHrpId);
                        }
                    }
                }
            }
            return positionIdDictionary;
        }
    }
}