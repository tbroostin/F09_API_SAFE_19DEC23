// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType]
    public class AcademicCredentialsRepository : BaseColleagueRepository, IAcademicCredentialsRepository
    {

        private ApplValcodes _institutionTypes;
        private readonly string acadIntgKeyIDXCriteria = "WITH LDM.GUID.SECONDARY.FLD EQ 'ACAD.INTG.KEY.IDX' AND LDM.GUID.ENTITY EQ 'ACAD.CREDENTIALS' AND LDM.GUID.SECONDARY.KEY EQ '{0}'";


        public AcademicCredentialsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetAcademicCredentialsIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var guidRecord = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", guid);
            if (guidRecord == null)
            {
                throw new KeyNotFoundException("ACAD.CREDENTIALS GUID " + guid + " not found.");
            }
            if ((guidRecord.LdmGuidEntity != "ACAD.CREDENTIALS") || (string.IsNullOrEmpty(guidRecord.LdmGuidSecondaryFld)))
            {
                throw new KeyNotFoundException("GUID " + guid + " has different entity, than expected, ACAD.CREDENTIALS");
            }
            return guidRecord.LdmGuidPrimaryKey;
        }

        /// <summary>
        /// Get a single AcademicCredential using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>AcademicCredential</returns>
        private async Task<Domain.Base.Entities.AcademicCredential> GetAcademicCredentialsByGuidAsync(string guid)
        {
            var hostInstitutionId = string.Empty;
            var acadCredentialId = await GetAcademicCredentialsIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(acadCredentialId))
            {
                throw new KeyNotFoundException(string.Format("AcademicCredential id not found for guid {0}", guid));
            }

            var acadCredential = await GetAcademicCredentialsByIdAsync(acadCredentialId);

            if (acadCredential != null && !(string.IsNullOrEmpty(acadCredential.AcadInstitutionsId)))
            {
                var coreDefaultData = GetDefaults();
                if (coreDefaultData != null)
                {
                    hostInstitutionId = coreDefaultData.DefaultHostCorpId;
                    if ((!string.IsNullOrEmpty(hostInstitutionId)) && (hostInstitutionId == acadCredential.AcadInstitutionsId))
                    {
                        throw new KeyNotFoundException(string.Format("AcademicCredential not found for guid {0}", guid));
                    }
                }
            }
            return acadCredential;
        }

        /// <summary>
        /// Get a single AcademicCredentials domain entity from an id.
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>AcademicCredentials domain entity object</returns>
        public async Task<Domain.Base.Entities.AcademicCredential> GetAcademicCredentialsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a AcademicCredentials.");
            }
            var guidRecord = await DataReader.SelectAsync("LDM.GUID", string.Format(acadIntgKeyIDXCriteria, id));
            if (guidRecord == null || guidRecord.Count() > 1)
            {
                throw new KeyNotFoundException(string.Format("No AcademicCredential was found for id {0}.", id));
            }
            var acadCredential = await DataReader.ReadRecordAsync<AcadCredentials>(id);
            if (acadCredential == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or AcademicCredentials with ID ", id, "invalid."));
            }
            if (acadCredential != null && !(string.IsNullOrEmpty(acadCredential.AcadInstitutionsId)))
            {
                var coreDefaultData = GetDefaults();
                if (coreDefaultData != null)
                {
                    var hostInstitutionId = coreDefaultData.DefaultHostCorpId;
                    if ((!string.IsNullOrEmpty(hostInstitutionId)) && (hostInstitutionId == acadCredential.AcadInstitutionsId))
                    {
                        throw new KeyNotFoundException(string.Format("AcademicCredential not found for id {0}", id));
                    }
                }
            }
            return BuildAcademicCredential(acadCredential, guidRecord.FirstOrDefault());
        }

        /// <summary>
        ///  Get all AcademicCredentials domain entity data 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="offset"></param>
        /// <returns>Collection of AcademicCredentials domain entities</returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.AcademicCredential>, int>> GetAcademicCredentialsAsync(int offset, int limit, bool bypassCache = false)
        {
            var criteria = string.Empty;
            var hostInstitutionId = string.Empty;

            var coreDefaultData = GetDefaults();
            if (coreDefaultData != null)
            {
                hostInstitutionId = coreDefaultData.DefaultHostCorpId;

                if (!string.IsNullOrEmpty(hostInstitutionId))
                {
                    criteria += "WITH ACAD.INSTITUTIONS.ID NE '' AND WITH ACAD.INSTITUTIONS.ID NE '"
                        + hostInstitutionId + "'";
                }
            }

            //get initial list of academic.credentials and apply filter if possible            
            var academicCredentials = await DataReader.SelectAsync("ACAD.CREDENTIALS", criteria);

            //after applying initial criteria, need to get a collection of institution records that 
            //are associated with an INSTITUTIONS record with special processing codes
            var types = (await this.GetInstitutionTypesAsync()).ValsEntityAssociation.Where(x => !string.IsNullOrEmpty(x.ValActionCode1AssocMember))
                .Select(z => z.ValInternalCodeAssocMember).ToArray();

            var institutionIds = await DataReader.SelectAsync("INSTITUTIONS", "WITH INST.TYPE = '?'", types);

            // since the institutionIds will exceed the query limit, need to process in chunks
            var academicCredentialsIds = await GetAcademicCredentialsFilteredAsync(institutionIds, "WITH ACAD.INSTITUTIONS.ID EQ", academicCredentials);

            var totalCount = academicCredentialsIds.Count();
            Array.Sort(academicCredentialsIds);
            var subList = academicCredentialsIds.Skip(offset).Take(limit).ToArray();
            var academicCredentialsData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", subList);
            var academicCredentialsEntities = await BuildAcademicCredentialsAsync(subList, academicCredentialsData);
            return new Tuple<IEnumerable<Domain.Base.Entities.AcademicCredential>, int>(academicCredentialsEntities, totalCount);
        }

        private async Task<IEnumerable<Domain.Base.Entities.AcademicCredential>> BuildAcademicCredentialsAsync(string[] academicCredentialsIds, Collection<AcadCredentials> sources)
        {
            var acadCredentialCollection = new List<Domain.Base.Entities.AcademicCredential>();

            var academicCredentialsDict = await GetAcadCredentialsGuidDictionary(academicCredentialsIds, acadIntgKeyIDXCriteria);

            foreach (var source in sources)
            {
                var academicCredentialsGuid = GetGuid(source.Recordkey, academicCredentialsDict);
                acadCredentialCollection.Add(BuildAcademicCredential(source, academicCredentialsGuid));
            }
            return acadCredentialCollection.AsEnumerable();
        }

        private Domain.Base.Entities.AcademicCredential BuildAcademicCredential(AcadCredentials source, string guid)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var academicCredentials = new Domain.Base.Entities.AcademicCredential(guid)
            {
                AcadPersonId = source.AcadPersonId,
                AcadInstitutionsId = source.AcadInstitutionsId,
                AcadDegree = source.AcadDegree,
                AcadCcd = source.AcadCcd,
                AcadMajors = source.AcadMajors,
                AcadMinors = source.AcadMinors,
                AcadSpecialization = source.AcadSpecialization,
                AcadStartDate = source.AcadStartDate,
                AcadEndDate = source.AcadEndDate,
                AcadGpa = source.AcadGpa,
                AcadHonors = source.AcadHonors,
                AcadCommencementDate = source.AcadCommencementDate,
                AcadDegreeDate = source.AcadDegreeDate,
                AcadThesis = source.AcadThesis,
                AcadRankDenominator = source.AcadRankDenominator,
                AcadRankPercent = source.AcadRankPercent,
                AcadRankNumerator = source.AcadRankNumerator,
                AcadCddDate = source.AcadCcdDate
            };

            return academicCredentials;
        }

        /// <summary>
        /// The maximum number of attributes permitted in each query is determined by the QueryAttributeLimit.
        /// If the number of attributes is greater than the QueryAttributeLimit, perform multiple subqueries
        /// </summary>
        /// <param name="limitingList"></param>
        /// <param name="dataElements"></param>
        /// <param name="queryPrefix"></param>
        /// <returns></returns>
        private async Task<string[]> GetAcademicCredentialsFilteredAsync(IReadOnlyCollection<string> dataElements, string queryPrefix, string[] limitingList = null)
        {
            if ((dataElements == null) || (!dataElements.Any()))
            {
                throw new ArgumentNullException("dataElements");
            }
            if (string.IsNullOrEmpty(queryPrefix))
            {
                throw new ArgumentNullException("queryPrefix");
            }

            var queryAttributeLimit = Configuration.ColleagueSDKParameters.QueryAttributeLimit;
            if (queryAttributeLimit == 0) queryAttributeLimit = 100;
            string[] academicCredentialsIds = null;

            for (var i = 0; i < (dataElements.Count / queryAttributeLimit) + 1; i++)
            {
                var dataToQuery = string.Empty;

                // Retrieve the range of attributes
                var filteredElements = dataElements.Take(queryAttributeLimit * (i + 1)).Skip(i * queryAttributeLimit).ToArray();

                // Concatenate the list of attributes in the specified range
                dataToQuery = filteredElements.Aggregate(dataToQuery, (current, element) => current + string.Concat("'", element, "'"));

                if ((academicCredentialsIds == null) || (!academicCredentialsIds.Any()))
                    academicCredentialsIds = await DataReader.SelectAsync("ACAD.CREDENTIALS", limitingList, string.Concat(queryPrefix, " ", dataToQuery));
                else
                    academicCredentialsIds = academicCredentialsIds.Concat(await DataReader.SelectAsync("ACAD.CREDENTIALS", limitingList, string.Concat(queryPrefix, " ", dataToQuery))).ToArray();
            }
            return academicCredentialsIds;
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
                    Data.Base.DataContracts.Defaults coreDefaults = DataReader.ReadRecord<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Return the Validation Table InstTypes for determination of High School or College
        /// within the Institutions Attended data.
        /// </summary>
        /// <returns>Validation Table Object for Institution Types</returns>
        private async Task<ApplValcodes> GetInstitutionTypesAsync()
        {
            if (_institutionTypes != null)
            {
                return _institutionTypes;
            }

            _institutionTypes = await GetOrAddToCacheAsync<ApplValcodes>("InstitutionTypes",
                async () =>
                {
                    ApplValcodes typesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INST.TYPES");
                    if (typesTable == null)
                    {
                        var errorMessage = "Unable to access INST.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return typesTable;
                }, Level1CacheTimeoutValue);
            return _institutionTypes;
        }

        /// <summary>
        /// Gets dictionary with colleague id and guid key pair for ACAD.CREDENTIALS.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> GetAcadCredentialsGuidDictionary(IEnumerable<string> ids, string criteria)
        {
            if (ids == null || !Enumerable.Any<string>(ids))
            {
                throw new ArgumentNullException("AcadCredentials id's are required.");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var id in ids)
            {
                var guidRecords = await DataReader.SelectAsync("LDM.GUID", string.Format(criteria, id));
                if (!dict.ContainsKey(id))
                {
                    if (guidRecords != null && guidRecords.Any())
                    {
                        dict.Add(id, guidRecords[0]);
                    }
                }
            }
            return dict;
        }

        private string GetGuid(string key, IDictionary<string, string> dict)
        {
            string guid = null;
            if (!string.IsNullOrEmpty(key))
            {
                if (dict.TryGetValue(key, out guid))
                {
                    return guid;
                }
            }
            return guid;
        }
    }
}