// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
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

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class InstitutionRepository : BaseColleagueRepository, IInstitutionRepository
    {
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public InstitutionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;

            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="instType"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Institution>, int>> GetInstitutionAsync(int offset, int limit, InstType? instType = null)
        {
            List<Institution> institutions = new List<Institution>();
            var defaultHostCorpId = string.Empty;
            var types = GetValcode<InstitutionType>("CORE", "INST.TYPES", ii => new InstitutionType(ii.ValInternalCodeAssocMember, ii.ValExternalRepresentationAssocMember, ii.ValActionCode1AssocMember));

            string criteria = "WITH INST.TYPE NE ''";

            if (instType != null)
            {
                string typeCriteria = "";
                switch (instType)
                {
                    case InstType.College:
                        var colleageTypes = types.Where(tx => tx.Category == "C").Select(ty => ty.Code);
                        if (colleageTypes != null && colleageTypes.Any())
                        {
                            foreach (var type in colleageTypes)
                            {
                                typeCriteria = string.Concat(typeCriteria, "'", type, "' ");
                            }
                        }
                        break;
                    case InstType.HighSchool:
                        var hsTypes = types.Where(tx => tx.Category == "H").Select(ty => ty.Code);
                        if (hsTypes != null && hsTypes.Any())
                        {
                            foreach (var type in hsTypes)
                            {
                                typeCriteria = string.Concat(typeCriteria, "'", type, "' ");
                            }
                        }
                        break;
                    default:
                        break;
                }
                criteria += " AND WITH INST.TYPE EQ " + typeCriteria;
            }

            var institutionIds = await DataReader.SelectAsync("INSTITUTIONS", criteria);
            var totalCount = institutionIds.Count();
            Array.Sort(institutionIds);
            var subList = institutionIds.Skip(offset).Take(limit).ToArray();

            var institutionData = await DataReader.BulkReadRecordAsync<Institutions>("INSTITUTIONS", subList);

            if (institutionData == null)
            {
                return new Tuple<IEnumerable<Institution>, int>(new List<Institution>(), 0);
            }

            var coreDefaultData = GetDefaults();
            if (coreDefaultData != null)
            {
                defaultHostCorpId = coreDefaultData.DefaultHostCorpId;
            }

            var faSystemParams = GetSystemParameters();
            var bulkPersonData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", institutionData.Select(p => p.Recordkey).Distinct().ToArray());
            var bulkAddressesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", bulkPersonData.Select(p => p.PreferredAddress).Distinct().ToArray());

            foreach (var record in institutionData)
            {
                DataContracts.Person personRecord = null;
                DataContracts.Address addressRecord = null;

                if (bulkPersonData != null && bulkPersonData.Any())
                {
                    personRecord = bulkPersonData.FirstOrDefault(x => x.Recordkey == record.Recordkey);
                    if ((bulkAddressesData != null) && (personRecord != null) && (!string.IsNullOrEmpty(personRecord.PreferredAddress)))
                        addressRecord = bulkAddressesData.FirstOrDefault(y => y.Recordkey == personRecord.PreferredAddress);
                }
                institutions.Add(BuildInstitution(types, defaultHostCorpId, faSystemParams, personRecord, addressRecord, record));
            }
            return new Tuple<IEnumerable<Institution>, int>(institutions, totalCount);
        }


        /// <summary>
        /// Using a list of person ids, determine which ones are associated with an institution
        /// </summary>
        /// <param name="subList">list of person ids</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetInstitutionIdsFromListAsync(string[] subList)
        {
            if (subList == null || !subList.Any())
            {
                return new string[] { };
            }
            return await DataReader.SelectAsync("INSTITUTIONS", subList, "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subList"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Institution>> GetInstitutionsFromListAsync(string[] subList)
        {
            List<Institution> institutions = new List<Institution>();

            if (subList == null || !subList.Any())
            {
                return institutions;
            }
            var institutionData = await DataReader.BulkReadRecordAsync<Institutions>("INSTITUTIONS", subList);

            if ((institutionData == null) || (!institutionData.Any()))
            {
                return institutions;
            }
            
            var defaultHostCorpId = string.Empty;

            var types = GetValcode<InstitutionType>("CORE", "INST.TYPES", ii => new InstitutionType(ii.ValInternalCodeAssocMember, ii.ValExternalRepresentationAssocMember, ii.ValActionCode1AssocMember));
            var coreDefaultData = GetDefaults();
            if (coreDefaultData != null)
            {
                defaultHostCorpId = coreDefaultData.DefaultHostCorpId;
            }

            var faSystemParams = GetSystemParameters();
            var bulkPersonData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", institutionData.Select(p => p.Recordkey).Distinct().ToArray());
            var bulkAddressesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", bulkPersonData.Select(p => p.PreferredAddress).Distinct().ToArray());

            foreach (var record in institutionData)
            {
                DataContracts.Person personRecord = null;
                DataContracts.Address addressRecord = null;

                if (bulkPersonData != null && bulkPersonData.Any())
                {
                    personRecord = bulkPersonData.FirstOrDefault(x => x.Recordkey == record.Recordkey);
                    if ((bulkAddressesData != null) && (personRecord != null) && (!string.IsNullOrEmpty(personRecord.PreferredAddress)))
                        addressRecord = bulkAddressesData.FirstOrDefault(y => y.Recordkey == personRecord.PreferredAddress);
                }
                institutions.Add(BuildInstitution(types, defaultHostCorpId, faSystemParams, personRecord, addressRecord, record));
            }
            return institutions.Any() ? institutions : null;
        }



        /// <summary>
        /// Get a single Institution using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The Institution</returns>

        public async Task<Institution> GetInstitutionByGuidAsync(string guid)
        {
            return await GetInstitutionAsync(await GetInstitutionFromGuidAsync(guid));
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetInstitutionFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Institution GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Institution GUID " + guid + " lookup failed.");
            }

            if ((foundEntry.Value.Entity != "INSTITUTIONS") && (foundEntry.Value.Entity != "PERSON"))
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected.");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single Institution using an ID
        /// </summary>
        /// <param name="id">The Institution GUID</param>
        /// <returns>The Institution</returns>
        public async Task<Institution> GetInstitutionAsync(string id)
        {
            Institution institution = null;
            var defaultHostCorpId = string.Empty;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a Institution.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<Institutions>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or Institution with ID ", id, "invalid."));
            }

            var types = GetValcode<InstitutionType>("CORE", "INST.TYPES", ii => new InstitutionType(ii.ValInternalCodeAssocMember, ii.ValExternalRepresentationAssocMember, ii.ValActionCode1AssocMember));
            var coreDefaultData = GetDefaults();
            if (coreDefaultData != null)
            {
                defaultHostCorpId = coreDefaultData.DefaultHostCorpId;
            }
            var faSystemParams = GetSystemParameters();
            var personRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(record.Recordkey);
            DataContracts.Address addressRecord = null;
            if ( (personRecord != null) && (!string.IsNullOrEmpty(personRecord.PreferredAddress)))
                addressRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(personRecord.PreferredAddress);

            institution = BuildInstitution(types, defaultHostCorpId, faSystemParams, personRecord, addressRecord, record);

            return institution;
        }

        /// <summary>
        /// Get a list of all institutions
        /// </summary>
        /// <returns>Institution Objects</returns>
        public IEnumerable<Institution> Get()
        {
            var institutions = GetOrAddToCache<IEnumerable<Institution>>("AllInstitutions",
                () =>
                {
                    // Get Special Processing on Institution Type
                    var types = GetValcode<InstitutionType>("CORE", "INST.TYPES", ii => new InstitutionType(ii.ValInternalCodeAssocMember, ii.ValExternalRepresentationAssocMember, ii.ValActionCode1AssocMember));

                    // Get Institutions if not in cache
                    var instList = new List<Institution>();
                    var institutionData = new List<Institutions>();
                    var personData = new List<Ellucian.Colleague.Data.Base.DataContracts.Person>();
                    var addressesData = new List<Ellucian.Colleague.Data.Base.DataContracts.Address>();
                    var coreDefaultData = GetDefaults();
                    var faSystemParams = GetSystemParameters();
                    var institutionIds = DataReader.Select("INSTITUTIONS", "WITH INST.TYPE NE ''");
                    for (int x = 0; x < institutionIds.Count(); x += readSize)
                    {
                        var subList = institutionIds.Skip(x).Take(readSize).ToArray();
                        var bulkInstitutionData = DataReader.BulkReadRecord<Institutions>("INSTITUTIONS", subList);
                        var bulkPersonData = DataReader.BulkReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", bulkInstitutionData.Select(p => p.Recordkey).Distinct().ToArray());
                        var bulkAddressesData = DataReader.BulkReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", bulkPersonData.Select(p => p.PreferredAddress).Distinct().ToArray());
                        institutionData.AddRange(bulkInstitutionData);
                        personData.AddRange(bulkPersonData);
                        addressesData.AddRange(bulkAddressesData);
                    }

                    var personLookup = personData.ToLookup(x => x.Recordkey);
                    var addressLookup = addressesData.ToLookup(x => x.Recordkey);

                    foreach (var i in institutionData)
                    {
                        try
                        {
                            InstType instType;
                            var codeAssoc = "";
                            foreach (var type in types)
                            {
                                if (type.Code == i.InstType)
                                {
                                    codeAssoc = type.Category;
                                }
                            }
                            if (!string.IsNullOrEmpty(codeAssoc))
                            {
                                switch (codeAssoc)
                                {
                                    case ("C"):
                                        instType = InstType.College;
                                        break;
                                    case ("H"):
                                        instType = InstType.HighSchool;
                                        break;
                                    default:
                                        instType = InstType.Unknown;
                                        break;
                                }
                                Institution inst = new Institution(i.Recordkey, instType);
                                inst.Ceeb = i.InstCeeb;
                            
                                // Update the Preferred Name and Address Data from Preferred Address
                                var person = personLookup[i.Recordkey].FirstOrDefault();
                                if (person != null)
                                {
                                    inst.Name = person.PreferredName;
                                    if (string.IsNullOrEmpty(inst.Name))
                                    {
                                        inst.Name = person.LastName;
                                    }

                                    var addressData = addressLookup[person.PreferredAddress].FirstOrDefault();
                                    if (addressData != null)
                                    {
                                        inst.City = addressData.City;
                                        inst.State = addressData.State;
                                    }
                                }
                                inst.IsHostInstitution = (i.Recordkey == coreDefaultData.DefaultHostCorpId);
                                if (inst.IsHostInstitution && faSystemParams != null)
                                {
                                    inst.FinancialAidInstitutionName = faSystemParams.FspInstitutionName;
                                }

                                instList.Add(inst);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogDataError("Institutions", i.Recordkey, i, ex);
                        }
                    }
                    return instList;
                }
            );
            return institutions;
        }

       
     
        private Institution BuildInstitution(IEnumerable<InstitutionType> types,  string defaultHostCorpId , FaSysParams faSystemParams, DataContracts.Person person, DataContracts.Address addressData, Institutions institution)
        {
            Institution inst = null;

            try
            {
                InstType instType;
                var codeAssoc = "";
                foreach (var type in types)
                {
                    if (type.Code == institution.InstType)
                    {
                        codeAssoc = type.Category;
                    }
                }
                if (!string.IsNullOrEmpty(codeAssoc))
                {
                    switch (codeAssoc)
                    {
                        case ("C"):
                            instType = InstType.College;
                            break;
                        case ("H"):
                            instType = InstType.HighSchool;
                            break;
                        default:
                            instType = InstType.Unknown;
                            throw new ArgumentException(string.Concat("The educational institution ", person.RecordGuid, " does not have a category that indicates secondarySchool or postSecondarySchool."));
                    }
                    inst = new Institution(institution.Recordkey, instType);
                    inst.Ceeb = institution.InstCeeb;

                    // Update the Preferred Name and Address Data from Preferred Address
                    //var person = personLookup[institution.Recordkey].FirstOrDefault();
                    if (person != null)
                    {
                        inst.Name = person.PreferredName;
                        if (string.IsNullOrEmpty(inst.Name))
                        {
                            inst.Name = person.LastName;
                        }

                       // var addressData = addressLookup[person.PreferredAddress].FirstOrDefault();
                        if (addressData != null)
                        {
                            inst.City = addressData.City;
                            inst.State = addressData.State;
                        }
                    }
                    inst.IsHostInstitution = (institution.Recordkey == defaultHostCorpId);
                    if (inst.IsHostInstitution && faSystemParams != null)
                    {
                        inst.FinancialAidInstitutionName = faSystemParams.FspInstitutionName;
                    }
                }
                else
                {
                    throw new ArgumentException(string.Concat("The educational institution ", person.RecordGuid, " does not have a valid institution type."));
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogDataError("Institutions", institution.Recordkey, institution, ex);
            }
            return inst;
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
        /// Get and Cache the FaSysParams record
        /// </summary>
        /// <returns>FaSysParams DataContract</returns>
        private FaSysParams GetSystemParameters()
        {
            return GetOrAddToCache<FaSysParams>("FinancialAidSystemParameters",
                            () =>
                            {
                                var sysParams = DataReader.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");
                                if (sysParams == null)
                                {
                                    logger.Info("Unable to read FA.SYS.PARAMS from database");
                                    sysParams = new FaSysParams();
                                }
                                return sysParams;
                            });
        }
    }
}