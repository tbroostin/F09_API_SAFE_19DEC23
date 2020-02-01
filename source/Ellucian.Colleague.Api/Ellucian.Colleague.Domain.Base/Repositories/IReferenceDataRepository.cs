// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

using System;
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
        /// Gets guid for credential.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<string> GetAcadCredentialsGuidAsync(string code);


        /// <summary>
        /// AcademicDisciplines
        /// </summary>
        Task<IEnumerable<AcademicDiscipline>> GetAcademicDisciplinesAsync(bool ignoreCache);

        /// <summary>
        /// Gets guid for discipline based on code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<string> GetAcadDisciplinesGuidAsync(string code);

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
        /// Get a collection of commerce tax code rates
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of commerce tax code rates</returns>
        Task<IEnumerable<CommerceTaxCodeRate>> GetCommerceTaxCodeRatesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for CommerceTaxCodeRate code
        /// </summary>
        /// <param name="code">CommerceTaxCodeRate code</param>
        /// <returns>Guid</returns>
        Task<string> GetCommerceTaxCodeRateGuidAsync(string code);

        /// <summary>
        /// Get a collection of commerce tax codes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of commerce tax codes</returns>
        Task<IEnumerable<CommerceTaxCode>> GetCommerceTaxCodesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for CommerceTaxCode code
        /// </summary>
        /// <param name="code">CommerceTaxCode code</param>
        /// <returns>Guid</returns>
        Task<string> GetCommerceTaxCodeGuidAsync(string code);

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
        /// Get guid for Departments code
        /// </summary>
        /// <param name="code">Departments code</param>
        /// <returns>Guid</returns>
        Task<string> GetDepartments2GuidAsync(string code);


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
        /// Gets guid for Instructional Platform code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<string> GetInstructionalPlatformsGuidAsync(string code);

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
        /// Get a collection of Language ISO Codes
        /// </summary>
        Task<IEnumerable<LanguageIsoCodes>> GetLanguageIsoCodesAsync(bool ignoreCache);

        /// <summary>
        /// Get a Language ISO Code by Guid
        /// </summary>
        Task<LanguageIsoCodes> GetLanguageIsoCodeByGuidAsync(string guid);

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
        /// Get guid for Locations code
        /// </summary>
        /// <param name="code">Locations code</param>
        /// <returns>Guid</returns>
        Task<string> GetLocationsGuidAsync(string code);


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
        /// Get guid for Other ccds code
        /// </summary>
        /// <param name="code">Other Ccds code</param>
        /// <returns>Guid</returns>
        Task<string> GetOtherCcdsGuidAsync(string code);

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
        /// Get guid for Other Degrees code
        /// </summary>
        /// <param name="code">Other Degrees code</param>
        /// <returns>Guid</returns>
        Task<string> GetOtherDegreeGuidAsync(string code);


        /// <summary>
        /// Get a collection of majors
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of majors</returns>
        Task<IEnumerable<OtherMajor>> GetOtherMajorsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Other Majors code
        /// </summary>
        /// <param name="code">Other Majors code</param>
        /// <returns>Guid</returns>
        Task<string> GetOtherMajorsGuidAsync(string code);

        /// <summary>
        /// Get a collection of minors
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of minors</returns>
        Task<IEnumerable<OtherMinor>> GetOtherMinorsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Other Minors code
        /// </summary>
        /// <param name="code">Other Minors code</param>
        /// <returns>Guid</returns>
        Task<string> GetOtherMinorsGuidAsync(string code);

        /// <summary>
        /// Get a collection of Other honors
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<OtherHonor>> GetOtherHonorsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Other Honors code
        /// </summary>
        /// <param name="code">Other Honors code</param>
        /// <returns>Guid</returns>
        Task<string> GetOtherHonorsGuidAsync(string code);

        /// <summary>
        /// Get a collection of specializations
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns>Collection of specializations</returns>
        Task<IEnumerable<OtherSpecial>> GetOtherSpecialsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Other specializations code
        /// </summary>
        /// <param name="code">Other specializations code</param>
        /// <returns>Guid</returns>
        Task<string> GetOtherSpecialsGuidAsync(string code);

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
        /// Get a collection of IntgPersonEmerPhoneTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgPersonEmerPhoneTypes</returns>
        Task<IEnumerable<IntgPersonEmerPhoneTypes>> GetIntgPersonEmerPhoneTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of IntgPersonEmerTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgPersonEmerTypes</returns>
        Task<IEnumerable<IntgPersonEmerTypes>> GetIntgPersonEmerTypesAsync(bool ignoreCache);

        
        /// <summary>
        /// Get a collection of Person Filters
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Person Filters</returns>
        Task<IEnumerable<PersonFilter>> GetPersonFiltersAsync(bool ignoreCache);

        /// <summary>
        /// Get a Person Filter by GUID
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Person Filter</returns>
        Task<PersonFilter> GetPersonFilterByGuidAsync(string guid);

        /// <summary>
        /// Get a Person Filter by GUID
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>string list of Colleague person IDs</returns>
        Task<string[]> GetPersonIdsByPersonFilterGuidAsync(string guid);

        /// <summary>
        /// Get a collection of PersonOriginCodes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of PersonOriginCodes</returns>
        Task<IEnumerable<PersonOriginCodes>> GetPersonOriginCodesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Person Origin code
        /// </summary>
        /// <param name="code">Person Origin code</param>
        /// <returns>Guid</returns>
        Task<string> GetPersonOriginCodesGuidAsync(string code);

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
        /// Get guid for GetRelationshipStatuses
        /// </summary>
        /// <param name="code">RelationshipStatuses code</param>
        /// <returns>Guid</returns>
        Task<string> GetRelationshipStatusesGuidAsync(string code);

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
        /// Get guid for RelationTypes3
        /// </summary>
        /// <param name="code">RelationTypes3 code</param>
        /// <returns>Tuple of relationshuip type Guid & reciprocal_type guid</returns>
        Task<Tuple<string, string>> GetRelationTypes3GuidAsync(string code);

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
        /// Get guid for Schools code
        /// </summary>
        /// <param name="code">Schools code</param>
        /// <returns>Guid</returns>
        Task<string> GetSchoolsGuidAsync(string code);

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
        /// Get a collection of all Place domain entities  
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of Place domain entities</returns>
        Task<IEnumerable<Place>> GetPlacesAsync(bool bypassCache);

        /// <summary>
        ///  Get and return a single place entity by guid without using the cache
        ///  where the PLACES.COUNTRY ne null but PLACES.REGION and PLACES.SUB.REGION equal null.  
        /// </summary>
        /// <returns>Place domain entity</returns>
        Task<Place> GetPlaceByGuidAsync(string guid);

        /// <summary>
        /// Get a collection of AlternativeCredentialTypes.
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AltIdTypes</returns>
        Task<IEnumerable<AltIdTypes>> GetAlternateIdTypesAsync(bool ignoreCache);

        Task<IEnumerable<Domain.Base.Entities.BoxCodes>> GetAllBoxCodesAsync();
    }
}