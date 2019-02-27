// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class RecruiterRepositoryTests
    {
        private Mock<ICacheProvider> cacheProviderMock;
        private ICacheProvider cacheProvider;
        private Mock<IColleagueTransactionInvoker> transactionInvokerMock;
        private Mock<IColleagueTransactionFactory> transactionFactoryMock;
        private IColleagueTransactionFactory transactionFactory;
        private Mock<ILogger> loggerMock;
        private ILogger logger;

        private RecruiterRepository repository;

        [TestInitialize]
        public void RecruiterRepositoryTests_Initialize()
        {
            cacheProviderMock = new Mock<ICacheProvider>();
            cacheProvider = cacheProviderMock.Object;
            transactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            transactionFactoryMock = new Mock<IColleagueTransactionFactory>();
            transactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvokerMock.Object);
            transactionFactory = transactionFactoryMock.Object;
            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            repository = new RecruiterRepository(cacheProvider, transactionFactory, logger);
        }

        [TestClass]
        public class RecruiterRepository_UpdateApplicationAsync_Tests : RecruiterRepositoryTests
        {
            private Application application;
            private ApplStatusImportResponse response;

            [TestInitialize]
            public void RecruiterRepository_UpdateApplicationAsync_Tests_Initialize()
            {
                base.RecruiterRepositoryTests_Initialize();
                application = new Application()
                {
                    AcademicProgram = "AcademicProgram",
                    Activity = "Activity",
                    AddressLines1 = "AddressLines1",
                    AddressLines2 = "AddressLines2",
                    AddressLines3 = "AddressLines3",
                    AdmitType = "AdmitType",
                    ApplicantCounty = "ApplicantCounty",
                    ApplicantUser1 = "ApplicantUser1",
                    ApplicantUser10 = "ApplicantUser10",
                    ApplicantUser2 = "ApplicantUser2",
                    ApplicantUser3 = "ApplicantUser3",
                    ApplicantUser4 = "ApplicantUser4",
                    ApplicantUser5 = "ApplicantUser5",
                    ApplicantUser6 = "ApplicantUser6",
                    ApplicantUser7 = "ApplicantUser7",
                    ApplicantUser8 = "ApplicantUser8",
                    ApplicantUser9 = "ApplicantUser9",
                    ApplicationStatus = "ApplicationStatus",
                    ApplicationStatusDate = DateTime.Today,
                    ApplicationUser1 = "ApplicationUser1",
                    ApplicationUser10 = "ApplicationUser10",
                    ApplicationUser2 = "ApplicationUser2",
                    ApplicationUser3 = "ApplicationUser3",
                    ApplicationUser4 = "ApplicationUser4",
                    ApplicationUser5 = "ApplicationUser5",
                    ApplicationUser6 = "ApplicationUser6",
                    ApplicationUser7 = "ApplicationUser7",
                    ApplicationUser8 = "ApplicationUser8",
                    ApplicationUser9 = "ApplicationUser9",
                    Attention = "Attention",
                    BirthCity = "BirthCity",
                    BirthCountry = "BirthCountry",
                    BirthDate = DateTime.Today.AddYears(-18),
                    BirthDateSibling1 = DateTime.Today.AddYears(-21),
                    BirthDateSibling2 = DateTime.Today.AddYears(-20),
                    BirthDateSibling3 = DateTime.Today.AddYears(-19),
                    BirthState = "BirthState",
                    CareerGoal = "CareerGoal",
                    CellPhone = "123-456-7890",
                    Citizenship = "Citizenship",
                    CitizenshipStatus = "CitizenshipStatus",
                    City = "City",
                    CollegeAttendFromMonths = "CollegeAttendFromMonths",
                    CollegeAttendFromYears = "CollegeAttendFromYears",
                    CollegeAttendToMonths = "CollegeAttendToMonths",
                    CollegeAttendToYears = "CollegeAttendToYears",
                    CollegeCeebs = "CollegeCeebs",
                    CollegeDegreeDates = "CollegeDegreeDates",
                    CollegeDegrees = "CollegeDegrees",
                    CollegeGraduated = "CollegeGraduated",
                    CollegeHoursEarned = "CollegeHoursEarned",
                    CollegeNames = "CollegeNames",
                    CollegeNonCeebInfo = "CollegeNonCeebInfo",
                    CollegeTranscriptClassPercentage = "CollegeTranscriptClassPercentage",
                    CollegeTranscriptClassRank = "CollegeTranscriptClassRank",
                    CollegeTranscriptClassSize = "CollegeTranscriptClassSize",
                    CollegeTranscriptGpa = "CollegeTranscriptGpa",
                    CollegeTranscriptLocation = "CollegeTranscriptLocation",
                    CollegeTranscriptStored = "CollegeTranscriptStored",
                    Comments = "Comments",
                    Country = "Country",
                    CountryEntryDate = DateTime.Today.AddYears(50),
                    CourseLoad = "CourseLoad",
                    CrmApplicationId = new Guid().ToString(),
                    CrmProspectId = new Guid().ToString(),
                    CustomFields = new List<CustomField>()
                    {
                        null, // Nulls should be handled gracefully
                        new CustomField()
                        {
                            AttributeSchema = "AttributeSchema",
                            EntitySchema = "EntitySchema",
                            Value = "Value"
                        }
                    },
                    CustomFieldsXML = "CustomFieldsXML",
                    DecisionFactor1 = "DecisionFactor1",
                    DecisionFactor2 = "DecisionFactor2",
                    DecisionPlan = "DecisionPlan",
                    Denomination = "Denomination",
                    Disability = "Disability",
                    EducationalGoal = "EducationalGoal",
                    EmailAddress = "applicant@email.com",
                    EmergencyFirstName = "EmergencyFirstName",
                    EmergencyLastName = "EmergencyLastName",
                    EmergencyMiddleName = "EmergencyMiddleName",
                    EmergencyPhone = "234-567-8901",
                    EmergencyPrefix = "EmergencyPrefix",
                    EmergencySuffix = "EmergencySuffix",
                    ErpProspectId = "0001234",
                    Ethnicity = "Ethnicity",
                    FaPlan = "FaPlan",
                    FirstName = "ApplicantFirst",
                    FirstNameSibling1 = "FirstNameSibling1",
                    FirstNameSibling2 = "FirstNameSibling2",
                    FirstNameSibling3 = "FirstNameSibling3",
                    ForeignRegistrationId = "ForeignRegistrationId",
                    FutureActivity = "FutureActivity",
                    Gender = "Gender",
                    GuardianAddress1 = "GuardianAddress1",
                    GuardianAddress2 = "GuardianAddress2",
                    GuardianAddress3 = "GuardianAddress3",
                    GuardianBirthCountry = "GuardianBirthCountry",
                    GuardianBirthDate = DateTime.Today.AddYears(-45),
                    GuardianCity = "GuardianCity",
                    GuardianCountry = "GuardianCountry",
                    GuardianEmailAddress = "guardian@email.com",
                    GuardianFirstName = "GuardianFirst",
                    GuardianLastName = "GuardianLast",
                    GuardianMiddleName = "GuardianMiddle",
                    GuardianPhone = "345-678-9012",
                    GuardianPrefix = "GuardianPrefix",
                    GuardianRelationType = "GuardianRelationType",
                    GuardianSameAddress = "GuardianSameAddress",
                    GuardianState = "GuardianState",
                    GuardianSuffix = "GuardianSuffix",
                    GuardianZip = "12345",
                    HighSchoolAttendFromMonths = "HighSchoolAttendFromMonths",
                    HighSchoolAttendFromYears = "HighSchoolAttendFromYears",
                    HighSchoolAttendToMonths = "HighSchoolAttendToMonths",
                    HighSchoolAttendToYears = "HighSchoolAttendToYears",
                    HighSchoolCeebs = "HighSchoolCeebs",
                    HighSchoolGraduated = "HighSchoolGraduated",
                    HighSchoolNames = "HighSchoolNames",
                    HighSchoolNonCeebInfo = "HighSchoolNonCeebInfo",
                    HighSchoolTranscriptClassPercentage = "HighSchoolTranscriptClassPercentage",
                    HighSchoolTranscriptClassRank = "HighSchoolTranscriptClassRank",
                    HighSchoolTranscriptClassSize = "HighSchoolTranscriptClassSize",
                    HighSchoolTranscriptGpa = "HighSchoolTranscriptGpa",
                    HighSchoolTranscriptLocation = "HighSchoolTranscriptLocation",
                    HighSchoolTranscriptStored = "HighSchoolTranscriptStored",
                    HomePhone = "456-789-0123",
                    HoursWeekActivity = "HoursWeekActivity",
                    HousingPlan = "HousingPlan",
                    ImAddress = "ImAddress",
                    ImProvider = "ImProvider",
                    LastName = "ApplicantLast",
                    LastNameSibling1 = "LastNameSibling1",
                    LastNameSibling2 = "LastNameSibling2",
                    LastNameSibling3 = "LastNameSibling3",
                    Location = "Location",
                    MaritalStatus = "MartialStatus",
                    MiddleName = "ApplicantMiddle",
                    MiddleNameSibling1 = "MiddleNameSibling1",
                    MiddleNameSibling2 = "MiddleNameSibling2",
                    MiddleNameSibling3 = "MiddleNameSibling3",
                    Misc1 = "Misc1",
                    Misc2 = "Misc2",
                    Misc3 = "Misc3",
                    Misc4 = "Misc4",
                    Misc5 = "Misc5",
                    Nickname = "ApplicantNickname",
                    OtherFirstName = "ApplicantOtherFirst",
                    OtherLastName = "ApplicantOtherLast",
                    Parent1Address1 = "Parent1Address1",
                    Parent1Address2 = "Parent1Address2",
                    Parent1Address3 = "Parent1Address3",
                    Parent1BirthCountry = "Parent1BirthCountry",
                    Parent1BirthDate = DateTime.Today.AddYears(-50),
                    Parent1City = "Parent1City",
                    Parent1Country = "Parent1Country",
                    Parent1EmailAddress = "parent1@email.com",
                    Parent1FirstName = "Parent1First",
                    Parent1LastName = "Parent1Last",
                    Parent1Living = "Parent1Living",
                    Parent1MiddleName = "Parent1Middle",
                    Parent1Phone = "567-890-1234",
                    Parent1Prefix = "Parent1Prefix",
                    Parent1RelationType = "Parent1RelationType",
                    Parent1SameAddress = "Parent1SameAddress",
                    Parent1State = "Parent1State",
                    Parent1Suffix = "Parent1Suffix",
                    Parent1Zip = "Parent1Zip",
                    Parent2Address1 = "Parent2Address1",
                    Parent2Address2 = "Parent2Address2",
                    Parent2Address3 = "Parent2Address3",
                    Parent2BirthCountry = "Parent2BirthCountry",
                    Parent2BirthDate = DateTime.Today.AddYears(-50),
                    Parent2City = "Parent2City",
                    Parent2Country = "Parent2Country",
                    Parent2EmailAddress = "Parent2@email.com",
                    Parent2FirstName = "Parent2First",
                    Parent2LastName = "Parent2Last",
                    Parent2Living = "Parent2Living",
                    Parent2MiddleName = "Parent2Middle",
                    Parent2Phone = "567-890-1234",
                    Parent2Prefix = "Parent2Prefix",
                    Parent2RelationType = "Parent2RelationType",
                    Parent2SameAddress = "Parent2SameAddress",
                    Parent2State = "Parent2State",
                    Parent2Suffix = "Parent2Suffix",
                    Parent2Zip = "Parent2Zip",
                    ParentMaritalStatus = "ParentMaritalStatus",
                    Part10Activity = "Part10Activity",
                    Part11Activity = "Part11Activity",
                    Part12Activity = "Part12Activity",
                    Part9Activity = "Part9Activity",
                    PartPgActivity = "PartPgActivity",
                    Prefix = "ApplicantPrefix",
                    PrefixSibling1 = "PrefixSibling1",
                    PrefixSibling2 = "PrefixSibling2",
                    PrefixSibling3 = "PrefixSibling3",
                    PrimaryLanguage = "PrimaryLanguage",
                    ProspectSource = "ProspectSource",
                    Race1 = "Race1",
                    Race2 = "Race2",
                    Race3 = "Race3",
                    Race4 = "Race4",
                    Race5 = "Race5",
                    RecruiterOrganizationId = "RecruiterOrganizationId",
                    RecruiterOrganizationName = "RecruiterOrganizationName",
                    RelationTypeSibling1 = "RelationTypeSibling1",
                    RelationTypeSibling2 = "RelationTypeSibling2",
                    RelationTypeSibling3 = "RelationTypeSibling3",
                    ResidencyStatus = "ResidencyStatus",
                    Sin = "Sin",
                    Ssn = "Ssn",
                    StartTerm = "StartTerm",
                    State = "State",
                    SubmittedDate = DateTime.Today,
                    Suffix = "Suffix",
                    SuffixSibling1 = "SuffixSibling1",
                    SuffixSibling2 = "SuffixSibling2",
                    SuffixSibling3 = "SuffixSibling3",
                    TempAddressLines1 = "TempAddressLines1",
                    TempAddressLines2 = "TempAddressLines2",
                    TempAddressLines3 = "TempAddressLines3",
                    TempAttention = "TempAttention",
                    TempCity = "TempCity",
                    TempCountry = "TempCountry",
                    TempEndDate = DateTime.Today.AddDays(30),
                    TempPhone = "678-901-2345",
                    TempStartDate = DateTime.Today.AddDays(-30),
                    TempState = "TempState",
                    TempZip = "TempZip",
                    Veteran = "Veteran",
                    VisaType = "VisaType",
                    WeeksYearActivity = "WeeksYearActivity",
                    Zip = "Zip"
                };
                response = new ApplStatusImportResponse()
                {
                    _AppServerVersion = 1
                };

                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ApplStatusImportRequest, Transactions.ApplStatusImportResponse>(It.IsAny<Transactions.ApplStatusImportRequest>())).ReturnsAsync(response);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RecruiterRepository_UpdateApplicationAsync_Ctx_error_throws_InvalidOperationException()
            {
                ApplicationException ctxException = new ApplicationException("Error occurred with CTX");
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ApplStatusImportRequest, Transactions.ApplStatusImportResponse>(It.IsAny<Transactions.ApplStatusImportRequest>())).ThrowsAsync(ctxException);

                await repository.UpdateApplicationAsync(application);
                loggerMock.Verify(l => l.Error(ctxException, "Transaction Invoker Execute Error for ApplStatusImport"));
            }

            [TestMethod]
            public async Task RecruiterRepository_UpdateApplicationAsync_success()
            {
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ApplStatusImportRequest, Transactions.ApplStatusImportResponse>(It.IsAny<Transactions.ApplStatusImportRequest>())).ReturnsAsync(response);

                await repository.UpdateApplicationAsync(application);
            }
        }

        [TestClass]
        public class RecruiterRepository_ImportApplicationAsync_Tests : RecruiterRepositoryTests
        {
            private Application application;
            private ApplicationImportResponse response;

            [TestInitialize]
            public void RecruiterRepository_ImportApplicationAsync_Tests_Initialize()
            {
                base.RecruiterRepositoryTests_Initialize();
                application = new Application()
                {
                    AcademicProgram = "AcademicProgram",
                    Activity = "Activity",
                    AddressLines1 = "AddressLines1",
                    AddressLines2 = "AddressLines2",
                    AddressLines3 = "AddressLines3",
                    AdmitType = "AdmitType",
                    ApplicantCounty = "ApplicantCounty",
                    ApplicantUser1 = "ApplicantUser1",
                    ApplicantUser10 = "ApplicantUser10",
                    ApplicantUser2 = "ApplicantUser2",
                    ApplicantUser3 = "ApplicantUser3",
                    ApplicantUser4 = "ApplicantUser4",
                    ApplicantUser5 = "ApplicantUser5",
                    ApplicantUser6 = "ApplicantUser6",
                    ApplicantUser7 = "ApplicantUser7",
                    ApplicantUser8 = "ApplicantUser8",
                    ApplicantUser9 = "ApplicantUser9",
                    ApplicationStatus = "ApplicationStatus",
                    ApplicationStatusDate = DateTime.Today,
                    ApplicationUser1 = "ApplicationUser1",
                    ApplicationUser10 = "ApplicationUser10",
                    ApplicationUser2 = "ApplicationUser2",
                    ApplicationUser3 = "ApplicationUser3",
                    ApplicationUser4 = "ApplicationUser4",
                    ApplicationUser5 = "ApplicationUser5",
                    ApplicationUser6 = "ApplicationUser6",
                    ApplicationUser7 = "ApplicationUser7",
                    ApplicationUser8 = "ApplicationUser8",
                    ApplicationUser9 = "ApplicationUser9",
                    Attention = "Attention",
                    BirthCity = "BirthCity",
                    BirthCountry = "BirthCountry",
                    BirthDate = DateTime.Today.AddYears(-18),
                    BirthDateSibling1 = DateTime.Today.AddYears(-21),
                    BirthDateSibling2 = DateTime.Today.AddYears(-20),
                    BirthDateSibling3 = DateTime.Today.AddYears(-19),
                    BirthState = "BirthState",
                    CareerGoal = "CareerGoal",
                    CellPhone = "123-456-7890",
                    Citizenship = "Citizenship",
                    CitizenshipStatus = "CitizenshipStatus",
                    City = "City",
                    CollegeAttendFromMonths = "CollegeAttendFromMonths",
                    CollegeAttendFromYears = "CollegeAttendFromYears",
                    CollegeAttendToMonths = "CollegeAttendToMonths",
                    CollegeAttendToYears = "CollegeAttendToYears",
                    CollegeCeebs = "CollegeCeebs",
                    CollegeDegreeDates = "CollegeDegreeDates",
                    CollegeDegrees = "CollegeDegrees",
                    CollegeGraduated = "CollegeGraduated",
                    CollegeHoursEarned = "CollegeHoursEarned",
                    CollegeNames = "CollegeNames",
                    CollegeNonCeebInfo = "CollegeNonCeebInfo",
                    CollegeTranscriptClassPercentage = "CollegeTranscriptClassPercentage",
                    CollegeTranscriptClassRank = "CollegeTranscriptClassRank",
                    CollegeTranscriptClassSize = "CollegeTranscriptClassSize",
                    CollegeTranscriptGpa = "CollegeTranscriptGpa",
                    CollegeTranscriptLocation = "CollegeTranscriptLocation",
                    CollegeTranscriptStored = "CollegeTranscriptStored",
                    Comments = "Comments",
                    Country = "Country",
                    CountryEntryDate = DateTime.Today.AddYears(50),
                    CourseLoad = "CourseLoad",
                    CrmApplicationId = new Guid().ToString(),
                    CrmProspectId = new Guid().ToString(),
                    CustomFields = new List<CustomField>()
                    {
                        null, // Nulls should be handled gracefully
                        new CustomField()
                        {
                            AttributeSchema = "AttributeSchema",
                            EntitySchema = "EntitySchema",
                            Value = "Value"
                        }
                    },
                    CustomFieldsXML = "CustomFieldsXML",
                    DecisionFactor1 = "DecisionFactor1",
                    DecisionFactor2 = "DecisionFactor2",
                    DecisionPlan = "DecisionPlan",
                    Denomination = "Denomination",
                    Disability = "Disability",
                    EducationalGoal = "EducationalGoal",
                    EmailAddress = "applicant@email.com",
                    EmergencyFirstName = "EmergencyFirstName",
                    EmergencyLastName = "EmergencyLastName",
                    EmergencyMiddleName = "EmergencyMiddleName",
                    EmergencyPhone = "234-567-8901",
                    EmergencyPrefix = "EmergencyPrefix",
                    EmergencySuffix = "EmergencySuffix",
                    ErpProspectId = "0001234",
                    Ethnicity = "Ethnicity",
                    FaPlan = "FaPlan",
                    FirstName = "ApplicantFirst",
                    FirstNameSibling1 = "FirstNameSibling1",
                    FirstNameSibling2 = "FirstNameSibling2",
                    FirstNameSibling3 = "FirstNameSibling3",
                    ForeignRegistrationId = "ForeignRegistrationId",
                    FutureActivity = "FutureActivity",
                    Gender = "Gender",
                    GuardianAddress1 = "GuardianAddress1",
                    GuardianAddress2 = "GuardianAddress2",
                    GuardianAddress3 = "GuardianAddress3",
                    GuardianBirthCountry = "GuardianBirthCountry",
                    GuardianBirthDate = DateTime.Today.AddYears(-45),
                    GuardianCity = "GuardianCity",
                    GuardianCountry = "GuardianCountry",
                    GuardianEmailAddress = "guardian@email.com",
                    GuardianFirstName = "GuardianFirst",
                    GuardianLastName = "GuardianLast",
                    GuardianMiddleName = "GuardianMiddle",
                    GuardianPhone = "345-678-9012",
                    GuardianPrefix = "GuardianPrefix",
                    GuardianRelationType = "GuardianRelationType",
                    GuardianSameAddress = "GuardianSameAddress",
                    GuardianState = "GuardianState",
                    GuardianSuffix = "GuardianSuffix",
                    GuardianZip = "12345",
                    HighSchoolAttendFromMonths = "HighSchoolAttendFromMonths",
                    HighSchoolAttendFromYears = "HighSchoolAttendFromYears",
                    HighSchoolAttendToMonths = "HighSchoolAttendToMonths",
                    HighSchoolAttendToYears = "HighSchoolAttendToYears",
                    HighSchoolCeebs = "HighSchoolCeebs",
                    HighSchoolGraduated = "HighSchoolGraduated",
                    HighSchoolNames = "HighSchoolNames",
                    HighSchoolNonCeebInfo = "HighSchoolNonCeebInfo",
                    HighSchoolTranscriptClassPercentage = "HighSchoolTranscriptClassPercentage",
                    HighSchoolTranscriptClassRank = "HighSchoolTranscriptClassRank",
                    HighSchoolTranscriptClassSize = "HighSchoolTranscriptClassSize",
                    HighSchoolTranscriptGpa = "HighSchoolTranscriptGpa",
                    HighSchoolTranscriptLocation = "HighSchoolTranscriptLocation",
                    HighSchoolTranscriptStored = "HighSchoolTranscriptStored",
                    HomePhone = "456-789-0123",
                    HoursWeekActivity = "HoursWeekActivity",
                    HousingPlan = "HousingPlan",
                    ImAddress = "ImAddress",
                    ImProvider = "ImProvider",
                    LastName = "ApplicantLast",
                    LastNameSibling1 = "LastNameSibling1",
                    LastNameSibling2 = "LastNameSibling2",
                    LastNameSibling3 = "LastNameSibling3",
                    Location = "Location",
                    MaritalStatus = "MartialStatus",
                    MiddleName = "ApplicantMiddle",
                    MiddleNameSibling1 = "MiddleNameSibling1",
                    MiddleNameSibling2 = "MiddleNameSibling2",
                    MiddleNameSibling3 = "MiddleNameSibling3",
                    Misc1 = "Misc1",
                    Misc2 = "Misc2",
                    Misc3 = "Misc3",
                    Misc4 = "Misc4",
                    Misc5 = "Misc5",
                    Nickname = "ApplicantNickname",
                    OtherFirstName = "ApplicantOtherFirst",
                    OtherLastName = "ApplicantOtherLast",
                    Parent1Address1 = "Parent1Address1",
                    Parent1Address2 = "Parent1Address2",
                    Parent1Address3 = "Parent1Address3",
                    Parent1BirthCountry = "Parent1BirthCountry",
                    Parent1BirthDate = DateTime.Today.AddYears(-50),
                    Parent1City = "Parent1City",
                    Parent1Country = "Parent1Country",
                    Parent1EmailAddress = "parent1@email.com",
                    Parent1FirstName = "Parent1First",
                    Parent1LastName = "Parent1Last",
                    Parent1Living = "Parent1Living",
                    Parent1MiddleName = "Parent1Middle",
                    Parent1Phone = "567-890-1234",
                    Parent1Prefix = "Parent1Prefix",
                    Parent1RelationType = "Parent1RelationType",
                    Parent1SameAddress = "Parent1SameAddress",
                    Parent1State = "Parent1State",
                    Parent1Suffix = "Parent1Suffix",
                    Parent1Zip = "Parent1Zip",
                    Parent2Address1 = "Parent2Address1",
                    Parent2Address2 = "Parent2Address2",
                    Parent2Address3 = "Parent2Address3",
                    Parent2BirthCountry = "Parent2BirthCountry",
                    Parent2BirthDate = DateTime.Today.AddYears(-50),
                    Parent2City = "Parent2City",
                    Parent2Country = "Parent2Country",
                    Parent2EmailAddress = "Parent2@email.com",
                    Parent2FirstName = "Parent2First",
                    Parent2LastName = "Parent2Last",
                    Parent2Living = "Parent2Living",
                    Parent2MiddleName = "Parent2Middle",
                    Parent2Phone = "567-890-1234",
                    Parent2Prefix = "Parent2Prefix",
                    Parent2RelationType = "Parent2RelationType",
                    Parent2SameAddress = "Parent2SameAddress",
                    Parent2State = "Parent2State",
                    Parent2Suffix = "Parent2Suffix",
                    Parent2Zip = "Parent2Zip",
                    ParentMaritalStatus = "ParentMaritalStatus",
                    Part10Activity = "Part10Activity",
                    Part11Activity = "Part11Activity",
                    Part12Activity = "Part12Activity",
                    Part9Activity = "Part9Activity",
                    PartPgActivity = "PartPgActivity",
                    Prefix = "ApplicantPrefix",
                    PrefixSibling1 = "PrefixSibling1",
                    PrefixSibling2 = "PrefixSibling2",
                    PrefixSibling3 = "PrefixSibling3",
                    PrimaryLanguage = "PrimaryLanguage",
                    ProspectSource = "ProspectSource",
                    Race1 = "Race1",
                    Race2 = "Race2",
                    Race3 = "Race3",
                    Race4 = "Race4",
                    Race5 = "Race5",
                    RecruiterOrganizationId = "RecruiterOrganizationId",
                    RecruiterOrganizationName = "RecruiterOrganizationName",
                    RelationTypeSibling1 = "RelationTypeSibling1",
                    RelationTypeSibling2 = "RelationTypeSibling2",
                    RelationTypeSibling3 = "RelationTypeSibling3",
                    ResidencyStatus = "ResidencyStatus",
                    Sin = "Sin",
                    Ssn = "Ssn",
                    StartTerm = "StartTerm",
                    State = "State",
                    SubmittedDate = DateTime.Today,
                    Suffix = "Suffix",
                    SuffixSibling1 = "SuffixSibling1",
                    SuffixSibling2 = "SuffixSibling2",
                    SuffixSibling3 = "SuffixSibling3",
                    TempAddressLines1 = "TempAddressLines1",
                    TempAddressLines2 = "TempAddressLines2",
                    TempAddressLines3 = "TempAddressLines3",
                    TempAttention = "TempAttention",
                    TempCity = "TempCity",
                    TempCountry = "TempCountry",
                    TempEndDate = DateTime.Today.AddDays(30),
                    TempPhone = "678-901-2345",
                    TempStartDate = DateTime.Today.AddDays(-30),
                    TempState = "TempState",
                    TempZip = "TempZip",
                    Veteran = "Veteran",
                    VisaType = "VisaType",
                    WeeksYearActivity = "WeeksYearActivity",
                    Zip = "Zip"
                };
                response = new ApplicationImportResponse()
                {
                    _AppServerVersion = 1
                };

                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ApplicationImportRequest, Transactions.ApplicationImportResponse>(It.IsAny<Transactions.ApplicationImportRequest>())).ReturnsAsync(response);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RecruiterRepository_ImportApplicationAsync_Ctx_error_throws_InvalidOperationException()
            {
                ApplicationException ctxException = new ApplicationException("Error occurred with CTX");
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ApplicationImportRequest, Transactions.ApplicationImportResponse>(It.IsAny<Transactions.ApplicationImportRequest>())).ThrowsAsync(ctxException);

                await repository.ImportApplicationAsync(application);
                loggerMock.Verify(l => l.Error(ctxException, "Transaction Invoker Execute Error for ApplicationImport"));
            }

            [TestMethod]
            public async Task RecruiterRepository_ImportApplicationAsync_success()
            {
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ApplicationImportRequest, Transactions.ApplicationImportResponse>(It.IsAny<Transactions.ApplicationImportRequest>())).ReturnsAsync(response);

                await repository.ImportApplicationAsync(application);
            }
        }

        [TestClass]
        public class RecruiterRepository_ImportTestScoresAsync_Tests : RecruiterRepositoryTests
        {
            private TestScore testScore;
            private TestScoreImportResponse response;

            [TestInitialize]
            public void RecruiterRepository_ImportTestScoresAsync_Tests_Initialize()
            {
                base.RecruiterRepositoryTests_Initialize();
                testScore = new TestScore()
                {
                    CustomFields = new List<CustomField>()
                    {
                        null, // Nulls should be handled gracefully
                        new CustomField()
                        {
                            AttributeSchema = "AttributeSchema",
                            EntitySchema = "EntitySchema",
                            Value = "Value"
                        }
                    },
                    ErpProspectId = "0001234",
                    RecruiterOrganizationId = new Guid().ToString(),
                    RecruiterOrganizationName = "RecruiterOrganizationName",
                    Score = "Score",
                    Source = "Source",
                    SubtestType = "SubtestType",
                    TestDate = DateTime.Today.AddMonths(-6).ToShortDateString(),
                    TestType = "TestType"
                };
                response = new TestScoreImportResponse()
                {
                    _AppServerVersion = 1
                };

                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.TestScoreImportRequest, Transactions.TestScoreImportResponse>(It.IsAny<Transactions.TestScoreImportRequest>())).ReturnsAsync(response);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RecruiterRepository_ImportTestScoresAsync_Ctx_error_throws_InvalidOperationException()
            {
                ApplicationException ctxException = new ApplicationException("Error occurred with CTX");
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.TestScoreImportRequest, Transactions.TestScoreImportResponse>(It.IsAny<Transactions.TestScoreImportRequest>())).ThrowsAsync(ctxException);

                await repository.ImportTestScoresAsync(testScore);
                loggerMock.Verify(l => l.Error(ctxException, "Transaction Invoker Execute Error for TestScoreImport"));
            }

            [TestMethod]
            public async Task RecruiterRepository_ImportTestScoresAsync_success()
            {
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.TestScoreImportRequest, Transactions.TestScoreImportResponse>(It.IsAny<Transactions.TestScoreImportRequest>())).ReturnsAsync(response);

                await repository.ImportTestScoresAsync(testScore);
            }
        }

        [TestClass]
        public class RecruiterRepository_ImportTranscriptCoursesAsync_Tests : RecruiterRepositoryTests
        {
            private TranscriptCourse transcriptCourse;
            private TranscriptImportResponse response;

            [TestInitialize]
            public void RecruiterRepository_ImportTranscriptCoursesAsync_Tests_Initialize()
            {
                base.RecruiterRepositoryTests_Initialize();
                transcriptCourse = new TranscriptCourse()
                {
                    Category  ="Category",
                    Comments = "Comments",
                    Course = "Course",
                    CreatedOn = DateTime.Today.AddDays(-60).ToShortDateString(),
                    Credits = "Credits",
                    CustomFields = new List<CustomField>()
                    {
                        null, // Nulls should be handled gracefully
                        new CustomField()
                        {
                            AttributeSchema = "AttributeSchema",
                            EntitySchema = "EntitySchema",
                            Value = "Value"
                        }
                    },
                    EndDate = DateTime.Today.AddDays(30).ToShortDateString(),
                    ErpInstitutionId = new Guid().ToString(),
                    ErpProspectId = "0001234",
                    Grade = "Grade",
                    InterimGradeFlag = "InterimGradeFlag",
                    RecruiterOrganizationId = new Guid().ToString(),
                    RecruiterOrganizationName = "RecruiterOrganizationName",
                    Source = "Source",
                    StartDate = DateTime.Today.AddDays(-30).ToShortDateString(),
                    Status = "Status",
                    Term = "Term",
                    Title = "Title"
                };
                response = new TranscriptImportResponse()
                {
                    _AppServerVersion = 1
                };

                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.TranscriptImportRequest, Transactions.TranscriptImportResponse>(It.IsAny<Transactions.TranscriptImportRequest>())).ReturnsAsync(response);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RecruiterRepository_ImportTranscriptCoursesAsync_Ctx_error_throws_InvalidOperationException()
            {
                ApplicationException ctxException = new ApplicationException("Error occurred with CTX");
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.TranscriptImportRequest, Transactions.TranscriptImportResponse>(It.IsAny<Transactions.TranscriptImportRequest>())).ThrowsAsync(ctxException);

                await repository.ImportTranscriptCoursesAsync(transcriptCourse);
                loggerMock.Verify(l => l.Error(ctxException, "Transaction Invoker Execute Error for TranscriptCourseImport"));
            }

            [TestMethod]
            public async Task RecruiterRepository_ImportTranscriptCoursesAsync_success()
            {
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.TranscriptImportRequest, Transactions.TranscriptImportResponse>(It.IsAny<Transactions.TranscriptImportRequest>())).ReturnsAsync(response);

                await repository.ImportTranscriptCoursesAsync(transcriptCourse);
            }
        }

        [TestClass]
        public class RecruiterRepository_ImportCommunicationHistoryAsync_Tests : RecruiterRepositoryTests
        {
            private CommunicationHistory communicationHistory;
            private CommCodeImportResponse response;

            [TestInitialize]
            public void RecruiterRepository_ImportCommunicationHistoryAsync_Tests_Initialize()
            {
                base.RecruiterRepositoryTests_Initialize();
                communicationHistory = new CommunicationHistory()
                {
                    CommunicationCode = "CommunicationCode",
                    CrmActivityId = "CrmActivityId",
                    CrmProspectId = new Guid().ToString(),
                    Date = DateTime.Today,
                    ErpProspectId = "0001234",
                    Location = "Location",
                    RecruiterOrganizationId = new Guid().ToString(),
                    RecruiterOrganizationName = "RecruiterOrganizationName",
                    Status = "Status",
                    Subject = "Subject"
                };
                response = new CommCodeImportResponse()
                {
                    _AppServerVersion = 1
                };

                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.CommCodeImportRequest, Transactions.CommCodeImportResponse>(It.IsAny<Transactions.CommCodeImportRequest>())).ReturnsAsync(response);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RecruiterRepository_ImportCommunicationHistoryAsync_Ctx_error_throws_InvalidOperationException()
            {
                ApplicationException ctxException = new ApplicationException("Error occurred with CTX");
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.CommCodeImportRequest, Transactions.CommCodeImportResponse>(It.IsAny<Transactions.CommCodeImportRequest>())).ThrowsAsync(ctxException);

                await repository.ImportCommunicationHistoryAsync(communicationHistory);
                loggerMock.Verify(l => l.Error(ctxException, "Transaction Invoker Execute Error for CommunicationHistoryImport"));
            }

            [TestMethod]
            public async Task RecruiterRepository_ImportCommunicationHistoryAsync_success()
            {
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.CommCodeImportRequest, Transactions.CommCodeImportResponse>(It.IsAny<Transactions.CommCodeImportRequest>())).ReturnsAsync(response);

                await repository.ImportCommunicationHistoryAsync(communicationHistory);
            }
        }

        [TestClass]
        public class RecruiterRepository_RequestCommunicationHistoryAsync_Tests : RecruiterRepositoryTests
        {
            private CommunicationHistory communicationHistory;
            private CommDataRequestResponse response;

            [TestInitialize]
            public void RecruiterRepository_RequestCommunicationHistoryAsync_Tests_Initialize()
            {
                base.RecruiterRepositoryTests_Initialize();
                communicationHistory = new CommunicationHistory()
                {
                    CommunicationCode = "CommunicationCode",
                    CrmActivityId = "CrmActivityId",
                    CrmProspectId = new Guid().ToString(),
                    Date = DateTime.Today,
                    ErpProspectId = "0001234",
                    Location = "Location",
                    RecruiterOrganizationId = new Guid().ToString(),
                    RecruiterOrganizationName = "RecruiterOrganizationName",
                    Status = "Status",
                    Subject = "Subject"
                };
                response = new CommDataRequestResponse()
                {
                    _AppServerVersion = 1
                };

                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.CommDataRequestRequest, Transactions.CommDataRequestResponse>(It.IsAny<Transactions.CommDataRequestRequest>())).ReturnsAsync(response);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RecruiterRepository_RequestCommunicationHistoryAsync_Ctx_error_throws_InvalidOperationException()
            {
                ApplicationException ctxException = new ApplicationException("Error occurred with CTX");
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.CommDataRequestRequest, Transactions.CommDataRequestResponse>(It.IsAny<Transactions.CommDataRequestRequest>())).ThrowsAsync(ctxException);

                await repository.RequestCommunicationHistoryAsync(communicationHistory);
                loggerMock.Verify(l => l.Error(ctxException, "Transaction Invoker Execute Error for CommunicationHistoryRequest"));
            }

            [TestMethod]
            public async Task RecruiterRepository_RequestCommunicationHistoryAsync_success()
            {
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.CommDataRequestRequest, Transactions.CommDataRequestResponse>(It.IsAny<Transactions.CommDataRequestRequest>())).ReturnsAsync(response);

                await repository.RequestCommunicationHistoryAsync(communicationHistory);
            }
        }

        [TestClass]
        public class RecruiterRepository_PostConnectionStatusAsync_Tests : RecruiterRepositoryTests
        {
            private ConnectionStatus connectionStatus;
            private ConnectionTestResponse response;

            [TestInitialize]
            public void RecruiterRepository_PostConnectionStatusAsync_Tests_Initialize()
            {
                base.RecruiterRepositoryTests_Initialize();
                connectionStatus = new ConnectionStatus()
                {
                    Duration = "Duration",
                    Message = "Message",
                    RecruiterOrganizationId = new Guid().ToString(),
                    RecruiterOrganizationName = "RecruiterOrganizationName",
                    ResponseServiceURL = "ResponseServiceURL",
                    Success = "Success"
                };
                response = new ConnectionTestResponse()
                {
                    _AppServerVersion = 1,
                    Duration = "ResponseDuration",
                    Message = "ResponseMessage",
                    ResponseServiceURL = "ResponseServiceURL",
                    Success = "ResponseSuccess"
                };

                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ConnectionTestRequest, Transactions.ConnectionTestResponse>(It.IsAny<Transactions.ConnectionTestRequest>())).ReturnsAsync(response);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RecruiterRepository_PostConnectionStatusAsync_Ctx_error_throws_InvalidOperationException()
            {
                ApplicationException ctxException = new ApplicationException("Error occurred with CTX");
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ConnectionTestRequest, Transactions.ConnectionTestResponse>(It.IsAny<Transactions.ConnectionTestRequest>())).ThrowsAsync(ctxException);

                var status =  await repository.PostConnectionStatusAsync(connectionStatus);
                loggerMock.Verify(l => l.Error(ctxException, "Transaction Invoker Execute Error for ConnectionTest"));
            }

            [TestMethod]
            public async Task RecruiterRepository_PostConnectionStatusAsync_success()
            {
                // Setup for CTX call
                transactionInvokerMock.Setup(ti => ti.ExecuteAsync<Transactions.ConnectionTestRequest, Transactions.ConnectionTestResponse>(It.IsAny<Transactions.ConnectionTestRequest>())).ReturnsAsync(response);

                var status = await repository.PostConnectionStatusAsync(connectionStatus);
                Assert.IsNotNull(status);
                Assert.AreEqual(response.Duration, status.Duration);
                Assert.AreEqual(response.Message, status.Message);
                Assert.AreEqual(response.ResponseServiceURL, status.ResponseServiceURL);
                Assert.AreEqual(response.Success, status.Success);
            }
        }

    }
}
