/*Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.DataContracts;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ExternalEducationRepository : BaseColleagueRepository, IExternalEducationRepository
    {
        private readonly int _readSize;
     
        public ExternalEducationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        ///  Get all external education data consisting of academic credential data, excluding home insitution,
        /// along with some data from institutions attended
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="personId">person id</param>
        /// <param name="offset"></param>
        /// <returns>Collection of ExternalEducation domain entities</returns>
        public async Task<Tuple<IEnumerable<ExternalEducation>, int>> GetExternalEducationAsync(int offset, int limit, bool bypassCache = false, string personId = "")
        {
            var criteria = string.Empty;
            var coreDefaultData = GetDefaults();

            var hostInstitutionId = coreDefaultData.DefaultHostCorpId;

            if (!(string.IsNullOrEmpty(hostInstitutionId)))
            {
                criteria += "WITH ACAD.INSTITUTIONS.ID NE '" + hostInstitutionId + "'";
            }

            if (!string.IsNullOrEmpty(personId))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }

                criteria += "WITH ACAD.PERSON.ID EQ '" + personId + "'";
            }

            var acadCredentialsIds = await DataReader.SelectAsync("ACAD.CREDENTIALS", criteria);
            var totalCount = acadCredentialsIds.Count();
            Array.Sort(acadCredentialsIds);
            var subList = acadCredentialsIds.Skip(offset).Take(limit).ToArray();
            var externalEducationData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", subList);
            var externalEducationEntities = await BuildExternalEducationsAsync(subList, externalEducationData);
            return new Tuple<IEnumerable<ExternalEducation>, int>(externalEducationEntities, totalCount);
        }

        /// <summary>
        /// Get a single External Education domain entity from an academic credential guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<ExternalEducation> GetExternalEducationByGuidAsync(string guid)
        {
            return await GetExternalEducationByIdAsync(await GetExternalEducationIdFromGuidAsync(guid));
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetExternalEducationIdFromGuidAsync(string guid)
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
            if ((guidRecord.LdmGuidEntity != "ACAD.CREDENTIALS") || (!string.IsNullOrEmpty(guidRecord.LdmGuidSecondaryFld)))
            {
                throw new KeyNotFoundException("GUID " + guid + " has different entity, than expected, ACAD.CREDENTIALS");
            }
            return guidRecord.LdmGuidPrimaryKey;
        }


        /// <summary>
        /// Get a single External Education domain entity from an academic credential id.
        /// </summary>
        /// <param name="id">The academic credential id</param>
        /// <returns>External Education domain entity object</returns>
        public async Task<ExternalEducation> GetExternalEducationByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a AcademicCredentials.");
            }
            // var guidRecord = await DataReader.SelectAsync("LDM.GUID", string.Format(academicCredentialCriteria, id));

            //var academicCredentialsDict = await GetAcadCredGuidsCollectionAsync(academicCredentialsIds);
            var academicCredentialsDict = await GetAcadCredGuidsCollectionAsync(new List<string>() { id });
            var guidRecord = GetGuid(id, academicCredentialsDict);

            if (string.IsNullOrEmpty(guidRecord))
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
                        throw new ArgumentException("id", "Record associated with home institution can not be returned.");

                    }
                }
            }
            return await BuildExternalEducationAsync(acadCredential, guidRecord);
        }


        public async Task<IEnumerable<ExternalEducation>> BuildExternalEducationsAsync(string[] academicCredentialsIds, Collection<AcadCredentials> sources)
        {
            var acadCredentialCollection = new List<ExternalEducation>();
            if (academicCredentialsIds != null && academicCredentialsIds.Any())
            {
                var academicCredentialsDict = await GetAcadCredGuidsCollectionAsync(academicCredentialsIds);

                foreach (var source in sources)
                {
                    var academicCredentialsGuid = GetGuid(source.Recordkey, academicCredentialsDict);
                    acadCredentialCollection.Add(await BuildExternalEducationAsync(source, academicCredentialsGuid));
                }
            }
            return acadCredentialCollection.AsEnumerable();
        }

        public async Task<ExternalEducation> BuildExternalEducationAsync(AcadCredentials source, string guid)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "source required to build AcadCredentials.");
            }

            ExternalEducation externalEducation = null;
            var instAttendData = new Collection<Base.DataContracts.InstitutionsAttend>();
            var instAttendIds = new List<string>();

            externalEducation = new ExternalEducation(guid)
            {
                AcadPersonId = source.AcadPersonId,
                AcadInstitutionsId = source.AcadInstitutionsId,
                AcadDegree = source.AcadDegree,
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

            };

            if ((!string.IsNullOrEmpty(source.AcadPersonId)) && (!string.IsNullOrEmpty(source.AcadInstitutionsId)))
            {

                instAttendIds.Add(source.AcadPersonId + "*" + source.AcadInstitutionsId);

                instAttendData = await DataReader.BulkReadRecordAsync<Base.DataContracts.InstitutionsAttend>(instAttendIds.ToArray());

                var instAttend = instAttendData.FirstOrDefault();
                if (instAttend != null)
                {
                    externalEducation.InstExtCredits = instAttend.InstaExtCredits;
                    externalEducation.InstTransciptDate = instAttend.InstaTranscriptDate;
                }
            }
            return externalEducation;
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
        /// Using a collection of acadCred ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="acadCredentialIds">collection of acadCred ids</param>
        /// <returns>Dictionary consisting of a acadCredId (key) and guid (value)</returns>
        private async Task<Dictionary<string, string>> GetAcadCredGuidsCollectionAsync(IEnumerable<string> acadCredentialIds)
        {
  
            if ((acadCredentialIds == null) || (acadCredentialIds != null && !acadCredentialIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var acadCredGuidCollection = new Dictionary<string, string>();

            var personGuidLookup = acadCredentialIds
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList()
                .ConvertAll(p => new RecordKeyLookup("ACAD.CREDENTIALS", p, false)).ToArray();
            var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!acadCredGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        acadCredGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
                catch (Exception) // Do not throw error.
                {
                }
            }

            return acadCredGuidCollection;
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