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
    public class InstitutionsAttendRepository : BaseColleagueRepository, IInstitutionsAttendRepository
    {
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;
        private ApplValcodes _institutionTypes;


        public InstitutionsAttendRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;

            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Using a collection of  ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="insAttendIds">collection of ids</param>
        /// <returns>Dictionary consisting of an id (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetInsAttendGuidsCollectionAsync(IEnumerable<string> insAttendIds)
        {
            if ((insAttendIds == null) || (insAttendIds != null && !insAttendIds.Any()))
            {
                return new Dictionary<string, string>();
            }

            var personGuidCollection = new Dictionary<string, string>();
            try
            {
                var instAttendGuidLookup = insAttendIds.Distinct().ToList().ConvertAll(p => new RecordKeyLookup("INSTITUTIONS.ATTEND", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(instAttendGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!personGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        if ((recordKeyLookupResult.Value != null) && (!string.IsNullOrEmpty(recordKeyLookupResult.Value.Guid)))
                            personGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                        else
                            throw new KeyNotFoundException(string.Concat("LDM GUID not found, Entity: ‘INSTITUTIONS.ATTEND’, Record ID: '", splitKeys[1], "'"));
                    }
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex); ;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occured while getting INSTITUTIONS.ATTEND guids.", ex); ;
            }

            return personGuidCollection;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetInstitutionsAttendIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("InstitutionsAttend GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("InstitutionsAttend GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "INSTITUTIONS.ATTEND")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, REMARKS");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single InstitutionsAttend domain entity from an id.
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>InstitutionsAttend domain entity object</returns>
        public async Task<Domain.Base.Entities.InstitutionsAttend> GetInstitutionAttendByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a InstitutionsAttend.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<InstitutionsAttend>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or InstitutionsAttend with ID ", id, "invalid."));
            }

            var defaults = GetDefaults();
            if ((defaults != null) && (!string.IsNullOrEmpty(record.Recordkey)))
            {
                var splitKey = id.Split('*');
                if (splitKey.Count() != 2)
                {
                    throw new ArgumentException(string.Concat("InstitutionAttend Record Key is not valid: ", id), "source.RecordKey");
                }

                if (splitKey[1] == defaults.DefaultHostCorpId)
                {
                    throw new ArgumentException("id", "Record associated with home institution can not be returned.");
                }
            }
            return BuildInstitutionsAttend(record);
        }

        /// <summary>
        ///  Get all InstitutionsAttend domain entity data 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="personId">person id</param>
        /// <param name="offset"></param>
        /// <returns>Collection of InstitutionsAttend domain entities</returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>> GetInstitutionsAttendAsync(int offset, int limit, bool bypassCache = false, string personId = "")
        {
            var criteria = string.Empty;
            var hostInstitutionId = string.Empty;

            var coreDefaultData = GetDefaults();
            if (coreDefaultData != null)
            {
                hostInstitutionId = coreDefaultData.DefaultHostCorpId;

                if (!string.IsNullOrEmpty(hostInstitutionId))
                {
                    criteria += "WITH INSTA.INSTITUTIONS.ID NE '" + hostInstitutionId + "'";
                }
            }

            if (!string.IsNullOrEmpty(personId))
            {
                if (string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }

                criteria += "WITH INSTA.PERSON.ID EQ '" + personId + "'";
            }

            var institutionAttends = await DataReader.SelectAsync("INSTITUTIONS.ATTEND", criteria);

            //after applying initial criteria, need to get a collection of institution records that 
            //are  associated with an INSTITUTIONS record with special processing codes
            var types = (await this.GetInstitutionTypesAsync()).ValsEntityAssociation.Where(x => !string.IsNullOrEmpty(x.ValActionCode1AssocMember))
                .Select(z => z.ValInternalCodeAssocMember).ToArray();

            var institutionIds = await DataReader.SelectAsync("INSTITUTIONS", "WITH INST.TYPE = '?'", types);
            var institutionAttendIds =
                institutionAttends.Join(
                    institutionIds,
                    l1 => l1.Split('*')[1], //Selector for items from the inner list splits on '*'
                    l2 => l2,               //Select the current item
                    (l1, l2) => l1).ToArray();


            var totalCount = institutionAttendIds.Count();
            Array.Sort(institutionAttendIds);
            var subList = institutionAttendIds.Skip(offset).Take(limit).ToArray();
            var institutionAttendsData = await DataReader.BulkReadRecordAsync<InstitutionsAttend>("INSTITUTIONS.ATTEND", subList);
            var institutionAttendsEntities = BuildInstitutionsAttended(institutionAttendsData);
            return new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(institutionAttendsEntities, totalCount);
        }

        private IEnumerable<Domain.Base.Entities.InstitutionsAttend> BuildInstitutionsAttended(Collection<InstitutionsAttend> sources)
        {
            var acadCredentialCollection = new List<Domain.Base.Entities.InstitutionsAttend>();
            foreach (var source in sources)
            {
                acadCredentialCollection.Add(BuildInstitutionsAttend(source));
            }
            return acadCredentialCollection.AsEnumerable();
        }

        private Domain.Base.Entities.InstitutionsAttend BuildInstitutionsAttend(InstitutionsAttend institutionsAttendData)
        {
            if (institutionsAttendData == null)
            {
                throw new ArgumentNullException("institutionsAttendData");
            }

            var institutionsAttend = new Domain.Base.Entities.InstitutionsAttend(institutionsAttendData.RecordGuid, institutionsAttendData.Recordkey)
            {
                AcadCredentials = institutionsAttendData.InstaAcadCredentials,
                Comments = institutionsAttendData.InstaComments,
                ExtCredits = institutionsAttendData.InstaExtCredits,
                ExtGpa = institutionsAttendData.InstaExtGpa,
                GradType = institutionsAttendData.InstaGradType,
                RankDenominator = institutionsAttendData.InstaRankDenominator,
                RankNumerator = institutionsAttendData.InstaRankNumerator,
                RankPercent = institutionsAttendData.InstaRankPercent,
                TranscriptDate = institutionsAttendData.InstaTranscriptDate,
                TranscriptStatus = institutionsAttendData.InstaTranscriptStatus,
                TranscriptType = institutionsAttendData.InstaTranscriptType,
                InstaIntgHsGradDate = institutionsAttendData.InstaIntgHsGradDate,
            };

            var datesAttended = new List<Tuple<DateTime?, DateTime?>>();

            if (institutionsAttendData.DatesAttendedEntityAssociation != null &&
                institutionsAttendData.DatesAttendedEntityAssociation.Any())
            {
                foreach (var dateAttended in institutionsAttendData.DatesAttendedEntityAssociation)
                {
                    datesAttended.Add(new Tuple<DateTime?, DateTime?>(dateAttended.InstaStartDatesAssocMember, dateAttended.InstaEndDatesAssocMember));
                }

                if (datesAttended.Any())
                {
                    institutionsAttend.DatesAttended = datesAttended;
                }
            }

            var yearsAttended = new List<Tuple<int?, string>>();

            if (institutionsAttendData.YearsAttendedEntityAssociation != null &&
                institutionsAttendData.YearsAttendedEntityAssociation.Any())
            {
                foreach (var yearAttended in institutionsAttendData.YearsAttendedEntityAssociation)
                {
                    yearsAttended.Add(new Tuple<int?, string>(yearAttended.InstaYearAttendStartAssocMember, yearAttended.InstaYearAttendEndAssocMember));
                }

                if (yearsAttended.Any())
                {
                    institutionsAttend.YearsAttended = yearsAttended;
                }
            }

            return institutionsAttend;
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

    }
}