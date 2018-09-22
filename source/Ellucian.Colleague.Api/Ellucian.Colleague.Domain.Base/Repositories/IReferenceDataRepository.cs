// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Provides read-only access to the fundamental data necessary for the Student Self Service system to function.
    /// </summary>
    public interface IReferenceDataRepository
    {
        /// <summary>
        /// Get the GuidLookupResult for a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>GuidLookupResult or KeyNotFoundException if supplied Guid was not found</returns>
        Task<GuidLookupResult> GetGuidLookupResultFromGuidAsync(string guid);

        /// <summary>
        /// AcadCredentials
        /// </summary>
        Task<IEnumerable<AcadCredential>> GetAcadCredentialsAsync(bool ignoreCache);


        /// <summary>
        /// AcademicDisciplines
        /// </summary>
        Task<IEnumerable<AcademicDiscipline>> GetAcademicDisciplinesAsync(bool ignoreCache);

        /// <summary>
        /// AcademicDisciplines OtherMajors
        /// </summary>
        Task<AcademicDiscipline> GetAcademicDisciplinesMajorAsync(string id, bool ignoreCache);

        /// <summary>
        /// AcademicDisciplines OtherMinors
        /// </summary>
        Task<AcademicDiscipline> GetAcademicDisciplinesMinorAsync(string id, bool ignoreCache);

        /// <summary>
        /// AcademicDisciplines OtherSpecials
        /// </summary>
        Task<AcademicDiscipline> GetAcademicDisciplinesSpecialAsync(string id, bool ignoreCache);

        /// <summary>
        /// AcademicDisciplines helper method
        /// </summary>
        Task<AcademicDisciplineType> GetRecordInfoFromGuidAcademicDisciplineAsync(string guid);

        /// <summary>
        /// AcademicDisciplines helper method
        /// </summary>
        Task<GuidLookupResult> GetRecordInfoFromGuidReferenceDataRepoAsync(string guid);

        /// <summary>
        /// Address Relation Types
        /// </summary>
        Task<IEnumerable<AddressChangeSource>> GetAddressChangeSourcesAsync(bool ignoreCache);

        /// <summary>
        /// Address Relation Types
        /// </summary>
        IEnumerable<AddressRelationType> AddressRelationTypes { get; }

        ///// <summary>
        ///// Get a collection of Address Types
        ///// </summary>
        ///// <param name="ignoreCache">Bypass cache flag</param>
        ///// <returns>Collection of Address Types</returns>
        //Task<IEnumerable<AddressTypeItem>> GetAddressTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Address Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Address Types</returns>
        Task<IEnumerable<AddressType2>> GetAddressTypes2Async(bool ignoreCache);

        /// <summary>
        /// Building
        /// </summary>
        Task<IEnumerable<Building>> BuildingsAsync();

        /// <summary>
        /// Get a collection of buildings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of buildings</returns>
        Task<IEnumerable<Building>> GetBuildingsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of buildings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of buildings</returns>
        Task<IEnumerable<Building>> GetBuildings2Async(bool ignoreCache);

        /// <summary>
        /// BuildingTypes
        /// </summary>
        IEnumerable<BuildingType> BuildingTypes { get; }

        /// <summary>
        /// Get a collection of CcdType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CcdType</returns>
        Task<IEnumerable<Domain.Base.Entities.CcdType>> GetCcdTypeAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of chapters
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of chapters</returns>
        Task<IEnumerable<Chapter>> GetChaptersAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of citizenship statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of citizenship statuses</returns>
        Task<IEnumerable<CitizenshipStatus>> GetCitizenshipStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Citizen Types (Alien Statuses)
        /// </summary>
        IEnumerable<CitizenType> CitizenTypes { get; }

        /// <summary>
        /// Get a collection of commerce tax codes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of commerce tax codes</returns>
        Task<IEnumerable<CommerceTaxCode>> GetCommerceTaxCodesAsync(bool ignoreCache);

        /// <summary>
        /// Communication codes
        /// </summary>
        IEnumerable<CommunicationCode> CommunicationCodes { get; }

        /// <summary>
        /// Get a collection of admission application supporting item types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of admission application supporting item types</returns>
        Task<IEnumerable<CommunicationCode>> GetAdmissionApplicationSupportingItemTypesAsync(bool ignoreCache);

        /// <summary>
        /// Counties
        /// </summary>
        IEnumerable<County> Counties { get; }

        /// <summary>
        /// Get a collection of counties
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of counties</returns>
        Task<IEnumerable<County>> GetCountiesAsync(bool ignoreCache);

        
        /// <summary>
        /// Countries
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        Task<IEnumerable<Country>> GetCountryCodesAsync(bool ignoreCache);

        /// <summary>
        /// Degree Types
        /// </summary>
        IEnumerable<DegreeType> DegreeTypes { get; }

        /// <summary>
        /// Denominations
        /// </summary>
        IEnumerable<Denomination> Denominations { get; }

        /// <summary>
        /// Denominations
        /// </summary>
        Task<IEnumerable<Denomination>> DenominationsAsync();

        /// <summary>
        /// Get a collection of denominations
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of denominations</returns>
        Task<IEnumerable<Denomination>> GetDenominationsAsync(bool bypassCache);

        /// <summary>
        /// Academic departments.
        /// </summary>
        Task<IEnumerable<Department>> DepartmentsAsync();

        /// <summary>
        /// Get a collection of departments
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of departments</returns>
        Task<IEnumerable<Department>> GetDepartmentsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of departments
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of departments</returns>
        Task<IEnumerable<Department>> GetDepartments2Async(bool ignoreCache);


        /// <summary>
        /// Get a single department by guid
        /// </summary>
        /// <param name="guid">Guid of department to return</param>
        /// <returns>Department</returns>
        Task<Department> GetDepartmentByGuidAsync(string guid);

        /// <summary>
        /// Get a single school  by guid
        /// </summary>
        /// <param name="guid">Guid of school to return</param>
        /// <returns>School</returns>
        Task<School> GetSchoolByGuidAsync(string guid);

        /// <summary>
        /// Get a single division by guid
        /// </summary>
        /// <param name="guid">Guid of department to return</param>
        /// <returns>Division </returns>
        Task<Division> GetDivisionByGuidAsync(string guid);
        /*
        /// <summary>
        /// Get a collection of departments
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of departments</returns>
        Task<IEnumerable<Department>> GetDepartments2Async(bool ignoreCache);
        */

        /// <summary>
        /// Disability Types (Health Conditions)
        /// </summary>
        IEnumerable<DisabilityType> DisabilityTypes { get; }

        /// <summary>
        /// Get a collection of divisions
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of divisions</returns>
        Task<IEnumerable<Division>> GetDivisionsAsync(bool ignoreCache);


        /// <summary>
        /// Get a collection of email types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of email types</returns>
        Task<IEnumerable<EmailType>> GetEmailTypesAsync(bool ignoreCache);

        /// <summary>
        /// Ethnicities
        /// </summary>
        Task<IEnumerable<Ethnicity>> EthnicitiesAsync();

        /// <summary>
        /// Get a collection of ethnicities
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ethnicities</returns>
        Task<IEnumerable<Ethnicity>> GetEthnicitiesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of ExternalEmploymentStatus
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ExternalEmploymentStatus</returns>
        Task<IEnumerable<ExternalEmploymentStatus>> GetExternalEmploymentStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of geographic area types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of geographic area types</returns>
        Task<IEnumerable<GeographicAreaType>> GetGeographicAreaTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Grade Change Reason
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<GradeChangeReason>> GetGradeChangeReasonAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of identity document types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of identity document types</returns>
        Task<IEnumerable<IdentityDocumentType>> GetIdentityDocumentTypesAsync(bool ignoreCache);

        /// <summary>
        /// Institution Types
        /// </summary>
        IEnumerable<InstitutionType> InstitutionTypes { get; }

        /// <summary>
        /// Gets a collection of Instructional Platforms
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns>collection of Instructional Platforms</returns>
        Task<IEnumerable<InstructionalPlatform>> GetInstructionalPlatformsAsync(bool ignoreCache);

        /// <summary>
        /// Interests
        /// </summary>
        IEnumerable<Interest> Interests { get; }

        /// <summary>
        /// Get a collection of interests
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of interests</returns>
        Task<IEnumerable<Interest>> GetInterestsAsync(bool ignoreCache);


        /// <summary>
        /// Get a collection of interest types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of interest types</returns>
        Task<IEnumerable<InterestType>> GetInterestTypesAsync(bool ignoreCache);

        /// <summary>
        /// Languages
        /// </summary>
        IEnumerable<Language> Languages { get; }

        /// <summary>
        /// Campus locations.
        /// </summary>
        IEnumerable<Location> Locations { get; }

        /// <summary>
        /// Get a collection of locations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of locations</returns>
        IEnumerable<Location> GetLocations(bool ignoreCache);

        /// <summary>
        /// Get a collection of locations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of locations</returns>
        Task<IEnumerable<Location>> GetLocationsAsync(bool ignoreCache);



        /// <summary>
        /// Get a collection of Location Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Location Types</returns>
        Task<IEnumerable<LocationTypeItem>> GetLocationTypesAsync(bool ignoreCache);

        /// <summary>
        /// Marital Statuses
        /// </summary>
        Task<IEnumerable<MaritalStatus>> MaritalStatusesAsync();

        /// <summary>
        /// Office Codes
        /// </summary>
        IEnumerable<OfficeCode> OfficeCodes { get; }


        /// <summary>
        /// Get a collection of CCDs
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CCDs</returns>
        IEnumerable<OtherCcd> GetOtherCcds(bool ignoreCache);

        /// <summary>
        /// Get a collection of CCDs
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CCDs</returns>
        Task<IEnumerable<OtherCcd>> GetOtherCcdsAsync(bool ignoreCache);


        /// <summary>
        /// Get a collection of degrees
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of degrees</returns>
        IEnumerable<OtherDegree> GetOtherDegrees(bool ignoreCache);

        /// <summary>
        /// Get a collection of degrees
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of degrees</returns>
        Task<IEnumerable<OtherDegree>> GetOtherDegreesAsync(bool ignoreCache);



        /// <summary>
        /// Get a collection of majors
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of majors</returns>
        Task<IEnumerable<OtherMajor>> GetOtherMajorsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of minors
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of minors</returns>
        Task<IEnumerable<OtherMinor>> GetOtherMinorsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Other honors
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<OtherHonor>> GetOtherHonorsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of specializations
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns>Collection of specializations</returns>
        Task<IEnumerable<OtherSpecial>> GetOtherSpecialsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Marital Statuses
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// /// <returns>Collection of marital statuses</returns>
        Task<IEnumerable<MaritalStatus>> GetMaritalStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of MilStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of MilStatuses</returns>
        Task<IEnumerable<MilStatuses>> GetMilStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Personal Relationship Status
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Personal Relationship Status</returns>
        Task<IEnumerable<PersonalRelationshipStatus>> GetPersonalRelationshipStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Person Filters
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Person Filters</returns>
        Task<IEnumerable<PersonFilter>> GetPersonFiltersAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of personal pronoun types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of personal pronoun types</returns>
        Task<IEnumerable<PersonalPronounType>> GetPersonalPronounTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of gender identity types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of gender identity types</returns>
        Task<IEnumerable<GenderIdentityType>> GetGenderIdentityTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of person name types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of person name types</returns>
        Task<IEnumerable<PersonNameTypeItem>> GetPersonNameTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of phone types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of phone types</returns>
        Task<IEnumerable<PhoneType>> GetPhoneTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a non-guid collection of phone types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of phone types</returns>
        Task<IEnumerable<PhoneType>> GetPhoneTypesBaseAsync(bool ignoreCache);

        /// <summary>
        /// Get project types.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProjectType>> GetProjectTypesAsync();

        /// <summary>
        /// Get a collection of Positions
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Positions</returns>
        Task<IEnumerable<Positions>> GetPositionsAsync(bool ignoreCache);

        /// <summary>
        /// Prefixes
        /// </summary>
        IEnumerable<Prefix> Prefixes { get; }

        /// <summary>
        /// Prefixes
        /// </summary>
        Task<IEnumerable<Prefix>> GetPrefixesAsync();


        /// <summary>
        /// Get a collection of privacy statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of privacy statuses</returns>
        Task<IEnumerable<PrivacyStatus>> GetPrivacyStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a dictionary of privacy status codes and associated messages
        /// </summary>
        /// <returns>Dictionary of privacy status codes and messages</returns>
        Task<IDictionary<string, string>> GetPrivacyMessagesAsync();

        /// <summary>
        /// Prospect Sources
        /// </summary>
        IEnumerable<ProspectSource> ProspectSources { get; }

        /// <summary>
        /// Races
        /// </summary>
        Task<IEnumerable<Race>> RacesAsync();

        /// <summary>
        /// Get a collection of races
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of races</returns>
        Task<IEnumerable<Race>> GetRacesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of RelationshipStatus
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RelationshipStatus</returns>
        Task<IEnumerable<RelationshipStatus>> GetRelationshipStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Relation Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Relation Types</returns>
        Task<IEnumerable<RelationType>> GetRelationTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Relation Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Relation Types</returns>
        Task<IEnumerable<RelationType>> GetRelationTypes2Async(bool ignoreCache);

        
        /// <summary>
        /// Get a collection of Relation Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Relation Types</returns>
        Task<IEnumerable<RelationType>> GetRelationTypes3Async(bool ignoreCache);

        /// <summary>
        /// Get a collection of remark codes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RemarkCodes</returns>
        Task<IEnumerable<RemarkCode>> GetRemarkCodesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of remark types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RemarkTypes</returns>
        Task<IEnumerable<RemarkType>> GetRemarkTypesAsync(bool ignoreCache);

        /// <summary>
        /// Restrictions
        /// </summary>
        Task<IEnumerable<Restriction>> RestrictionsAsync();

        /// <summary>
        /// Get a collection of restrictions
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of restrictions</returns>
        Task<IEnumerable<Restriction>> GetRestrictionsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of restrictions with Category
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Restriction>> GetRestrictionsWithCategoryAsync(bool ignoreCache);
        /// <summary>
        /// Get a collection of Room Types
        /// </summary>
        Task<IEnumerable<RoomTypes>> RoomTypesAsync();

        /// <summary>
        /// Get a collection of room wings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RoomWing Entities</returns>
        Task<IEnumerable<RoomWing>> GetRoomWingsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Room Types
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<RoomTypes>> GetRoomTypesAsync(bool ignoreCache);

        /// <summary>
        /// Schedule Repeats
        /// </summary>
        IEnumerable<ScheduleRepeat> ScheduleRepeats { get; }

        /// <summary>
        /// Get a collection of schools
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of schools</returns>
        Task<IEnumerable<School>> GetSchoolsAsync(bool ignoreCache);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetHomeInstitutionIdList();

        /// <summary>
        /// Get a collection of social media types
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<SocialMediaType>> GetSocialMediaTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of source contexts
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<SourceContext>> GetSourceContextsAsync(bool ignoreCache);

        /// <summary>
        /// States
        /// </summary>
        Task<IEnumerable<State>> GetStateCodesAsync();

        /// <summary>
        /// States
        /// </summary>
        Task<IEnumerable<State>> GetStateCodesAsync(bool ignoreCache = false);

        ///<summary>
        /// Suffixes
        /// </summary>
        IEnumerable<Suffix> Suffixes { get; }

        ///<summary>
        /// Suffixes
        /// </summary>
        Task<IEnumerable<Suffix>> GetSuffixesAsync();

        /// <summary>
        /// Visa Types
        /// </summary>
        IEnumerable<VisaType> VisaTypes { get; }

        /// <summary>
        /// Get a collection of visa types with category
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<VisaTypeGuidItem>> GetVisaTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of zipcode xlat
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of zipcode xlats</returns>
        Task<IEnumerable<ZipcodeXlat>> GetZipCodeXlatAsync(bool ignoreCache);

        /// <summary>
        /// Divisions.
        /// </summary>
        IEnumerable<Division> Divisions { get; }

        /// <summary>
        /// Schedule Repeat Frequency.
        /// </summary>
        IEnumerable<FrequencyCode> FrequencyCodes { get; }

        /// <summary>
        /// Schools Codes
        /// </summary>
        IEnumerable<School> Schools { get; }

        /// <summary>
        /// Special Needs Validation Table
        /// </summary>
        IEnumerable<SpecialNeed> SpecialNeeds { get; }

        /// <summary>
        /// List of health conditions
        /// </summary>
        IEnumerable<HealthConditions> HealthConditions { get; }

        /// <summary>
        /// Task of type of list of commencement sites.
        /// </summary>
        Task<IEnumerable<CommencementSite>> GetCommencementSitesAsync();

        /// <summary>
        /// Task of type of list of relationship types
        /// </summary>
        Task<IEnumerable<RelationshipType>> GetRelationshipTypesAsync();

        Task<GeographicAreaTypeCategory> GetRecordInfoFromGuidGeographicAreaAsync(string guid);

        Task<IEnumerable<PersonRelationType>> GetPersonRelationTypesAsync(bool ignoreCache);

        /// <summary>
        /// Gets all room characteristics
        /// </summary>
        /// <param name="bypassCache">bypassCache</param>
        /// <returns>IEnumerable<RoomCharacteristic></returns>
        Task<IEnumerable<RoomCharacteristic>> GetRoomCharacteristicsAsync(bool bypassCache);

        /// <summary>
        /// Get Unidata formatted date for filters.
        /// </summary>   
        /// <param name="date">date </param>
        /// <returns>date in undiata format</returns>
        Task<string> GetUnidataFormattedDate(string date);

        /// <summary>
        /// Gets all Vocations
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Vocation>> GetVocationsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of CorrStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CorrStatuses</returns>
        Task<IEnumerable<CorrStatus>> GetCorrStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get host country for Colleague environment.
        /// </summary>   
        /// <param name="hostCountry">hostCountry</param>
        /// <returns>Colleague host country</returns>
        Task<string> GetHostCountry(string hostCountry);

        /// <summary>
        /// Gets items condition.
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<ItemCondition>> GetItemConditionsAsync(bool ignoreCache);

        /// <summary>
        /// Gets acquisition methods.
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<AcquisitionMethod>> GetAcquisitionMethodsAsync(bool ignoreCache);

        /// <summary>
        /// Get a Place 
        /// </summary>
        /// <returns>A collection of Place entities</returns>
        Task<IEnumerable<Place>> GetPlacesAsync(bool bypassCache);
    }
}