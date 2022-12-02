/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */

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
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Interact with Positions data from database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PositionRepository : BaseColleagueRepository, IPositionRepository
    {
        private const string PositionsCacheKey = "Positions";
        private const string PositionsCacheKeyCollection = "PositionsCollection";
        private readonly int bulkReadSize;
        const string AllPositionsRecordsCache = "AllPositionsRecordsKeys";
        const int AllPositionsRecordsCacheTimeout = 20;
        private RepositoryException exception = new RepositoryException();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public PositionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;

        #region Position

        /// <summary>
        /// Get all Position objects, built from database data
        /// </summary>
        /// <returns>A list of Position objects</returns>
        public async Task<IEnumerable<Position>> GetPositionsErrorCollectionsAsync()
        {
            //Marilyn said something about not caching positions. 
            //Potentially only get active positions unless otherwise specified.
            //Maybe Cache historical positions

            return await GetOrAddToCacheAsync<IEnumerable<Position>>(PositionsCacheKeyCollection, async () => await this.BuildAllPositionsCollection(), Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Get all Position objects, built from database data
        /// </summary>
        /// <returns>A list of Position objects</returns>
        public async Task<IEnumerable<Position>> GetPositionsAsync()
        {
            //Marilyn said something about not caching positions. 
            //Potentially only get active positions unless otherwise specified.
            //Maybe Cache historical positions

            return await GetOrAddToCacheAsync<IEnumerable<Position>>(PositionsCacheKey, async () => await this.BuildAllPositions(), Level1CacheTimeoutValue);
        }

        private async Task<IEnumerable<Position>> BuildAllPositions()
        {
                var positionEntities = new List<Position>();
                var positionIds = await DataReader.SelectAsync("POSITION", "");
                if (positionIds == null)
                {
                    var message = "Unexpected: Null position Ids returned from select";
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                for (int i = 0; i < positionIds.Count(); i += bulkReadSize)
                {
                    var subList = positionIds.Skip(i).Take(bulkReadSize);
                    var positionRecords = await DataReader.BulkReadRecordAsync<DataContracts.Position>(subList.ToArray());
                    if (positionRecords == null)
                    {
                        var message = string.Format("Unexpected: Bulk data read using keys of position records returned null.");
                        logger.Error(message);
                    }
                    else
                    {
                        foreach (var positionRecord in positionRecords)
                        {
                            try
                            {
                                // If the logExceptionsOnly flag is passed, then call the 'legacy' build positions helper method which will
                                // throw execeptions as they occur, as opposed to the BuildPositionErrorCollection method which will
                                // add the errors to the RepositoryException
                                var positionDomainEntity = BuildPosition(positionRecord);

                            if (positionDomainEntity != null)
                                positionEntities.Add(positionDomainEntity);
                            
                        }
                        catch (RepositoryException ex)
                        {
                            foreach (var error in ex.Errors)
                            {
                                LogDataError("Position", positionRecord.Recordkey, null, ex, error.Message);
                            }
                        }
                        catch (Exception e)
                        {
                            LogDataError("Position", positionRecord.Recordkey,null, e, e.Message);
                        }
                    }

                }
            }

            return positionEntities;
        }

        private async Task<IEnumerable<Position>> BuildAllPositionsCollection()
        {
            var positionEntities = new List<Position>();
            var positionIds = await DataReader.SelectAsync("POSITION", "");
            if (positionIds == null)
            {
                var message = "Unexpected: Null position Ids returned from select";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            for (int i = 0; i < positionIds.Count(); i += bulkReadSize)
            {
                var subList = positionIds.Skip(i).Take(bulkReadSize);
                var positionRecords = await DataReader.BulkReadRecordAsync<DataContracts.Position>(subList.ToArray());
                if (positionRecords == null)
                {
                    var message = string.Format("Unexpected: Bulk data read using keys of position records returned null.");
                    logger.Error(message);
                }
                else
                {
                    foreach (var positionRecord in positionRecords)
                    {
                        try
                        {
                            // If the logExceptionsOnly flag is passed, then call the 'legacy' build positions helper method which will
                            // throw execeptions as they occur, as opposed to the BuildPositionErrorCollection method which will
                            // add the errors to the RepositoryException
                            var positionDomainEntity = BuildPosition(positionRecord);

                            if (positionDomainEntity != null)
                                positionEntities.Add(positionDomainEntity);

                        }
                        catch (RepositoryException ex)
                        {
                            foreach (var error in ex.Errors)
                            {
                                LogDataError("Position", positionRecord.Recordkey, positionRecord, ex, error.Message);
                            }
                        }
                        catch (Exception e)
                        {
                            LogDataError("Position", positionRecord.Recordkey, positionRecord, e, e.Message);
                        }
                    }

                    if (exception != null && exception.Errors != null && exception.Errors.Any())
                    {
                        return null;
                    }
                }
            }

            return positionEntities;
        }


        /// <summary>
        /// Helper to build a position object from a Position record
        /// </summary>
        /// <param name="positionRecord"></param>
        /// <returns></returns>
        private Position BuildPositionErrorCollection(DataContracts.Position positionRecord)
        {
            if (positionRecord == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Failed to build position.  The positionRecord is null."));
                //throw new ArgumentNullException("positionRecord");
            }

            if (positionRecord == null)
            {
                return null;
            }
            else
            {
                if (!positionRecord.PosStartDate.HasValue)
                {
                    exception.AddError(new RepositoryError("Bad.Data", "Position Start Date must have a value.")
                    {
                        Id = positionRecord.RecordGuid,
                        SourceId = positionRecord.Recordkey
                    });
                    //throw new ArgumentException("Position Start Date must have a value");
                }

                bool isSalary = false;
                if (string.IsNullOrEmpty(positionRecord.PosHrlyOrSlry))
                {
                    exception.AddError(new RepositoryError("Bad.Data", "Position must be Hourly or Salary.")
                    {
                        Id = positionRecord.RecordGuid,
                        SourceId = positionRecord.Recordkey
                    });
                    //throw new ArgumentException("Position must be Hourly or Salary");
                }
                else
                {
                    //position is salaried if db column is "S". posiiton is hourly if db column is "H"
                    isSalary = positionRecord.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase);
                }

                TimecardType timecardType;
                if (!Enum.TryParse(positionRecord.PosTimeEntryForm, true, out timecardType))
                {
                    timecardType = TimecardType.None;
                }

                if (string.IsNullOrWhiteSpace(positionRecord.PosTitle))
                {
                    exception.AddError(new RepositoryError("Bad.Data", "The position title is required but is missing from the position record.")
                    {
                        Id = positionRecord.RecordGuid,
                        SourceId = positionRecord.Recordkey
                    });
                }
                if (string.IsNullOrWhiteSpace(positionRecord.PosShortTitle))
                {
                    exception.AddError(new RepositoryError("Bad.Data", "The position short title is required but is missing from the position record.")
                    {
                        Id = positionRecord.RecordGuid,
                        SourceId = positionRecord.Recordkey
                    });
                }
                if (string.IsNullOrWhiteSpace(positionRecord.PosDept))
                {
                    exception.AddError(new RepositoryError("Bad.Data", "The position department is required but is missing from the position record.")
                    {
                        Id = positionRecord.RecordGuid,
                        SourceId = positionRecord.Recordkey
                    });
                }

                Position positionEntity;
                try
                {
                    positionEntity = new Position(positionRecord.Recordkey, positionRecord.PosTitle, positionRecord.PosShortTitle, positionRecord.PosDept, positionRecord.PosStartDate.Value, isSalary)
                    {
                        EndDate = positionRecord.PosEndDate,
                        IsExempt = positionRecord.PosExemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase),
                        SupervisorPositionId = positionRecord.PosSupervisorPosId,
                        AlternateSupervisorPositionId = positionRecord.PosAltSuperPosId,
                        PositionPayScheduleIds = positionRecord.AllPospay,
                        TimecardType = timecardType,

                        Guid = positionRecord.RecordGuid,
                        PositionAuthorizedDate = positionRecord.PosAuthorizedDate,
                        PositionClass = positionRecord.PosClass,
                        PositionLocation = positionRecord.PosLocation,
                        PositionJobDesc = positionRecord.PosJobDesc
                    };
                }
                catch (Exception)
                {
                    return null;
                }


                return positionEntity;
            }
        }

        /// <summary>
        /// Helper to build a position object from a Position record
        /// </summary>
        /// <param name="positionRecord"></param>
        /// <returns></returns>
        private Position BuildPosition(DataContracts.Position positionRecord)
        {
            if (positionRecord == null)
            {
                throw new ArgumentNullException("positionRecord");
            }

            if (!positionRecord.PosStartDate.HasValue)
            {
                throw new ArgumentException("Position Start Date must have a value");
            }

            if (string.IsNullOrEmpty(positionRecord.PosHrlyOrSlry))
            {
                throw new ArgumentException("Position must be Hourly or Salary");
            }

            //position is salaried if db column is "S". posiiton is hourly if db column is "H"
            var isSalary = positionRecord.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase);

            TimecardType timecardType;
            if (!Enum.TryParse(positionRecord.PosTimeEntryForm, true, out timecardType))
            {
                timecardType = TimecardType.None;
            }

            var positionEntity = new Position(positionRecord.Recordkey, positionRecord.PosTitle, positionRecord.PosShortTitle, positionRecord.PosDept, positionRecord.PosStartDate.Value, isSalary)
            {
                EndDate = positionRecord.PosEndDate,
                IsExempt = positionRecord.PosExemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase),
                SupervisorPositionId = positionRecord.PosSupervisorPosId,
                AlternateSupervisorPositionId = positionRecord.PosAltSuperPosId,
                PositionPayScheduleIds = positionRecord.AllPospay,
                TimecardType = timecardType,

                Guid = positionRecord.RecordGuid,
                PositionAuthorizedDate = positionRecord.PosAuthorizedDate,
                PositionClass = positionRecord.PosClass,
                PositionLocation = positionRecord.PosLocation,
                PositionJobDesc = positionRecord.PosJobDesc
            };

            return positionEntity;
        }



        /// <summary>
        /// Returns Position Entity as per the page criteria
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="campus">The physical location of the institution position</param>
        /// <param name="status">The status of the position (e.g. active, frozen, cancelled, inactive)</param>
        /// <param name="bargainingUnit">The group or union associated with the position</param>
        /// <param name="reportsToPositions">The positions to which this position reports</param>
        /// <param name="exemptionType">An indicator if the position is exempt or non-exempt</param>
        /// <param name="compensationType">The type of compensation awarded (e.g. salary, wages, etc.)</param>
        /// <param name="startOn">The date when the position is first available</param>
        /// <param name="endOn">The date when the position is last available</param>
        /// <returns>List of Position Entities</returns>
        public async Task<Tuple<IEnumerable<Position>, int>> GetPositionsAsync(int offset, int limit, string code = "", string campus = "", string status = "", string bargainingUnit = "",
            List<string> reportsToPositions = null, string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false)
        {
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllPositionsRecordsCache, code, campus, status, bargainingUnit,
                reportsToPositions, exemptionType, compensationType, startOn, endOn);

            int totalCount = 0;
            StringBuilder criteria = new StringBuilder();

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                "POSITION",
                offset,
                limit,
                AllPositionsRecordsCacheTimeout,
                async () =>
                {
                    if (!string.IsNullOrEmpty(startOn) || !string.IsNullOrEmpty(endOn))
                    {
                        if (!string.IsNullOrEmpty(startOn))
                        {
                            criteria.Append("WITH POS.START.DATE GE '");
                            criteria.Append(startOn);
                            criteria.Append("'");
                        }
                        if (!string.IsNullOrEmpty(endOn))
                        {
                            if (criteria.Length > 0)
                            {
                                criteria.Append("AND ");
                            }
                            criteria.Append("WITH POS.END.DATE LE '");
                            criteria.Append(endOn);
                            criteria.Append("'");
                            criteria.Append(" AND POS.END.DATE NE ''");
                        }
                    }
                    else
                    {
                        criteria.Append("WITH POS.START.DATE NE ''");
                    }
                    criteria.Append(" AND WITH POS.HRLY.OR.SLRY NE ''");

                    var today = await GetUnidataFormatDateAsync(DateTime.Now);
                    if (!string.IsNullOrEmpty(status))
                    {
                        switch (status)
                        {
                            case "active":
                                criteria.AppendFormat(" AND WITH POS.START.DATE LE '{0}'", today);
                                criteria.Append(" AND WITH POS.END.DATE EQ ''");
                                criteria.AppendFormat(" OR POS.END.DATE GT '{0}'", today);
                                break;
                            case "inactive":
                                criteria.AppendFormat(" AND WITH POS.START.DATE GE '{0}'", today);
                                break;
                            case "cancelled":
                                criteria.AppendFormat(" AND WITH POS.END.DATE LE '{0}'", today);
                                criteria.Append(" AND WITH POS.END.DATE NE ''");
                                break;
                            default:
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(campus))
                    {
                        criteria.Append(" AND WITH POS.LOCATION = '");
                        criteria.Append(campus);
                        criteria.Append("'");
                    }

                    if (!string.IsNullOrEmpty(bargainingUnit))
                    {
                        criteria.Append(" AND WITH POS.BARGAINING.UNIT = '");
                        criteria.Append(bargainingUnit);
                        criteria.Append("'");
                    }

                    if (reportsToPositions != null && reportsToPositions.Any())
                    {
                        foreach (var reportsToPosition in reportsToPositions)
                        {
                            if (!string.IsNullOrEmpty(reportsToPosition))
                            {
                                criteria.Append("AND WITH POS.SUPERVISOR.POS.ID = '");
                                criteria.Append(reportsToPosition);
                                criteria.Append("'");
                                criteria.AppendFormat(" OR POS.ALT.SUPER.POS.ID = '{0}'", reportsToPosition);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(exemptionType))
                    {
                        switch (exemptionType.ToLower())
                        {
                            case "exempt":
                                criteria.Append(" AND WITH POS.EXEMPT.OR.NOT = 'E'");
                                break;
                            case "nonexempt":
                                criteria.Append(" AND WITH POS.EXEMPT.OR.NOT = 'N'");
                                break;
                            default:
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(compensationType))
                    {
                        switch (compensationType)
                        {
                            case "salary":
                                criteria.Append(" AND WITH POS.HRLY.OR.SLRY = 'S'");
                                break;
                            case "wages":
                                criteria.Append(" AND WITH POS.HRLY.OR.SLRY = 'H'");
                                break;
                            default:
                                break;
                        }
                    }
                    var limitingKeys = new List<string>();
                    if (!string.IsNullOrEmpty(code))
                    {
                        limitingKeys.Add(code);
                    }

                    return new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = criteria.ToString(),
                        limitingKeys = limitingKeys != null && limitingKeys.Any() ? limitingKeys : null
                    };
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<Position>, int>(new List<Position>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            var subList = keyCacheObject.Sublist.ToArray();

            if (subList == null || !subList.Any())
            {
                return new Tuple<IEnumerable<Position>, int>(new List<Position>(), 0);
            }

            var positionData = await DataReader.BulkReadRecordAsync<DataContracts.Position>("POSITION", subList);

            var positionEntities = new List<Position>();
            foreach (var position in positionData)
            {
                var positionEntity = BuildPositionErrorCollection(position);
                if (positionEntity != null)
                    positionEntities.Add(positionEntity);
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<Position>, int>(positionEntities, totalCount);
        }

        /// <summary>
        /// Get Position by a guid
        /// </summary>
        /// <param name="guid">Guids</param>
        /// <returns>Positions entity objects</returns>
        public async Task<Position> GetPositionByGuidAsync(string guid)
        {
            Position position = null;
            if (!(string.IsNullOrEmpty(guid)))
            {
                var id = await GetPositionIdFromGuidAsync(guid);

                try
                {
                    if (!(string.IsNullOrEmpty(id)))
                    {
                        var positions = await DataReader.ReadRecordAsync<DataContracts.Position>("POSITION", id);

                        position = BuildPositionErrorCollection(positions);
                    }
                }
                catch
                    (Exception e)
                {
                    logger.Error(string.Format("Could not build position for guid {0}", guid));
                    logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);

                }
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return position;
        }

        /// <summary>
        /// Get Position by ID
        /// </summary>
        /// <param name="id">Guids</param>
        /// <returns>Positions entity objects</returns>
        public async Task<Position> GetPositionByIdAsync(string id)
        {
            exception = new RepositoryException();
            Position position = null;
            if (!(string.IsNullOrEmpty(id)))
            {
                var positions = await DataReader.ReadRecordAsync<DataContracts.Position>("POSITION", id);
                position = BuildPositionErrorCollection(positions);
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return position;
        }

        /// <summary>
        /// Get the GUID from a position ID
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <returns>The GUID</returns>
        public async Task<string> GetPositionGuidFromIdAsync(string positionId)
        {
            if (string.IsNullOrEmpty(positionId))
                throw new ArgumentNullException("positionId", "Position ID is a required argument.");

            var lookup = new RecordKeyLookup("POSITION", positionId, false);
            var result = await DataReader.SelectAsync(new RecordKeyLookup[] { lookup });
            if (result != null && result.Count > 0)
            {
                RecordKeyLookupResult lookupResult = null;
                if (result.TryGetValue(lookup.ResultKey, out lookupResult))
                {
                    if (lookupResult != null)
                    {
                        return lookupResult.Guid;
                    }
                }
            }

            throw new ArgumentOutOfRangeException("positionId", "No GUID found for position ID " + positionId);
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetPositionIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Position GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Position GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "POSITION")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, POSITION", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("GUID.Wrong.Type", errorMessage)
                {
                    Id = guid,
                    SourceId = foundEntry.Value.PrimaryKey
                });
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        #endregion

        #region PositionPay

        /// <summary>
        /// Get PositionPay by a id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>PositionPay entity objects</returns>
        public async Task<PositionPay> GetPositionPayByIdAsync(string id)
        {
            PositionPay positionPay = null;
            try
            {
                if (!(string.IsNullOrEmpty(id)))
                {
                    var positions = await DataReader.ReadRecordAsync<DataContracts.Pospay>("POSPAY", id);
                    positionPay = await BuildPositionPay(positions);
                }
            }
            catch
                (Exception e)
            {
                logger.Error(string.Format("Could not build positionPay for id {0}", id));
                logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
            }
            return positionPay;
        }

        /// <summary>
        /// Get PositionPay by a Collection of ids
        /// </summary>
        /// <param name="ids">List of Ids</param>
        /// <returns>Collection of PositionPay entity objects</returns>
        public async Task<IEnumerable<PositionPay>> GetPositionPayByIdsAsync(IEnumerable<string> ids)
        {
            if ((ids == null) || (!ids.Any()))
            {
                throw new ArgumentNullException("Ids are required to get PositionPay");
            }
            var positionPays = new List<PositionPay>();
            foreach (var id in ids.ToList())
            {
                try
                {
                    positionPays.Add(await GetPositionPayByIdAsync(id));
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Could not build positionPay for id {0}", id));
                    logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
                }
            }
            return positionPays;
        }

        /// <summary>
        /// Helper to build a positionPay object from a PosPay record
        /// </summary>
        /// <param name="posPayRecord"></param>
        /// <returns></returns>
        private async Task<PositionPay> BuildPositionPay(DataContracts.Pospay posPayRecord)
        {
            if (posPayRecord == null)
            {
                throw new ArgumentNullException("posPayRecord");
            }

            var positionPayEntity = new PositionPay(posPayRecord.Recordkey);
            positionPayEntity.AuthorizedDate = posPayRecord.PospayAuthorizedDate;
            positionPayEntity.BargainingUnit = posPayRecord.PospayBargainingUnit;

            positionPayEntity.EndDate = posPayRecord.PospayEndDate;
            positionPayEntity.StartDate = posPayRecord.PospayStartDate;

            positionPayEntity.SalaryMaximum = posPayRecord.PospaySalaryMax;
            positionPayEntity.SalaryMinimum = posPayRecord.PospaySalaryMin;

            positionPayEntity.CycleWorkTimeAmount = posPayRecord.PospayCycleWorkTimeAmt;
            positionPayEntity.YearWorkTimeAmount = posPayRecord.PospayYearWorkTimeAmt;
            positionPayEntity.CycleWorkTimeUnits = posPayRecord.PospayCycleWorkTimeUnits;
            positionPayEntity.YearWorkTimeUnits = posPayRecord.PospayYearWorkTimeUnits;

            positionPayEntity.PospayFndgGlNo = posPayRecord.PospayFndgGlNo;
            positionPayEntity.PospayPrjFndgGlNo = positionPayEntity.PospayPrjFndgGlNo;

            positionPayEntity.PospayFndgPct = posPayRecord.PospayFndgPct;

            positionPayEntity.HostCountry = await GetHostCountryAsync();

            var fundingSources = new List<PositionFundingSource>();
            foreach (var fundingRecord in posPayRecord.PospayFndgEntityAssociation)
            {

                var fundingSource = new PositionFundingSource(
                    fundingRecord.PospayFndgGlNoAssocMember,
                    posPayRecord.PospayFndgGlNo.IndexOf(fundingRecord.PospayFndgGlNoAssocMember)
                    )
                {
                    ProjectId = fundingRecord.PospayFndgProjIdAssocMember
                };
                if (!string.IsNullOrEmpty(fundingRecord.PospayFndgProjIdAssocMember))
                {
                    var project = await DataReader.ReadRecordAsync<Projects>("PROJECTS", fundingRecord.PospayFndgProjIdAssocMember);

                    if (project != null)
                    {
                        fundingSource.ProjectRefNumber = project.PrjRefNo;
                    }
                }
                fundingSources.Add(fundingSource);
            }
            positionPayEntity.FundingSource = fundingSources;

            return positionPayEntity;
        }
        #endregion

        #region Private methods

        private async Task<string> GetHostCountryAsync()
        {
            if (_internationalParameters == null)
                _internationalParameters = await GetInternationalParametersAsync();
            return _internationalParameters.HostCountry;
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        private async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await GetInternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }

        public Task<IEnumerable<Position>> GetPositionsErrorCollectionAsync()
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
