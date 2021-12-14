// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
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
        public static char _SM = Convert.ToChar(DynamicArray.SM);
        const string AllInstitutionsCache = "AllInstitutions";
        const int AllInstitutionsCacheTimeout = 20; // Clear from cache every 20 minutes
        RepositoryException repositoryException = null;

        public InstitutionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;

            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get a collection of Institution domain entities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="instType"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Institution>, int>> GetInstitutionAsync(int offset, int limit, InstType? instType = null,
             List<Tuple<string, string>> creds = null)
        {
            List<Institution> institutions = new List<Institution>();
            int totalCount = 0;
            string[] subList = null;
            var defaultHostCorpId = string.Empty;

            var institutionTypes = GetValcode<InstitutionType>("CORE", "INST.TYPES", 
                ii => new InstitutionType(ii.ValInternalCodeAssocMember, ii.ValExternalRepresentationAssocMember, ii.ValActionCode1AssocMember));

            if (institutionTypes == null)
            {
                return new Tuple<IEnumerable<Institution>, int>(new List<Institution>(), 0);
            }
            
            string institutionsCacheKey = CacheSupport.BuildCacheKey(AllInstitutionsCache, instType != null ? instType.ToString(): null);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                institutionsCacheKey,
                "INSTITUTIONS",
                offset,
                limit,
                AllInstitutionsCacheTimeout,
                async () =>
                {
                    string criteria = "WITH INST.TYPE NE ''";

                    if (instType != null)
                    {
                        string typeCriteria = "";
                        switch (instType)
                        {
                            case InstType.College:
                                var colleageTypes = institutionTypes.Where(tx => tx.Category == "C").Select(ty => ty.Code);
                                if (colleageTypes != null && colleageTypes.Any())
                                {
                                    foreach (var type in colleageTypes)
                                    {
                                        typeCriteria = string.Concat(typeCriteria, "'", type, "' ");
                                    }
                                }
                                break;
                            case InstType.HighSchool:
                                var hsTypes = institutionTypes.Where(tx => tx.Category == "H").Select(ty => ty.Code);
                                if (hsTypes != null && hsTypes.Any())
                                {
                                    foreach (var type in hsTypes)
                                    {
                                        typeCriteria = string.Concat(typeCriteria, "'", type, "' ");
                                    }
                                }
                                break;
                            default:
                                {
                                    return new CacheSupport.KeyCacheRequirements()
                                    {
                                        NoQualifyingRecords = true
                                    };
                                }

                        }
                        criteria = "WITH INST.TYPE EQ " + typeCriteria;
                    }

                    var colleaguePersonIds = new List<string>();

                    if (creds != null)
                    {
                        foreach (var cred in creds)
                        {
                            if (cred.Item1.ToLowerInvariant() == "colleaguepersonid")
                            {
                                colleaguePersonIds = new List<string>();
                                // to address issue if there is single quote around ColleagueId
                                var personId = cred.Item2;
                                if (personId.Contains("'"))
                                {
                                    personId = personId.Replace("'", string.Empty);
                                }
                                var ids = await DataReader.SelectAsync("PERSON", new string[] { personId }, string.Empty);
                                if (ids == null || !ids.Any())
                                {
                                    return new CacheSupport.KeyCacheRequirements()
                                    {
                                        NoQualifyingRecords = true
                                    };
                                }
                                else
                                {
                                    colleaguePersonIds.AddRange(ids);
                                }
                            }
                        }
                    }
                    return new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = criteria.ToString(),
                        limitingKeys = colleaguePersonIds
                    };
                });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<Institution>, int>(new List<Institution>(), 0);
            }

            subList = keyCache.Sublist.ToArray();
            totalCount = keyCache.TotalCount.Value;

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
            var bulkPersonIntgData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.PersonIntg>("PERSON.INTG", institutionData.Select(p => p.Recordkey).Distinct().ToArray());
            ICollection<Ellucian.Colleague.Data.Base.DataContracts.Address> bulkAddressesData = null;
            try
            {

                bulkAddressesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", bulkPersonData.SelectMany(p => p.PersonAddresses).Distinct().ToArray());
            }
            catch (Exception)
            {
                //do not throw exception.
            }
            var socialMediaKeys = await DataReader.SelectAsync("SOCIAL.MEDIA.HANDLES", "WITH SMH.PERSON.ID = '?'", institutionData.Select(p => p.Recordkey).Distinct().ToArray());
            var bulkSocialMediaData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.SocialMediaHandles>("SOCIAL.MEDIA.HANDLES", socialMediaKeys);

            foreach (var record in institutionData)
            {
                DataContracts.Person personRecord = null;
                DataContracts.PersonIntg personIntgRecord = null;
                List<DataContracts.Address> addressRecords = null;
                List<DataContracts.SocialMediaHandles> socialMediaRecords = null;

                try
                {
                    if (bulkPersonData != null && bulkPersonData.Any())
                    {
                        personRecord = bulkPersonData.FirstOrDefault(x => x.Recordkey == record.Recordkey);
                        personIntgRecord = bulkPersonIntgData.FirstOrDefault(x => x.Recordkey == record.Recordkey);
                        socialMediaRecords = bulkSocialMediaData.Where(sm => sm.SmhPersonId == personRecord.Recordkey).ToList();
                        if ((bulkAddressesData != null) && (personRecord != null) && (!string.IsNullOrEmpty(personRecord.PreferredAddress)))
                        {
                            addressRecords = bulkAddressesData.Where(y => personRecord.PreferredAddress.Contains(y.Recordkey)).ToList();
                        }
                    }

                    institutions.Add(BuildInstitution(institutionTypes, defaultHostCorpId, faSystemParams, personRecord, personIntgRecord, addressRecords, socialMediaRecords, record));
                }
                catch (Exception ex)
                {
                    if (repositoryException == null)
                        repositoryException = new RepositoryException();
                    repositoryException.AddError(new RepositoryError("Bad.Data", ex.Message));
                }
            }

            if (repositoryException != null && repositoryException.Errors != null && repositoryException.Errors.Any())
            {
                throw repositoryException;
            }

            return new Tuple<IEnumerable<Institution>, int>(institutions, totalCount);
        }


        /// <summary>
        /// Using a list of person ids, determine which ones are associated with an institution
        /// </summary>
        /// <param name="subList">list of person ids</param>
        /// <returns>Collection of ids</returns>
        public async Task<IEnumerable<string>> GetInstitutionIdsFromListAsync(string[] subList)
        {
            if (subList == null || !subList.Any())
            {
                return new string[] { };
            }
            return await DataReader.SelectAsync("INSTITUTIONS", subList, "");
        }

        /// <summary>
        /// sing a list of person ids, determine which ones are associated with an institution
        /// </summary>
        /// <param name="subList"></param>
        /// <returns>Collection of Institution domain entities</returns>
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
            // Even though this method could form a proper HEDM representation of an institution, we don't use it
            // for anything other than validation.  This method is called from vendors and payment transactions services
            // to decide if a GUID on an institution should be formatted.  The only thing these care about is that the
            // institution exists.  They don't reference any data from the institutions record so we don't need to spend 
            // the time building the additional data for phones, social media, emails, and addresses.
            //
            //var bulkPersonIntgData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.PersonIntg>("PERSON.INTG", institutionData.Select(p => p.Recordkey).Distinct().ToArray());
            //var socialMediaKeys = await DataReader.SelectAsync("SOCIAL.MEDIA.HANDLES", "WITH SMH.PERSON.ID = '?'", institutionData.Select(p => p.Recordkey).Distinct().ToArray());
            //var bulkSocialMediaData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.SocialMediaHandles>("SOCIAL.MEDIA.HANDLES", socialMediaKeys);
            //var bulkAddressesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", bulkPersonData.SelectMany(p => p.PersonAddresses).Distinct().ToArray());

            foreach (var record in institutionData)
            {
                DataContracts.Person personRecord = null;
                DataContracts.PersonIntg personIntgRecord = null;
                List<DataContracts.Address> addressRecords = null;
                List<DataContracts.SocialMediaHandles> socialMediaRecords = null;

                if (bulkPersonData != null && bulkPersonData.Any())
                {
                    personRecord = bulkPersonData.FirstOrDefault(x => x.Recordkey == record.Recordkey);
                    //personIntgRecord = bulkPersonIntgData.FirstOrDefault(x => x.Recordkey == record.Recordkey);
                    //socialMediaRecords = bulkSocialMediaData.Where(sm => sm.SmhPersonId == record.Recordkey).ToList();
                    //if ((bulkAddressesData != null) && (personRecord != null) && (!string.IsNullOrEmpty(personRecord.PreferredAddress)))
                    //    addressRecords = bulkAddressesData.Where(y => personRecord.PreferredAddress.Contains(y.Recordkey)).ToList();
                }
                institutions.Add(BuildInstitution(types, defaultHostCorpId, faSystemParams, personRecord, personIntgRecord, addressRecords, socialMediaRecords, record));
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
        /// <param name="id">The Institution ID</param>
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
            var personIntgRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.PersonIntg>(record.Recordkey);
            var socialMediaRecords = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.SocialMediaHandles>("SOCIAL.MEDIA.HANDLES", "WITH SMH.PERSON.ID = '" + record.Recordkey + "'");
            List<DataContracts.Address> addressRecords = null;
            if ( (personRecord != null) && (!string.IsNullOrEmpty(personRecord.PreferredAddress)))
                addressRecords = (await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(personRecord.PersonAddresses.Distinct().ToArray())).ToList();

            institution = BuildInstitution(types, defaultHostCorpId, faSystemParams, personRecord, personIntgRecord, addressRecords, socialMediaRecords.ToList(), record);

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

        private Institution BuildInstitution(IEnumerable<InstitutionType> institutionTypes, string defaultHostCorpId, FaSysParams faSystemParams, DataContracts.Person person,
            DataContracts.PersonIntg personIntg, List<DataContracts.Address> addressDataContracts, List<DataContracts.SocialMediaHandles> socialMediaRecords, Institutions institution)
        {

            if (institutionTypes == null)
            {
                throw new ArgumentNullException("institutionTypes", "institutionTypes are a required parameter.");
            }
            
            Institution inst = null;
            InstType instType;

            var institutionType = institutionTypes.FirstOrDefault(x => x.Code == institution.InstType);

            if ((institutionType == null) || (string.IsNullOrEmpty(institutionType.Category)))
            {
                throw new RepositoryException(string.Concat("The educational institution ", person != null ? person.RecordGuid : "", " does not have a valid institution type."));
            }

            switch (institutionType.Category)
            {
                case ("C"):
                    instType = InstType.College;
                    break;
                case ("H"):
                    instType = InstType.HighSchool;
                    break;
                default:
                    instType = InstType.Unknown;
                    throw new RepositoryException(string.Concat("The educational institution ", person != null ? person.RecordGuid : "", " does not have a category that indicates secondarySchool or postSecondarySchool."));
            }
            try
            {
                inst = new Institution(institution.Recordkey, instType);
            }
            catch (Exception ex)
            {
                throw new RepositoryException(ex.Message);

            }
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
                if (addressDataContracts != null)
                {
                    var addressData = addressDataContracts.FirstOrDefault(ad => ad.Recordkey == person.PreferredAddress);
                    if (addressData != null)
                    {
                        inst.City = addressData.City;
                        inst.State = addressData.State;
                    }
                }
            }
            inst.IsHostInstitution = (institution.Recordkey == defaultHostCorpId);
            if (inst.IsHostInstitution && faSystemParams != null)
            {
                inst.FinancialAidInstitutionName = faSystemParams.FspInstitutionName;
            }


            // Populate address, email, social media and phone numbers
            var tuplePerson = GetPersonIntegrationDataAsync(institution.Recordkey, person, personIntg, addressDataContracts, socialMediaRecords);
            inst.EmailAddresses = tuplePerson.Item1;
            inst.Phones = tuplePerson.Item2;
            inst.Addresses = tuplePerson.Item3;
            inst.SocialMedia = tuplePerson.Item4;

            return inst;
        }

        /// <summary>
        /// Get person addresses, email addresses and phones used for integration.
        /// </summary>
        /// <param name="personId">Person's Colleague ID</param>
        /// <param name="emailAddresses">List of <see cref="EmailAddress"> email addresses</see></param>
        /// <param name="phones">List of <see cref="Phone"> phones</see></param>
        /// <param name="addresses">List of <see cref="Address">addresses</see></param>
        /// <returns>Boolean where true is success and false otherwise</returns>
        private Tuple<List<EmailAddress>, List<Phone>, List<Domain.Base.Entities.Address>, List<Domain.Base.Entities.SocialMedia>, bool> GetPersonIntegrationDataAsync(string personId, 
            DataContracts.Person personData, PersonIntg personIntgData, List<DataContracts.Address> addressData, List<DataContracts.SocialMediaHandles> socialMediaRecords)
        {
            List<Domain.Base.Entities.Address> addresses = new List<Domain.Base.Entities.Address>();
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            List<Phone> phones = new List<Phone>();
            List<SocialMedia> socialMedias = new List<SocialMedia>();

            if (personData != null && personData.PersonCorpIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(personData.PreferredAddress) && addressData != null)
                {
                    var address = addressData.FirstOrDefault(ad => ad.Recordkey == personData.PreferredAddress);
                    if (address != null)
                    {
                        foreach (var phone in address.AdrPhonesEntityAssociation)
                        {
                            try
                            {
                                bool isPreferred = false;
                                string countryCallingCode = null;
                                if (personIntgData != null && personIntgData.PerIntgPhonesEntityAssociation != null && personIntgData.PerIntgPhonesEntityAssociation.Any())
                                {
                                    var matchingPhone = personIntgData.PerIntgPhonesEntityAssociation.FirstOrDefault(pi => pi.PerIntgPhoneNumberAssocMember == phone.AddressPhonesAssocMember);
                                    if (matchingPhone != null)
                                    {
                                        isPreferred = (!string.IsNullOrEmpty(matchingPhone.PerIntgPhonePrefAssocMember) ? matchingPhone.PerIntgPhonePrefAssocMember.Equals("Y", StringComparison.OrdinalIgnoreCase) : false);
                                        countryCallingCode = matchingPhone.PerIntgCtryCallingCodeAssocMember;
                                    }
                                }
                                phones.Add(new Phone(phone.AddressPhonesAssocMember, phone.AddressPhoneTypeAssocMember, phone.AddressPhoneExtensionAssocMember)
                                {
                                    CountryCallingCode = countryCallingCode,
                                    IsPreferred = isPreferred
                                });
                            }
                            catch (Exception exception)
                            {
                                logger.Error(exception, string.Format("Could not load phone number for person id '{0}' with GUID '{1}'", personData.Recordkey, personData.RecordGuid));
                            }
                        }
                    }
                }
            }
            // For now, we will not display personal phone numbers for non-corporations.
            // An INSTITUTION should never be a person but should always be a corporation however,
            // Colleague does allow this to happen and previously, the CTX did look at corp indicator
            // to determine where the phone number should come from.  At some point, we probably won't
            // want to even expose non-corporations with this API even though they have an INSTITUTIONS record.
            //if (personData != null && !personData.PersonCorpIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase))
            //{
            //    var index = 0;
            //    foreach (var phoneNumber in personData.PersonalPhoneNumber)
            //    {
            //        try
            //        {
            //            var phoneType = personData.PersonalPhoneType[index];
            //            var phoneExt = personData.PersonalPhoneExtension[index];
            //            bool isPreferred = false;
            //            string countryCallingCode = null;
            //            if (personIntgData != null && personIntgData.PerIntgPhonesEntityAssociation != null && personIntgData.PerIntgPhonesEntityAssociation.Any())
            //            {
            //                var matchingPhone = personIntgData.PerIntgPhonesEntityAssociation.FirstOrDefault(pi => pi.PerIntgPhoneNumberAssocMember == phoneNumber);
            //                if (matchingPhone != null)
            //                {
            //                    isPreferred = (!string.IsNullOrEmpty(matchingPhone.PerIntgPhonePrefAssocMember) ? matchingPhone.PerIntgPhonePrefAssocMember.Equals("Y", StringComparison.OrdinalIgnoreCase) : false);
            //                    countryCallingCode = matchingPhone.PerIntgCtryCallingCodeAssocMember;
            //                }
            //            }
            //            phones.Add(new Phone(phoneNumber, phoneType, phoneExt)
            //            {
            //                CountryCallingCode = countryCallingCode,
            //                IsPreferred = isPreferred
            //            });
            //        }
            //        catch (Exception exception)
            //        {
            //            logger.Error(exception, string.Format("Could not load phone number for person id '{0}' with GUID '{1}'", personData.Recordkey, personData.RecordGuid));
            //        }
            //        index++;
            //    }
            //}

            if (socialMediaRecords != null && socialMediaRecords.Any())
            {
                // create the email address entities
                foreach (var socialMedia in socialMediaRecords)
                {
                    try
                    {
                        var preferred = false;
                        if (!string.IsNullOrEmpty(socialMedia.SmhPreferred))
                        {
                            preferred = socialMedia.SmhPreferred.Equals("y", StringComparison.OrdinalIgnoreCase);
                        }
                        socialMedias.Add(new SocialMedia(socialMedia.SmhNetwork, socialMedia.SmhHandle, preferred));
                    }
                    catch (Exception exception)
                    {
                        logger.Error(exception, string.Format("Could not load social media for person id '{0}' with GUID '{1}'", personData.Recordkey, personData.RecordGuid));
                    }
                }
            }
            if (personData != null && !string.IsNullOrEmpty(personData.PersonWebsiteAddress))
            {
                socialMedias.Add(new SocialMedia("website", personData.PersonWebsiteAddress, false));
            }

            if (personData != null && personData.PersonEmailAddresses != null && personData.PersonEmailAddresses.Any())
            {
                for (int i = 0; i < personData.PersonEmailAddresses.Count; i++)
                {
                    try
                    {
                        var emailAddress = personData.PersonEmailAddresses[i];
                        var emailAddressType = personData.PersonEmailTypes.Count > i
                            ? personData.PersonEmailTypes[i]
                            : null;
                        var emailAddressPreferred = personData.PersonPreferredEmail.Count > i
                            ? personData.PersonPreferredEmail[i]
                            : string.Empty;

                        var emailToAdd = new EmailAddress(emailAddress, emailAddressType)
                        {
                            IsPreferred = emailAddressPreferred.Equals("y", StringComparison.OrdinalIgnoreCase)
                        };
                        
                        emailAddresses.Add(emailToAdd);
                    }
                    catch (Exception exception)
                    {
                       // do not log error
                        //logger.Error(exception, string.Format("Could not load email address for person id '{0}' with GUID '{1}'", personData.Recordkey, personData.RecordGuid));
                    }
                }
            }

            if (personData != null && addressData != null && personData.PersonAddresses != null && personData.PersonAddresses.Any())
            {
                // Current Addresses
                var addressIds = personData.PersonAddresses;
                var addressDataContracts = addressData.Where(ad => addressIds.Contains(ad.Recordkey));
                if (addressDataContracts.Any())
                {
                    // create the address entities
                    foreach (var address in addressDataContracts)
                    {
                        var addressEntity = new Domain.Base.Entities.Address();
                        addressEntity.Guid = address.RecordGuid;
                        addressEntity.City = address.City;
                        addressEntity.State = address.State;
                        addressEntity.PostalCode = address.Zip;
                        addressEntity.Country = address.Country;
                        addressEntity.County = address.County;
                        addressEntity.AddressLines = address.AddressLines;
                        // Find Addrel Association in Person contract
                        var assocEntity = personData.PseasonEntityAssociation.FirstOrDefault(pa => address.Recordkey == pa.PersonAddressesAssocMember);
                        if (assocEntity != null)
                        {
                            //addressEntity.TypeCode = assocEntity.AddrTypeAssocMember.Split(_SM).FirstOrDefault();
                            addressEntity.TypeCode = assocEntity.AddrTypeAssocMember;
                            addressEntity.EffectiveStartDate = assocEntity.AddrEffectiveStartAssocMember;
                            addressEntity.EffectiveEndDate = assocEntity.AddrEffectiveEndAssocMember;
                            addressEntity.SeasonalDates = new List<AddressSeasonalDates>();
                            if (!string.IsNullOrEmpty(assocEntity.AddrSeasonalStartAssocMember) && !string.IsNullOrEmpty(assocEntity.AddrSeasonalEndAssocMember))
                            {
                                // This could be subvalued so need to split on subvalue mark ASCII 252.
                                string[] startDate = assocEntity.AddrSeasonalStartAssocMember.Split(_SM);
                                string[] endDate = assocEntity.AddrSeasonalEndAssocMember.Split(_SM);
                                for (int i = 0; i < startDate.Length; i++)
                                {
                                    try
                                    {
                                        // add in the address override phones into the person's list of phones
                                        AddressSeasonalDates seasonalDates = new AddressSeasonalDates(startDate[i], endDate[i]);
                                        addressEntity.SeasonalDates.Add(seasonalDates);
                                    }
                                    catch (Exception ex)
                                    {
                                        var error = "Person address seasonal start/end information is invalid. PersonId: " + personId;

                                        // Log the original exception
                                        logger.Error(ex.ToString());
                                        logger.Info(error);
                                    }
                                }
                            }
                        }
                        addressEntity.IsPreferredAddress = (address.Recordkey == personData.PreferredAddress);
                        addressEntity.IsPreferredResidence = (address.Recordkey == personData.PreferredResidence);
                        addressEntity.Status = "Current";
                        addresses.Add(addressEntity);
                    }
                }
            }

            return new Tuple<List<EmailAddress>, List<Phone>, List<Domain.Base.Entities.Address>, List<Domain.Base.Entities.SocialMedia>, bool>(emailAddresses, phones, addresses, socialMedias, true);
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