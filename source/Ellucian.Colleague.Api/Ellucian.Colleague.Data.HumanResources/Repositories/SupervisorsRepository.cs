/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
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
    public class SupervisorsRepository : PersonEmploymentStatusRepository, ISupervisorsRepository
    {
        private readonly int bulkReadSize;

        public SupervisorsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger, null)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }


        /// <summary>
        /// Gets direct and position supervisors
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetSuperviseesBySupervisorAsync(string supervisorId, DateTime? lookupStartDate = null)
        {
            if (string.IsNullOrWhiteSpace(supervisorId))
            {
                throw new ArgumentNullException("supervisorId");
            }

            var superviseeIds = new List<string>();


            //select PERPOS records that specify this supervisor Id as the supervisor
            var superviseePerposKeys = await getDirectlySupervisedPerposIds(supervisorId, lookupStartDate);
            logger.Debug(string.Format("**********PERPOS records that specify {0} as the supervisor fetched**********", supervisorId));
            //next, get the supervisor's list of positions
            var supervisorPositionIds = await getPositionIdsOfSupervisor(supervisorId, lookupStartDate);
            logger.Debug(string.Format("**********List of positions for {0} fetched**********", supervisorId));
            // 2. now get positions where the supervisor's position is specified as the supervisorPosition; 
            var superviseePositionIds = await getPositionIdsSupervisedByPositionIds(supervisorPositionIds);
            logger.Debug(string.Format("**********List of the Position records that have a Supervisor Position Id equal to one of {0} fetched**********", string.Join(",", supervisorPositionIds)));

            // 3.  get the people from perpos who have those positions of the supervisor assignment is empty
            var additionalSuperviseePerposKeys = await getPerposIdsByPositionIdsWithoutDirectSupervisor(superviseePositionIds, lookupStartDate);
            logger.Debug(string.Format("**********List of the Perpos records that have a Position Id equal to one of {0} and that aren't already assigned to a specific supervisor fetched**********", string.Join(",", superviseePositionIds)));

            var allSuperviseePerposKeys = superviseePerposKeys.Concat(additionalSuperviseePerposKeys).ToArray();

            var superviseePerposRecords = await getPerposRecords(allSuperviseePerposKeys);

            foreach (var superviseePerposRecord in superviseePerposRecords)
            {
                if (string.IsNullOrWhiteSpace(superviseePerposRecord.PerposHrpId))
                {
                    LogDataError("PerposHrpId", superviseePerposRecord.Recordkey, new object());
                }
                else
                {
                    superviseeIds.Add(superviseePerposRecord.PerposHrpId);
                }
            }
            logger.Debug("*******Complete list of direct and position supervisors fetched successfully********");
            // return the complete list
            return superviseeIds.Distinct().ToList();
        }

        /// <summary>
        /// Gets direct and position supervisors for a supervisee
        /// </summary>
        /// <param name="superviseeId">ID of the employee in which to get data based on</param>
        /// <param name="lookupStartDate">Optional lookback date to limit returned results</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetSupervisorsBySuperviseeAsync(string superviseeId, DateTime? lookupStartDate = null)
        {
            if (string.IsNullOrWhiteSpace(superviseeId))
            {
                throw new ArgumentNullException("superviseeId");
            }

            var allSupervisorIds = new List<string>();
            var positionsIdsWithoutDirectSupervisor = new List<string>();

            // get all the PERPOS ids for this supervisee
            // CH-12939: lookback added to limit PERPOS records to only those without an end date or with end dates after the lookback start date.
            var allSuperviseeIdPerposIds = await getSuperviseePerposIds(superviseeId, lookupStartDate);

            logger.Debug(string.Format("***********PERPOS ids fetched for {0}*******", superviseeId));
            
            if (allSuperviseeIdPerposIds.Any())
            {
                // get all the PERPOS records for this supervisee
                var superviseePerposRecords = await getPerposRecords(allSuperviseeIdPerposIds);
                logger.Debug(string.Format("********PERPOS records fetched for {0}********", superviseeId));

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
                            LogDataError("PERPOS", superviseePerposRecord.Recordkey, new object(), null, "PERPOS does not have position ID specified.");
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
            logger.Debug(string.Format("********Direct and position supervisors fetched successfully for {0}********", superviseeId));
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
                logger.Error(string.Format("No PERPOS keys exist for positions: {0}", string.Join(", ", positionIds.Distinct())));
            }
            logger.Debug(string.Format("*********Ids of the Perpos records that have a PositionId equal to one of {0} obtained successfully*******", string.Join(",", positionIds.Distinct())));
            return perposKeys;
        }

        /// <summary>
        /// Gets all PERPOS records for an employee 
        /// </summary>
        /// <param name="superviseeId"></param>
        /// <param name="lookupStartDate"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getSuperviseePerposIds(string superviseeId, DateTime? lookupStartDate = null)
        {
            // select PERPOS records for this superviseeId
            var superviseePerposCriteria = string.Format("WITH PERPOS.HRP.ID EQ {0}", superviseeId);

            // restricts PERPOS records to only ones with an end date after the lookupStartDate OR with no end date specified
            if (lookupStartDate.HasValue)
            {
                superviseePerposCriteria += " AND (PERPOS.END.DATE GE '" +
                    UniDataFormatter.UnidataFormatDate(lookupStartDate.Value, InternationalParameters.HostShortDateFormat,
                    InternationalParameters.HostDateDelimiter)
                    + "' OR PERPOS.END.DATE EQ '')";
            }

            var superviseePerposKeys = await DataReader.SelectAsync("PERPOS", superviseePerposCriteria);

            if (superviseePerposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                superviseePerposKeys = new string[0];
            }

            if (!superviseePerposKeys.Any())
            {
                logger.Error(string.Format("No PERPOS keys exist for employee {0}", superviseeId));
            }
            logger.Debug(string.Format("***********All PERPOS records obtained for {0}*********", superviseeId));
            return superviseePerposKeys;
        }

        /// <summary>
        /// Gets all PERPOS records for a list of employees
        /// </summary>
        /// <param name="superviseeId"></param>
        /// <returns>List of PERPOS records</returns>
        private async Task<IEnumerable<string>> getSuperviseePerposIds(IEnumerable<string> superviseeIds)
        {
            // select PERPOS records for this superviseeId
            var superviseePerposCriteria = "WITH PERPOS.HRP.ID EQ ?";
            var superviseePerposKeys = await DataReader.SelectAsync("PERPOS", superviseePerposCriteria, superviseeIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

            if (superviseePerposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                superviseePerposKeys = new string[0];
            }

            if (!superviseePerposKeys.Any())
            {
                logger.Debug("No PERPOS keys exist for given list of supervisees");
            }

            logger.Debug(string.Format("**********All PERPOS records obtained for {0}********", string.Join(",", superviseeIds.Distinct())));

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

            logger.Debug(string.Format("*********PERPOS records obtained for {0}*********", string.Join(",", perposIds.Distinct())));

            return returnList;
        }

        /// <summary>
        /// Get the ids of the PerPos records that have a SupervisorId equal to the given supervisor id
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getDirectlySupervisedPerposIds(string supervisorId, DateTime? lookupStartDate = null)
        {
            //select PERPOS records that specify this supervisor Id as the supervisor
            var superviseePerposCriteria = string.Format("WITH INDEX.PERPOS.SUPERVISOR EQ {0}", supervisorId); // note: supervisorId is indexed, but alternate is not
            if (lookupStartDate.HasValue)
            {
                superviseePerposCriteria += " AND (PERPOS.END.DATE GE '" + UniDataFormatter.UnidataFormatDate(lookupStartDate.Value, InternationalParameters.HostShortDateFormat, InternationalParameters.HostDateDelimiter)
                    + "' OR PERPOS.END.DATE EQ '')";
            }

            var superviseePerposKeys = await DataReader.SelectAsync("PERPOS", superviseePerposCriteria);

            if (superviseePerposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                superviseePerposKeys = new string[0];
            }
            if (!superviseePerposKeys.Any())
            {
                logger.Error(string.Format("No PERPOS keys exist where {0} is supervisor", supervisorId));
            }
            return superviseePerposKeys;
        }

        /// <summary>
        /// Get the ids of the Position records for the given supervisor id
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getPositionIdsOfSupervisor(string supervisorId, DateTime? lookupStartDate = null)
        {
            var supervisorPerposCriteria = string.Format("WITH PERPOS.HRP.ID EQ {0}", supervisorId);
            if (lookupStartDate.HasValue)
            {
                supervisorPerposCriteria += " AND (PERPOS.END.DATE GE '" + UniDataFormatter.UnidataFormatDate(lookupStartDate.Value, InternationalParameters.HostShortDateFormat, InternationalParameters.HostDateDelimiter)
                    + "' OR PERPOS.END.DATE EQ '')";
            }

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
                    LogDataError("PerposPositionId", position.Recordkey, new object());
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
                logger.Error(string.Format("No POSITION keys exist for position supervisors: {0}", string.Join(", ", supervisorPositionIds.Distinct())));

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
                    LogDataError("Position", "no record key!", new object());
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
        private async Task<IEnumerable<string>> getPerposIdsByPositionIdsWithoutDirectSupervisor(IEnumerable<string> positionIds, DateTime? lookupStartDate = null)
        {
            var perposCriteria = "WITH PERPOS.POSITION.INDEX EQ ? AND WITH INDEX.PERPOS.SUPERVISOR EQ ''";
            var perposValues = positionIds.Distinct().Select(id => string.Format("\"{0}\"", id));

            if (lookupStartDate.HasValue)
            {
                perposCriteria += " AND (PERPOS.END.DATE GE '" + UniDataFormatter.UnidataFormatDate(lookupStartDate.Value, InternationalParameters.HostShortDateFormat, InternationalParameters.HostDateDelimiter)
                    + "' OR PERPOS.END.DATE EQ '')";
            }
            var perposKeys = await DataReader.SelectAsync("PERPOS", perposCriteria, perposValues.ToArray());

            if (perposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                perposKeys = new string[0];
            }

            if (!perposKeys.Any())
            {
                logger.Error(string.Format("No PERPOS keys exist for supervisees: {0}", string.Join(", ", positionIds.Distinct())));
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
            logger.Debug(string.Format("*******Fetching supervisor perpos ids assigned to the following positionIds - {0}***********", string.Join(", ", positionIds.Distinct())));
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
            else  // Check if the Position is a Non Employee Position. Select HRPER Records based on Non Employee Position IDs and add them to the PositionId Dictionary.
            {
                // get all the supervisor Hrper ids assigned to these positionIds
                logger.Debug(string.Format("*********Fetching supervisor Hrper ids assigned to the following positionIds - {0}***********", string.Join(", ", positionIds.Distinct())));
                var positionLevelSupervisorHrPerIds = await getHrPerIdsByPositionIds(positionIds);

                if (positionLevelSupervisorHrPerIds.Any())
                {
                    // get all the supervisor Hrper records
                    var positionLevelSupervisorPerposRecords = await getHrPerRecords(positionLevelSupervisorHrPerIds);
                    if (positionLevelSupervisorPerposRecords != null)
                    {
                        foreach (var hrPer in positionLevelSupervisorPerposRecords)
                        {
                            if (positionIdDictionary.ContainsKey(hrPer.HrpNonempPosition))
                            {
                                if (!positionIdDictionary[hrPer.HrpNonempPosition].Contains(hrPer.Recordkey))
                                {
                                    positionIdDictionary[hrPer.HrpNonempPosition].Add(hrPer.Recordkey);
                                }
                            }
                        }
                    }
                }
            }
            return positionIdDictionary;
        }

        /// <summary>
        /// Get the ids of the HRPER records that have Non Employee Position IDs equal to one of the given position ids 
        /// </summary>
        /// <param name="positionIds"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> getHrPerIdsByPositionIds(IEnumerable<string> positionIds)
        {
            var HrPerCriteria = string.Format("WITH HRP.NONEMP.POSITION EQ ?");

            var HrPerValues = positionIds.Distinct().Select(id => string.Format("\"{0}\"", id));

            var HrPerKeys = await DataReader.SelectAsync("HRPER", HrPerCriteria, HrPerValues.ToArray());

            if (HrPerKeys == null)
            {
                var message = "Unexpected null returned from HRPER SelectAsyc";
                logger.Error(message);
                HrPerKeys = new string[0];
            }

            if (!HrPerKeys.Any())
            {
                logger.Error(string.Format("No HRPER keys exist for positions: {0}", string.Join(", ", positionIds.Distinct())));
            }


            logger.Debug(string.Format("*********Ids of the HRPER records that have Non Employee Position IDs equal to one of {0} obtained*******", string.Join(",", positionIds.Distinct())));

            return HrPerKeys;
        }

        /// <summary>
        /// Get HrperRecords for the given ids
        /// </summary>
        /// <param name="hrPerIds"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Hrper>> getHrPerRecords(IEnumerable<string> hrPerIds)
        {
            var distinctHrPerIdsIds = hrPerIds.Distinct();
            var returnList = new List<Hrper>();
            for (int i = 0; i < distinctHrPerIdsIds.Count(); i += bulkReadSize)
            {
                var subList = distinctHrPerIdsIds.Skip(i).Take(bulkReadSize);
                var hrPerRecords = await DataReader.BulkReadRecordAsync<Hrper>(subList.ToArray());
                if (hrPerRecords == null)
                {
                    logger.Error("Unexpected null from bulk read of HRPER records");
                }
                else
                {
                    returnList.AddRange(hrPerRecords);
                }
            }

            logger.Debug(string.Format("********HrperRecords obtained successfully for {0}**********", string.Join(",", hrPerIds.Distinct())));

            return returnList;
        }

        /// <summary>
        /// Gets the list of supervisors for the given position of a supervisee.</summary>
        /// <param name="positionId">Position Id.</param>
        /// <param name="superviseeId">Id of the supervise.</param>
        /// <returns>A list of all the supervisorIds for the given position of a supervisee.</returns>
        public async Task<IEnumerable<string>> GetSupervisorsByPositionIdAsync(string positionId, string superviseeId)
        {
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId");
            }
            if (string.IsNullOrEmpty(superviseeId))
            {
                throw new ArgumentNullException("superviseeId");
            }
            try
            {
                var allSupervisorIds = new List<string>();
                var positionsIdsWithoutDirectSupervisor = new List<string>();
                // Get all the PERPOS ids for this supervisee
                logger.Debug(string.Format("********Fetching the PERPOS ids for {0}***********", superviseeId));
                var allSuperviseeIdPerposIds = await getSuperviseePerposIds(superviseeId);
                if (allSuperviseeIdPerposIds != null && allSuperviseeIdPerposIds.Any())
                {
                    // Get all the PERPOS records for this supervisee
                    var superviseePerposRecords = await getPerposRecords(allSuperviseeIdPerposIds);
                    if (superviseePerposRecords == null || !superviseePerposRecords.Any())
                    {
                        var message = string.Format("No PERPOS records were found for the supervisee {0}", superviseeId);
                        logger.Error(message);
                        throw new ColleagueWebApiException(message);
                    }
                    var requiredPerPosRecord = superviseePerposRecords.Where(ppr => ppr.PerposPositionId == positionId && (ppr.PerposStartDate <= DateTime.Today && (ppr.PerposEndDate == null || ppr.PerposEndDate >= DateTime.Today))).First();
                    if (requiredPerPosRecord == null)
                    {
                        var message = string.Format("No PERPOS records were found for position {0} of the supervisee {1}", positionId, superviseeId);
                        logger.Error(message);
                        throw new ColleagueWebApiException(message);
                    }
                    if (!string.IsNullOrWhiteSpace(requiredPerPosRecord.PerposSupervisorHrpId))
                    {
                        // This PERPOS has a direct supervisor, so add them to the list of directs
                        allSupervisorIds.Add(requiredPerPosRecord.PerposSupervisorHrpId);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(requiredPerPosRecord.PerposPositionId))
                        {
                            // This PERPOS does not have a direct supervisor, so add this position id to a list for use later
                            positionsIdsWithoutDirectSupervisor.Add(requiredPerPosRecord.PerposPositionId);
                        }
                        else
                        {
                            LogDataError("PERPOS", requiredPerPosRecord.Recordkey, new object(), null, "PERPOS does not have position ID specified.");
                        }
                    }
                }
                if (positionsIdsWithoutDirectSupervisor.Any())
                {
                    // Get all the position ids that supervise these position ids
                    logger.Debug(string.Format("******Fetching the position ids that supervise the following position ids without direct supervisor - {0}*******", string.Join(", ", positionsIdsWithoutDirectSupervisor.Distinct())));
                    var supervisorPositionIds = await getSupervisorPositionIdsForPositions(positionsIdsWithoutDirectSupervisor);

                    if (supervisorPositionIds != null && supervisorPositionIds.Any())
                    {
                        // Get all person ids in these supervisor level positions
                        var positionLevelSupervisorDict = await GetSupervisorIdsForPositionsAsync(supervisorPositionIds);
                        allSupervisorIds.AddRange(positionLevelSupervisorDict.SelectMany(i => i.Value));
                    }

                }

                allSupervisorIds = allSupervisorIds.Distinct().ToList();
                logger.Debug(string.Format("*********List of supervisors for employee {0} with position {1} obtained successfully********", superviseeId, positionId));

                return allSupervisorIds;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }

            catch (Exception e)
            {
                var message = "Unexpected error occurred while getting the position supervisors information";
                logger.Error(message);
                throw e;
            }
        }


        /// <summary>
        /// Retrieves list of Supervisees for a Leave Approver/Supervisor.
        /// </summary>
        /// <param name="supervisorId">effective person id of supervisor</param>
        /// <returns>List of person ids of supervisees who are either directly supervised by logged in user (or) managed by their primary position by one of the positions of logged in user</returns>
        public async Task<IEnumerable<string>> GetSuperviseesByPrimaryPositionForSupervisorAsync(string supervisorId)
        {
            if (string.IsNullOrWhiteSpace(supervisorId))
            {
                throw new ArgumentNullException("supervisorId");
            }
            var superviseeIds = new List<string>();

            var currentDate = DateTime.Now;

            #region Direct Supervisees

            //select PERPOS records that specify this supervisor Id as the supervisor
            var directSuperviseePerposKeys = await getDirectlySupervisedPerposIds(supervisorId, currentDate);

            var directSuperviseePerposRecords = await getPerposRecords(directSuperviseePerposKeys);
            logger.Debug(string.Format("**********PERPOS records that specify {0} as the supervisor fetched**********", supervisorId));

            //Extract PersonIds to fetch primary position from person employment status
            IEnumerable<string> directSuperviseesPersonIds = null;
            if (directSuperviseePerposRecords != null && directSuperviseePerposRecords.Any())
            {
                //Extract distinct PersonIds from active perpos records
                directSuperviseesPersonIds = directSuperviseePerposRecords.Where(ds => ds.PerposStartDate <= currentDate && (ds.PerposEndDate >= currentDate || ds.PerposEndDate == null))
                    .Select(ds => ds.PerposHrpId).Distinct();

                if (directSuperviseesPersonIds != null && directSuperviseesPersonIds.Any())
                {
                    //Get Person Employment Statuses
                    var directSuperviseePerstatRecords = await GetPersonEmploymentStatusesAsync(directSuperviseesPersonIds);

                    if (directSuperviseePerstatRecords != null && directSuperviseePerstatRecords.Any())
                    {
                        directSuperviseePerstatRecords = directSuperviseePerstatRecords.Where(p => (p.StartDate <= currentDate && (p.EndDate >= currentDate || p.EndDate == null)));
                        ExtractLeaveApprovalSupervisees(directSuperviseePerposRecords, directSuperviseePerstatRecords, supervisorId, ref superviseeIds);
                    }
                }
            }


            #endregion

            #region POSD level Supervisees
            //next, get the supervisor's list of positions
            var supervisorPositionIds = await getPositionIdsOfSupervisor(supervisorId, currentDate);
            logger.Debug(string.Format("*********Supervisor position Ids obtained for {0}*********", supervisorId));

            if (supervisorPositionIds != null && supervisorPositionIds.Any())
            {
                // 2. now get positions where the supervisor's position is specified as the supervisorPosition; 
                var superviseePositionIds = await getPositionIdsSupervisedByPositionIds(supervisorPositionIds);

                if (superviseePositionIds != null && superviseePositionIds.Any())
                {
                    //Get PERSTAT info based on supervisee positions
                    var posdSuperviseePerstatRecords = await GetPersonEmploymentStatusesFromPositionIds(superviseePositionIds);

                    //Get primary position as of current date.
                    if (posdSuperviseePerstatRecords != null && posdSuperviseePerstatRecords.Any())
                    {
                        posdSuperviseePerstatRecords = posdSuperviseePerstatRecords.Where(p => (p.StartDate <= currentDate && (p.EndDate >= currentDate || p.EndDate == null)));

                        //Extract Person Records
                        IEnumerable<string> posdSuperviseePersonIds = posdSuperviseePerstatRecords.Select(ps => ps.PersonId).Distinct();

                        if (posdSuperviseePersonIds != null && posdSuperviseePersonIds.Any())
                        {
                            //get Perpos recordkeys
                            var posdSuperviseePerposKeys = await getSuperviseePerposIds(posdSuperviseePersonIds);

                            if (posdSuperviseePerposKeys != null && posdSuperviseePerposKeys.Any())
                            {
                                //get all perpos records (all the positions of superviees incl. primary position)
                                var posdSuperviseePerposRecords = await getPerposRecords(posdSuperviseePerposKeys);

                                ExtractLeaveApprovalSupervisees(posdSuperviseePerposRecords, posdSuperviseePerstatRecords, supervisorId, ref superviseeIds);
                            }
                        }

                    }
                }
            }

            #endregion
            logger.Debug(string.Format("********List of supervisees for {0} retrieved successfully*********", supervisorId));

            // return the complete list of supervisees
            return superviseeIds.Distinct().ToList();
        }
        
        /// <summary>
        /// Fetch list of PersonEmployment Statuses based on positionIds
        /// </summary>
        /// <param name="superviseePositionIds">List of position Ids</param>
        /// <returns>List of PersonEmployment Status DTOs</returns>
        private async Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesFromPositionIds(IEnumerable<string> superviseePositionIds)
        {
            if (superviseePositionIds == null) { throw new ArgumentNullException("superviseePositionIds"); }
            if (!superviseePositionIds.Any()) { throw new ArgumentException("Cannot pass in an empty list of positionIds to get PersonEmploymentStatuses"); }

            var criteria = "WITH PERSTAT.PRIMARY.POS.ID EQ ?";
            var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria, superviseePositionIds.Select(id => string.Format("\"{0}\"", id)).ToArray()); // wrap each Id in quotes for successful read

            if (perstatKeys == null)
            {
                var message = "Unexpected null returned from PERSTAT SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!perstatKeys.Any())
            {
                logger.Error("No PERSTAT keys exist for the given person Ids: " + string.Join(",", superviseePositionIds));
            }

            var perstatRecords = new List<Perstat>();
            for (int i = 0; i < perstatKeys.Count(); i += bulkReadSize)
            {
                var subList = perstatKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Perstat records");
                }
                else
                {
                    perstatRecords.AddRange(records);
                }
            }

            var domainPersonEmploymentStatuses = new List<PersonEmploymentStatus>();
            foreach (var record in perstatRecords)
            {
                PersonEmploymentStatus entityToAdd = null;
                try
                {
                    if (!string.IsNullOrEmpty(record.PerstatPrimaryPosId) && !string.IsNullOrEmpty(record.PerstatPrimaryPerposId))
                    {
                        entityToAdd = new PersonEmploymentStatus(
                        record.Recordkey,
                        record.PerstatHrpId,
                        record.PerstatPrimaryPosId,
                        record.PerstatPrimaryPerposId,
                        record.PerstatStartDate,
                        record.PerstatEndDate
                    );
                    }

                }
                catch (Exception e)
                {
                    // we don't want to use this record if there is an error creating it
                    LogDataError("Perstat", record.Recordkey, new object(), e, e.Message);
                    entityToAdd = null;
                }
                if (entityToAdd != null)
                    domainPersonEmploymentStatuses.Add(entityToAdd);
            }
            logger.Debug(string.Format("*************List of PersonEmployment Statuses based on the following positions {0} retrieved successfully**********", string.Join(",", superviseePositionIds.Distinct())));
            return domainPersonEmploymentStatuses;
        }


        /// <summary>
        /// Extracts supervisee Ids based on their person position and person employment status information
        /// PERPOS records contain all position history of a person
        /// PERSTAT records contain only primary position history of a person
        /// </summary>
        /// <param name="perposrecords">List of perpos records</param>
        /// <param name="perstatrecords">List of person employment status records</param>
        /// <param name="supervisorid">person id of the logged in supervisor</param>
        /// <param name="superviseeIds">Output - List of supervisees for the logged in supervisor Id</param>
        private void ExtractLeaveApprovalSupervisees(IEnumerable<Perpos> perposrecords, IEnumerable<PersonEmploymentStatus> perstatrecords, string supervisorid, ref List<string> superviseeIds)
        {
            var currentDate = DateTime.Now;
            if (perposrecords != null && perposrecords.Any())
            {
                //Extract Currently Active Perpos records
                perposrecords = perposrecords.Where(ds => ds.PerposStartDate <= currentDate && (ds.PerposEndDate >= currentDate || ds.PerposEndDate == null));

                //Extract only POSD supervisees based on their primary position without any direct supervisor (or) currently logged in user as their supervisor
                foreach (var perposrecord in perposrecords)
                {
                    if (string.IsNullOrWhiteSpace(perposrecord.PerposHrpId))
                    {
                        LogDataError("PosdSuperviseePerposHrpId", perposrecord.Recordkey, new object());
                    }
                    else
                    {
                        //get the primary position for the supervisee from perstat and filter the same from perpos
                        string primaryPositionId = perstatrecords.Where(ps => ps.PersonId == perposrecord.PerposHrpId).Select(ps => ps.PrimaryPositionId).FirstOrDefault(fd => fd != null);
                        if ((perposrecord.PerposPositionId == primaryPositionId) && (string.IsNullOrWhiteSpace(perposrecord.PerposSupervisorHrpId) || perposrecord.PerposSupervisorHrpId == supervisorid))
                            superviseeIds.Add(perposrecord.PerposHrpId);
                    }
                }
            }

        }
    }
}