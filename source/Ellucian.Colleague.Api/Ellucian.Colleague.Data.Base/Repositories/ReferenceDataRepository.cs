// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Configuration;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for reference data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ReferenceDataRepository : BaseColleagueRepository, IReferenceDataRepository
    {
        private const string _preferredHierarchyCode = "PREFERRED";
        private RepositoryException exception;
        private int bulkReadSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public ReferenceDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
            if (apiSettings != null)
            {
                bulkReadSize = apiSettings.BulkReadSize;
            }
        }

        /// <summary>
        /// Private method to return all active major codes
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Collection of all active major codes</returns>
        private IEnumerable<string> _activeMajors = null;
        private async Task<IEnumerable<string>> GetActiveMajorsAsync(bool ignoreCache = false)
        {
            if (_activeMajors == null || ignoreCache)
            {
                IEnumerable<Majors> majors = await DataReader.BulkReadRecordAsync<Majors>("", true);
                _activeMajors = majors.Where(m => !string.IsNullOrEmpty(m.MajActiveFlag) && m.MajActiveFlag == "Y").OrderBy(m => m.Recordkey).Select(m => m.Recordkey);
            }

            return _activeMajors;
        }
        
 
        /// <summary>
        /// Get the GuidLookupResult for a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>GuidLookupResult or KeyNotFoundException if supplied Guid was not found</returns>
        public async Task<GuidLookupResult> GetGuidLookupResultFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || !idDict.Any())
            {
                throw new KeyNotFoundException("GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();

            if (!string.IsNullOrEmpty(foundEntry.Key) && foundEntry.Value != null)
            {
                return foundEntry.Value;
            }
            else
            {
                throw new KeyNotFoundException("GUID " + guid + " not found.");
            }
        }

        /// <summary>
        /// Get a collection of AcadCredentials
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AcadCredentials</returns>
        public async Task<IEnumerable<AcadCredential>> GetAcadCredentialsAsync(bool ignoreCache)
        {
            var academicDisciplineCollection = new List<AcadCredential>();

            var ccds = await GetGuidCodeItemAsync<OtherCcds, AcadCredential>("AllAcadCredentialCcd", "OTHER.CCDS",
                (m, g) => new
                    AcadCredential(g, m.Recordkey, 
                    (string.IsNullOrEmpty(m.OccdDesc) ? m.Recordkey : m.OccdDesc),
                    AcademicCredentialType.Certificate), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
            if (ccds != null)
            {
                academicDisciplineCollection.AddRange(ccds.ToList());
            }

            var degrees = await GetGuidCodeItemAsync<OtherDegrees, AcadCredential>("AllAcadCredentialDegree", "OTHER.DEGREES",
                (m, g) => new
                    AcadCredential(g, m.Recordkey,
                    (string.IsNullOrEmpty(m.OdegDesc) ? m.Recordkey : m.OdegDesc), 
                     AcademicCredentialType.Degree), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);

            if (degrees != null)
            {
                academicDisciplineCollection.AddRange(degrees.ToList());
            }

            return academicDisciplineCollection;
        }

        /// <summary>
        /// Get guid for AcadCredentials code
        /// </summary>
        /// <param name="code">AcadCredentials code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAcadCredentialsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAcadCredentialsAsync(false);
            AcadCredential codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAcadCredentialsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.CCDS or OTHER.DEGREES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.CCDS or OTHER.DEGREES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.CCDS or OTHER.DEGREES', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of AltIdTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AltIdTypes</returns>
        public async Task<IEnumerable<AltIdTypes>> GetAlternateIdTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<AltIdTypes>("CORE", "ALT.ID.TYPES",
                (cl, g) => new AltIdTypes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of Academic Discipline
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Academic Disciplines</returns>
        public async Task<IEnumerable<AcademicDiscipline>> GetAcademicDisciplinesAsync(bool ignoreCache)
        {
            var academicDisciplineCollection = new List<AcademicDiscipline>();

            var majors = await GetAcademicDisciplinesMajorAsync(ignoreCache);
            if (majors != null) academicDisciplineCollection.AddRange(majors.ToList());
            
            var minors = await GetAcademicDisciplinesMinorAsync(ignoreCache);
            if (minors != null) academicDisciplineCollection.AddRange(minors.ToList());

            var specials = await GetAcademicDisciplinesSpecialAsync(ignoreCache);
            if (specials != null) academicDisciplineCollection.AddRange(specials.ToList());

            return academicDisciplineCollection;
        }

        /// <summary>
        /// Get guid for AcadDisciplines code
        /// </summary>
        /// <param name="code">AcadDisciplines code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAcadDisciplinesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAcademicDisciplinesAsync(false);
            AcademicDiscipline codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAcademicDisciplinesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MAJORS, OTHER.MINORS or OTHER.SPECIALS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MAJORS, OTHER.MINORS or OTHER.SPECIALS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MAJORS, OTHER.MINORS or OTHER.SPECIALS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of Academic Discipline
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Academic Disciplines</returns>
        public async Task<IEnumerable<AcademicDiscipline>> GetAcademicDisciplinesMajorAsync(bool ignoreCache)
        {
            var academicDisciplineCollection = new List<AcademicDiscipline>();
            // Get the active flag from the MAJORS record
            var activeMajors = await GetActiveMajorsAsync(ignoreCache);

            var majors = await GetGuidCodeItemAsync<OtherMajors, AcademicDiscipline>("AllAcademicDisciplinesMajor", "OTHER.MAJORS",
                (m, g) => BuildAcademicDisciplineMajorAsync(m, g, activeMajors),CacheTimeout, DataReader.IsAnonymous, ignoreCache);

            if (majors != null)
            {
                academicDisciplineCollection.AddRange(majors.ToList());
            }

            return academicDisciplineCollection;
        }

        /// <summary>
        /// Private method to build the Major Academic Discipline type
        /// </summary>
        /// <param name="m">OtherMajors record</param>
        /// <param name="g">Guid string</param>
        /// <returns>Academic Discipline</returns>
        /// 
        private AcademicDiscipline BuildAcademicDisciplineMajorAsync(OtherMajors m, string g, IEnumerable<string> activeMajors)
        {
            var disc = new AcademicDiscipline(g, m.Recordkey,
                    (string.IsNullOrEmpty(m.OmajDesc) ? m.Recordkey : m.OmajDesc),

                    AcademicDisciplineType.Major);
           
            disc.ActiveMajor = activeMajors.Contains(m.Recordkey);
            return disc;
        }
        
        /// <summary>
        /// Get a collection of Academic Discipline
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Academic Disciplines</returns>
        public async Task<IEnumerable<AcademicDiscipline>> GetAcademicDisciplinesMinorAsync(bool ignoreCache)
        {
            var academicDisciplineCollection = new List<AcademicDiscipline>();

            var minors = await GetGuidCodeItemAsync<OtherMinors, AcademicDiscipline>("AllAcademicDisciplinesMinor", "OTHER.MINORS",
                (m, g) => new
                    AcademicDiscipline(g, m.Recordkey,
                    (string.IsNullOrEmpty(m.OminDesc) ? m.Recordkey : m.OminDesc),
                    AcademicDisciplineType.Minor), CacheTimeout, DataReader.IsAnonymous, ignoreCache);
            if (minors != null)
            {
                academicDisciplineCollection.AddRange(minors.ToList());
            }

            return academicDisciplineCollection;
        }

        /// <summary>
        /// Get a collection of Academic Discipline
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Academic Disciplines</returns>
        public async Task<IEnumerable<AcademicDiscipline>> GetAcademicDisciplinesSpecialAsync(bool ignoreCache)
        {
            var academicDisciplineCollection = new List<AcademicDiscipline>();

            var special = await GetGuidCodeItemAsync<OtherSpecials, AcademicDiscipline>("AllAcademicDisciplinesSpecial", "OTHER.SPECIALS",
                (s, g) => new
                    AcademicDiscipline(g, s.Recordkey,
                    (string.IsNullOrEmpty(s.OspecDesc) ? s.Recordkey : s.OspecDesc),
                    AcademicDisciplineType.Concentration), CacheTimeout, DataReader.IsAnonymous, ignoreCache);

            academicDisciplineCollection.AddRange(special.ToList());

            return academicDisciplineCollection;
        }

        /// <summary>
        /// Get a single AcademicDisciplinesMajor using an ID
        /// </summary>
        /// <param name="id">The AcademicDiscipline id</param>
        /// <returns>The AcademicDiscipline</returns>
        public async Task<AcademicDiscipline> GetAcademicDisciplinesMajorAsync(string id, bool ignoreCache = false)
        {
            AcademicDiscipline academicDiscipline = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a academic discipline.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<OtherMajors>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or academic discipline with ID ", id, "invalid."));
            }

            // Build the academic discipline data
            academicDiscipline = new AcademicDiscipline(record.RecordGuid, record.Recordkey, string.IsNullOrEmpty(record.OmajDesc)
                ? record.Recordkey : record.OmajDesc, AcademicDisciplineType.Major);

            return academicDiscipline;
        }

        /// <summary>
        /// Get a single AcademicDiscipline using an ID
        /// </summary>
        /// <param name="id">The AcademicDiscipline id</param>
        /// <returns>The AcademicDiscipline</returns>
        public async Task<AcademicDiscipline> GetAcademicDisciplinesMinorAsync(string id, bool ignoreCache = false)
        {
            AcademicDiscipline academicDiscipline = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a academic discipline.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<OtherMinors>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or academic discipline with ID ", id, "invalid."));
            }

            // Build the academic discipline data
            academicDiscipline = new AcademicDiscipline(record.RecordGuid, record.Recordkey, string.IsNullOrEmpty(record.OminDesc)
                ? record.Recordkey : record.OminDesc, AcademicDisciplineType.Minor);

            return academicDiscipline;
        }

        /// <summary>
        /// Get a single academic discipline using an ID
        /// </summary>
        /// <param name="id">The academic discipline GUID</param>
        /// <returns>The academic discipline</returns>
        public async Task<AcademicDiscipline> GetAcademicDisciplinesSpecialAsync(string id, bool ignoreCache = false)
        {
            AcademicDiscipline academicDiscipline = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a academic discipline.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<OtherSpecials>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or academic discipline with ID ", id, "invalid."));
            }

            // Build the academic discipline data
            academicDiscipline = new AcademicDiscipline(record.RecordGuid, record.Recordkey, string.IsNullOrEmpty(record.OspecDesc)
                ? record.Recordkey : record.OspecDesc, AcademicDisciplineType.Concentration);

            return academicDiscipline;
        }

        public async Task<AcademicDisciplineType> GetRecordInfoFromGuidAcademicDisciplineAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Academic Discipline ID must be provided.");
            }
            var recordInfo = await this.GetRecordInfoFromGuidAsync(guid);
            if (recordInfo != null)
            {
                switch (recordInfo.Entity)
                {
                    case ("OTHER.MAJORS"):
                        return AcademicDisciplineType.Major;
                    case ("OTHER.MINORS"):
                        return AcademicDisciplineType.Minor;
                    case ("OTHER.SPECIALS"):
                        return AcademicDisciplineType.Concentration;
                    default:
                        throw new KeyNotFoundException("Academic Discipline ID not found.");
                }
            }
            else
            {
                throw new KeyNotFoundException("Academic Discipline ID not found.");
            }
        }

        public async Task<GuidLookupResult> GetRecordInfoFromGuidReferenceDataRepoAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Academic Discipline ID must be provided.");
            }
            var recordInfo = await this.GetRecordInfoFromGuidAsync(guid);
            if (recordInfo != null)
            {
                return recordInfo;
            }
            else
            {
                throw new KeyNotFoundException("Academic Discipline ID not found.");
            }
        }

        /// <summary>
        /// Get a collection of address change sources
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of address change sources</returns>
        public async Task<IEnumerable<AddressChangeSource>> GetAddressChangeSourcesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<AddressChangeSource>("CORE", "ADDRESS.CHANGE.SOURCES",
                (cl, g) => new AddressChangeSource(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Address Relationship Types
        /// </summary>
        public IEnumerable<AddressRelationType> AddressRelationTypes
        {
            get
            {
                return GetValcode<AddressRelationType>("CORE", "ADREL.TYPES", Adrel => new AddressRelationType(Adrel.ValInternalCodeAssocMember, Adrel.ValExternalRepresentationAssocMember, Adrel.ValActionCode1AssocMember, Adrel.ValActionCode2AssocMember));
            }
        }

        ///// <summary>
        ///// Get a collection of address types
        ///// </summary>
        ///// <param name="ignoreCache">Bypass cache flag</param>
        ///// <returns>Collection of address types</returns>
        //public async Task<IEnumerable<AddressTypeItem>> GetAddressTypesAsync(bool ignoreCache)
        //{

        //    return await GetGuidValcodeAsync<AddressTypeItem>("CORE", "ADREL.TYPES",
        //            (e, g) => new AddressTypeItem(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
        //                ConvertEntityTypeCodeToEntityType(e.ValActionCode3AssocMember, e.ValActionCode4AssocMember),
        //                ConvertPersonAddressTypeCodeToPersonAddressType(e.ValActionCode3AssocMember),
        //                ConvertOrgAddressTypeCodeToOrgAddressType(e.ValActionCode4AssocMember)), bypassCache: ignoreCache);
        //}

        /// <summary>
        /// Get a collection of address types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of address types</returns>
        public async Task<IEnumerable<AddressType2>> GetAddressTypes2Async(bool ignoreCache)
        {

            return await GetGuidValcodeAsync<AddressType2>("CORE", "ADREL.TYPES",
                    (e, g) => new AddressType2(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
                        ConvertAddressTypeCodeToAddressType(e.ValActionCode3AssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Building.
        /// </summary>
        public async Task<IEnumerable<Building>> BuildingsAsync()
        {
            return await GetBuildingsAsync(false);
        }
                
        /// <summary>
        /// Get a collection of buildings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of buildings</returns>
        public async Task<IEnumerable<Building>> GetBuildingsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Buildings, Building>("AllBuildings", "BUILDINGS",
                (b, g) => new Building(g, b.Recordkey, b.BldgDesc, b.BldgLocation, b.BldgType, b.BldgLongDesc, b.BldgAddress, b.BldgCity, b.BldgState, b.BldgZip, b.BldgCountry, b.BldgLatitude,
                                  b.BldgLongitude, b.BldgImageResource, b.BldgAddnlServices, b.BldgExportToMobile), anonymous: this.DataReader.IsAnonymous, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of buildings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of buildings</returns>
        public async Task<IEnumerable<Building>> GetBuildings2Async(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Buildings, Building>("AllHedmBuildings", "BUILDINGS",
                (b, g) => new Building(g, b.Recordkey, b.BldgDesc, b.BldgLocation, b.BldgLongDesc, b.BldgAddress, b.BldgCity, b.BldgState, b.BldgZip, b.BldgCountry, b.BldgLatitude,
                                  b.BldgLongitude, b.BldgImageResource, b.BldgAddnlServices, b.BldgComments, b.BldgExportToMobile), anonymous: this.DataReader.IsAnonymous, bypassCache: ignoreCache);
        }


        /// <summary>
        /// BuildingTypes
        /// </summary>
        public IEnumerable<BuildingType> BuildingTypes
        {
            get
            {
                var buildingTypes = GetOrAddToCache<List<BuildingType>>("AllBuildingTypes",
                    () =>
                    {
                        string cacheKeyToUse = string.Empty;
                        if (this.DataReader.IsAnonymous)
                        {
                            cacheKeyToUse = "BuildingTypes_Anonymous";
                        }
                        else
                        {
                            cacheKeyToUse = "BuildingTypes";
                        }
                        List<BuildingType> bldgTypes = new List<BuildingType>();
                        try
                        {
                            ApplValcodes bldgTypeValcode = this.DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "BUILDING.TYPES");
                            if (bldgTypeValcode != null)
                            {
                                foreach (ApplValcodesVals applVal in bldgTypeValcode.ValsEntityAssociation)
                                {
                                    bldgTypes.Add(new BuildingType(applVal.ValInternalCodeAssocMember, applVal.ValExternalRepresentationAssocMember));
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // if the valcode is not public, don't throw, but return an empty list
                            //throw new ApplicationException("Anonymous data reader request denied. Table is not public.");
                        }
                        return bldgTypes;
                    }
                );
                return buildingTypes;
            }
        }

        /// <summary>
        /// Get a collection of CcdType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CcdType</returns>
        public async Task<IEnumerable<Domain.Base.Entities.CcdType>> GetCcdTypeAsync(bool ignoreCache)
        {

            return await GetCodeItemAsync<DataContracts.CcdType, Domain.Base.Entities.CcdType>("AllCcdType", "CCD.TYPE",
                   c => new Domain.Base.Entities.CcdType(c.Recordkey, c.CcdtypeDescription, c.CcdtypeCredentialLevel), 
                   CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get a collection of chapters
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of chapters</returns>
        public async Task<IEnumerable<Chapter>> GetChaptersAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Chapters, Chapter>("AllChapters", "CHAPTERS",
               (c, g) => new Chapter(g, c.Recordkey, c.ChaptersDesc), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get a collection of citizenship statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of citizenship statuses</returns>
        public async Task<IEnumerable<CitizenshipStatus>> GetCitizenshipStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<CitizenshipStatus>("CORE", "ALIEN.STATUSES",
                (c, g) => new CitizenshipStatus(g, c.ValInternalCodeAssocMember, c.ValExternalRepresentationAssocMember,
                    ConvertCitizenshipStatusTypeCodeToCitizenshipStatusType(c.ValActionCode1AssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Citizen Types (Alien Statuses)
        /// </summary>
        public IEnumerable<CitizenType> CitizenTypes
        {
            get
            {
                return GetValcode<CitizenType>("CORE", "ALIEN.STATUSES", citizenType => new CitizenType(citizenType.ValInternalCodeAssocMember, citizenType.ValExternalRepresentationAssocMember));
            }
        }

        /// <summary>
        /// Get a collection of Commerce Tax Code Rates
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CommerceTaxCodeRate domain entities</returns>
        public async Task<IEnumerable<CommerceTaxCodeRate>> GetCommerceTaxCodeRatesAsync(bool ignoreCache)
        {
            if (ignoreCache)
            {
                var apTaxesData = await DataReader.BulkReadRecordAsync<DataContracts.ApTaxes>("AP.TAXES", "");
                var commerceTaxCodeRates = (await BuildCommerceTaxCodeRateAsync(apTaxesData)).ToList();
                return await AddOrUpdateCacheAsync<IEnumerable<CommerceTaxCodeRate>>("AllCommerceTaxCodeRates", commerceTaxCodeRates);

            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<CommerceTaxCodeRate>>("AllCommerceTaxCodeRates", async () =>
                    await BuildCommerceTaxCodeRateAsync(await DataReader.BulkReadRecordAsync<DataContracts.ApTaxes>("AP.TAXES", "")), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Build a collection of CommerceTaxCodeRate domain entities from an ApTaxes datacontract collection
        /// </summary>
        /// <param name="apTaxDataContracts">ApTaxes data contract collection</param>
        /// <returns>Collection of CommerceTaxCodeRate domain entities</returns>
        private async Task<IEnumerable<CommerceTaxCodeRate>> BuildCommerceTaxCodeRateAsync(IEnumerable<DataContracts.ApTaxes> apTaxDataContracts)
        {
            var repositoryException = new RepositoryException();

            var commerceTaxCodeRateCollection = new List<CommerceTaxCodeRate>();

            if (apTaxDataContracts == null)
            {
                return commerceTaxCodeRateCollection;
            }

            var subList = apTaxDataContracts.Select(x => x.Recordkey).ToArray();

            if (subList == null || !subList.Any())
            {
                return commerceTaxCodeRateCollection;
            }
            // Execute primary select against indexed field that is most likely to return the smallest resulting set of guids
            var guidPrimaryList = await DataReader.SelectAsync("LDM.GUID", "WITH LDM.GUID.PRIMARY.KEY EQ '?'", subList);

            // Execute secondary select against this subset of guids to further filter it down to the one(s) we want
            var guidList = await DataReader.SelectAsync("LDM.GUID", guidPrimaryList, "WITH LDM.GUID.SECONDARY.FLD EQ 'AP.TAX.EFFECTIVE.DATE' AND LDM.GUID.ENTITY EQ 'AP.TAXES' AND LDM.GUID.REPLACED.BY EQ ''");


            if (guidList == null || !guidList.Any())
            {
                repositoryException.AddError(new RepositoryError("CommerceTaxCodeRates", "An error orrurred extracting valid guids with LDM.GUID.SECONDARY.FLD EQ 'AP.TAX.EFFECTIVE.DATE' "));
                throw repositoryException;
            }

            var ldmGuidRecords = await DataReader.BulkReadRecordAsync<LdmGuid>(guidList);

            foreach (var apTaxDataContract in apTaxDataContracts)
            {
                try
                {
                    if (apTaxDataContract.TaxInfoEntityAssociation != null && apTaxDataContract.TaxInfoEntityAssociation.Any())
                    {

                        var taxinfoEntities = apTaxDataContract.TaxInfoEntityAssociation.Where(d => d.ApTaxEffectiveDateAssocMember.HasValue);

                        foreach (var taxInfo in taxinfoEntities)
                        {
                            var effectiveDate = DmiString.DateTimeToPickDate(Convert.ToDateTime(taxInfo.ApTaxEffectiveDateAssocMember));

                            var ldmGuidRecord = ldmGuidRecords.FirstOrDefault(x => x.LdmGuidPrimaryKey.Equals(apTaxDataContract.Recordkey, StringComparison.OrdinalIgnoreCase)
                                    && x.LdmGuidSecondaryKey.Equals(effectiveDate.ToString()));

                            if (ldmGuidRecord == null)
                            {
                                repositoryException.AddError(new RepositoryError("CommerceTaxCodeRates",
                                    string.Format("Guid not found for key {0} and effectiveDate {1}", apTaxDataContract.Recordkey, effectiveDate.ToString())));

                                continue;
                            }

                            var commerceTaxCodeRateEntity = new CommerceTaxCodeRate(ldmGuidRecord.Recordkey, apTaxDataContract.Recordkey, apTaxDataContract.ApTaxDesc)
                            {
                                ApTaxCompoundingSequence = taxInfo.ApTaxCompoundingSeqAssocMember,
                                ApTaxEffectiveDate = taxInfo.ApTaxEffectiveDateAssocMember,
                                ApTaxExemptPercent = taxInfo.ApTaxExemptPctAssocMember,
                                ApTaxPercent = taxInfo.ApTaxPctAssocMember,
                                ApTaxRebatePercent = taxInfo.ApTaxRebatePctAssocMember
                            };

                            commerceTaxCodeRateCollection.Add(commerceTaxCodeRateEntity);
                        }
                    }
                }
                catch (Exception ex)
                {

                    repositoryException.AddError(new RepositoryError("CommerceTaxCodeRates", ex.Message));
                }
            }         

            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return commerceTaxCodeRateCollection;
        }

        /// <summary>
        /// Get guid for CommerceTaxCodeRate code
        /// </summary>
        /// <param name="code">CommerceTaxCode code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCommerceTaxCodeRateGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCommerceTaxCodeRatesAsync(false);
            CommerceTaxCodeRate codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCommerceTaxCodeRatesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TAXES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TAXES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TAXES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of Commerce Tax Codes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of commerce tax codes</returns>
        public async Task<IEnumerable<CommerceTaxCode>> GetCommerceTaxCodesAsync(bool ignoreCache)
        {
            if (ignoreCache)
            {
                var apTaxesData = await DataReader.BulkReadRecordAsync<DataContracts.ApTaxes>("AP.TAXES", "");
                var commerceTaxCodes = (await BuildCommerceTaxCodesAsync(apTaxesData)).ToList();
                return await AddOrUpdateCacheAsync<IEnumerable<CommerceTaxCode>>("AllCommerceTaxCodes", commerceTaxCodes);

            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<CommerceTaxCode>>("AllCommerceTaxCodes", async () =>
                    await BuildCommerceTaxCodesAsync(await DataReader.BulkReadRecordAsync<DataContracts.ApTaxes>("AP.TAXES", "")), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Build a collection of CommerceTaxCodeRate domain entities from an ApTaxes datacontract collection
        /// </summary>
        /// <param name="apTaxDataContracts">ApTaxes data contract collection</param>
        /// <returns>Collection of CommerceTaxCodeRate domain entities</returns>
        private async Task<IEnumerable<CommerceTaxCode>> BuildCommerceTaxCodesAsync(IEnumerable<DataContracts.ApTaxes> apTaxDataContracts)
        {
            var repositoryException = new RepositoryException();

            var commerceTaxCodesCollection = new List<CommerceTaxCode>();

            if (apTaxDataContracts == null)
            {
                return commerceTaxCodesCollection;
            }

            var subList = apTaxDataContracts.Select(x => x.Recordkey).ToArray();

            if (subList == null || !subList.Any())
            {
                return commerceTaxCodesCollection;
            }
            // Execute primary select against indexed field that is most likely to return the smallest resulting set of guids
            var guidPrimaryList = await DataReader.SelectAsync("LDM.GUID", "WITH LDM.GUID.PRIMARY.KEY EQ '?'", subList);

            // Execute secondary select against this subset of guids to further filter it down to the one(s) we want
            var guidList = await DataReader.SelectAsync("LDM.GUID", guidPrimaryList, "WITH LDM.GUID.SECONDARY.FLD EQ '' AND LDM.GUID.ENTITY EQ 'AP.TAXES' AND LDM.GUID.SECONDARY.KEY EQ ''");


            if (guidList == null || !guidList.Any())
            {
                repositoryException.AddError(new RepositoryError("CommerceTaxCodes", "An error orrurred extracting valid guids."));
                throw repositoryException;
            }

            var ldmGuidRecords = await DataReader.BulkReadRecordAsync<LdmGuid>(guidList);

            foreach (var apTaxDataContract in apTaxDataContracts)
            {
                try
                {
                    var ldmGuidRecord = ldmGuidRecords.FirstOrDefault(x => x.LdmGuidPrimaryKey.Equals(apTaxDataContract.Recordkey, StringComparison.OrdinalIgnoreCase));
                    if (ldmGuidRecord == null)
                    {
                        repositoryException.AddError(new RepositoryError("CommerceTaxCodes",
                            string.Format("Guid not found for key {0}", apTaxDataContract.Recordkey)));

                        continue;
                    }
                    var commerceTaxCodeEntity = new CommerceTaxCode(ldmGuidRecord.Recordkey, apTaxDataContract.Recordkey, apTaxDataContract.ApTaxDesc)
                    {
                        AppurEntryFlag = !string.IsNullOrEmpty(apTaxDataContract.ApTaxAppurEntryFlag) && apTaxDataContract.ApTaxAppurEntryFlag == "Y" ? true : false,
                        UseTaxFlag = !string.IsNullOrEmpty(apTaxDataContract.ApUseTaxFlag) && apTaxDataContract.ApUseTaxFlag == "Y" ? true : false,
                        ApTaxEffectiveDates = apTaxDataContract.ApTaxEffectiveDate
                    };
                    commerceTaxCodesCollection.Add(commerceTaxCodeEntity);
                }
                catch (Exception ex)
                {
                    repositoryException.AddError(new RepositoryError("CommerceTaxCodes", ex.Message));
                }
            }

            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return commerceTaxCodesCollection;
        }

        /// <summary>
        /// Get guid for CommerceTaxCode code
        /// </summary>
        /// <param name="code">CommerceTaxCode code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCommerceTaxCodeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCommerceTaxCodesAsync(false);
            CommerceTaxCode codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCommerceTaxCodesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TAXES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TAXES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TAXES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Communication codes
        /// </summary>
        public IEnumerable<CommunicationCode> CommunicationCodes
        {
            get
            {
                //Communication codes on CCWP form (to be displayed)
                CorewebDefaults corewebDefaultsRecord = null;
                try
                {
                    corewebDefaultsRecord = GetCorewebDefaults();
                }
                catch (Exception e)
                {
                    //If COREWEB.DEFAULTS doesn't exist (possibility for newer clients, clients who
                    //don't use CM, etc.), an error could be thrown by the cache mechanism because of a null record.
                    logger.Info(e, "Error retrieving COREWEB.DEFAULTS record");
                }

                return GetOrAddToCache<IEnumerable<CommunicationCode>>("AllCommunicationCodes",
                    () =>
                    {
                        var ccRecords = DataReader.BulkReadRecord<CcCodes>("", true);
                        if (ccRecords == null || ccRecords.Count == 0)
                        {
                            logger.Error("Null CcCodes records returned by DataReader");
                            return new List<CommunicationCode>();
                        }



                        var communicationCodeEntities = new List<CommunicationCode>();
                        foreach (var ccRecord in ccRecords)
                        {
                            try
                            {
                                communicationCodeEntities.Add(new CommunicationCode(ccRecord.RecordGuid, ccRecord.Recordkey, ccRecord.CcDescription)
                                    {
                                        AwardYear = ccRecord.CcFaYear,
                                        Explanation = ccRecord.CcExplanation,
                                        OfficeCodeId = ccRecord.CcOffice,
                                        IsStudentViewable = (corewebDefaultsRecord == null || corewebDefaultsRecord.CorewebCcCodes == null) ? false :
                                            corewebDefaultsRecord.CorewebCcCodes.Contains(ccRecord.Recordkey, StringComparer.Create(CultureInfo.CurrentCulture, true)),
                                        AllowsAttachments = (string.IsNullOrEmpty(ccRecord.CcAllowAttachments) || ccRecord.CcAllowAttachments.ToUpper() != "Y") ? false : true,
                                        Hyperlinks = (ccRecord.CcUrlsEntityAssociation == null) ? new List<CommunicationCodeHyperlink>() :
                                            ccRecord.CcUrlsEntityAssociation.Select(urlRecord =>
                                                new CommunicationCodeHyperlink(urlRecord.CcUrlAssocMember, urlRecord.CcTitleAssocMember)).ToList()
                                    });
                            }
                            catch (Exception e)
                            {
                                LogDataError("CC.CODES", ccRecord.Recordkey, ccRecord, e, "Error creating CommunicationCode");
                            }
                        }

                        return communicationCodeEntities;
                    }, Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Get a collection of admission application supporting item types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of admission application supporting item types</returns>
        public async Task<IEnumerable<CommunicationCode>> GetAdmissionApplicationSupportingItemTypesAsync(bool ignoreCache)
        {
            var admissionApplicationSupportingItemTypes = new List<CommunicationCode>();
            //
            // get all communication codes
            //
            var communicationCodes = await GetGuidCodeItemAsync<CcCodes, CommunicationCode>("AllCommunicationCodesHedm", "CC.CODES",
              (t, g) => new CommunicationCode(g, t.Recordkey, t.CcDescription, t.CcOffice), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
            
            if (communicationCodes != null && communicationCodes.Any())
            {
                // get valid office codes for admission applications
                var ldmDefaults = GetLdmDefaults();
                var officeCodes = ldmDefaults.LdmdDfltAdmOfficeCodes;
                if (officeCodes == null || !officeCodes.Any())
                {
                    throw new ConfigurationException("Specify the office codes to integrate on the Admissions Integration Parameters (CDAM) form.");
                }
                foreach (var communcationCode in communicationCodes)
                {
                    var officeCode = communcationCode.OfficeCodeId;
                    //
                    // if communication code has a valid office code, add it to the returned list of
                    // admissiom application supporting item types.

                    if (officeCodes != null && officeCodes.Contains(officeCode))
                    {
                        admissionApplicationSupportingItemTypes.Add(communcationCode);
                    }
                }
            }
            return admissionApplicationSupportingItemTypes;
        }

        /// <summary>
        /// Gets a CorewebDefaults data contract
        /// </summary>
        /// <returns>CorewebDefaults data contract object</returns>
        private CorewebDefaults GetCorewebDefaults()
        {
            return GetOrAddToCache<CorewebDefaults>("CorewebDefaults", () =>
                {
                    var corewebDefaults = DataReader.ReadRecord<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS");
                    if (corewebDefaults != null)
                    {
                        return corewebDefaults;
                    }
                    else
                    {
                        logger.Info("Null CorewebDefaults record returned from database");
                        return new CorewebDefaults();
                    }
                });
        }

        /// <summary>
        /// Counties
        /// </summary>
        public IEnumerable<County> Counties
        {
            get
            {
                return GetCodeItem<Counties, County>("AllCounties", "COUNTIES",
                    c => new County(c.RecordGuid, c.Recordkey, c.CntyDesc));
            }
        }

        /// <summary>
        /// Counties
        /// </summary>
        public async Task<IEnumerable<County>> GetCountiesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Counties, County>("AllCounties", "COUNTIES",
               (c, g) => new County(g, c.Recordkey, c.CntyDesc), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }


  
        /// <summary>
        /// Countries
        /// </summary>
        public async Task<IEnumerable<Country>> GetCountryCodesAsync(bool ignoreCache = false)
        {
            return await GetCodeItemAsync<Countries, Country>("AllCountries", "COUNTRIES",
                c => new Country(c.Recordkey, c.CtryDesc, c.CtryIsoCode, c.CtryIsoAlpha3Code, c.CtryNotInUseFlag.ToUpper() == "Y"), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Countries code
        /// </summary>
        /// <param name="code">Countries code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCountryGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCountryCodesAsync(false);
            Country codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCountryCodesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'COUNTRIES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'COUNTRIES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'COUNTRIES', Record ID:'", code, "'"));
            }
            return guid;

        }


        /// <summary>
        /// Get country from isoAlpha3Code code
        /// </summary>
        /// <param name="code">Countries code</param>
        /// <returns>Guid</returns>
        public async Task<Country> GetCountryFromIsoAlpha3CodeAsync(string isoAlpha3Code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(isoAlpha3Code))
                return null;
            var allCountries = await GetCountryCodesAsync(false);
            Country country = null;
            if (allCountries != null && allCountries.Any())
            {
                country = allCountries.FirstOrDefault(c => c.IsoAlpha3Code.Equals(isoAlpha3Code, StringComparison.OrdinalIgnoreCase));
            }
            //if we cannot find that country in the cache, then refresh the cache and try again.
            if (country == null)
            {
                var allCodesNoCache = await GetCountryCodesAsync(true);
                if (allCountries == null)
                {
                    throw new RepositoryException(string.Concat("No record found, Entity:'COUNTRIES', Record ID:'", isoAlpha3Code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.IsoAlpha3Code.Equals(isoAlpha3Code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No record found, Entity:'COUNTRIES', Record ID:'", isoAlpha3Code, "'"));
            }            
            return country;
        }

        /// <summary>
        /// Degree Types
        /// </summary>
        public IEnumerable<DegreeType> DegreeTypes
        {
            get
            {
                return GetValcode<DegreeType>("CORE", "DEGREE.TYPES", degreeType => new DegreeType(degreeType.ValInternalCodeAssocMember, degreeType.ValExternalRepresentationAssocMember));
            }
        }

        /// <summary>
        /// Denominations
        /// </summary>
        public IEnumerable<Denomination> Denominations
        {
            get
            {
                return GetGuidCodeItem<Denominations, Denomination>("AllDenominations", "DENOMINATIONS",
                    (d, g) => new Denomination(g, d.Recordkey, d.DenomDesc));
            }
        }

        /// <summary>
        /// Denominations
        /// </summary>
        public async Task<IEnumerable<Denomination>> DenominationsAsync()
        {
            return await GetDenominationsAsync(false);

        }

        /// <summary>
        /// Get a collection of denominations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of denominations</returns>
        public async Task<IEnumerable<Denomination>> GetDenominationsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Denominations, Denomination>("AllDenominations", "DENOMINATIONS",
                (d, g) => new Denomination(g, d.Recordkey, d.DenomDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Institutional departments.
        /// </summary>
        public async Task<IEnumerable<Department>> DepartmentsAsync()
        {
            return await GetDepartmentsAsync(false);
        }
        
        /// <summary>
        /// Get a collection of departments
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of academic departments</returns>
        public async Task<IEnumerable<Department>> GetDepartmentsAsync(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                var departments = await BuildAllDepartments();
                return await AddOrUpdateCacheAsync<IEnumerable<Department>>("AllBaseDepartments", departments);

            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<Department>>("AllBaseDepartments", async () => await this.BuildAllDepartments(), Level1CacheTimeoutValue);
            }
        }


        /// <summary>
        /// Get a collection of departments
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param> 
        /// <returns>Collection of academic departments</returns>
        public async Task<IEnumerable<Department>> GetDepartments2Async(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                var departments = await BuildAllDepartments2();
                return await AddOrUpdateCacheAsync<IEnumerable<Department>>("AllBaseDepartments2", departments);

            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<Department>>("AllBaseDepartments2", async () => await this.BuildAllDepartments2(), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Get guid for Departments code
        /// </summary>
        /// <param name="code">Departments code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetDepartments2GuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetDepartments2Async(false);
            Department codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetDepartments2Async(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'DEPTS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'DEPTS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'DEPTS', Record ID:'", code, "'"));
            }
            return guid;

        }


        /// <summary>
        ///  Build Department domain entity collection.
        ///  Contains guid lookups to ensure the associated LDM.GUID does not contain a secondary key and/or index
        /// </summary>
        /// <returns>Collection of Department domain entities</returns>
        private async Task<IEnumerable<Department>> BuildAllDepartments2()
        {
            var departmentEntities = new List<Department>();
            var departmentRecords = await DataReader.BulkReadRecordAsync<Depts>("");

            foreach (var departmentRecord in departmentRecords)
            {
                var dept = new Department(departmentRecord.RecordGuid, departmentRecord.Recordkey, departmentRecord.DeptsDesc, "A".Equals(departmentRecord.DeptsActiveFlag))
                {
                    Division = departmentRecord.DeptsDivision,
                    School = departmentRecord.DeptsSchool,
                    InstitutionId = departmentRecord.DeptsInstitutionsId,
                    DepartmentType = departmentRecord.DeptsType
                };

                departmentEntities.Add(dept);
            }

            return departmentEntities;
        }


        /// <summary>
        ///  Build Department domain entity collection.
        ///  Contains guid lookups to ensure the associated LDM.GUID does not contain a secondary key and/or index
        /// </summary>
        /// <returns>Collection of Department domain entities</returns>
        private async Task<IEnumerable<Department>> BuildAllDepartments()
        {
            var departmentEntities = new List<Department>();
            // exclude any LDM.GUID entries that contain a secondary key and/or index
            string criteria = "WITH LDM.GUID.ENTITY EQ 'DEPTS' AND LDM.GUID.SECONDARY.KEY EQ '' ";

            //retrive string array of applicable guids
            var ldmGuidDepartment = await DataReader.SelectAsync("LDM.GUID", criteria);
            if ((ldmGuidDepartment != null) && (ldmGuidDepartment.Any()))
            {
                // using the string array of applicable guids, convert this into a guidLookup
                var guidLookUp = ldmGuidDepartment.Select(guid => new GuidLookup(guid)).ToArray();
                if ((guidLookUp == null) || (!guidLookUp.Any()))
                {
                    return departmentEntities;
                }
                // BulkReadRecord to get a collection of Department data contracts. The guid returned
                // in the data contract may not be correct, therefore additional processing is required.
                var departmentRecords = await DataReader.BulkReadRecordAsync<Depts>("DEPTS", guidLookUp);

                // Perform a guid lookup to get a collection of guids and ids
                var departmentDictionary = await DataReader.SelectAsync(guidLookUp);

                if ((departmentDictionary == null) || (!departmentDictionary.Any()))
                {
                    return departmentEntities;
                }

                foreach (var departmentRecord in departmentRecords)
                {
                    // using the id, retrieve the correct guid from the collection of guids and ids
                    var ldmGuid = departmentDictionary.FirstOrDefault(id => id.Value != null && departmentRecord.Recordkey.Equals(id.Value.PrimaryKey, StringComparison.OrdinalIgnoreCase));
                    if (!ldmGuid.Equals(new KeyValuePair<string, GuidLookupResult>()))
                    {
                        if (string.IsNullOrEmpty(departmentRecord.DeptsDesc))
                        {
                            throw new ApplicationException("Department record for id '" + departmentRecord.Recordkey + "', guid " + departmentRecord.RecordGuid + " is missing a required description.");
                        }

                        var dept = new Department(ldmGuid.Key, departmentRecord.Recordkey, departmentRecord.DeptsDesc, "A".Equals(departmentRecord.DeptsActiveFlag))
                        {
                            Division = departmentRecord.DeptsDivision,
                            School = departmentRecord.DeptsSchool,
                            InstitutionId = departmentRecord.DeptsInstitutionsId,
                            DepartmentType = departmentRecord.DeptsType
                        };

                        departmentEntities.Add(dept);
                    }
                }
            }
            return departmentEntities;
        }


        /// <summary>
        ///  Get and return a single department entity by guid without using the cache
        /// </summary>
        /// <returns>Department domain entity</returns>
        public async Task<Department> GetDepartmentByGuidAsync(string guid)
        {
            var dept = await DataReader.ReadRecordAsync<Depts>(new GuidLookup(guid), false);

            if (dept == null)
            {
                throw new ApplicationException("Department record not found for guid " + guid + ".");
            }

            if (string.IsNullOrEmpty(dept.DeptsDesc)){
                throw new ApplicationException("Department record for guid " + guid + " is missing a required description.");
            }

            return new Department(guid, dept.Recordkey, dept.DeptsDesc, "A".Equals(dept.DeptsActiveFlag))
            {
                Division = dept.DeptsDivision,
                School = dept.DeptsSchool,
                InstitutionId = dept.DeptsInstitutionsId,
                DepartmentType = dept.DeptsType
            };
        }



        /// <summary>
        ///  Get and return a single school entity by guid without using the cache
        /// </summary>
        /// <returns>School domain entity</returns>
        public async Task<School> GetSchoolByGuidAsync(string guid)
        {
            var school = await DataReader.ReadRecordAsync<Schools>(new GuidLookup(guid), false);

            if (school == null)
            {
                throw new ApplicationException("School record not found for guid " + guid + ".");
            }

            if (string.IsNullOrEmpty(school.SchoolsDesc))
            {
                throw new ApplicationException("School record for guid " + guid + " is missing a required description.");
            }

            return BuildSchools(guid, school);
        }


        /// <summary>
        ///  Get and return a single division entity by guid without using the cache
        /// </summary>
        /// <returns>Division domain entity</returns>
        public async Task<Division> GetDivisionByGuidAsync(string guid)
        {
            Divisions divs = await DataReader.ReadRecordAsync<Divisions>(new GuidLookup(guid), false);

            if (divs == null)
            {
                throw new ApplicationException("Division record not found for guid " + guid + ".");
            }

            if (string.IsNullOrEmpty(divs.DivDesc))
            {
                throw new ApplicationException("Division record for id '" + divs.Recordkey + "', guid " + divs.RecordGuid + " is missing a required description.");
            }

            var div = new Division(guid, (String.IsNullOrEmpty(divs.DivDesc) ? divs.Recordkey : divs.DivDesc), divs.DivDesc)
                                    { SchoolCode = divs.DivSchool, InstitutionId = divs.DivInstitutionsId,  };
            
            return div;

        }



        public IEnumerable<DisabilityType> DisabilityTypes
        {
            get
            {
                return GetCodeItem<DisabilityTypes, DisabilityType>("AllDisabilityTypes", "DISABILITY",
                    d => new DisabilityType(d.Recordkey, d.HcDesc));
            }
        }

        public IEnumerable<Division> Divisions
        {
            get
            {
                return GetCodeItem<Divisions, Division>("AllDivisions", "DIVISIONS",
                    divs =>
                    {
                        if (string.IsNullOrEmpty(divs.DivDesc))
                        {
                            throw new ApplicationException("Division record for id '" + divs.Recordkey + "', guid " + divs.RecordGuid + " is missing a required description.");
                        }
                        return new Division(divs.RecordGuid, divs.Recordkey, divs.DivDesc) { SchoolCode = divs.DivSchool };
                    });
            }
        }

        /// <summary>
        /// Get a collection of divisions
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of divisions</returns>
        public async Task<IEnumerable<Division>> GetDivisionsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Divisions, Division>("AllEedmDivisions", "DIVISIONS",
              (m, g) => new Division(g, m.Recordkey, (String.IsNullOrEmpty(m.DivDesc) ? m.Recordkey : m.DivDesc)) { SchoolCode = m.DivSchool, InstitutionId = m.DivInstitutionsId }, CacheTimeout,
                this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get a collection of email types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of email types</returns>
        public async Task<IEnumerable<EmailType>> GetEmailTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<EmailType>("CORE", "PERSON.EMAIL.TYPES",
                (e, g) => new EmailType(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
                            ConvertEmailTypeCodeToEmailType(e.ValActionCode3AssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for email type.
        /// </summary>
        /// <param name="code">EmailType code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetEmailTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetEmailTypesAsync(false);
            EmailType  codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetEmailTypesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, PERSON.EMAIL.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, PERSON.EMAIL.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, PERSON.EMAIL.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }


        /// <summary>
        /// Ethnicities
        /// </summary>
        public async Task<IEnumerable<Ethnicity>> EthnicitiesAsync()
        {
            return await GetEthnicitiesAsync(false);

        }

        /// <summary>
        /// Get a collection of ethnicities
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ethnicities</returns>
        public async Task<IEnumerable<Ethnicity>> GetEthnicitiesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<Ethnicity>("CORE", "PERSON.ETHNICS",
                (e, g) => new Ethnicity(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
                    ConvertEthnicityTypeCodeToEthnicityType(e.ValActionCode1AssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of ExternalEmploymentStatus
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ExternalEmploymentStatus</returns>
        public async Task<IEnumerable<ExternalEmploymentStatus>> GetExternalEmploymentStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<ExternalEmploymentStatus>("CORE", "EMPLOYMT.STATUSES",
                (cl, g) => new ExternalEmploymentStatus(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of geographic area types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of geographic area types</returns>
        public async Task<IEnumerable<GeographicAreaType>> GetGeographicAreaTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<GeographicAreaType>("CORE", "INTG.GEO.AREA.TYPES",
                (c, g) => new GeographicAreaType(g, c.ValInternalCodeAssocMember, c.ValExternalRepresentationAssocMember,
                    ConvertGeographicAreaTypeCategoryCodeToGeographicAreaTypeCategory(c.ValInternalCodeAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of grade change reasons
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns></returns>
        public async Task<IEnumerable<GradeChangeReason>> GetGradeChangeReasonAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<GradeChangeReason>("ST", "INTG.GRADE.CHANGE.REASONS",
                (gcr, g) => new GradeChangeReason(g, gcr.ValInternalCodeAssocMember, gcr.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of identity document types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of identity document types</returns>
        public async Task<IEnumerable<IdentityDocumentType>> GetIdentityDocumentTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<IdentityDocumentType>("CORE", "INTG.IDENTITY.DOC.TYPES",
                (i, g) => new IdentityDocumentType(g, i.ValInternalCodeAssocMember, i.ValExternalRepresentationAssocMember,
                    ConvertIdentityDocumentTypeCategoryCodeToIdentityDocumentTypeCategory(i.ValInternalCodeAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Institution Types
        /// </summary>
        public IEnumerable<InstitutionType> InstitutionTypes
        {
            get
            {
                return GetValcode<InstitutionType>("CORE", "INST.TYPES",
                    i => new InstitutionType(i.ValInternalCodeAssocMember, i.ValExternalRepresentationAssocMember, i.ValActionCode1AssocMember));
            }
        }

        /// <summary>
        /// Interests
        /// </summary>
        public IEnumerable<Interest> Interests
        {
            get
            {
                return GetCodeItem<Interests, Interest>("AllInterests", "INTERESTS",
                    i => new Interest(i.RecordGuid, i.Recordkey, i.IntDesc));
            }
        }

        /// <summary>
        /// Get a collection of Interests
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of interests</returns>
        public async Task<IEnumerable<Interest>> GetInterestsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Interests, Interest>("AllHedmInterests", "INTERESTS",
              (m, g) => new Interest(g, m.Recordkey, m.IntDesc, m.IntType), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get a collection of interests types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of interest types</returns>
        public async Task<IEnumerable<InterestType>> GetInterestTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<InterestType>("CORE", "INTEREST.TYPES",
                (e, g) => new InterestType(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Languages
        /// </summary>
        public IEnumerable<Language> Languages
        {
            get
            {
                return GetValcode<Language>("CORE", "LANGUAGES",
                    l => new Language(l.ValInternalCodeAssocMember, l.ValExternalRepresentationAssocMember));
            }
        }

        /// <summary>
        /// Get languageISO codes/// </summary>
        /// <returns>A collection of Language ISO Codes</returns>
        public async Task<IEnumerable<LanguageIsoCodes>> GetLanguageIsoCodesAsync(bool bypassCache)
        {
            var languageIsoCodes = await GetOrAddToCacheAsync<List<LanguageIsoCodes>>("AllLanguageIsoCodes",
             async () =>
             {
                 var languageIsoCodesData = await DataReader.BulkReadRecordAsync<DataContracts.IntgIsoLanguages>("INTG.ISO.LANGUAGES", "");
                 var languageIsoCodesList = BuildLanguageIsoCodes(languageIsoCodesData.ToList());
                 return languageIsoCodesList.ToList();
             }
          );
            return languageIsoCodes;
        }

        /// <summary>
        ///  Get and return a single language ISO code entity by guid without using the cache
        /// </summary>
        /// <returns>Language Iso Code domain entity</returns>
        public async Task<LanguageIsoCodes> GetLanguageIsoCodeByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new KeyNotFoundException("GUID is required to get a languageIsoCode by GUID.");
            }
            var languageIsoCode = await DataReader.ReadRecordAsync<DataContracts.IntgIsoLanguages>(new GuidLookup(guid), false);

            if (languageIsoCode == null)
            {
                throw new KeyNotFoundException("languageIsoCode record not found for guid " + guid + ".");
            }


            var languageIsoCodes = BuildLanguageIsoCodes(new List<IntgIsoLanguages>() { languageIsoCode });
            if ((languageIsoCodes == null) || (!languageIsoCodes.Any()))
            {
                var execption = new RepositoryException();
                exception.AddError(new RepositoryError("languageIsoCodes", "An error ocurred building the languageIsoCodes record.")
                {
                    Id = guid,
                    SourceId = languageIsoCode.Recordkey
                });
                throw execption;
            }
            return languageIsoCodes.FirstOrDefault();

        }

        /// <summary>
        /// Build a collection of Language ISO Code domain entities from an INTG.ISO.LANGUAGES datacontract collection
        /// </summary>
        /// <param name="languageIsoCodeData">language ISO Codes data contract</param>
        /// <returns>Collection of Language ISO Code domain entities</returns>
        private IEnumerable<LanguageIsoCodes> BuildLanguageIsoCodes(List<DataContracts.IntgIsoLanguages> languageIsoCodeData)
        {
            List<LanguageIsoCodes> languageIsoCodeCollection = new List<LanguageIsoCodes>();
            if (languageIsoCodeData != null)
            {
                foreach (var languageIsoCode in languageIsoCodeData)
                {
                    try
                    {
                        var languageIsoCodeItem = new LanguageIsoCodes(languageIsoCode.RecordGuid, languageIsoCode.Recordkey, languageIsoCode.IilangDesc)
                        { InactiveFlag = languageIsoCode.IilangInactive };
                        languageIsoCodeCollection.Add(languageIsoCodeItem);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Language ISO Code", languageIsoCode.Recordkey, languageIsoCodeData, ex);
                    }
                }
            }
            return languageIsoCodeCollection;
        }

        public IEnumerable<FrequencyCode> FrequencyCodes
        {
            get
            {
                return GetValcode<FrequencyCode>("CORE", "SCHED.REPEATS",
                    fc => new FrequencyCode(fc.ValInternalCodeAssocMember, fc.ValExternalRepresentationAssocMember));
            }
        }

        /// <summary>
        /// Campus locations.
        /// </summary>
        public IEnumerable<Location> Locations
        {
            get
            {
                return GetLocations(false);
            }
        }

        /// <summary>
        /// Get a collection of locations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of locations</returns>
        public IEnumerable<Location> GetLocations(bool ignoreCache)
        {
            //TODO: this will fail if location is missing
            return GetGuidCodeItem<Locations, Location>("AllLocations", "LOCATIONS",
                (l, g) => new Location(g, l.Recordkey, l.LocDesc, l.LocLatitude1, l.LocLongitude1, l.LocLatitude2, l.LocLongitude2,
                    l.LocExportToMobile, l.LocBuildings, l.LocHideInSsCourseSearch.ToUpperInvariant() == "Y")
                        {
                            AddressLines = l.LocAddress,
                            City = l.LocCity,
                            State = l.LocState,
                            PostalCode = l.LocZip,
                            Country = l.LocCountry,
                            CampusLocation = l.LocCampusLocation,
                            SortOrder = l.LocSortOrder
                        }, CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get a collection of locations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of locations</returns>
        public async Task<IEnumerable<Location>> GetLocationsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Locations, Location>("AllLocations", "LOCATIONS",
                (l, g) => new Location(g, l.Recordkey, (String.IsNullOrEmpty(l.LocDesc)? l.Recordkey : l.LocDesc), l.LocLatitude1, l.LocLongitude1, l.LocLatitude2, l.LocLongitude2,
                    l.LocExportToMobile, l.LocBuildings, l.LocHideInSsCourseSearch.ToUpperInvariant() == "Y")
                {
                    AddressLines = l.LocAddress,
                    City = l.LocCity,
                    State = l.LocState,
                    PostalCode = l.LocZip,
                    Country = l.LocCountry,
                    CampusLocation = l.LocCampusLocation,
                    SortOrder = l.LocSortOrder
                }, CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get guid for Locations code
        /// </summary>
        /// <param name="code">Locations code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetLocationsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetLocationsAsync(false);
            Location codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
             //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetLocationsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'LOCATIONS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'LOCATIONS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'LOCATIONS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of location types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of location types</returns>
        public async Task<IEnumerable<LocationTypeItem>> GetLocationTypesAsync(bool ignoreCache)
        {

            return await GetGuidValcodeAsync<LocationTypeItem>("CORE", "ADREL.TYPES",
                    (e, g) => new LocationTypeItem(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
                        ConvertEntityTypeCodeToEntityType(e.ValActionCode3AssocMember, e.ValActionCode4AssocMember),
                        ConvertPersonLocationTypeCodeToPersonLocationType(e.ValActionCode3AssocMember),
                        ConvertOrgLocationTypeCodeToOrgLocationType(e.ValActionCode4AssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of instructional platforms
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of instructional platforms</returns>
        public async Task<IEnumerable<InstructionalPlatform>> GetInstructionalPlatformsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<InstructionalPlatform>("UT", "PORTAL.LEARN.TARGETS",
                (i, g) => new InstructionalPlatform(g, i.ValInternalCodeAssocMember, i.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Instructional Platform code
        /// </summary>
        /// <param name="code">Departments code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetInstructionalPlatformsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetInstructionalPlatformsAsync(false);
            InstructionalPlatform codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetInstructionalPlatformsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'PORTAL.LEARN.TARGETS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'PORTAL.LEARN.TARGETS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'PORTAL.LEARN.TARGETS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Marital Statuses
        /// </summary>
        public async Task<IEnumerable<MaritalStatus>> MaritalStatusesAsync()
        {
            return await GetMaritalStatusesAsync(false);

        }

        /// <summary>
        /// Office Codes
        /// </summary>
        public IEnumerable<OfficeCode> OfficeCodes
        {
            get
            {
                return GetValcode<OfficeCode>("CORE", "OFFICE.CODES",
                    office =>
                        new OfficeCode(office.ValInternalCodeAssocMember, office.ValExternalRepresentationAssocMember)
                        {
                            Type = (office.ValInternalCodeAssocMember.ToUpper() == "FA" || office.ValActionCode1AssocMember.ToUpper() == "FA") ? OfficeCodeType.FinancialAid : OfficeCodeType.Other
                        }
                    );
            }
        }

        /// <summary>
        /// Get a collection of Other CCDs
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of marital statuses</returns>
        public async Task<IEnumerable<MaritalStatus>> GetMaritalStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<MaritalStatus>("CORE", "MARITAL.STATUSES",
                (m, g) => new MaritalStatus(g, m.ValInternalCodeAssocMember, m.ValExternalRepresentationAssocMember) { Type = ConvertMaritalStatusTypeCodeToMaritalStatusType(m.ValActionCode1AssocMember) }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of MilStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of MilStatuses</returns>
        public async Task<IEnumerable<MilStatuses>> GetMilStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<MilStatuses>("CORE", "MIL.STATUSES",
                (cl, g) => new MilStatuses(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember))
                    { Category = ConvertVeteranStatusCodeToVeteranStatusCategory(cl.ValActionCode3AssocMember) }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of Other Degrees
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of degrees</returns>
        public IEnumerable<OtherDegree> GetOtherDegrees(bool ignoreCache)
        {
            return GetGuidCodeItem<OtherDegrees, OtherDegree>("BaseAllOtherDegrees", "OTHER.DEGREES",
              (m, g) => new OtherDegree(g, m.Recordkey, (string.IsNullOrEmpty(m.OdegDesc) ? m.Recordkey : m.OdegDesc)),
              CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }


        /// <summary>
        /// Get a collection of Other Degrees
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of degrees</returns>
        public async Task<IEnumerable<OtherDegree>> GetOtherDegreesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<OtherDegrees, OtherDegree>("BaseAllOtherDegrees", "OTHER.DEGREES",
              (m, g) => new OtherDegree(g, m.Recordkey, (string.IsNullOrEmpty(m.OdegDesc) ? m.Recordkey : m.OdegDesc)),
              CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get guid for Other Degrees code
        /// </summary>
        /// <param name="code">Other Degrees code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetOtherDegreeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetOtherDegreesAsync(false);
            OtherDegree codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
           
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetOtherDegreesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.DEGREES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.DEGREES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.DEGREES', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of Other Ccds
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Ccds</returns>
        public IEnumerable<OtherCcd> GetOtherCcds(bool ignoreCache)
        {
            return GetGuidCodeItem<OtherCcds, OtherCcd>("BaseAllOtherCcds", "OTHER.CCDS",
              (m, g) => new OtherCcd(g, m.Recordkey, (string.IsNullOrEmpty(m.OccdDesc) ? m.Recordkey : m.OccdDesc)) { CredentialTypeID = m.OccdType }, 
              CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }


        /// <summary>
        /// Get a collection of Other Ccds
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Ccds</returns>
        public async Task<IEnumerable<OtherCcd>> GetOtherCcdsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<OtherCcds, OtherCcd>("BaseAllOtherCcds", "OTHER.CCDS",
              (m, g) => new OtherCcd(g, m.Recordkey, (string.IsNullOrEmpty(m.OccdDesc) ? m.Recordkey : m.OccdDesc)) { CredentialTypeID = m.OccdType }, 
              CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get guid for Other Ccds code
        /// </summary>
        /// <param name="code">Other Ccds code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetOtherCcdsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetOtherCcdsAsync(false);
            OtherCcd codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetOtherCcdsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.CCDS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.CCDS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.CCDS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of majors
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of majors</returns>
        public async Task<IEnumerable<OtherMajor>> GetOtherMajorsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<OtherMajors, OtherMajor>("BaseAllOtherMajors", "OTHER.MAJORS",
                (m, g) => new OtherMajor(g, m.Recordkey, (string.IsNullOrEmpty(m.OmajDesc) ? m.Recordkey : m.OmajDesc)),
                CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get guid for other Majors code
        /// </summary>
        /// <param name="code">Other Majors code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetOtherMajorsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetOtherMajorsAsync(false);
            OtherMajor codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetOtherMajorsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MAJORS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MAJORS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MAJORS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of minors
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of minors</returns>
        public async Task<IEnumerable<OtherMinor>> GetOtherMinorsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<OtherMinors, OtherMinor>("BaseAllOtherMinors", "OTHER.MINORS",
                (m, g) => new OtherMinor(g, m.Recordkey, (string.IsNullOrEmpty(m.OminDesc) ? m.Recordkey : m.OminDesc)), 
                CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get guid for other Minors code
        /// </summary>
        /// <param name="code">Other Minors code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetOtherMinorsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetOtherMinorsAsync(false);
            OtherMinor codeCache = null; 
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
             
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetOtherMinorsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MINORS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MINORS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.MINORS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of specializations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of specializations</returns>
        public async Task<IEnumerable<OtherSpecial>> GetOtherSpecialsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<OtherSpecials, OtherSpecial>("BaseAllOtherSpecials", "OTHER.SPECIALS",
                (s, g) => new OtherSpecial(g, s.Recordkey, (string.IsNullOrEmpty(s.OspecDesc) ? s.Recordkey : s.OspecDesc) ),
                CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get guid for specializations code
        /// </summary>
        /// <param name="code">Other specializations code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetOtherSpecialsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetOtherSpecialsAsync(false);
            OtherSpecial codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetOtherSpecialsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.SPECIALS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.SPECIALS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.SPECIALS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        ///  A collection of Other Honors
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Other Honors</returns>
        public async Task<IEnumerable<OtherHonor>> GetOtherHonorsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<OtherHonors, OtherHonor>("BaseAllOtherHonors", "OTHER.HONORS",
                 (al, g) => new OtherHonor(g, al.Recordkey, (string.IsNullOrEmpty(al.OhonDesc) ? al.Recordkey : al.OhonDesc)),
                 bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Honors Ccds code
        /// </summary>
        /// <param name="code">Other Honors code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetOtherHonorsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetOtherHonorsAsync(false);
            OtherHonor codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetOtherHonorsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.HONORS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.HONORS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.HONORS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of IntgPersonEmerPhoneTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgPersonEmerPhoneTypes</returns>
        public async Task<IEnumerable<IntgPersonEmerPhoneTypes>> GetIntgPersonEmerPhoneTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<IntgPersonEmerPhoneTypes>("CORE", "INTG.PERSON.EMER.PHONE.TYPES",
                (cl, g) => new IntgPersonEmerPhoneTypes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get a collection of IntgPersonEmerTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgPersonEmerTypes</returns>
        public async Task<IEnumerable<IntgPersonEmerTypes>> GetIntgPersonEmerTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<IntgPersonEmerTypes>("CORE", "INTG.PERSON.EMER.TYPES",
                (cl, g) => new IntgPersonEmerTypes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }
        /// <summary>
        /// PersonFilters
        /// </summary>
        public async Task<IEnumerable<PersonFilter>> GetPersonFiltersAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<SaveListParms, PersonFilter>("AllPersonFilters", "SAVE.LIST.PARMS",
               (c, g) => new PersonFilter(g, c.Recordkey, string.IsNullOrEmpty(c.SlpDescription) ? c.Recordkey : c.SlpDescription), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get a person filter
        /// </summary>
        /// <param name="guid">person filters GUID</param>
        /// <returns>person filter</returns>
        public async Task<PersonFilter> GetPersonFilterByGuidAsync(string guid)
        {
            GuidLookupResult lookupResult = await GetGuidLookupResultFromGuidAsync(guid);
            SaveListParms personFilter = await DataReader.ReadRecordAsync<SaveListParms>(lookupResult.PrimaryKey, true);
            return new PersonFilter(guid, personFilter.Recordkey, !string.IsNullOrEmpty(personFilter.SlpDescription) ? personFilter.SlpDescription : personFilter.Recordkey);
        }

        /// <summary>
        /// Get a person filter
        /// </summary>
        /// <param name="guid">person filters GUID</param>
        /// <returns>string list of person IDs</returns>
        public async Task<string[]> GetPersonIdsByPersonFilterGuidAsync(string guid)
        {
            var lookupResult = new GuidLookupResult();
            try
            {
                lookupResult = await GetGuidLookupResultFromGuidAsync(guid);
            }
            catch
            {
                return null;
            }
            SaveListParms personFilter = await DataReader.ReadRecordAsync<SaveListParms>(lookupResult.PrimaryKey, true);
            if (personFilter != null && personFilter.Recordkey != null)
            {
                var personIds = new List<string>();
                // limit the size/amount of person IDs returned from Colleague because the list could potentially be large
                var batchLimit = bulkReadSize * 10;
                bool haveAllPersonIds = false;
                int i = 0;
                while (haveAllPersonIds == false)
                {
                    var request = new GetPersonFilterRequest()
                    {
                        SaveListParmsId = personFilter.Recordkey,
                        Offset = i * batchLimit,
                        Limit = batchLimit
                    };
                    i++;
                    // Execute request
                    var response = await transactionInvoker.ExecuteAsync<GetPersonFilterRequest, GetPersonFilterResponse>(request);
                    if (response != null)
                    {
                        if (response.ErrorMessages != null && response.ErrorMessages.Any())
                        {
                            // Throw exception if error encountered
                            foreach (var message in response.ErrorMessages)
                            {
                                exception.AddError(new RepositoryError("invalid.personFillter", message));
                            }
                            throw exception;
                        }
                        if (response.PersonIds == null || !response.PersonIds.Any())
                        {
                            return null;
                        }
                        // add all retrieved person IDs to ongoing list                            
                        personIds.AddRange(response.PersonIds);                        
                        if (personIds.Count() >= response.TotalRecords)
                        {
                            // abort loop now that we have all personIds from the savedlist
                            haveAllPersonIds = true;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                personIds = personIds.Distinct().ToList();
                Array.Sort(personIds.ToArray());
                return personIds.ToArray();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get a collection of PersonOriginCodes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of PersonOriginCodes</returns>
        public async Task<IEnumerable<PersonOriginCodes>> GetPersonOriginCodesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<PersonOriginCodes>("CORE", "PERSON.ORIGIN.CODES",
                (cl, g) => new PersonOriginCodes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Person Origin code
        /// </summary>
        /// <param name="code">Person Origin code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetPersonOriginCodesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetPersonOriginCodesAsync(false);
            PersonOriginCodes codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetPersonOriginCodesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Valcode:'PERSON.ORIGIN.CODES', Code:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Valcode:'PERSON.ORIGIN.CODES', Code:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'OTHER.HONORS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of personal pronoun types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of personal pronoun types</returns>
        public async Task<IEnumerable<PersonalPronounType>> GetPersonalPronounTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<PersonalPronounType>("CORE", "PERSONAL.PRONOUNS",
                (e, g) => new PersonalPronounType(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of personal pronoun types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of personal pronoun types</returns>
        public async Task<IEnumerable<GenderIdentityType>> GetGenderIdentityTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<GenderIdentityType>("CORE", "GENDER.IDENTITIES",
                (e, g) => new GenderIdentityType(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of person name types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of person name types</returns>
        public async Task<IEnumerable<PersonNameTypeItem>> GetPersonNameTypesAsync(bool ignoreCache)
        {

            return await GetGuidValcodeAsync<PersonNameTypeItem>("CORE", "INTG.PERSON.NAME.TYPES",
                (e, g) => new PersonNameTypeItem(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
                   ConvertPersonNameTypeCodeToPersonNameType(e.ValActionCode3AssocMember)), bypassCache: ignoreCache);

        }


        /// <summary>
        /// Get a collection of phone types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of phone types</returns>
        public async Task<IEnumerable<PhoneType>> GetPhoneTypesAsync(bool ignoreCache)
        {

            return await GetGuidValcodeAsync<PhoneType>("CORE", "PHONE.TYPES",
                (e, g) => new PhoneType(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
                    ConvertPhoneTypeCodeToPhoneType(e.ValActionCode3AssocMember)), bypassCache: ignoreCache);

        }

        /// <summary>
        /// Get guid for phone type.
        /// </summary>
        /// <param name="code">PhoneType code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetPhoneTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetPhoneTypesAsync(false);
            PhoneType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetPhoneTypesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, PHONE.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, PHONE.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, PHONE.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of phone types, non-guid
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        //public async Task<IEnumerable<PhoneType>> GetPhoneTypesBaseAsync(bool ignoreCache)
        //{
        //    return await GetValcodeAsync<PhoneType>("CORE", "PHONE.TYPES",
        //        e => new PhoneType(e.ValInternalCodeAssocMember, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
        //            ConvertPhoneTypeCodeToPhoneType(e.ValActionCode3AssocMember)), bypassCache: ignoreCache);
        //}

        /// <summary>
        /// Get a collection of item condition.
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ItemCondition>> GetItemConditionsAsync(bool ignoreCache)
        {
            return await GetValcodeAsync<ItemCondition>("CORE", "ITEM.CONDITION",
                e => new ItemCondition(e.ValInternalCodeAssocMember, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember),bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of acquisition method.
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AcquisitionMethod>> GetAcquisitionMethodsAsync(bool ignoreCache)
        {
            return await GetCodeItemAsync<DataContracts.AcquisitionMethods, AcquisitionMethod>("AllAacquisionMethods", "ACQUISITION.METHODS",
                ac => new AcquisitionMethod(ac.Recordkey, ac.AcqDesc, ac.AcqType), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Return a list of project types.
        /// </summary>
        public async Task<IEnumerable<ProjectType>> GetProjectTypesAsync()
        {
            return await GetValcodeAsync<ProjectType>("CORE", "PROJECT.TYPES",
                projectType =>
                {
                    var x = projectType;
                    return new ProjectType(projectType.ValInternalCodeAssocMember, projectType.ValExternalRepresentationAssocMember);
                }
            );
        }

        /// <summary>
        /// Get a collection of Positions
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Positions</returns>
        public async Task<IEnumerable<Positions>> GetPositionsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<Positions>("CORE", "POSITIONS",
                (cl, g) => new Positions(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of privacy statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of privacy statuses</returns>
        public async Task<IEnumerable<PrivacyStatus>> GetPrivacyStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<PrivacyStatus>("CORE", "PRIVACY.CODES",
                (c, g) => new PrivacyStatus(g, c.ValInternalCodeAssocMember, c.ValExternalRepresentationAssocMember,
                    PrivacyStatusType.restricted), bypassCache: ignoreCache);
        }

        public async Task<IDictionary<string, string>> GetPrivacyMessagesAsync()
        {
            var privacyMessages = await GetOrAddToCacheAsync<Dictionary<string, string>>("AllPrivacyMessages",
                async () =>
                {
                    Dictionary<string, string> messages = new Dictionary<string, string>();
                    var defaults = await DataReader.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true);
                    if (defaults == null)
                    {
                        throw new ConfigurationException("Default configuration setup not complete.");
                    }

                    foreach (var item in defaults.DfltsPrivacyEntityAssociation)
                    {
                        messages.Add(item.DfltsPrivacyCodesAssocMember, item.DfltsPrivacyMsgsAssocMember);
                    }

                    return messages;
                }
            );
            return privacyMessages;
        }

        /// <summary>
        /// Prospect Sources
        /// </summary>
        public IEnumerable<ProspectSource> ProspectSources
        {
            get
            {
                return GetValcode<ProspectSource>("CORE", "PERSON.ORIGIN.CODES",
                    s => new ProspectSource(s.ValInternalCodeAssocMember, s.ValExternalRepresentationAssocMember));
            }
        }

        /// <summary>
        /// Prefixes
        /// </summary>
        public IEnumerable<Prefix> Prefixes
        {
            get
            {
                /// Get all prefixes from cache. If not there add them.
                var prefixes = GetOrAddToCache<List<Prefix>>("AllPrefixes",
                () =>
                {
                    // Get prefixes from the database if not in cache. 
                    Ellucian.Colleague.Data.Base.DataContracts.Prefixes prefixData = DataReader.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Prefixes>("CORE.PARMS", "PREFIXES");

                    var prefixList = new List<Prefix>();
                    List<string> invalidPrefixEntries = new List<string>();
                    if (prefixData != null)
                    {
                        prefixList = BuildAffixList<Prefix>(prefixData.PrefixesCodes, prefixData.PrefixesDescs, prefixData.PrefixesInternalCodes, out invalidPrefixEntries);
                    }

                    // Log the data error if invalid prefix entries were found
                    if (invalidPrefixEntries.Count > 0)
                    {
                        string prefixErrorMessage = "Prefix data defined in Colleague contains some incomplete entries; please access the Prefix & Suffix Definition (PPS) form to correct these incomplete entries";
                        LogDataError("Prefixes", "PREFIXES", invalidPrefixEntries, null, prefixErrorMessage);
                    }

                    return prefixList;
                });
                return prefixes;
            }
        }

        /// <summary>
        /// Prefixes
        /// </summary>
        public async Task<IEnumerable<Prefix>> GetPrefixesAsync()
        {

            // Get all prefixes from cache. If not there add them.
            var prefixes = await GetOrAddToCacheAsync<List<Prefix>>("AllPrefixes",
               async () =>
               {
                   // Get prefixes from the database if not in cache. 
                   var prefixData = await DataReader.ReadRecordAsync<Prefixes>("CORE.PARMS", "PREFIXES");

                   var prefixList = new List<Prefix>();
                   var invalidPrefixEntries = new List<string>();
                   if (prefixData != null)
                   {
                       prefixList = BuildAffixList<Prefix>(prefixData.PrefixesCodes, prefixData.PrefixesDescs,
                           prefixData.PrefixesInternalCodes, out invalidPrefixEntries);
                   }

                   // Log the data error if invalid prefix entries were found
                   if (invalidPrefixEntries.Count > 0)
                   {
                       const string prefixErrorMessage = "Prefix data defined in Colleague contains some incomplete entries; please access the Prefix & Suffix Definition (PPS) form to correct these incomplete entries";
                       LogDataError("Prefixes", "PREFIXES", invalidPrefixEntries, null, prefixErrorMessage);
                   }

                   return prefixList;
               });
            return prefixes;

        }

        /// <summary>
        /// Races
        /// </summary>
        public async Task<IEnumerable<Race>> RacesAsync()
        {
            return await GetRacesAsync(false);

        }

        /// <summary>
        /// Get a collection of races
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of races</returns>
        public async Task<IEnumerable<Race>> GetRacesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<Race>("CORE", "PERSON.RACES",
                (r, g) => new Race(g, r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember,
                    ConvertRaceTypeCodeToRaceType(r.ValActionCode1AssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of RelationshipStatus
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RelationshipStatus</returns>
        public async Task<IEnumerable<RelationshipStatus>> GetRelationshipStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<RelationshipStatus>("CORE", "RELATION.STATUSES",
                (cl, g) => new RelationshipStatus(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get guid for RelationshipStatuses
        /// </summary>
        /// <param name="code">RelationshipStatuses code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetRelationshipStatusesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetRelationshipStatusesAsync(false);
            RelationshipStatus codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetRelationshipStatusesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, RELATION.STATUSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, RELATION.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, RELATION.STATUSES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of personal relation type valcodes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of races</returns>
        public async Task<IEnumerable<PersonRelationType>> GetPersonRelationTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<PersonRelationType>("CORE", "INTG.PERSON.RELATION.TYPES",
                (r, g) => new PersonRelationType(g, r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of Relationship Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Relationship Types</returns>
        public async Task<IEnumerable<RelationType>> GetRelationTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<RelationTypes, RelationType>("AllRelationTypes", "RELATION.TYPES",
                (m, g) => new RelationType(g, m.Recordkey, m.ReltyDesc, m.ReltyOrgIndicator,
                    ConvertPersonalRelationshipTypeCodeToPersonalRelationshipType(m.ReltyIntgPersonRelType),
                    ConvertPersonalRelationshipTypeCodeToPersonalRelationshipType(m.ReltyIntgMaleRelType),
                    ConvertPersonalRelationshipTypeCodeToPersonalRelationshipType(m.ReltyIntgFemaleRelType), m.ReltyInverseRelationType),
                    bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of Relationship Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Relationship Types</returns>
        public async Task<IEnumerable<RelationType>> GetRelationTypes2Async(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<RelationTypes, RelationType>("AllRelationTypes2", "RELATION.TYPES",
                (m, g) => new RelationType(g, m.Recordkey, m.ReltyDesc, m.ReltyOrgIndicator, m.ReltyInverseRelationType),
                    bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of Relationship Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Relationship Types</returns>
        public async Task<IEnumerable<RelationType>> GetRelationTypes3Async(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<RelationTypes, RelationType>("AllRelationTypes3", "RELATION.TYPES",
                (m, g) => new RelationType(g, m.Recordkey, m.ReltyDesc, m.ReltyOrgIndicator, m.ReltyInverseRelationType, m.ReltyCategory),
                    bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for RelationTypes3
        /// </summary>
        /// <param name="code">RelationTypes3 code</param>
        /// <returns>Guid</returns>
        public async Task<Tuple<string,string>> GetRelationTypes3GuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            string reciprocal_guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return new Tuple<string, string>(guid, reciprocal_guid);
            var allCodesCache = await GetRelationTypes3Async(false);
            RelationType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetRelationTypes3Async(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'RELATION.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'RELATION.TYPES', Record ID:'", code, "'"));
                //get reciprocal relationship type guid
                var reci_codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(codeNoCache.InverseRelType, StringComparison.OrdinalIgnoreCase));
                if (reci_codeNoCache != null && !string.IsNullOrEmpty(reci_codeNoCache.Guid))
                    reciprocal_guid = reci_codeNoCache.Guid;
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'RELATION.TYPES', Record ID:'", code, "'"));
                var reci_codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(codeCache.InverseRelType, StringComparison.OrdinalIgnoreCase));
                if (reci_codeCache != null && !string.IsNullOrEmpty(reci_codeCache.Guid))
                    reciprocal_guid = reci_codeCache.Guid;
            }
            return new Tuple<string, string>(guid, reciprocal_guid);
        }

        /// <summary>
        /// Get a collection of remark codes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of remark codes</returns>
        public async Task<IEnumerable<RemarkCode>> GetRemarkCodesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<RemarkCode>("CORE", "REMARK.CODES",
                (cl, g) => new RemarkCode(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);

        }

        /// <summary>
        /// Get guid for REMARK.CODES code
        /// </summary>
        /// <param name="code">REMARK.CODES code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetRemarkCodesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetRemarkCodesAsync(false);
            RemarkCode codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetRemarkCodesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No GUID found, Entity: 'CORE.VALCODES-REMARK.CODES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No GUID found,  Entity: 'CORE.VALCODES-REMARK.CODES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No GUID found,  Entity: 'CORE.VALCODES-REMARK.CODES', Record ID:'", code, "'"));
            }
            return guid;
        }


        /// <summary>
        /// Get a collection of remark types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of remark types</returns>
        public async Task<IEnumerable<RemarkType>> GetRemarkTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<RemarkType>("CORE", "REMARK.TYPES",
                (cl, g) => new RemarkType(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for REMARK.TYPES code
        /// </summary>
        /// <param name="code">REMARK.TYPES code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetRemarkTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetRemarkTypesAsync(false);
            RemarkType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetRemarkTypesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No GUID found,  Entity: 'CORE.VALCODES-REMARK.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No GUID found,  Entity: 'CORE.VALCODES-REMARK.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No GUID found,  Entity: 'CORE.VALCODES-REMARK.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Restrictions
        /// </summary>
        public async Task<IEnumerable<Restriction>> RestrictionsAsync()
        {
            return await GetRestrictionsAsync(false);

        }

        /// <summary>
        /// Get a collection of restrictions
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of restrictions</returns>
        public async Task<IEnumerable<Restriction>> GetRestrictionsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Restrictions, Restriction>("AllRestrictions", "RESTRICTIONS",
                (r, g) => BuildRestriction(g, r), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of restrictions
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of restrictions</returns>
        public async Task<IEnumerable<Restriction>> GetRestrictionsWithCategoryAsync(bool ignoreCache)
        {            
            return await GetGuidCodeItemAsync<Restrictions, Restriction>("AllRestrictionsWithCategories", "RESTRICTIONS",                
            (r, g) => BuildRestrictionWithCategory(g, r), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Builds restrictions with category.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="restriction"></param>
        /// <returns></returns>
        private Restriction BuildRestrictionWithCategory(string guid, Restrictions restriction)
        {
            Restriction rest;
            try
            {
                rest = new Restriction(guid, restriction.Recordkey, restriction.RestDesc, null, string.Empty, string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, string.Empty);
                rest.RestIntgCategory = ConvertRestrictionCategoryToType(restriction.RestIntgCategory);
                return rest;
            }
            catch (Exception ex)
            {
                LogDataError("Restriction", restriction.Recordkey, restriction, ex);
                return null;
            }
        }
        /// <summary>
        ///  A collection of Room Characteristics
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Room Characteristics</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RoomCharacteristic>> GetRoomCharacteristicsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<Ellucian.Colleague.Domain.Base.Entities.RoomCharacteristic>("CORE", "ROOM.CHARACTERISTICS",
                (rc, g) => new RoomCharacteristic(g, rc.ValInternalCodeAssocMember, rc.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of Room Types
        /// </summary>
        /// <returns>Collection of RoomTypes</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RoomTypes>> RoomTypesAsync()
        {
            return await GetRoomTypesAsync(false);

        }

        /// <summary>
        /// Get a collection of room wings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RoomWing</returns>
        public async Task<IEnumerable<RoomWing>> GetRoomWingsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<RoomWing>("CORE", "ROOM.WINGS",
                (cl, g) => new RoomWing(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        ///  A collection of Room Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RoomTypes</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RoomTypes>> GetRoomTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Ellucian.Colleague.Data.Base.DataContracts.RoomTypes, Ellucian.Colleague.Domain.Base.Entities.RoomTypes>("AllRoomTypes", "ROOM.TYPES",
           (rt, g) => new Ellucian.Colleague.Domain.Base.Entities.RoomTypes(g, rt.Recordkey,
               rt.RmtpDescription, ConvertRoomTypeCodeToRoomType(rt.RmtpIntgRoomType)), bypassCache: ignoreCache);
        }




        /// <summary>
        /// Schedule repeat codes
        /// </summary>
        public IEnumerable<ScheduleRepeat> ScheduleRepeats
        {
            get
            {
                var scheduleRepeats = GetOrAddToCache<List<ScheduleRepeat>>("AllScheduleRepeats",
                        () =>
                        {
                            List<ScheduleRepeat> scheduleRepeatList = new List<ScheduleRepeat>();

                            ApplValcodes schedRepeatsValcode = this.DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "SCHED.REPEATS");
                            if (schedRepeatsValcode != null)
                            {
                                foreach (ApplValcodesVals applVal in schedRepeatsValcode.ValsEntityAssociation)
                                {
                                    scheduleRepeatList.Add(new ScheduleRepeat(applVal.ValInternalCodeAssocMember, applVal.ValExternalRepresentationAssocMember, 
                                        applVal.ValActionCode1AssocMember, ConvertCodeToFrequencyType(applVal.ValActionCode2AssocMember)));
                                }
                            }
                            return scheduleRepeatList;
                        }
                    );
                return scheduleRepeats;
            }
        }

        /// <summary>
        /// Get a collection of social media types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of social media types</returns>
        public async Task<IEnumerable<SocialMediaType>> GetSocialMediaTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<SocialMediaType>("CORE", "SOCIAL.MEDIA.NETWORKS",
                (r, g) => new SocialMediaType(g, r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember,
                    ConvertSocialMediaTypeCategoryCodeToSocialMediaTypeCategory(r.ValActionCode3AssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of source contexts
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns></returns>
        public async Task<IEnumerable<SourceContext>> GetSourceContextsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<SourceContext>("CORE", "INTG.SOURCE.CONTEXTS",
                (gcr, g) => new SourceContext(g, gcr.ValInternalCodeAssocMember, gcr.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get a collection of visa types with Category
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of visa type guid items</returns>
        public async Task<IEnumerable<VisaTypeGuidItem>> GetVisaTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<VisaTypeGuidItem>("CORE", "VISA.TYPES",
                (r, g) => new VisaTypeGuidItem(g, r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember,
                    ConvertVisaTypeCategoryCodeToVisaTypeCategory(r.ValActionCode3AssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for VISA.TYPES code
        /// </summary>
        /// <param name="code">VISA.TYPE code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetVisaTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetVisaTypesAsync(false);
            VisaTypeGuidItem codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetVisaTypesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No GUID found,  Entity: 'CORE.VALCODES-VISA.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No GUID found,  Entity: 'CORE.VALCODES-VISA.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No GUID found,  Entity: 'CORE.VALCODES-VISA.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// States
        /// </summary>
        public async Task<IEnumerable<State>> GetStateCodesAsync()
        {
            return await GetCodeItemAsync<States, State>("AllStates", "STATES",
                 c => new State(c.Recordkey, c.StDesc, c.StCountry));
        }

        /// <summary>
        /// States
        /// </summary>
        public async Task<IEnumerable<State>> GetStateCodesAsync(bool ignoreCache = false)
        {
            return await GetCodeItemAsync<States, State>("AllStates", "STATES",
                c => new State(c.Recordkey, c.StDesc, c.StCountry), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Suffixes
        /// </summary>
        public IEnumerable<Suffix> Suffixes
        {
            get
            {
                // Get all suffixes from cache. If not there add them.
                var suffixes = GetOrAddToCache<List<Suffix>>("AllSuffixes",
                () =>
                {
                    // Get suffixes from the database if not in cache. 
                    Ellucian.Colleague.Data.Base.DataContracts.Suffixes suffixData = DataReader.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Suffixes>("CORE.PARMS", "SUFFIXES");

                    var suffixList = new List<Suffix>();
                    List<string> invalidSuffixEntries = new List<string>();
                    if (suffixData != null)
                    {
                        suffixList = BuildAffixList<Suffix>(suffixData.SuffixesCodes, suffixData.SuffixesDescs, suffixData.SuffixesInternalCodes, out invalidSuffixEntries);
                    }

                    // Log the data error if invalid suffix entries were found
                    if (invalidSuffixEntries.Count > 0)
                    {
                        string suffixErrorMessage = "Suffix data defined in Colleague contains some incomplete entries; please access the Prefix & Suffix Definition (PPS) form to correct these incomplete entries";
                        LogDataError("Suffixes", "SUFFIXES", invalidSuffixEntries, null, suffixErrorMessage);
                    }

                    return suffixList;
                });
                return suffixes;
            }
        }


        /// <summary>
        /// Suffixes
        /// </summary>
        public async Task<IEnumerable<Suffix>> GetSuffixesAsync()
        {
            {
                // Get all suffixes from cache. If not there add them.
                var suffixes = await GetOrAddToCacheAsync("AllSuffixes",
                async () =>
                {
                    // Get suffixes from the database if not in cache. 
                    var suffixData = await DataReader.ReadRecordAsync<Suffixes>("CORE.PARMS", "SUFFIXES");

                    var suffixList = new List<Suffix>();
                    var invalidSuffixEntries = new List<string>();
                    if (suffixData != null)
                    {
                        suffixList = BuildAffixList<Suffix>(suffixData.SuffixesCodes, suffixData.SuffixesDescs, suffixData.SuffixesInternalCodes, out invalidSuffixEntries);
                    }

                    // Log the data error if invalid suffix entries were found
                    if (invalidSuffixEntries.Count > 0)
                    {
                        const string suffixErrorMessage = "Suffix data defined in Colleague contains some incomplete entries; please access the Prefix & Suffix Definition (PPS) form to correct these incomplete entries";
                        LogDataError("Suffixes", "SUFFIXES", invalidSuffixEntries, null, suffixErrorMessage);
                    }

                    return suffixList;
                });
                return suffixes;
            }
        }

        /// <summary>
        /// ZipCodeXlat
        /// </summary>
        public async Task<IEnumerable<ZipcodeXlat>> GetZipCodeXlatAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<ZipCodeXlat, ZipcodeXlat>("AllZipxlat", "ZIP.CODE.XLAT",
               (z, g) => new ZipcodeXlat(g, z.Recordkey, "Zipcode"), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Builds the list of affix (prefix or suffix) data to be accessible in the Colleague Web API.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="affixCodes">The list of affix codes (minimum entry values) defined in Colleague.</param>
        /// <param name="affixDescriptions">The list of affix descriptions defined in Colleague.</param>
        /// <param name="affixInternalCodes">The list of affix internal codes (stored values) defined in Colleague.</param>
        /// <param name="invalidAffixEntries">Output parameter: List of invalid affix entries identified while building the affix data list.</param>
        /// <returns></returns>
        private List<T> BuildAffixList<T>(List<string> affixCodes, List<string> affixDescriptions, List<string> affixInternalCodes, out List<string> invalidAffixEntries)
        {
            var affixList = new List<T>();
            invalidAffixEntries = new List<string>();

            if (affixCodes != null)
            {
                // Get the number of affix entries; use the list with the greatest number of values
                int affixEntryCount = affixCodes.Count;
                if (affixDescriptions.Count > affixEntryCount) { affixEntryCount = affixDescriptions.Count; }
                if (affixInternalCodes.Count > affixEntryCount) { affixEntryCount = affixInternalCodes.Count; }

                for (int i = 0; i < affixEntryCount; i++)
                {
                    string affixCode = (affixCodes.Count > i) ? affixCode = affixCodes[i] : string.Empty;
                    string affixDescription = (affixDescriptions.Count > i) ? affixDescription = affixDescriptions[i] : string.Empty;
                    string affixInternalCode = (affixInternalCodes.Count > i) ? affixInternalCode = affixInternalCodes[i] : string.Empty;

                    // Only add to the list of affix codes in the API if (at a minimum) both the code and internal code values are not empty
                    if (!String.IsNullOrEmpty(affixCode) && !String.IsNullOrEmpty(affixInternalCode))
                    {
                        // Handle case of the affix description being empty
                        if (String.IsNullOrEmpty(affixDescription))
                        {
                            // Add the invalid affix entry so it can be logged
                            invalidAffixEntries.Add("Incomplete entry: Code='" + affixCode + "'  Description='" + affixDescription + "'  Internal Code='" + affixInternalCode + "'");

                            // Default description to the internal code value so it can be used in the API
                            affixDescription = affixInternalCode;
                        }

                        // NOTE: The CreateInstance() method, used here, in the Activator class can have negative performance implications; for its usage
                        // in the context of suffixes and prefixes, it is fine, but wanted to note the performance implications here in case this code
                        // were used as a frame of reference to be copied and used elsewhere in other code
                        affixList.Add((T)Activator.CreateInstance(typeof(T), new object[] { affixCode, affixDescription, affixInternalCode }));
                    }
                    else
                    {
                        // Document the invalid affix entry so it can be logged
                        invalidAffixEntries.Add("Incomplete entry: Code='" + affixCode + "'  Description='" + affixDescription + "'  Internal Code='" + affixInternalCode + "'");
                    }
                }
            }
            return affixList;
        }

        /// <summary>
        /// Get guid for Schools code
        /// </summary>
        /// <param name="code">Schools code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetSchoolsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetSchoolsAsync(false);
            School codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetSchoolsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SCHOOL', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SCHOOL', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SCHOOL', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get all Schools
        /// </summary>
        public async Task<IEnumerable<School>> GetSchoolsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Schools, School>("AllEedmSchools", "SCHOOLS",
              (s, g) => BuildSchools(g, s), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);

        }

        /// <summary>
        /// Get all Vocations
        /// </summary>
        public async Task<IEnumerable<Vocation>> GetVocationsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Vocations, Vocation>("AllEedmVocations", "VOCATIONS",
               (v, g) => new Ellucian.Colleague.Domain.Base.Entities.Vocation(g, v.Recordkey,
               v.VocationsDesc),  bypassCache: ignoreCache);

        }

        /// <summary>
        /// Get list of institutions ids that are home institutions
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetHomeInstitutionIdList()
        {
            string schoolsCriteria = "WITH SCHOOLS.INSTITUTIONS.ID NE ''";
            string deptsCriteria = "WITH DEPTS.INSTITUTIONS.ID NE ''";
            string divsCriteria = "WITH DIV.INSTITUTIONS.ID NE ''";

            var schoolsWithInstitutions = await DataReader.BulkReadRecordAsync<Schools>(schoolsCriteria);
            var deptsWithInstitutions = await DataReader.BulkReadRecordAsync<Depts>(deptsCriteria);
            var divsWithInstitutions = await DataReader.BulkReadRecordAsync<Divisions>(divsCriteria);

            var homeInstitutionIds = new List<string>();

            homeInstitutionIds.AddRange(schoolsWithInstitutions.Select(s => s.SchoolsInstitutionsId).ToList());
            homeInstitutionIds.AddRange(deptsWithInstitutions.Select(s => s.DeptsInstitutionsId).ToList());
            homeInstitutionIds.AddRange(divsWithInstitutions.Select(s => s.DivInstitutionsId).ToList());

            return homeInstitutionIds.Distinct().ToList();
        }

        private School BuildSchools(string guid, Schools dt)
        {
            School school;
            try
            {
                var desc = String.IsNullOrEmpty(dt.SchoolsDesc) ? dt.Recordkey : dt.SchoolsDesc;

                school = new School(dt.RecordGuid, dt.Recordkey, desc);
                
                school.AcademicLevelCode = dt.SchoolsAcadLevel;
                school.InstitutionId = dt.SchoolsInstitutionsId;

                foreach (var dtcd in dt.SchoolsLocations)
                {
                    try
                    {
                        school.AddLocationCode(dtcd);
                    }
                    catch (Exception)
                    {
                        // Do nothing since we really don't care about null or duplicate items
                    }
                }
                foreach (var dtcd in dt.SchoolsDepts)
                {
                    try
                    {
                        school.AddDepartmentCode(dtcd);
                    }
                    catch (Exception)
                    {
                        // Do nothing since we really don't care about null or duplicate items
                    }
                }
                foreach (var dtcd in dt.SchoolsDivisions)
                {
                    try
                    {
                        school.AddDivisionCode(dtcd);
                    }
                    catch (Exception)
                    {
                        // Do nothing since we really don't care about null or duplicate items
                    }
                }
            }
            catch (Exception ex)
            {
                LogDataError("Restriction", dt.Recordkey, dt, ex);
                return null;
            }

            return school;
        }

        public IEnumerable<School> Schools
        {
            get
            {
                var schoolCodes = GetOrAddToCache<IEnumerable<School>>("AllSchools",
                    () =>
                    {
                        Collection<Schools> schoolsData = DataReader.BulkReadRecord<Schools>("SCHOOLS", "");
                        var schoolsList = BuildSchools(schoolsData);
                        return schoolsList;
                    }
                );
                return schoolCodes;
            }
        }

        private IEnumerable<School> BuildSchools(Collection<Schools> dtTypeData)
        {
            var schools = new List<School>();
            // If no data passed in, return a null collection
            if (dtTypeData != null)
            {
                foreach (var dt in dtTypeData)
                {
                    try
                    {
                        // For EEDM, don't fail if the description is missing, use the code for the description

                        var desc = String.IsNullOrEmpty(dt.SchoolsDesc) ? dt.Recordkey : dt.SchoolsDesc;

                        var school = new School(dt.RecordGuid, dt.Recordkey, desc);
                        school.AcademicLevelCode = dt.SchoolsAcadLevel;
                        school.InstitutionId = dt.SchoolsInstitutionsId;

                        foreach (var dtcd in dt.SchoolsLocations)
                        {
                            try
                            {
                                school.AddLocationCode(dtcd);
                            }
                            catch (Exception)
                            {
                                // Do nothing since we really don't care about null or duplicate items
                            }
                        }
                        foreach (var dtcd in dt.SchoolsDepts)
                        {
                            try
                            {
                                school.AddDepartmentCode(dtcd);
                            }
                            catch (Exception)
                            {
                                // Do nothing since we really don't care about null or duplicate items
                            }
                        }
                        foreach (var dtcd in dt.SchoolsDivisions)
                        {
                            try
                            {
                                school.AddDivisionCode(dtcd);
                            }
                            catch (Exception)
                            {
                                // Do nothing since we really don't care about null or duplicate items
                            }
                        }
                        schools.Add(school);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("School", dt.SchoolsInstitutionsId, dt, ex);
                        //throw new ArgumentException("Error occurred when trying to build schools " + dt.Recordkey);
                    }
                }
                return schools;
            }
            return schools;
        }

        public IEnumerable<SpecialNeed> SpecialNeeds
        {
            get
            {
                return GetValcode<SpecialNeed>("CORE", "SPECIAL.NEEDS",
                    specialNeeds => new SpecialNeed(specialNeeds.ValInternalCodeAssocMember, specialNeeds.ValExternalRepresentationAssocMember));
            }
        }

        /// <summary>
        /// Visa Types
        /// </summary>
        public IEnumerable<VisaType> VisaTypes
        {
            get
            {
                return GetValcode<VisaType>("CORE", "VISA.TYPES",
                    v => new VisaType(v.ValInternalCodeAssocMember, v.ValExternalRepresentationAssocMember));
            }
        }


        /// <summary>
        /// Converts an entity type code to the corresponding contact entity enumeration value
        /// </summary>
        /// <param name="personCode">person related code</param>
        /// <returns>contact entity type enumeration value</returns>
        private EntityType ConvertEntityTypeCodeToEntityType(string personCode, string orgCode)
        {
            // person code is defined, always select entity type of person
            if (!string.IsNullOrEmpty(personCode))
            {
                return EntityType.Person;
            }
            else
            {
                // Both person and organization code is not defined so use person
                if (string.IsNullOrEmpty(orgCode))
                {
                    return EntityType.Person;
                }
                // only organization code is defined so use organization entity type
                else
                {
                    return EntityType.Organization;
                }
            }
        }

        ///// <summary>
        ///// Converts a person address type code to the corresponding person address type enumeration value
        ///// </summary>
        ///// <param name="code">Type code</param>
        ///// <returns>person address type enumeration value</returns>
        //private PersonAddressType ConvertPersonAddressTypeCodeToPersonAddressType(string code)
        //{
        //    switch (code.ToLowerInvariant())
        //    {
        //        case "school":
        //            return PersonAddressType.School;
        //        case "home":
        //            return PersonAddressType.Home;
        //        case "vacation":
        //            return PersonAddressType.Vacation;
        //        case "billing":
        //            return PersonAddressType.Billing;
        //        case "shipping":
        //            return PersonAddressType.Shipping;
        //        case "mailing":
        //            return PersonAddressType.Mailing;
        //        case "business":
        //            return PersonAddressType.Business;
        //        default:
        //            return PersonAddressType.Other;
        //    }
        //}

        ///// <summary>
        ///// Converts an organization address type code to the corresponding organization address type enumeration value
        ///// </summary>
        ///// <param name="code">Type code</param>
        ///// <returns>organization address type enumeration value</returns>
        //private OrganizationAddressType ConvertOrgAddressTypeCodeToOrgAddressType(string code)
        //{
        //    switch (code.ToLowerInvariant())
        //    {
        //        case "business":
        //            return OrganizationAddressType.Business;
        //        case "pobox":
        //            return OrganizationAddressType.Pobox;
        //        case "main":
        //            return OrganizationAddressType.Main;
        //        case "branch":
        //            return OrganizationAddressType.Branch;
        //        case "regional":
        //            return OrganizationAddressType.Region;
        //        case "support":
        //            return OrganizationAddressType.Support;
        //        default:
        //            return OrganizationAddressType.Other;
        //    }
        //}

        /// <summary>
        /// Converts a address type code to the corresponding address type enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>address type enumeration value</returns>
        private AddressTypeCategory ConvertAddressTypeCodeToAddressType(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return AddressTypeCategory.Other;
            }
            switch (code.ToLowerInvariant())
            {
                case "school":
                    return AddressTypeCategory.School;
                case "home":
                    return AddressTypeCategory.Home;
                case "vacation":
                    return AddressTypeCategory.Vacation;
                case "billing":
                    return AddressTypeCategory.Billing;
                case "shipping":
                    return AddressTypeCategory.Shipping;
                case "mailing":
                    return AddressTypeCategory.Mailing;
                case "business":
                    return AddressTypeCategory.Business;
                case "parent":
                    return AddressTypeCategory.Parent;
                case "family":
                    return AddressTypeCategory.Family;
                case "pobox":
                    return AddressTypeCategory.Pobox;
                case "main":
                    return AddressTypeCategory.Main;
                case "branch":
                    return AddressTypeCategory.Branch;
                case "region":
                    return AddressTypeCategory.Region;
                case "support":
                    return AddressTypeCategory.Support;
                case "matchinggifts":
                    return AddressTypeCategory.MatchingGifts;
                default:
                    return AddressTypeCategory.Other;
            }
        }

        /// <summary>
        /// Converts a email type code to the corresponding email enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>email type enumeration value</returns>
        private EmailTypeCategory ConvertEmailTypeCodeToEmailType(string code)
        {
            switch (code.ToLowerInvariant())
            {
                case "personal":
                    return EmailTypeCategory.Personal;
                case "business":
                    return EmailTypeCategory.Business;
                case "school":
                    return EmailTypeCategory.School;
                case "parent":
                    return EmailTypeCategory.Parent;
                case "family":
                    return EmailTypeCategory.Family;
                case "sales":
                    return EmailTypeCategory.Sales;
                case "support":
                    return EmailTypeCategory.Support;
                case "general":
                    return EmailTypeCategory.General;
                case "billing":
                    return EmailTypeCategory.Billing;
                case "legal":
                    return EmailTypeCategory.Legal;
                case "hr":
                    return EmailTypeCategory.HR;
                case "media":
                    return EmailTypeCategory.Media;
                case "matchinggifts":
                    return EmailTypeCategory.MatchingGifts;
                default:
                    return EmailTypeCategory.Other;
            }
        }


        /// <summary>
        /// Converts a person location type code to the corresponding person location type enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>person location type enumeration value</returns>
        private PersonLocationType ConvertPersonLocationTypeCodeToPersonLocationType(string code)
        {
            switch (code.ToLowerInvariant())
            {
                case "school":
                    return PersonLocationType.School;
                case "home":
                    return PersonLocationType.Home;
                case "vacation":
                    return PersonLocationType.Vacation;
                case "billing":
                    return PersonLocationType.Billing;
                case "shipping":
                    return PersonLocationType.Shipping;
                case "mailing":
                    return PersonLocationType.Mailing;
                case "business":
                    return PersonLocationType.Business;
                default:
                    return PersonLocationType.Other;
            }
        }

        /// <summary>
        /// Converts an organization location type code to the corresponding organization location type enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>organization location type enumeration value</returns>
        private OrganizationLocationType ConvertOrgLocationTypeCodeToOrgLocationType(string code)
        {
            switch (code.ToLowerInvariant())
            {
                case "business":
                    return OrganizationLocationType.Business;
                case "pobox":
                    return OrganizationLocationType.Pobox;
                case "main":
                    return OrganizationLocationType.Main;
                case "branch":
                    return OrganizationLocationType.Branch;
                case "regional":
                    return OrganizationLocationType.Region;
                case "support":
                    return OrganizationLocationType.Support;
                default:
                    return OrganizationLocationType.Other;
            }
        }

        /// <summary>
        /// Converts an citizenship status type code to the corresponding citizenship status enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>Citizenship status enumeration value</returns>
        private CitizenshipStatusType ConvertCitizenshipStatusTypeCodeToCitizenshipStatusType(string code)
        {
            switch (code)
            {
                case "NA":
                    return CitizenshipStatusType.Citizen;
                case "NRA":
                    return CitizenshipStatusType.NonCitizen;
                default:
                    return CitizenshipStatusType.NonCitizen;
            }
        }

        /// <summary>
        /// Converts an ethnicity type code to the corresponding ethnicity enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>Ethnicity enumeration value</returns>
        private EthnicityType ConvertEthnicityTypeCodeToEthnicityType(string code)
        {
            switch (code)
            {
                case "H":
                    return EthnicityType.Hispanic;
                case "N":
                    return EthnicityType.NonHispanic;
                case "NRA":
                    return EthnicityType.NonResident;
                default:
                    return EthnicityType.NonHispanic;
            }
        }

        /// <summary>
        /// Converts an geographic area type code to the corresponding geographic area type enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>Geographic area type enumeration value</returns>
        private GeographicAreaTypeCategory ConvertGeographicAreaTypeCategoryCodeToGeographicAreaTypeCategory(string code)
        {
            switch (code)
            {
                case "GOV":
                    return GeographicAreaTypeCategory.Governmental;
                case "POST":
                    return GeographicAreaTypeCategory.Postal;
                case "FUND":
                    return GeographicAreaTypeCategory.Fundraising;
                default:
                    return GeographicAreaTypeCategory.Recruitment;
            }
        }

        /// <summary>
        /// Converts an identity document type category code to the corresponding identity document type enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>Identity Document Type enumeration value</returns>
        private IdentityDocumentTypeCategory ConvertIdentityDocumentTypeCategoryCodeToIdentityDocumentTypeCategory(string code)
        {
            switch (code)
            {
                case "OTHER":
                    return IdentityDocumentTypeCategory.Other;
                case "LICENSE":
                    return IdentityDocumentTypeCategory.PhotoId;
                case "PASSPORT":
                    return IdentityDocumentTypeCategory.Passport;
                default:
                    return IdentityDocumentTypeCategory.Other;
            }
        }

        /// <summary>
        /// Converts a person name type code to the corresponding person name enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>person name type enumeration value</returns>
        private PersonNameType ConvertPersonNameTypeCodeToPersonNameType(string code)
        {
            switch (code.ToLowerInvariant())
            {
                case "birth":
                    return PersonNameType.Birth;
                case "legal":
                    return PersonNameType.Legal;
                case "favored":
                    return PersonNameType.Chosen;
                default:
                    return PersonNameType.Personal;
            }
        }

        /// <summary>
        /// Converts a phone type code to the corresponding phone enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>phone type enumeration value</returns>
        private PhoneTypeCategory ConvertPhoneTypeCodeToPhoneType(string code)
        {
            switch (code.ToLowerInvariant())
            {
                case "business":
                    return PhoneTypeCategory.Business;
                case "vacation":
                    return PhoneTypeCategory.Vacation;
                case "fax":
                    return PhoneTypeCategory.Fax;
                case "home":
                    return PhoneTypeCategory.Home;
                case "mobile":
                    return PhoneTypeCategory.Mobile;
                case "school":
                    return PhoneTypeCategory.School;
                case "pager":
                    return PhoneTypeCategory.Pager;
                case "tdd":
                    return PhoneTypeCategory.TDD;
                case "parent":
                    return PhoneTypeCategory.Parent;
                case "family":
                    return PhoneTypeCategory.Family;
                case "billing":
                    return PhoneTypeCategory.Billing;
                case "branch":
                    return PhoneTypeCategory.Branch;
                case "main":
                    return PhoneTypeCategory.Main;
                case "region":
                    return PhoneTypeCategory.Region;
                case "support":
                    return PhoneTypeCategory.Support;
                case "matchinggifts":
                    return PhoneTypeCategory.MatchingGifts;
                default:
                    return PhoneTypeCategory.Other;
            }
        }

        /// <summary>
        /// Converts a marital status type code to the corresponding marital status enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>Marital Status enumeration value</returns>
        private MaritalStatusType? ConvertMaritalStatusTypeCodeToMaritalStatusType(string code)
        {
            switch (code)
            {
                case "1":
                    return MaritalStatusType.Single;
                case "2":
                    return MaritalStatusType.Married;
                case "3":
                    return MaritalStatusType.Divorced;
                case "4":
                    return MaritalStatusType.Widowed;
                case "5":
                    return MaritalStatusType.Separated;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts a race type code to the corresponding race enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>Race enumeration value</returns>
        private RaceType? ConvertRaceTypeCodeToRaceType(string code)
        {
            switch (code)
            {
                case "1":
                    return RaceType.AmericanIndian;
                case "2":
                    return RaceType.Asian;
                case "3":
                    return RaceType.Black;
                case "4":
                    return RaceType.PacificIslander;
                case "5":
                    return RaceType.White;
                default:
                    return null;
            }
        }


        private PersonalRelationshipType? ConvertPersonalRelationshipTypeCodeToPersonalRelationshipType(string code)
        {
            if (String.IsNullOrEmpty(code))
                return null;

            switch (code)
            {
                case "39": // "aunt":
                    return PersonalRelationshipType.Aunt;
                case "15": // "brother":
                    return PersonalRelationshipType.Brother;
                case "37": // "brotherinlaw":
                    return PersonalRelationshipType.BrotherInLaw;
                case "50": // "caregiver":
                    return PersonalRelationshipType.Caregiver;
                case "7":  // "child":
                    return PersonalRelationshipType.Child;
                case "31": // "childinlaw":
                    return PersonalRelationshipType.ChildInLaw;
                case "41": // "childofsibling":
                    return PersonalRelationshipType.ChildOfSibling;
                case "49": // "classmate":
                    return PersonalRelationshipType.Classmate;
                case "44": // "cousin":
                    return PersonalRelationshipType.Cousin;
                case "47": //"coworker":
                    return PersonalRelationshipType.Coworker;
                case "9":  // "daughter":
                    return PersonalRelationshipType.Daughter;
                case "32": // "daughterinlaw":
                    return PersonalRelationshipType.DaughterInLaw;
                case "5":  // "father":
                    return PersonalRelationshipType.Father;
                case "30": // "fatherinlaw":
                    return PersonalRelationshipType.FatherInLaw;
                case "45": // friend":
                    return PersonalRelationshipType.Friend;
                case "25": // "grandchild":
                    return PersonalRelationshipType.GrandChild;
                case "26": // "granddaughter":
                    return PersonalRelationshipType.GrandDaughter;
                case "24": // "grandfather":
                    return PersonalRelationshipType.GrandFather;
                case "23": // "grandmother":
                    return PersonalRelationshipType.GrandMother;
                case "22": // "grandparent":
                    return PersonalRelationshipType.GrandParent;
                case "27": // "grandson":
                    return PersonalRelationshipType.GrandSon;
                case "21": // "husband"
                    return PersonalRelationshipType.Husband;
                case "3": // "mother":
                    return PersonalRelationshipType.Mother;
                case "29": // "motherinlaw":
                    return PersonalRelationshipType.MotherInLaw;
                case "48": // "neighbor":
                    return PersonalRelationshipType.Neighbor;
                case "43": //"nephew":
                    return PersonalRelationshipType.Nephew;
                case "42": // "niece":
                    return PersonalRelationshipType.Niece;
                case "1": // "parent":
                    return PersonalRelationshipType.Parent;
                case "28": // "parentinlaw":
                    return PersonalRelationshipType.ParentInLaw;
                case "33": // "partner"
                    return PersonalRelationshipType.Partner;
                case "46": // "relative":
                    return PersonalRelationshipType.Relative;
                case "13": // "sibling":
                    return PersonalRelationshipType.Sibling;
                case "35": // "siblinginlaw":
                    return PersonalRelationshipType.SiblingInLaw;
                case "38": // "siblingofparent":
                    return PersonalRelationshipType.SiblingOfParent;
                case "17": // "sister":
                    return PersonalRelationshipType.Sister;
                case "36": // "sister-in-law"
                    return PersonalRelationshipType.SisterInLaw;
                case "11": //"son":
                    return PersonalRelationshipType.Son;
                case "34": // "soninlaw":
                    return PersonalRelationshipType.SonInLaw;
                case "19": //"spouse":
                    return PersonalRelationshipType.Spouse;
                case "16": //"stepbrother":
                    return PersonalRelationshipType.StepBrother;
                case "8": // "stepchild":
                    return PersonalRelationshipType.StepChild;
                case "10": // "stepdaughter":
                    return PersonalRelationshipType.StepDaughter;
                case "6": // "stepfather":
                    return PersonalRelationshipType.StepFather;
                case "4": // "stepmother":
                    return PersonalRelationshipType.StepMother;
                case "2": // "stepparent":
                    return PersonalRelationshipType.StepParent;
                case "14": // "stepsibling":
                    return PersonalRelationshipType.StepSibling;
                case "18": //"stepsister":
                    return PersonalRelationshipType.StepSister;
                case "12": //"stepson":
                    return PersonalRelationshipType.StepSon;
                case "40": // "uncle":
                    return PersonalRelationshipType.Uncle;
                case "20": // "wife":
                    return PersonalRelationshipType.Wife;
                case "51": // "other":
                    return PersonalRelationshipType.Other;
                default:
                    return PersonalRelationshipType.Other;
            }
        }


        private FrequencyType? ConvertCodeToFrequencyType(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            switch (code)
            {
                case "D":
                    return FrequencyType.Daily;
                case "W":
                    return FrequencyType.Weekly;
                case "M":
                    return FrequencyType.Monthly;
                case "Y":
                    return FrequencyType.Yearly;
                default:
                    return null;
            }
        }


        /// <summary>
        /// Converts a Room Type code to the corresponding RoomType enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>Room Type enumeration value</returns>
        private RoomType? ConvertRoomTypeCodeToRoomType(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            switch (code)
            {
                case "8": //"classroom":
                    return RoomType.Classroom;
                case "34": //"amphitheater":
                    return RoomType.Amphitheater;
                case "23": //"animalquarters":
                    return RoomType.Animalquarters;
                case "6": //"apartment":
                    return RoomType.Apartment;
                case "16": //"artstudio":
                    return RoomType.Artstudio;
                case "33": //"atrium":
                    return RoomType.Atrium;
                case "15": //"audiovisuallab":
                    return RoomType.Audiovisuallab;
                case "35": //"auditorium":
                    return RoomType.Auditorium;
                case "19": //"ballroom":
                    return RoomType.Ballroom;
                case "36": //"booth":
                    return RoomType.Booth;
                case "22": //"clinic":
                    return RoomType.Clinic;
                case "13": //"computerlaboratory":
                    return RoomType.Computerlaboratory;
                case "11": //"conferenceroom":
                    return RoomType.Conferenceroom;
                case "28": //"daycare":
                    return RoomType.Daycare;
                case "27": //"foodfacility":
                    return RoomType.Foodfacility;
                case "26": //"generalusefacility":
                    return RoomType.Generalusefacility;
                case "24": //"greenhouse":
                    return RoomType.Greenhouse;
                case "32": //"healthcarefacility":
                    return RoomType.Healthcarefacility;
                case "7": //"house":
                    return RoomType.House;
                case "9": //"lecturehall":
                    return RoomType.Lecturehall;
                case "29": //"lounge":
                    return RoomType.Lounge;
                case "14": //"mechanicslab":
                    return RoomType.Mechanicslab;
                case "30": //"merchandisingroom":
                    return RoomType.Merchandisingroom;
                case "17": //"musicroom":
                    return RoomType.Musicroom;
                case "20": //"office":
                    return RoomType.Office;
                case "37": //"other"
                    return RoomType.Other;
                case "18": //"performingartsstudio":
                    return RoomType.Performingartsstudio;
                case "1": //"residencehallroom":
                    return RoomType.Residencehallroom;
                case "3": //"residentialdoubleroom":
                    return RoomType.Residentialdoubleroom;
                case "2": //"residentialsingleroom":
                    return RoomType.Residentialsingleroom;
                case "5": //"residentialsuiteroom":
                    return RoomType.Residentialsuiteroom;
                case "4": //"residentialtripleroom":
                    return RoomType.Residentialtripleroom;
                case "12": //"sciencelaborator":
                    return RoomType.Sciencelaboratory;
                case "10": //"seminarroom":
                    return RoomType.Seminarroom;
                case "25": //"specialusefacility":
                    return RoomType.Specialusefacility;
                case "21": //"studyfacility":
                    return RoomType.Studyfacility;
                case "31": //"supportfacility":
                    return RoomType.Supportfacility;
                default:
                    return RoomType.Other;
            }
        }


        private Restriction BuildRestriction(string guid, Restrictions restriction)
        {
            Restriction rest;
            try
            {
                rest = new Restriction(restriction.RecordGuid, restriction.Recordkey, restriction.RestDesc, restriction.RestSeverity, restriction.RestPrtlDisplayFlag,
                    restriction.RestPrtlDisplayDesc, restriction.RestPrtlDisplayDescDtl, restriction.RestPrtlFollowUpApp, restriction.RestPrtlFollowUpLinkDef,
                    restriction.RestPrtlFollowUpWaForm, restriction.RestPrtlFollowUpLabel, restriction.RestPrtlFollowUpIsMtxt);

                GetRestrictionHyperlinksRequest hyperlinksRequest = new GetRestrictionHyperlinksRequest()
                    {
                        RestrictionIds = new List<string>() { rest.Code },
                        LinkLabelsIn = new List<string>() { rest.FollowUpLabel },
                        LinkDefinitionsIn = new List<string>() { rest.FollowUpLinkDefinition },
                        LinkApplicationsIn = new List<string>() { rest.FollowUpApplication },
                        WaFormsIn = new List<string>() { rest.FollowUpWebAdvisorForm },
                        MtxtFlagsIn = new List<string>() { rest.MiscellaneousTextFlag ? "Y" : "" }
                    };
                GetRestrictionHyperlinksResponse hyperlinksResponse = transactionInvoker.Execute<GetRestrictionHyperlinksRequest, GetRestrictionHyperlinksResponse>(hyperlinksRequest);

                if (string.IsNullOrEmpty(rest.FollowUpLabel))
                {
                    if (hyperlinksResponse.LinkLabelsOut != null && hyperlinksResponse.LinkLabelsOut.Count > 0)
                    {
                        rest.FollowUpLabel = hyperlinksResponse.LinkLabelsOut[0];
                    }
                }
                if (hyperlinksResponse.HyperlinksOut != null && hyperlinksResponse.HyperlinksOut.Count > 0)
                {
                    rest.Hyperlink = hyperlinksResponse.HyperlinksOut[0];
                }
                return rest;
            }
            catch (Exception ex)
            {
                LogDataError("Restriction", restriction.Recordkey, restriction, ex);
                return null;
            }
        }

        /// <summary>
        /// Returns RestrictionCategoryType
        /// </summary>
        /// <param name="restIntgCategory"></param>
        /// <returns>RestrictionCategoryType</returns>
        private RestrictionCategoryType ConvertRestrictionCategoryToType(string restIntgCategory)
        {

            string inputString = restIntgCategory;
            int num;
            var isParsed = int.TryParse(inputString, out num);
            int numToParse = num - 1;
            RestrictionCategoryType outResult;
            var outEnum = Enum.TryParse(numToParse.ToString(), out outResult);
            var isDefined = Enum.IsDefined(typeof(RestrictionCategoryType), outResult);
            if (isDefined)
            {
                return outResult;
            }
            else
            {
                return RestrictionCategoryType.Academic;
            }
        }

        public IEnumerable<HealthConditions> HealthConditions
        {
            get
            {
                var healthConditions = GetOrAddToCache<List<HealthConditions>>("AllHealthConditions",
                        () =>
                        {
                            List<HealthConditions> healthConditionList = new List<HealthConditions>();

                            ApplValcodes healthConditionsValcode = this.DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "HEALTH.CONDITIONS");
                            if (healthConditionsValcode != null)
                            {
                                foreach (ApplValcodesVals applVal in healthConditionsValcode.ValsEntityAssociation)
                                {
                                    healthConditionList.Add(new HealthConditions(applVal.ValInternalCodeAssocMember, applVal.ValExternalRepresentationAssocMember));
                                }
                            }
                            return healthConditionList;
                        }
                    );
                return healthConditions;
            }
        }
        public async Task<IEnumerable<CommencementSite>> GetCommencementSitesAsync()
        {
            return await GetValcodeAsync<CommencementSite>("CORE", "COMMENCEMENT.SITES", C => new CommencementSite(C.ValInternalCodeAssocMember, C.ValExternalRepresentationAssocMember));
        }

        /// <summary>
        /// Task of type of list of relationship types
        /// </summary>
        public async Task<IEnumerable<RelationshipType>> GetRelationshipTypesAsync()
        {
            var relationTypes = await GetOrAddToCacheAsync<List<RelationshipType>>("AllRelationshipTypes",
                async () =>
                {
                    List<RelationshipType> relTypeList = new List<RelationshipType>();
                    var relTypes = await this.DataReader.BulkReadRecordAsync<RelationTypes>("RELATION.TYPES", "");
                    relTypeList.AddRange(relTypes.Select(t => new RelationshipType(t.Recordkey, t.ReltyDesc, t.ReltyInverseRelationType)));

                    return relTypeList;
                }
            );
            return relationTypes;
        }



        /// <summary>
        /// Get a collection of CorrStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CorrStatuses</returns>
        public async Task<IEnumerable<CorrStatus>> GetCorrStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<CorrStatus>("CORE", "CORR.STATUSES",
                (cl, g) => new CorrStatus(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember), cl.ValActionCode1AssocMember), bypassCache: ignoreCache);
        }
        

        /// <summary>
        /// Converts a veteran status code to the corresponding veteran status enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>VeteranStatusCategory enumeration value</returns>
        private VeteranStatusCategory? ConvertVeteranStatusCodeToVeteranStatusCategory(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            switch (code.ToLower())
            {
                case "activeduty":
                    return VeteranStatusCategory.Activeduty;
                case "nonprotectedveteran":
                    return VeteranStatusCategory.Nonprotectedveteran;
                case "nonveteran":
                    return VeteranStatusCategory.Nonveteran;
                case "protectedveteran":
                    return VeteranStatusCategory.Protectedveteran;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts visa type category code to visa type category enumeration value
        /// </summary>
        /// <param name="code">visa type code</param>
        /// <returns>visa type category enumeration value</returns>
        private VisaTypeCategory ConvertVisaTypeCategoryCodeToVisaTypeCategory(string code)
        {
            if (string.IsNullOrEmpty(code))
                return VisaTypeCategory.NonImmigrant;

            if (string.Equals(code, "immigrant", StringComparison.OrdinalIgnoreCase))
                return VisaTypeCategory.Immigrant;

            if (string.Equals(code, "nonimmigrant", StringComparison.OrdinalIgnoreCase))
                return VisaTypeCategory.NonImmigrant;

            return VisaTypeCategory.NonImmigrant;
        }

        /// <summary>
        /// Converts an social media type code to the corresponding social media type enumeration value
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns>Social media type enumeration value</returns>
        private SocialMediaTypeCategory ConvertSocialMediaTypeCategoryCodeToSocialMediaTypeCategory(string code)
        {
            if (string.IsNullOrEmpty(code))
                return SocialMediaTypeCategory.other;

            switch (code.ToLowerInvariant())
            {
                case "windowslive":
                    return SocialMediaTypeCategory.windowsLive;
                case "yahoo":
                    return SocialMediaTypeCategory.yahoo;
                case "skype":
                    return SocialMediaTypeCategory.skype;
                case "qq":
                    return SocialMediaTypeCategory.qq;
                case "hangouts":
                    return SocialMediaTypeCategory.hangouts;
                case "icq":
                    return SocialMediaTypeCategory.icq;
                case "jabber":
                    return SocialMediaTypeCategory.jabber;
                case "facebook":
                    return SocialMediaTypeCategory.facebook;
                case "twitter":
                    return SocialMediaTypeCategory.twitter;
                case "instagram":
                    return SocialMediaTypeCategory.instagram;
                case "tumblr":
                    return SocialMediaTypeCategory.tumblr;
                case "pinterest":
                    return SocialMediaTypeCategory.pinterest;
                case "linkedin":
                    return SocialMediaTypeCategory.linkedin;
                case "foursquare":
                    return SocialMediaTypeCategory.foursquare;
                case "youtube":
                    return SocialMediaTypeCategory.youtube;
                case "blog":
                    return SocialMediaTypeCategory.blog;
                case "website":
                    return SocialMediaTypeCategory.website;
                default:
                    return SocialMediaTypeCategory.other;
            }
        }

        public async Task<GeographicAreaTypeCategory> GetRecordInfoFromGuidGeographicAreaAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Geographic Area ID must be provided.");
            }
            var recordInfo = await this.GetRecordInfoFromGuidAsync(guid);
            if (recordInfo != null)
            {
                switch (recordInfo.Entity)
                {
                    case ("CHAPTERS"):
                        return GeographicAreaTypeCategory.Fundraising;
                    case ("COUNTIES"):
                        return GeographicAreaTypeCategory.Governmental;
                    case ("ZIP.CODE.XLAT"):
                        return GeographicAreaTypeCategory.Postal;
                    default:
                        throw new KeyNotFoundException("Geographic Area ID not found.");
                }
            }
            else
            {
                throw new KeyNotFoundException("Geographic Area ID not found.");
            }
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        public async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await GetInternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }

        /// <summary>
        /// Return host country of the Colleague environment
        /// </summary>
        /// <param name="hostCountry">Host country</param>
        /// <returns>Collegue host country</returns>
        public async Task<string> GetHostCountry(string hostCountry)
        {
            var internationalParameters = await GetInternationalParametersAsync();
            return internationalParameters.HostCountry;
        }


        private LdmDefaults GetLdmDefaults()
        {
            var ldmDefaults = DataReader.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
            if (ldmDefaults == null)
            {
                throw new ConfigurationException("CDM configuration setup not complete.");
            }
            return ldmDefaults;
        }

        #region Places

        /// <summary>
        /// Get all Place domain entities
        /// </summary>
        /// <returns>A collection of Place domain entities</returns>
        public async Task<IEnumerable<Place>> GetPlacesAsync(bool bypassCache)
        {
            var places = await GetOrAddToCacheAsync<List<Place>>("AllPlaces",
             async () =>
             {
                 var placeData = await DataReader.BulkReadRecordAsync<DataContracts.Places>("PLACES", "");
                 var placesList = BuildPlaces(placeData.ToList());
                 return placesList.ToList();
             }
          );
            return places;
        }


        /// <summary>
        ///  Get and return a single place entity by guid without using the cache
        ///  where the PLACES.COUNTRY ne null but PLACES.REGION and PLACES.SUB.REGION equal null.  
        /// </summary>
        /// <returns>Place domain entity</returns>
        public async Task<Place> GetPlaceByGuidAsync(string guid)
        {
            var place = await DataReader.ReadRecordAsync<DataContracts.Places>(new GuidLookup(guid), false);

            if (place == null)
            {
                throw new KeyNotFoundException("Place record not found for guid " + guid + ".");
            }

            if ((!string.IsNullOrEmpty(place.PlacesCountry))
                && (string.IsNullOrEmpty(place.PlacesRegion)) && (string.IsNullOrEmpty(place.PlacesSubRegion)))
            {
                var places = BuildPlaces(new List<Places>() { place });
                if ((places == null) || (!places.Any()))
                {
                    var execption = new RepositoryException();
                    exception.AddError(new RepositoryError("places", "An error ocurred building the places record.")
                    {
                        Id = guid,
                        SourceId = place.Recordkey
                    });
                    throw execption;
                }
                return places.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Build a collection of Place domain entities from a Places datacontract collection
        /// </summary>
        /// <param name="placeData">places data contract</param>
        /// <returns>Collection of Place domain entities</returns>
        private IEnumerable<Place> BuildPlaces(List<DataContracts.Places> placeData)
        {
            List<Place> placeCollection = new List<Place>();
            if (placeData != null)
            {
                foreach (var place in placeData)
                {
                    try
                    {
                        var placeItem = new Place(place.RecordGuid);
                        placeItem.PlacesCountry = place.PlacesCountry;
                        placeItem.PlacesDesc = place.PlacesDesc;
                        placeItem.PlacesRegion = place.PlacesRegion;
                        placeItem.PlacesSubRegion = place.PlacesSubRegion;
                        placeItem.PlacesInactive = place.PlacesInactive;
                        placeCollection.Add(placeItem);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Place", place.Recordkey, placeData, ex);
                    }
                }
            }
            return placeCollection;
        }
        #endregion

        /// <summary>
        /// Get a collection of TaxForms
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of TaxForms</returns>
        public async Task<IEnumerable<TaxForms2>> GetTaxFormsBaseAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<TaxForms2>("CORE", "TAX.FORMS",
                (cl, g) => new TaxForms2(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember), cl.ValActionCode1AssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for TaxForms
        /// </summary>
        /// <param name="code">TaxForms code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetTaxFormsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetTaxFormsBaseAsync(false);
            TaxForms2 codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetTaxFormsBaseAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, TAX.FORMS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, TAX.FORMS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CORE-VALCODES, TAX.FORMS', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of BoxCodes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of BoxCodes</returns>
        public async Task<IEnumerable<Domain.Base.Entities.BoxCodes>> GetAllBoxCodesAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<Data.Base.DataContracts.BoxCodes, Domain.Base.Entities.BoxCodes>("CORE", "BOX.CODES",
                (bxc, g) => new Domain.Base.Entities.BoxCodes(g, bxc.Recordkey, bxc.BxcDesc, bxc.BxcTaxForm), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Box Codes
        /// </summary>
        /// <param name="code">Box Code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetBoxCodesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAllBoxCodesAsync(false);
            Domain.Base.Entities.BoxCodes codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAllBoxCodesAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'BOX.CODES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'BOX.CODES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'BOX.CODES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Gets a collection of case type
        /// By default the results are cached with the name AllCaseType
        /// </summary>
        /// <returns>Collection of Case Type</returns>
        public async Task<IEnumerable<CaseType>> GetCaseTypesAsync()
        {
            return await GetCodeItemAsync<CaseTypes, Domain.Base.Entities.CaseType>("AllCaseTypes", "CASE.TYPES",
             c => new CaseType(c.CtypName, c.CtypDescription, c.Recordkey, c.CtypCategory, c.CtypDefaultPriority, c.CtypActiveFlag.ToUpperInvariant() == "Y", c.CtypAllowCaseContrib.ToUpperInvariant() == "Y",c.CtypAvailCommCodes));

        }

        /// <summary>
        /// Gets a collection of Case priorities
        /// </summary>
        /// <returns>Collection of Case priorities</returns>
        public async Task<IEnumerable<CasePriority>> GetCasePrioritiesAsync()
        {          
            return await GetOrAddToCacheAsync<IEnumerable<Domain.Base.Entities.CasePriority>>("AllCasePriorities", async () =>
            {
                return await GetValcodeAsync<CasePriority>("CORE", "CASE.PRIORITIES",
                e => new CasePriority(e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember));
            });
        }

        /// <summary>
        /// Gets a collection of case categories
        /// By default the results are cached with the name AllCaseCategories
        /// </summary>
        /// <returns>Collection of Case Categories</returns>
        public async Task<IEnumerable<CaseCategory>> GetCaseCategoriesAsync()
        {
            IEnumerable<CaseCategory> CaseCategories = await GetCodeItemAsync<CaseCategories, Domain.Base.Entities.CaseCategory>("AllCaseCategories", "CASE.CATEGORIES",
             c => new CaseCategory(c.CcatName, c.CcatDescription, c.Recordkey, c.CcatTypes, c.CcatClosureReasons, c.CcatWorkerEmailHier));

            if (CaseCategories.Any())
            {
                Dflts defaults = await GetDefaultsAsync();
                foreach (var category in CaseCategories)
                {
                    category.CaseWorkerEmailHierarchy = await GetCaseWorkerEmailAddressHierarchyAsync(category.CaseWorkerEmailHierarchyCode, defaults);
                }
            }
            return CaseCategories;
        }
        
        private async Task<List<string>> GetCaseWorkerEmailAddressHierarchyAsync(string caseWorkerEmailHierarchyCode, Dflts defaults)
        {
            List<string> CaseWorkerEmailHierarchy = new List<string>();
            NameAddrHierarchy nameAddrHierarchy = null;
            if (!string.IsNullOrEmpty(caseWorkerEmailHierarchyCode))
            {
                nameAddrHierarchy = await DataReader.ReadRecordAsync<NameAddrHierarchy>("NAME.ADDR.HIERARCHY", caseWorkerEmailHierarchyCode);
                if (nameAddrHierarchy != null && nameAddrHierarchy.NahEmailHierarchy.Any())
                {
                    CaseWorkerEmailHierarchy.AddRange(nameAddrHierarchy.NahEmailHierarchy);
                }
            }
            else
            {
                if (defaults != null && !string.IsNullOrEmpty(defaults.DfltsEmailType))
                {
                    nameAddrHierarchy = await DataReader.ReadRecordAsync<NameAddrHierarchy>("NAME.ADDR.HIERARCHY", defaults.DfltsEmailType);
                    if (nameAddrHierarchy != null && nameAddrHierarchy.NahEmailHierarchy.Any())
                    {
                        CaseWorkerEmailHierarchy.AddRange(nameAddrHierarchy.NahEmailHierarchy);
                    }
                }
                else
                {
                    nameAddrHierarchy = await DataReader.ReadRecordAsync<NameAddrHierarchy>("NAME.ADDR.HIERARCHY", _preferredHierarchyCode);
                    if (nameAddrHierarchy != null && nameAddrHierarchy.NahEmailHierarchy.Any())
                    {
                        CaseWorkerEmailHierarchy.AddRange(nameAddrHierarchy.NahEmailHierarchy);
                    }
                }
            }

            return CaseWorkerEmailHierarchy;
        }

        /// <summary>
        ///  Gets Configuration
        /// </summary>
        /// <returns></returns>
        private async Task<Data.Base.DataContracts.Dflts> GetDefaultsAsync()
        {
            var defaults = await GetOrAddToCacheAsync<Data.Base.DataContracts.Dflts>("Dflts",
                async () =>
                {
                    var dflts = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Dflts>("CORE.PARMS", "DEFAULTS");
                    if (dflts == null)
                    {
                        throw new ConfigurationException("Default configuration setup not complete.");
                    }
                    return dflts;
                }
            );
            return defaults;
        }

        /// <summary>
        /// Gets a collection of case closure reasons
        /// By default the results are cached with the name AllCaseClosureReasons
        /// </summary>
        /// <returns>Collection of Case closure reasons</returns>
        public async Task<IEnumerable<CaseClosureReason>> GetCaseClosureReasonsAsync()
        {
            return await GetCodeItemAsync<CaseClosureReasons, Domain.Base.Entities.CaseClosureReason>("AllCaseClosureReasons", "CASE.CLOSURE.REASONS",
             c => new CaseClosureReason(c.CcreName, c.CcreDescription, c.Recordkey, c.CcreCategories));

        }
    }
}