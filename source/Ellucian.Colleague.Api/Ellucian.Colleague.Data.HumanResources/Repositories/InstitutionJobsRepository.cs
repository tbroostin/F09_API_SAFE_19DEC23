/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Data.Colleague.DataContracts;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{

    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class InstitutionJobsRepository : BaseColleagueRepository, IInstitutionJobsRepository
    {
        private readonly int _bulkReadSize;
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;

        public InstitutionJobsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get InstitutionJobs by a guid
        /// </summary>
        /// <param name="guid">Guids</param>
        /// <returns>InstitutionJobs entity objects</returns>
        public async Task<InstitutionJobs> GetInstitutionJobsByGuidAsync(string guid)
        {
            if (!(string.IsNullOrEmpty(guid)))
            {
                var id = await GetInstitutionJobsIdFromGuidAsync(guid);

                try
                {
                    if (!(string.IsNullOrEmpty(id)))
                    {
                        return await GetInstitutionJobsByIdAsync(id);
                    }
                }
                catch
                    (Exception e)
                {
                    logger.Error(string.Format("Could not build position for guid {0}", guid));
                    logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);

                }
            }
            return null;
        }

        /// <summary>
        /// GetInstitutionJobsAsync
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="personCode">The employee to which the job is associated</param>
        /// <param name="employerCode">institution to which the job belongs</param>
        /// <param name="positionCode">The position associated with the job. </param>
        /// <param name="departmentCode">The department of the institution to which the job belongs.</param>
        /// <param name="convertedStartOn">The first day of the job.</param>
        /// <param name="convertedEndOn">The last day of the job</param>
        /// <param name="status">The status of the employee on the job.</param>
        /// <param name="classificationCode">The employment classification associated with the position. </param>
        /// <param name="preference">The preference for a job </param>
        /// <param name="bypassCache"></param>
        /// <param name="filterQualifiers"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<InstitutionJobs>, int>> GetInstitutionJobsAsync(int offset, int limit, string personCode = "",
            string employerCode = "", string positionCode = "", string departmentCode = "", string convertedStartOn = "",
            string convertedEndOn = "", string status = "", string classificationCode = "", string preference = "", bool bypassCache = false, Dictionary<string,string> filterQualifiers = null)
        {
            var perposIds = new List<string>();
            var selectionCriteria = new StringBuilder();
            var coreDefaultData = GetDefaults();

            try
            {
                var startOnOperation = filterQualifiers != null && filterQualifiers.ContainsKey("StartOn") ? filterQualifiers["StartOn"] : "EQ";
                var endOnOperation = filterQualifiers != null && filterQualifiers.ContainsKey("EndOn") ? filterQualifiers["EndOn"] : "EQ";

                //startOn - select any records in PERPOS using PERPOS.START.DATE
                //endOn - select any records in PERPOS using PERPOS.END.DATE 
                if (!string.IsNullOrEmpty(convertedStartOn) || !string.IsNullOrEmpty(convertedEndOn))
                {
                    if (!string.IsNullOrEmpty(convertedStartOn))
                    {
                        selectionCriteria.Append(string.Format("WITH PERPOS.START.DATE {0} '", startOnOperation));
                        selectionCriteria.Append(convertedStartOn);
                        selectionCriteria.Append("'");
                    }
                    if (!string.IsNullOrEmpty(convertedEndOn))
                    {
                        if (selectionCriteria.Length > 0)
                        {
                            selectionCriteria.Append("AND ");
                        }
                        selectionCriteria.Append(string.Format("WITH PERPOS.END.DATE {0} '", endOnOperation));
                        selectionCriteria.Append(convertedEndOn);
                        selectionCriteria.Append("'");
                        selectionCriteria.Append(" AND PERPOS.END.DATE NE ''");
                    }
                }
                else
                {
                    selectionCriteria.Append("WITH PERPOS.START.DATE NE ''");
                }

                //person - select any record in PERPOS where the PERPOS.HRP.ID matches the person specified in the filter
                if (!string.IsNullOrEmpty(personCode))
                {
                    selectionCriteria.Append(" AND WITH PERPOS.HRP.ID EQ '");
                    selectionCriteria.Append(personCode);
                    selectionCriteria.Append("'");
                }


                //employer - if the employer specified in the filter matches the guid corresponding to the DEFAULT.HOST.CORP.ID then return all records in PERPOS. Otherwise if the employer is anything else, then the filter won't return any results.
                if (!(string.IsNullOrEmpty(employerCode)))
                {                   
                    if (coreDefaultData != null)
                    {
                        if (employerCode != coreDefaultData.DefaultHostCorpId)
                            return new Tuple<IEnumerable<InstitutionJobs>, int>(new List<InstitutionJobs>(), 0);
                    }
                }

                //position - select any records in PERPOS where the PERPOS.POSITION.ID matches the position ID corresponding to the guid specified in the filter.
                if (!string.IsNullOrEmpty(positionCode))
                {
                    selectionCriteria.Append(" AND WITH PERPOS.POSITION.ID EQ '");
                    selectionCriteria.Append(positionCode);
                    selectionCriteria.Append("'");
                }

                string[] positionData = null;
                //department - select any records in PERPOS where the corresponding position department (POS.DEPT) matches the department code specified in the filter (this filter does not use a guid!). Consider using the PERPOS.DEPT computed column.               
                if (!string.IsNullOrEmpty(departmentCode))
                {
                    //var positionData = await DataReader.BulkReadRecordAsync<DataContracts.Position>("POSITION", string.Concat("WITH POS.DEPT EQ '", departmentCode, "'"));
                    positionData = await DataReader.SelectAsync("POSITION", string.Concat("WITH POS.DEPT EQ '", departmentCode, "'"));
                    if (positionData != null && positionData.Any())
                    {
                        selectionCriteria.Append(" AND WITH PERPOS.POSITION.ID EQ ");
                        foreach (var pos in positionData)
                        {
                            selectionCriteria.Append("'");
                            //selectionCriteria.Append(pos.Recordkey);
                            selectionCriteria.Append(pos);
                            selectionCriteria.Append("' ");
                        }
                    }
                    else
                    {
                        return new Tuple<IEnumerable<InstitutionJobs>, int>(new List<InstitutionJobs>(), 0);
                    }
                }

                //classification - select any records in PERPOS where the corresponding position classification (POS.CLASS) matches the classification code corresponding to the guid specified in the filter.
                if (!string.IsNullOrEmpty(classificationCode))
                {
                    //var positionClassData = await DataReader.BulkReadRecordAsync<DataContracts.Position>("POSITION", string.Concat("WITH POS.CLASS EQ '", classificationCode, "'"));
                    var positionClassData = (positionData == null)
                        ? await DataReader.SelectAsync("POSITION", string.Concat("WITH POS.CLASS EQ '", classificationCode, "'"))
                        : await DataReader.SelectAsync("POSITION", positionData, string.Concat("WITH POS.CLASS EQ '", classificationCode, "'"));
                    if (positionClassData != null && positionClassData.Any())
                    {
                        selectionCriteria.Append(" AND WITH PERPOS.POSITION.ID EQ ");
                        foreach (var pos in positionClassData)
                        {
                            selectionCriteria.Append("'");
                            //selectionCriteria.Append(pos.Recordkey);
                            selectionCriteria.Append(pos);
                            selectionCriteria.Append("' ");
                        }
                    }
                    else
                    {
                        return new Tuple<IEnumerable<InstitutionJobs>, int>(new List<InstitutionJobs>(), 0);
                    }
                }

                if (!string.IsNullOrEmpty(status))
                {
                    var today = await GetUnidataFormatDateAsync(DateTime.Now);
                    switch (status.ToLower())
                    {
                        //status (active) - select any records in PERPOS where the PERPOS.END.DATE is null or the PERPOS.END.DATE 
                        //is on or after the request date.
                        case "active":
                            // selectionCriteria.AppendFormat(" AND WITH PERPOS.START.DATE LE '{0}'", today);
                            selectionCriteria.Append(" AND WITH PERPOS.END.DATE EQ ''");
                            selectionCriteria.AppendFormat(" OR PERPOS.END.DATE GT '{0}'", today);
                            break;
                        //status (ended) - select any records in PERPOS where the PERPOS.END.DATE is not null and is a 
                        //date prior to the request date.
                        case "ended":
                            selectionCriteria.AppendFormat(" AND WITH PERPOS.END.DATE LE '{0}'", today);
                            selectionCriteria.Append(" AND WITH PERPOS.END.DATE NE ''");
                            break;
                        default:
                            break;
                    }
                }

                // preference (primary) - select all active status records in PERSTAT to find the current status records (PERSTAT that's considered active based on the request date) and then publish the corresponding PERPOS record (PERSTAT.PRIMARY.PERPOS.ID). We might need to create a computed column for this filter, not 100% sure. Refer to the logic used to publish the preference property.
                // The HRPER file does capture the status date ranges itself in HRP.PERSTAT.START.DATE and 
                // HRP.PERSTAT.END.DATE...so we can check these date ranges from HRPER to determine the current status 
                // record, then read the corresponding PERSTAT entry to get the PERSTAT.PRIMARY.POS.ID and then see 
                // if it matches the current position we're integrating above in position.id then populate this 
                // property with the "primary" enumeration.            
                if (!(string.IsNullOrEmpty(preference)))
                {
                    //if we have additional selection criteria - use it to filter out preference records
                    var primaryids = (await GetPrimaryInstitutionJobsPerposIdsAsync(selectionCriteria.ToString())).ToArray();
                    if (primaryids != null)
                    {
                        perposIds.AddRange(primaryids);
                    }
                }
                else
                {
                    perposIds.AddRange(await DataReader.SelectAsync("PERPOS", selectionCriteria.ToString()));
                }

                var totalCount = perposIds.Count();

                perposIds.Sort();
                var perposSubList = perposIds.Skip(offset).Take(limit).ToArray();
                var perposRecords = await DataReader.BulkReadRecordAsync<DataContracts.Perpos>("PERPOS", perposSubList);

                var personIds = perposRecords.Select(e => e.PerposHrpId);

                var criteria = string.Format("WITH PPWG.PERPOS.ID EQ ? AND PPWG.TYPE NE 'M'");
                //var perposwgKeys = await DataReader.SelectAsync("PERPOSWG", criteria, perposSubList);
                var perposwgKeys = await DataReader.SelectAsync("PERPOSWG", criteria, perposSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                //bulkread the records for all the keys
                var perposwgRecords = new List<Perposwg>();
                for (var i = 0; i < perposwgKeys.Count(); i += _bulkReadSize)
                {
                    var subList = perposwgKeys.Skip(i).Take(_bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Perposwg>(subList.ToArray());
                    if (records != null)
                    {
                        perposwgRecords.AddRange(records);
                    }
                }


                var positionIds = perposRecords.Select(e => e.PerposPositionId);

                criteria = string.Format("WITH POSITION.ID EQ ?");
                var positionKeys = await DataReader.SelectAsync("POSITION", criteria, positionIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

                //bulkread the records for all the keys
                var positionRecords = new List<DataContracts.Position>();
                for (var i = 0; i < positionKeys.Count(); i += _bulkReadSize)
                {
                    var subList = positionKeys.Skip(i).Take(_bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<DataContracts.Position>(subList.ToArray());
                    if (records != null)
                    {
                        positionRecords.AddRange(records);
                    }
                }

                // select all the PERBEN ids with the HRP.ID equal to the input person id.
                criteria = string.Format("WITH PERBEN.HRP.ID EQ ?");
                var perbenKeys = await DataReader.SelectAsync("PERBEN", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

                var perbenRecords = new List<Perben>();
                for (int i = 0; i < perbenKeys.Count(); i += _bulkReadSize)
                {
                    var subList = perbenKeys.Skip(i).Take(_bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Perben>(subList.ToArray());
                    if (records != null)
                    {
                        perbenRecords.AddRange(records);
                    }
                }

                // select all the PERSTAT ids with the HRP.ID equal to the input person id.
                criteria = "WITH PERSTAT.HRP.ID EQ ?";
                var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

                var perstatRecords = new List<Perstat>();
                for (int i = 0; i < perstatKeys.Count(); i += _bulkReadSize)
                {

                    var subList = perstatKeys.Skip(i).Take(_bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                    if (records != null)
                    {
                        perstatRecords.AddRange(records);
                    }
                }

                // bulkread all HRPER records from Colleauge
                var hrperRecords = new List<Hrper>();
                for (int i = 0; i < personIds.Count(); i += _bulkReadSize)
                {
                    var subList = personIds.Skip(i).Take(_bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Hrper>(subList.ToArray());
                    if (records != null)
                    {
                        hrperRecords.AddRange(records);
                    }
                }

                var institutionJobsEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs>();

                var ldmDefaults = await DataReader.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");

                var hostCountry = await GetHostCountryAsync();
               
                var hostCorpId = coreDefaultData.DefaultHostCorpId;

                foreach (var perpos in perposRecords)
                {
                    if (perpos != null)
                    {
                        try
                        {
                            var positionRecord = positionRecords.Where(p => p.Recordkey == perpos.PerposPositionId);
                            var personPositionWages = perposwgRecords.Where(ppw => ppw.PpwgPerposId == perpos.Recordkey);
                            var personStatusRecords = perstatRecords.Where(ps => ps.PerstatHrpId == perpos.PerposHrpId);
                            var personBenefitsRecords = perbenRecords.Where(ps => ps.PerbenHrpId == perpos.PerposHrpId);
                            var hrPersonRecord = hrperRecords.Where(hr => hr.Recordkey == perpos.PerposHrpId);

                            //The data reader can sometimes get the wrong 
                            //var guid = perposRecord.RecordGuid;
                            //var criteria = string.Format("WITH LDM.GUID.PRIMARY.KEY EQ '{0}' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.ENTITY EQ 'PERPOS' AND LDM.GUID.REPLACED.BY EQ ''", perposSubList);
                            //var guidRecords = await DataReader.SelectAsync("LDM.GUID", criteria);
                            var guidList = await DataReader.SelectAsync("LDM.GUID", "WITH LDM.GUID.PRIMARY.KEY EQ '?' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.ENTITY EQ 'PERPOS' AND LDM.GUID.REPLACED.BY EQ ''", perposSubList);
                            var ldmGuidRecords = await DataReader.BulkReadRecordAsync<LdmGuid>(guidList);
                            Dictionary <string, string> ldmGuidCollection = new Dictionary<string, string>();
                            foreach (var ldmGuidRecord in ldmGuidRecords)
                            {
                                ldmGuidCollection.Add(ldmGuidRecord.LdmGuidPrimaryKey, ldmGuidRecord.Recordkey);
                            }
                          
                            institutionJobsEntities.Add(
                                await BuildInstitutionJobsAsync(perpos, personPositionWages, positionRecord, 
                                personBenefitsRecords, personStatusRecords, hrPersonRecord, ldmGuidCollection, 
                                ldmDefaults, hostCountry, hostCorpId));
                        }
                        catch (Exception e)
                        {
                            LogDataError("InstitutionJobs", perpos.Recordkey, perpos, e, e.Message);
                        }
                    }
                }
                return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobsEntities, totalCount);
            }
            catch (RepositoryException e)
            {
                throw e;
            }

        }


        /// <summary>
        /// Update an InstitutionJobs domain entity
        /// </summary>
        /// <param name="institutionJobsEntity"><see cref="InstitutionJobs">The InstitutionJobs domain entity to update</param>
        /// <returns><see cref="InstitutionJobs">The updated InstitutionJobs domain entity</returns>
        public async Task<InstitutionJobs> UpdateInstitutionJobsAsync(InstitutionJobs institutionJobsEntity)
        {

            if (institutionJobsEntity == null)
                throw new ArgumentNullException("institutionJobsEntity", "Must provide an institutionJobsEntity to update.");
            if (string.IsNullOrEmpty(institutionJobsEntity.Guid))
                throw new ArgumentNullException("institutionJobsEntity", "Must provide the guid of the institutionJobsEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var institutionJobsEntityId = await GetInstitutionJobsIdFromGuidAsync(institutionJobsEntity.Guid);

            if (!string.IsNullOrEmpty(institutionJobsEntityId))
            {
                var extendedDataTuple = GetEthosExtendedDataLists();

                var updateRequest = BuildInstitutionJobsUpdateRequest(institutionJobsEntity);

                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }
                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdateInstJobsRequest, CreateUpdateInstJobsResponse>(updateRequest);

                if (updateResponse.ErrorMessages.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating institutionJobsEntity '{0}':", institutionJobsEntity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.ErrorMessages.ForEach(e => exception.AddError(new RepositoryError("InstitutionJobs", e)));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated entity from the database
                return await GetInstitutionJobsByGuidAsync(institutionJobsEntity.Guid);
            }
            // perform a create instead
            return await CreateInstitutionJobsAsync(institutionJobsEntity);
        }

        /// <summary>
        /// Create an InstitutionJobs domain entity
        /// </summary>
        /// <param name="institutionJobsEntity"><see cref="InstitutionJobs">The InstitutionJobs domain entity to create</param>
        /// <returns><see cref="InstitutionJobs">The created InstitutionJobs domain entity</returns>
        public async Task<InstitutionJobs> CreateInstitutionJobsAsync(InstitutionJobs institutionJobsEntity)
        {
            if (institutionJobsEntity == null)
                throw new ArgumentNullException("institutionJobsEntity", "Must provide an institutionJobsEntity to create.");

            var extendedDataTuple = GetEthosExtendedDataLists();

            var createRequest = this.BuildInstitutionJobsUpdateRequest(institutionJobsEntity);

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            createRequest.InstJobId = null;
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<CreateUpdateInstJobsRequest, CreateUpdateInstJobsResponse>(createRequest);


            if (createResponse.ErrorMessages.Any())
            {
                var errorMessage = string.Format("Error(s) occurred creating institutionJobsEntity '{0}':", createResponse.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.ErrorMessages.ForEach(e => exception.AddError(new RepositoryError("InstitutionJobs", e)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created  from the database
            return await GetInstitutionJobsByGuidAsync(createResponse.Guid);
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetInstitutionJobsIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("PERPOS GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("PERPOS GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "PERPOS")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, PERPOS", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get an InstitutionJobs Entity object by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>InstitutionJobs Entity object</returns>
        private async Task<InstitutionJobs> GetInstitutionJobsByIdAsync(string guid)
        {
            InstitutionJobs institutionJobs = null;
            try
            {
                if (!(string.IsNullOrEmpty(guid)))
                {
                    var perpos = await DataReader.ReadRecordAsync<DataContracts.Perpos>("PERPOS", guid);

                    // select all the PERPOSWG ids with the HRP.ID equal to the input person id.
                    // excluding stipend records.
                    var criteria = string.Format("WITH PPWG.PERPOS.ID EQ '{0}' AND PPWG.TYPE NE 'M'", perpos.Recordkey);
                    var perposwgKeys = await DataReader.SelectAsync("PERPOSWG", criteria);


                    //bulkread the records for all the keys
                    var perposwgRecords = new List<Perposwg>();
                    for (var i = 0; i < perposwgKeys.Count(); i += _bulkReadSize)
                    {
                        var subList = perposwgKeys.Skip(i).Take(_bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Perposwg>(subList.ToArray());
                        if (records != null)
                        {
                            perposwgRecords.AddRange(records);
                        }
                    }

                    var personIds = perposwgRecords.Select(e => e.PpwgHrpId);

                    // select all the POSITION ids with the HRP.ID equal to the input person id.
                    criteria = string.Format("WITH POSITION.ID EQ '{0}'", perpos.PerposPositionId);
                    var positionKeys = await DataReader.SelectAsync("POSITION", criteria);

                    //bulkread the records for all the keys
                    var positionRecords = new List<DataContracts.Position>();
                    for (var i = 0; i < positionKeys.Count(); i += _bulkReadSize)
                    {
                        var subList = positionKeys.Skip(i).Take(_bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<DataContracts.Position>(subList.ToArray());
                        if (records != null)
                        {
                            positionRecords.AddRange(records);
                        }
                    }

                    // select all the PERBEN ids with the HRP.ID equal to the input person id.
                    criteria = string.Format("WITH PERBEN.HRP.ID EQ '{0}'", perpos.PerposHrpId);
                    var perbenKeys = await DataReader.SelectAsync("PERBEN", criteria);

                    var perbenRecords = new List<Perben>();
                    for (int i = 0; i < perbenKeys.Count(); i += _bulkReadSize)
                    {
                        var subList = perbenKeys.Skip(i).Take(_bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Perben>(subList.ToArray());
                        if (records != null)
                        {
                            perbenRecords.AddRange(records);
                        }
                    }

                    // select all the PERSTAT ids with the HRP.ID equal to the input person id.
                    criteria = "WITH PERSTAT.HRP.ID EQ ?";
                    var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

                    var perstatRecords = new List<Perstat>();
                    for (int i = 0; i < perstatKeys.Count(); i += _bulkReadSize)
                    {
                        var subList = perstatKeys.Skip(i).Take(_bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                        if (records != null)
                        {
                            perstatRecords.AddRange(records);
                        }
                    }

                    // bulkread all HRPER records from Colleauge
                    var hrperRecords = new List<Hrper>();
                    for (int i = 0; i < personIds.Count(); i += _bulkReadSize)
                    {
                        var subList = personIds.Skip(i).Take(_bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Hrper>(subList.ToArray());
                        if (records != null)
                        {
                            hrperRecords.AddRange(records);
                        }
                    }

                    Dictionary<string, string> ldmGuidCollection = new Dictionary<string, string>();                   
                    var ldmGuidCriteria = string.Format("WITH LDM.GUID.PRIMARY.KEY EQ '{0}' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.ENTITY EQ 'PERPOS' AND LDM.GUID.REPLACED.BY EQ ''", perpos.Recordkey);
                    var guidRecords = await DataReader.SelectAsync("LDM.GUID", ldmGuidCriteria);
                    if ((guidRecords == null) || (!guidRecords.Any()))
                    {
                        throw new ArgumentNullException("guid", string.Format("LDM.GUID Unable to locate guid for perpos id: {0}", perpos.Recordkey));

                    }
                    ldmGuidCollection.Add(perpos.Recordkey, guidRecords[0]);

                    var ldmDefaults = DataReader.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
                    var hostCountry = await GetHostCountryAsync();
                    var coreDefaultData = GetDefaults();
                    var hostCorpId = coreDefaultData.DefaultHostCorpId;

                    institutionJobs = await BuildInstitutionJobsAsync(perpos, perposwgRecords, positionRecords, 
                        perbenRecords, perstatRecords, hrperRecords, ldmGuidCollection, ldmDefaults, hostCountry, hostCorpId);
                }
            }
            catch
                (Exception e)
            {
                logger.Error(string.Format("Could not build positionPay for id {0}", guid));
                logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
            }
            return institutionJobs;
        }

        /// <summary>
        /// Helper to build InstitutionJobs objects
        /// </summary>
        /// <param name="perposRecord">the Perpos db record</param>
        /// <param name="perposwgRecords"></param>
        /// <param name="positionRecords"></param>
        /// <param name="personBenefitsRecords"></param>
        /// <param name="perstatRecords"></param>
        /// <param name="hrperRecords"></param>
        /// <returns>InstitutionJobs Entity object</returns>
        private async Task<InstitutionJobs> BuildInstitutionJobsAsync(Perpos perposRecord,
            IEnumerable<Perposwg> perposwgRecords, IEnumerable<DataContracts.Position> positionRecords,
            IEnumerable<Perben> personBenefitsRecords, IEnumerable<Perstat> perstatRecords, IEnumerable<Hrper> hrperRecords,
            Dictionary<string, string> perposGuidCollection, LdmDefaults ldmDefaults, string hostCountry, string hostCorpId)
        {
            if (perposRecord == null)
            {
                throw new ArgumentNullException("perposRecord");
            }

            if (!perposRecord.PerposStartDate.HasValue)
            {
                throw new ArgumentException(string.Concat("Perpos StartDate must have a value. RecordKey: ", perposRecord.Recordkey));
            }

            var currentDate = DateTime.Now;
         
            string guid = "";
            if (!perposGuidCollection.TryGetValue(perposRecord.Recordkey, out guid))
            {
                throw new ArgumentNullException("guid", string.Format("LDM.GUID Unable to locate guid for perpos id: {0}", perposRecord.Recordkey));
            }
            
            var institutionJobs = new InstitutionJobs(guid,
                perposRecord.Recordkey, perposRecord.PerposHrpId,
                perposRecord.PerposPositionId, perposRecord.PerposStartDate.Value)
            {
                EndDate = perposRecord.PerposEndDate,
                AlternateSupervisorId = perposRecord.PerposAltSupervisorId,
                SupervisorId = perposRecord.PerposSupervisorHrpId
            };

            institutionJobs.HostCountry = hostCountry;
            institutionJobs.Employer = hostCorpId;

            institutionJobs.EndReason = perposRecord.PerposEndReason;

            if (positionRecords != null)
            {
                var position = positionRecords.FirstOrDefault(p => p.Recordkey == perposRecord.PerposPositionId);
                if (position != null)
                {
                    institutionJobs.Department = string.IsNullOrWhiteSpace(position.PosDept) ? null : position.PosDept;
                    institutionJobs.Classification = string.IsNullOrWhiteSpace(position.PosClass) ? null : position.PosClass;

                    //position is salaried if db column is "S". position is hourly if db column is "H"
                    institutionJobs.IsSalary = position.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase);
                }
            }

            if ((perposwgRecords != null) && (perposwgRecords.Any()))
            {

                var currentPerposwg =
                    perposwgRecords.FirstOrDefault(p => !p.PpwgEndDate.HasValue
                                                        && p.PpwgStartDate <= currentDate)
                    ?? perposwgRecords.FirstOrDefault(p => p.PpwgEndDate.HasValue
                                                           && p.PpwgStartDate <= currentDate && p.PpwgEndDate >= currentDate);
                institutionJobs.PayStatus = PayStatus.WithoutPay;
                ICollection<Projects> projectRecords = new List<Projects>();
                var projectIds = GetProjectIds(perposwgRecords, currentPerposwg);

                if (projectIds != null && projectIds.Any())
                {
                    projectRecords = await DataReader.BulkReadRecordAsync<Projects>(projectIds.Distinct().ToArray());
                }

                if (currentPerposwg != null)
                {
                    if (currentPerposwg.PpwgEndDate == null || currentPerposwg.PpwgEndDate > DateTime.Today)
                    {
                        institutionJobs.PayStatus = PayStatus.WithPay;
                    }

                    var accountingStrings = new List<string>();

                    // Loop through a second time this grabbing the accounting string and the correct project reference number
                    // if we can't get a reference number it will default the project ID
                    foreach (var currentPpwitem in currentPerposwg.PpwitemsEntityAssociation)
                    {
                        var projectId = currentPpwitem.PpwgProjectsIdsAssocMember;
                        if (!string.IsNullOrWhiteSpace(projectId) && projectRecords != null)
                        {
                            var projectRecord = projectRecords.FirstOrDefault(pr => pr.Recordkey == projectId);
                            // If there are project IDs, there should be project line item IDs
                            if ((projectRecord != null))
                            {
                                // For each project line item ID, get the project line item code

                                projectId = projectRecord.PrjRefNo;
                            }
                        }
                        //if there was a project Id then concatenate the accounting string with the project ID.
                        var accountingString = string.IsNullOrEmpty(projectId) ?
                            currentPpwitem.PpwgPiGlNoAssocMember : string.Concat(currentPpwitem.PpwgPiGlNoAssocMember, '*', projectId);
                        if (!(string.IsNullOrWhiteSpace(accountingString)))
                            accountingStrings.Add(accountingString);
                    }
                    if (accountingStrings.Any())
                    {
                        institutionJobs.AccountingStrings = accountingStrings;
                        institutionJobs.PpwgGlAccountNumber = currentPerposwg.PpwgPiGlNo;
                    }

                    institutionJobs.PpwgProjectIds = currentPerposwg.PpwgProjectsIds;

                    institutionJobs.Grade = string.IsNullOrWhiteSpace(currentPerposwg.PpwgGrade) ? null : currentPerposwg.PpwgGrade;
                    institutionJobs.Step = string.IsNullOrWhiteSpace(currentPerposwg.PpwgStep) ? null : currentPerposwg.PpwgStep;
                    institutionJobs.PayRate = string.IsNullOrWhiteSpace(currentPerposwg.PpwgPayRate) ? null : currentPerposwg.PpwgPayRate;

                    institutionJobs.PayClass = string.IsNullOrWhiteSpace(currentPerposwg.PpwgPayclassId) ? null : currentPerposwg.PpwgPayclassId;
                    institutionJobs.PayCycle = string.IsNullOrWhiteSpace(currentPerposwg.PpwgPaycycleId) ? null : currentPerposwg.PpwgPaycycleId;

                    institutionJobs.CycleWorkTimeAmount = currentPerposwg.PpwgCycleWorkTimeAmt;
                    institutionJobs.CycleWorkTimeUnits = currentPerposwg.PpwgCycleWorkTimeUnits;

                    institutionJobs.YearWorkTimeAmount = currentPerposwg.PpwgYearWorkTimeAmt;
                    institutionJobs.YearWorkTimeUnits = currentPerposwg.PpwgYearWorkTimeUnits;
                }

                var perposwgItems = new List<PersonPositionWageItem>();
               
                foreach (var perposwg in perposwgRecords)
                {
                    var perposwgItem = new PersonPositionWageItem(perposwg.Recordkey);

                    if (perposwg.PpwgStartDate != null && perposwg.PpwgStartDate.HasValue)
                        perposwgItem.StartDate = Convert.ToDateTime(perposwg.PpwgStartDate);

                    perposwgItem.Grade = string.IsNullOrWhiteSpace(perposwg.PpwgGrade) ? null : perposwg.PpwgGrade;
                    perposwgItem.Step = string.IsNullOrWhiteSpace(perposwg.PpwgStep) ? null : perposwg.PpwgStep;
                    perposwgItem.PayRate = string.IsNullOrWhiteSpace(perposwg.PpwgPayRate) ? null : perposwg.PpwgPayRate;

                    if (perposwg.PpwgEndDate != null && perposwg.PpwgEndDate.HasValue)
                        perposwgItem.EndDate = Convert.ToDateTime(perposwg.PpwgEndDate);

                    if (perposwg.PpwitemsEntityAssociation != null && perposwg.PpwitemsEntityAssociation.Any())
                    {                        
                        perposwgItem.AccountingStringAllocation = new List<PpwgAccountingStringAllocation>();
                        foreach (var ppwitem in perposwg.PpwitemsEntityAssociation)
                        {
                            var ppwgGlAssociation = new PpwgAccountingStringAllocation();
                            var projectId = ppwitem.PpwgProjectsIdsAssocMember;
                            if (!string.IsNullOrWhiteSpace(projectId) && projectRecords != null)
                            {
                                var projectRecord = projectRecords.FirstOrDefault(pr => pr.Recordkey == projectId);
                                if ((projectRecord != null))
                                {
                                    // For each project line item ID, get the project line item code
                                    projectId = projectRecord.PrjRefNo;
                                }
                            }
                            ppwgGlAssociation.PpwgProjectsId = projectId;
                            ppwgGlAssociation.GlNumber = ppwitem.PpwgPiGlNoAssocMember;
                            ppwgGlAssociation.GlPercentDistribution = ppwitem.PpwgPiPctDistAssocMember ?? 0; //ppwitem.PpwgPiGlPctDistAssocMember;

                            perposwgItem.AccountingStringAllocation.Add(ppwgGlAssociation);
                        }
                    }

                    perposwgItems.Add(perposwgItem);
                }
                institutionJobs.PerposwgItems = perposwgItems;
            }

            institutionJobs.BenefitsStatus = BenefitsStatus.WithoutBenefits;
            if ((personBenefitsRecords != null) && (personBenefitsRecords.Any()))
            {
                List<string> excludeBenefits = new List<string>();
                // Get list of benefits to exclude for consideration of employee benefit status 
                 if (ldmDefaults != null)
                {
                    excludeBenefits = ldmDefaults.LdmdExcludeBenefits;
                }
                foreach (var perben in personBenefitsRecords)
                {
                    var benefit = perben.PerbenBdId;
                    // Make sure this benefit is not on the list for exclusion before using it to determine 
                    // employee benefit status 
                    if (!excludeBenefits.Contains(benefit))
                    {
                        if (perben.PerbenCancelDate == null || perben.PerbenCancelDate > DateTime.Today)
                        {
                            institutionJobs.BenefitsStatus = BenefitsStatus.WithBenefits;
                            break;
                        }
                    }
                }
            }

            institutionJobs.FullTimeEquivalent = perposRecord.PerposFte;

            if (hrperRecords.Any() && perstatRecords.Any())
            {
                var terminated = hrperRecords.FirstOrDefault(hrper => !hrper.HrpEffectTermDate.HasValue
                                                                      && hrper.HrpEffectTermDate <= currentDate);
                if (terminated == null)
                {
                    //perstatRecords = perstatRecords.OrderByDescending(pr => pr.PerstatStartDate);
                    //get most currect person status record for that position
                    var perstatRecord =
                        perstatRecords.OrderByDescending(p => p.PerstatStartDate).FirstOrDefault(p => !p.PerstatEndDate.HasValue
                                                                                                      && p.PerstatStartDate <= currentDate)
                        ?? perstatRecords.OrderByDescending(p => p.PerstatStartDate).FirstOrDefault(p => p.PerstatEndDate.HasValue
                                                                                                         && p.PerstatStartDate <= currentDate && p.PerstatEndDate >= currentDate);

                    if (perstatRecord != null && perstatRecord.PerstatPrimaryPosId == perposRecord.PerposPositionId)
                        institutionJobs.Primary = true;
                }
            }
            return institutionJobs;
        }

        private List<string> GetProjectIds(IEnumerable<Perposwg> perposwgRecords, Perposwg currentPerposwg)
        {
            var projectIds = new List<string>();

            if (currentPerposwg != null && currentPerposwg.PpwitemsEntityAssociation.Any())
            {
                foreach (var currentPpwitem in currentPerposwg.PpwitemsEntityAssociation)
                {
                    if ((!string.IsNullOrWhiteSpace(currentPpwitem.PpwgProjectsIdsAssocMember))
                    && (!projectIds.Contains(currentPpwitem.PpwgProjectsIdsAssocMember)))
                    {
                        projectIds.Add(currentPpwitem.PpwgProjectsIdsAssocMember);
                    }
                }
            }

            if (perposwgRecords != null && perposwgRecords.Any())
            {
                foreach (var perposwg in perposwgRecords)
                {
                    if (perposwg.PpwitemsEntityAssociation != null && perposwg.PpwitemsEntityAssociation.Any())
                    {
                        foreach (var currentPpwitem in perposwg.PpwitemsEntityAssociation)
                        {
                            if ((!string.IsNullOrWhiteSpace(currentPpwitem.PpwgProjectsIdsAssocMember))
                              && (!projectIds.Contains(currentPpwitem.PpwgProjectsIdsAssocMember)))
                            {
                                projectIds.Add(currentPpwitem.PpwgProjectsIdsAssocMember);
                            }
                        }
                    }
                }
            }
            return projectIds;
        }

        /// <summary>
        /// Select all active status records in PERSTAT to find the current status records (PERSTAT that's considered active based on the request date) and then publish the corresponding PERPOS record (PERSTAT.PRIMARY.PERPOS.ID). We might need to create a computed column for this filter, not 100% sure. Refer to the logic used to publish the preference property.
        /// The HRPER file does capture the status date ranges itself in HRP.PERSTAT.START.DATE and 
        /// HRP.PERSTAT.END.DATE...so we can check these date ranges from HRPER to determine the current status 
        /// record, then read the corresponding PERSTAT entry to get the PERSTAT.PRIMARY.POS.ID and then see 
        /// if it matches the current position we're integrating above in position.id then populate this 
        /// property with the "primary" enumeration.
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<string>> GetPrimaryInstitutionJobsPerposIdsAsync(string selectionCriteria = "")
        {
            List<string> primaryInstitutionJobsPoposIds = new List<string>();

            var currentDate = DateTime.Now;
            
            var perposKeys = await DataReader.SelectAsync("PERPOS", selectionCriteria);

            var perposRecords = new List<Perpos>();
            for (int i = 0; i < perposKeys.Count(); i += _bulkReadSize)
            {
                var subList = perposKeys.Skip(i).Take(_bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perpos>(subList.ToArray());
                if (records != null)
                {
                    perposRecords.AddRange(records);
                }
            }

            if (!perposRecords.Any())
                return primaryInstitutionJobsPoposIds;

            var personIds = perposRecords.Select(e => e.PerposHrpId).ToList();

            // select all the PERSTAT ids with the HRP.ID equal to the input person id.
            var criteria = "WITH PERSTAT.HRP.ID EQ ?";
            var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

            var perstatRecords = new List<Perstat>();
            for (int i = 0; i < perstatKeys.Count(); i += _bulkReadSize)
            {

                var subList = perstatKeys.Skip(i).Take(_bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                if (records != null)
                {
                    perstatRecords.AddRange(records);
                }
            }

            // bulkread all HRPER records from Colleauge
            var hrperRecords = new List<Hrper>();
            for (int i = 0; i < personIds.Count(); i += _bulkReadSize)
            {
                var subList = personIds.Skip(i).Take(_bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Hrper>(subList.ToArray());
                if (records != null)
                {
                    hrperRecords.AddRange(records);
                }
            }

            foreach (var perpos in perposRecords)
            {
                try
                {
                    var personStatusRecords = perstatRecords.Where(ps => ps.PerstatHrpId == perpos.PerposHrpId).ToList();
                    var hrPersonRecord = hrperRecords.Where(hr => hr.Recordkey == perpos.PerposHrpId).ToList();

                    if (hrPersonRecord.Any() && personStatusRecords.Any())
                    {
                        var terminated = hrPersonRecord.FirstOrDefault(hrper => !hrper.HrpEffectTermDate.HasValue
                                                                                && hrper.HrpEffectTermDate <= currentDate);
                        if (terminated == null)
                        {

                            var perstatRecord =
                                personStatusRecords.OrderByDescending(p => p.PerstatStartDate).FirstOrDefault(p => !p.PerstatEndDate.HasValue
                                                                                                                   && p.PerstatStartDate <= currentDate)
                                ?? personStatusRecords.OrderByDescending(p => p.PerstatStartDate).FirstOrDefault(p => p.PerstatEndDate.HasValue
                                                                                                                      && p.PerstatStartDate <= currentDate && p.PerstatEndDate >= currentDate);

                            if (perstatRecord != null && perstatRecord.PerstatPrimaryPosId == perpos.PerposPositionId)
                            {
                                primaryInstitutionJobsPoposIds.Add(perpos.Recordkey);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return primaryInstitutionJobsPoposIds;
        }

        /// <summary>
        /// Get the Defaults from CORE to compare default institution Id
        /// </summary>
        /// <returns>Core Defaults</returns>
        private Base.DataContracts.Defaults GetDefaults()
        {
            return GetOrAddToCache<Data.Base.DataContracts.Defaults>("CoreDefaults",
                () =>
                {
                    var coreDefaults = DataReader.ReadRecord<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Get Host Country
        /// </summary>
        /// <returns>string</returns>
        private async Task<string> GetHostCountryAsync()
        {
            if (_internationalParameters == null)
                _internationalParameters = await GetInternationalParametersAsync();
            return _internationalParameters.HostCountry;
        }

        public async Task<string> GetInstitutionEmployerGuidAsync()
        {
            var humanResourcesInstitutionEmployerEntity = await GetGuidValcodeAsync<HumanResourcesInstitutionEmployer>("HR", "INTG.INST.EMPLOYER",
    (cl, g) => new HumanResourcesInstitutionEmployer(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: true);

            if (humanResourcesInstitutionEmployerEntity != null && humanResourcesInstitutionEmployerEntity.Any())
            {
                var thisEntity = humanResourcesInstitutionEmployerEntity.FirstOrDefault();
                return (thisEntity != null) ? thisEntity.Guid : string.Empty;
            }
            else
            {
                var errorMessage = "No guid found for institution-employers";
                logger.Error(errorMessage);
                throw new KeyNotFoundException(errorMessage);
            }
        }

        /// <summary>
        /// Create an CreateUpdateInstJobsRequest from an InstitutionJobs domain entity
        /// </summary>
        /// <param name="institutionJobsEntity">InstitutionJobs domain entity</param>
        /// <returns>CreateUpdateInstJobsRequest transaction object</returns>
        private CreateUpdateInstJobsRequest BuildInstitutionJobsUpdateRequest(InstitutionJobs institutionJobsEntity)
        {

            var request = new CreateUpdateInstJobsRequest
            {
                InstJobId = institutionJobsEntity.Id,
                Guid = institutionJobsEntity.Guid,
                ClassificationId = institutionJobsEntity.Classification,
                Department = institutionJobsEntity.Department,
                EmployerId = institutionJobsEntity.Employer,
                EndOn = institutionJobsEntity.EndDate,
                FullTimeEquiv = institutionJobsEntity.FullTimeEquivalent,
                JobChangeReasonId = institutionJobsEntity.EndReason,
                PersonId = institutionJobsEntity.PersonId,
                PositionId = institutionJobsEntity.PositionId,
                SalaryAmtCurrency = institutionJobsEntity.CurrencyCode,
                StartOn = institutionJobsEntity.StartDate,
                Preference = (institutionJobsEntity.Primary == true ? "primary" : ""),
                InstJobPayClassId = institutionJobsEntity.PayClass,
                InstJobPayCycle = institutionJobsEntity.PayCycle
            };

            if (institutionJobsEntity.PerposwgItems != null && institutionJobsEntity.PerposwgItems.Any())
            {
                var salariedPerposwgItems = institutionJobsEntity.PerposwgItems.Where(x => !string.IsNullOrEmpty(x.PayRate));

                if (salariedPerposwgItems != null && salariedPerposwgItems.Any())
                {
                    var salaries = new List<Salaries>();
                    var accountingAllocations = new List<AccountingAllocation>();

                    foreach (var perposwgItems in salariedPerposwgItems)
                    {
                        var salary = new Salaries();

                        salary.SalaryAmtValue = perposwgItems.PayRate;
                        salary.SalaryEndOn = perposwgItems.EndDate;
                        salary.SalaryGrade = perposwgItems.Grade;
                        salary.SalaryStartOn = perposwgItems.StartDate;
                        salary.SalaryStep = perposwgItems.Step;
                        salary.SalaryAmtPeriod = perposwgItems.HourlyOrSalary == "h" ? "hour" : "year";

                        salaries.Add(salary);

                        if (perposwgItems.AccountingStringAllocation != null && perposwgItems.AccountingStringAllocation.Any())
                        {
                            foreach (var ppwgGlAssoc in perposwgItems.AccountingStringAllocation)
                            {
                                var accountingAllocation = new AccountingAllocation();

                                accountingAllocation.AccountStrings = ppwgGlAssoc.GlNumber;
                                accountingAllocation.AccountStringPct = ppwgGlAssoc.GlPercentDistribution;
                                accountingAllocation.AccountStringStartDate = perposwgItems.StartDate;

                                if (perposwgItems.EndDate != default(DateTime))
                                    accountingAllocation.AccountStringEndDate = perposwgItems.EndDate;

                                accountingAllocations.Add(accountingAllocation);
                            }
                        }

                    }

                    if (accountingAllocations != null && accountingAllocations.Any())
                        request.AccountingAllocation = accountingAllocations;

                    if (salaries != null && salaries.Any())
                        request.Salaries = salaries;
                }
            }

            if ((!institutionJobsEntity.EndDate.HasValue) || (institutionJobsEntity.EndDate >= DateTime.Now))
            {
                request.Status = "active";
            }
            else if (institutionJobsEntity.EndDate < DateTime.Now)
            {
                request.Status = "ended";
            }

            //Hours per period
            var hoursPerPeriod = new List<hoursPerPeriod>();
            if (institutionJobsEntity.CycleWorkTimeAmount.HasValue)
            {
                hoursPerPeriod.Add(new Transactions.hoursPerPeriod()
                {
                    HoursPerPeriodHours = institutionJobsEntity.CycleWorkTimeAmount,
                    HoursPerPeriodPeriod = "payPeriod"
                });
            }
            if (institutionJobsEntity.YearWorkTimeAmount.HasValue)
            {
                hoursPerPeriod.Add(new Transactions.hoursPerPeriod()
                {
                    HoursPerPeriodHours = institutionJobsEntity.YearWorkTimeAmount,
                    HoursPerPeriodPeriod = "year"
                });
            }
            if (hoursPerPeriod.Any())
            {
                request.hoursPerPeriod = hoursPerPeriod;
            }

            //Supervisors
            var supervisors = new List<Transactions.supervisors>();
            if (!string.IsNullOrEmpty(institutionJobsEntity.SupervisorId))
            {
                supervisors.Add(new Transactions.supervisors()
                {
                    Supervisors = institutionJobsEntity.SupervisorId,
                    SupervisorsType = "primary"
                });
            }
            if (!string.IsNullOrEmpty(institutionJobsEntity.AlternateSupervisorId))
            {
                supervisors.Add(new Transactions.supervisors()
                {
                    Supervisors = institutionJobsEntity.AlternateSupervisorId,
                    SupervisorsType = "alternate"
                });
            }
            if (supervisors.Any())
            {
                request.supervisors = supervisors;
            }
            return request;
        }
    }
}