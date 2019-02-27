// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RecruiterRepository : BaseColleagueRepository, IRecruiterRepository
    {
        public RecruiterRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = 5; // 5 minutes            
        }

        /// <summary>
        /// Update application status
        /// </summary>
        /// <param name="newTranscriptCourse">An <see cref="Application">application</param>
        public async Task UpdateApplicationAsync(Ellucian.Colleague.Domain.Student.Entities.Application newApplication)
        {
            Transactions.ApplStatusImportRequest updateReq = new Transactions.ApplStatusImportRequest();
            updateReq.ErpProspectId = newApplication.ErpProspectId;
            updateReq.CrmApplicationId = newApplication.CrmApplicationId;
            updateReq.ApplicationStatus = newApplication.ApplicationStatus;
            updateReq.RecruiterOrganizationName = newApplication.RecruiterOrganizationName;
            updateReq.RecruiterOrganizationId = newApplication.RecruiterOrganizationId;
            updateReq.ApplicationStatusDate = newApplication.ApplicationStatusDate;
            // Update the application
            var updateResponse = new Transactions.ApplStatusImportResponse();
            try
            {
                updateResponse = await transactionInvoker.ExecuteAsync<Transactions.ApplStatusImportRequest, Transactions.ApplStatusImportResponse>(updateReq);
            }
            catch (Exception ex)
            {
                var errorText = "Transaction Invoker Execute Error for ApplStatusImport";
                logger.Error(ex, errorText);
                throw new InvalidOperationException(errorText);
            }
            return;
        }

        /// <summary>
        /// Import application/prospect
        /// </summary>
        /// <param name="newTranscriptCourse">An <see cref="Application">application</param>
        public async Task ImportApplicationAsync(Ellucian.Colleague.Domain.Student.Entities.Application newApplication)
        {
            Transactions.ApplicationImportRequest importReq = new Transactions.ApplicationImportRequest();
            importReq.CrmProspectId = newApplication.CrmProspectId;
            importReq.CrmApplicationId = newApplication.CrmApplicationId;
            importReq.ErpProspectId = newApplication.ErpProspectId;
            importReq.Ssn = newApplication.Ssn;
            importReq.Sin = newApplication.Sin;
            importReq.AcademicProgram = newApplication.AcademicProgram;
            importReq.StartTerm = newApplication.StartTerm;
            importReq.Prefix = newApplication.Prefix;
            importReq.FirstName = newApplication.FirstName;
            importReq.MiddleName = newApplication.MiddleName;
            importReq.LastName = newApplication.LastName;
            importReq.Suffix = newApplication.Suffix;
            importReq.Nickname = newApplication.Nickname;
            importReq.EmailAddress = newApplication.EmailAddress;
            importReq.ImAddress = newApplication.ImAddress;
            importReq.ImProvider = newApplication.ImProvider;
            importReq.AddressLines1 = newApplication.AddressLines1;
            importReq.AddressLines2 = newApplication.AddressLines2;
            importReq.AddressLines3 = newApplication.AddressLines3;
            importReq.City = newApplication.City;
            importReq.State = newApplication.State;
            importReq.Zip = newApplication.Zip;
            importReq.Country = newApplication.Country;
            importReq.Attention = newApplication.Attention;
            importReq.HomePhone = newApplication.HomePhone;
            importReq.CellPhone = newApplication.CellPhone;
            importReq.OtherFirstName = newApplication.OtherFirstName;
            importReq.OtherLastName = newApplication.OtherLastName;
            importReq.BirthDate = newApplication.BirthDate;
            importReq.Gender = newApplication.Gender;
            importReq.Ethnicity = newApplication.Ethnicity;
            importReq.Race1 = newApplication.Race1;
            importReq.Race2 = newApplication.Race2;
            importReq.Race3 = newApplication.Race3;
            importReq.Race4 = newApplication.Race4;
            importReq.Race5 = newApplication.Race5;
            importReq.Disability = newApplication.Disability;
            importReq.MaritalStatus = newApplication.MaritalStatus;
            importReq.Denomination = newApplication.Denomination;
            importReq.Veteran = newApplication.Veteran;
            importReq.CitizenshipStatus = newApplication.CitizenshipStatus;
            importReq.BirthCity = newApplication.BirthCity;
            importReq.BirthState = newApplication.BirthState;
            importReq.BirthCountry = newApplication.BirthCountry;
            importReq.PrimaryLanguage = newApplication.PrimaryLanguage;
            importReq.Citizenship = newApplication.Citizenship;
            importReq.ForeignRegistrationId = newApplication.ForeignRegistrationId;
            importReq.VisaType = newApplication.VisaType;
            importReq.CountryEntryDate = newApplication.CountryEntryDate;
            importReq.TempAddressLines1 = newApplication.TempAddressLines1;
            importReq.TempAddressLines2 = newApplication.TempAddressLines2;
            importReq.TempAddressLines3 = newApplication.TempAddressLines3;
            importReq.TempCity = newApplication.TempCity;
            importReq.TempState = newApplication.TempState;
            importReq.TempZip = newApplication.TempZip;
            importReq.TempCountry = newApplication.TempCountry;
            importReq.TempAttention = newApplication.TempAttention;
            importReq.TempPhone = newApplication.TempPhone;
            importReq.TempStartDate = newApplication.TempStartDate;
            importReq.TempEndDate = newApplication.TempEndDate;
            importReq.CareerGoal = newApplication.CareerGoal;
            importReq.EducationalGoal = newApplication.EducationalGoal;
            importReq.DecisionPlan = newApplication.DecisionPlan;
            importReq.CourseLoad = newApplication.CourseLoad;
            importReq.FaPlan = newApplication.FaPlan;
            importReq.HousingPlan = newApplication.HousingPlan;
            importReq.DecisionFactor1 = newApplication.DecisionFactor1;
            importReq.DecisionFactor2 = newApplication.DecisionFactor2;
            importReq.ParentMaritalStatus = newApplication.ParentMaritalStatus;
            importReq.Parent1RelationType = newApplication.Parent1RelationType;
            importReq.Parent1Prefix = newApplication.Parent1Prefix;
            importReq.Parent1FirstName = newApplication.Parent1FirstName;
            importReq.Parent1MiddleName = newApplication.Parent1MiddleName;
            importReq.Parent1LastName = newApplication.Parent1LastName;
            importReq.Parent1Suffix = newApplication.Parent1Suffix;
            importReq.Parent1Address1 = newApplication.Parent1Address1;
            importReq.Parent1Address2 = newApplication.Parent1Address2;
            importReq.Parent1Address3 = newApplication.Parent1Address3;
            importReq.Parent1City = newApplication.Parent1City;
            importReq.Parent1State = newApplication.Parent1State;
            importReq.Parent1Zip = newApplication.Parent1Zip;
            importReq.Parent1Country = newApplication.Parent1Country;
            importReq.Parent1Phone = newApplication.Parent1Phone;
            importReq.Parent1EmailAddress = newApplication.Parent1EmailAddress;
            importReq.Parent1BirthDate = newApplication.Parent1BirthDate;
            importReq.Parent1BirthCountry = newApplication.Parent1BirthCountry;
            importReq.Parent1Living = newApplication.Parent1Living;
            importReq.Parent1SameAddress = newApplication.Parent1SameAddress;
            importReq.Parent2RelationType = newApplication.Parent2RelationType;
            importReq.Parent2Prefix = newApplication.Parent2Prefix;
            importReq.Parent2FirstName = newApplication.Parent2FirstName;
            importReq.Parent2MiddleName = newApplication.Parent2MiddleName;
            importReq.Parent2LastName = newApplication.Parent2LastName;
            importReq.Parent2Suffix = newApplication.Parent2Suffix;
            importReq.Parent2Address1 = newApplication.Parent2Address1;
            importReq.Parent2Address2 = newApplication.Parent2Address2;
            importReq.Parent2Address3 = newApplication.Parent2Address3;
            importReq.Parent2City = newApplication.Parent2City;
            importReq.Parent2State = newApplication.Parent2State;
            importReq.Parent2Zip = newApplication.Parent2Zip;
            importReq.Parent2Country = newApplication.Parent2Country;
            importReq.Parent2Phone = newApplication.Parent2Phone;
            importReq.Parent2EmailAddress = newApplication.Parent2EmailAddress;
            importReq.Parent2BirthDate = newApplication.Parent2BirthDate;
            importReq.Parent2BirthCountry = newApplication.Parent2BirthCountry;
            importReq.Parent2Living = newApplication.Parent2Living;
            importReq.Parent2SameAddress = newApplication.Parent2SameAddress;
            importReq.GuardianRelationType = newApplication.GuardianRelationType;
            importReq.GuardianPrefix = newApplication.GuardianPrefix;
            importReq.GuardianFirstName = newApplication.GuardianFirstName;
            importReq.GuardianMiddleName = newApplication.GuardianMiddleName;
            importReq.GuardianLastName = newApplication.GuardianLastName;
            importReq.GuardianSuffix = newApplication.GuardianSuffix;
            importReq.GuardianAddress1 = newApplication.GuardianAddress1;
            importReq.GuardianAddress2 = newApplication.GuardianAddress2;
            importReq.GuardianAddress3 = newApplication.GuardianAddress3;
            importReq.GuardianCity = newApplication.GuardianCity;
            importReq.GuardianState = newApplication.GuardianState;
            importReq.GuardianZip = newApplication.GuardianZip;
            importReq.GuardianCountry = newApplication.GuardianCountry;
            importReq.GuardianPhone = newApplication.GuardianPhone;
            importReq.GuardianEmailAddress = newApplication.GuardianEmailAddress;
            importReq.GuardianBirthDate = newApplication.GuardianBirthDate;
            importReq.GuardianBirthCountry = newApplication.GuardianBirthCountry;
            importReq.GuardianSameAddress = newApplication.GuardianSameAddress;
            importReq.RelationTypeSibling1 = newApplication.RelationTypeSibling1;
            importReq.PrefixSibling1 = newApplication.PrefixSibling1;
            importReq.FirstNameSibling1 = newApplication.FirstNameSibling1;
            importReq.MiddleNameSibling1 = newApplication.MiddleNameSibling1;
            importReq.LastNameSibling1 = newApplication.LastNameSibling1;
            importReq.SuffixSibling1 = newApplication.SuffixSibling1;
            importReq.BirthDateSibling1 = newApplication.BirthDateSibling1;
            importReq.RelationTypeSibling2 = newApplication.RelationTypeSibling2;
            importReq.PrefixSibling2 = newApplication.PrefixSibling2;
            importReq.FirstNameSibling2 = newApplication.FirstNameSibling2;
            importReq.MiddleNameSibling2 = newApplication.MiddleNameSibling2;
            importReq.LastNameSibling2 = newApplication.LastNameSibling2;
            importReq.SuffixSibling2 = newApplication.SuffixSibling2;
            importReq.BirthDateSibling2 = newApplication.BirthDateSibling2;
            importReq.RelationTypeSibling3 = newApplication.RelationTypeSibling3;
            importReq.PrefixSibling3 = newApplication.PrefixSibling3;
            importReq.FirstNameSibling3 = newApplication.FirstNameSibling3;
            importReq.MiddleNameSibling3 = newApplication.MiddleNameSibling3;
            importReq.LastNameSibling3 = newApplication.LastNameSibling3;
            importReq.SuffixSibling3 = newApplication.SuffixSibling3;
            importReq.BirthDateSibling3 = newApplication.BirthDateSibling3;
            importReq.EmergencyPrefix = newApplication.EmergencyPrefix;
            importReq.EmergencyFirstName = newApplication.EmergencyFirstName;
            importReq.EmergencyMiddleName = newApplication.EmergencyMiddleName;
            importReq.EmergencyLastName = newApplication.EmergencyLastName;
            importReq.EmergencySuffix = newApplication.EmergencySuffix;
            importReq.EmergencyPhone = newApplication.EmergencyPhone;
            importReq.Activity = newApplication.Activity;
            importReq.Part9Activity = newApplication.Part9Activity;
            importReq.Part10Activity = newApplication.Part10Activity;
            importReq.Part11Activity = newApplication.Part11Activity;
            importReq.Part12Activity = newApplication.Part12Activity;
            importReq.PartPgActivity = newApplication.PartPgActivity;
            importReq.FutureActivity = newApplication.FutureActivity;
            importReq.HoursWeekActivity = newApplication.HoursWeekActivity;
            importReq.WeeksYearActivity = newApplication.WeeksYearActivity;
            importReq.HighSchoolCeebs = newApplication.HighSchoolCeebs;
            importReq.HighSchoolNonceebInfo = newApplication.HighSchoolNonCeebInfo;
            importReq.HighSchoolAttendFromYears = newApplication.HighSchoolAttendFromYears;
            importReq.HighSchoolAttendFromMonths = newApplication.HighSchoolAttendFromMonths;
            importReq.HighSchoolAttendToYears = newApplication.HighSchoolAttendToYears;
            importReq.HighSchoolAttendToMonths = newApplication.HighSchoolAttendToMonths;
            importReq.CollegeCeebs = newApplication.CollegeCeebs;
            importReq.CollegeNonceebInfo = newApplication.CollegeNonCeebInfo;
            importReq.CollegeAttendFromYears = newApplication.CollegeAttendFromYears;
            importReq.CollegeAttendFromMonths = newApplication.CollegeAttendFromMonths;
            importReq.CollegeAttendToYears = newApplication.CollegeAttendToYears;
            importReq.CollegeAttendToMonths = newApplication.CollegeAttendToMonths;
            importReq.CollegeDegrees = newApplication.CollegeDegrees;
            importReq.CollegeDegreeDates = newApplication.CollegeDegreeDates;
            importReq.CollegeHoursEarned = newApplication.CollegeHoursEarned;
            importReq.GuardianAddress3 = newApplication.GuardianAddress3;
            importReq.AdmitType = newApplication.AdmitType;
            importReq.ProspectSource = newApplication.ProspectSource;
            importReq.HighSchoolNames = newApplication.HighSchoolNames;
            importReq.CollegeNames = newApplication.CollegeNames;
            importReq.Comments = newApplication.Comments;
            importReq.Misc1 = newApplication.Misc1;
            importReq.Misc2 = newApplication.Misc2;
            importReq.Misc3 = newApplication.Misc3;
            importReq.Misc4 = newApplication.Misc4;
            importReq.Misc5 = newApplication.Misc5;
            importReq.ApplicationUser1 = newApplication.ApplicationUser1;
            importReq.ApplicationUser2 = newApplication.ApplicationUser2;
            importReq.ApplicationUser3 = newApplication.ApplicationUser3;
            importReq.ApplicationUser4 = newApplication.ApplicationUser4;
            importReq.ApplicationUser5 = newApplication.ApplicationUser5;
            importReq.ApplicationUser6 = newApplication.ApplicationUser6;
            importReq.ApplicationUser7 = newApplication.ApplicationUser7;
            importReq.ApplicationUser8 = newApplication.ApplicationUser8;
            importReq.ApplicationUser9 = newApplication.ApplicationUser9;
            importReq.ApplicationUser10 = newApplication.ApplicationUser10;
            importReq.ApplicantUser1 = newApplication.ApplicantUser1;
            importReq.ApplicantUser2 = newApplication.ApplicantUser2;
            importReq.ApplicantUser3 = newApplication.ApplicantUser3;
            importReq.ApplicantUser4 = newApplication.ApplicantUser4;
            importReq.ApplicantUser5 = newApplication.ApplicantUser5;
            importReq.ApplicantUser6 = newApplication.ApplicantUser6;
            importReq.ApplicantUser7 = newApplication.ApplicantUser7;
            importReq.ApplicantUser8 = newApplication.ApplicantUser8;
            importReq.ApplicantUser9 = newApplication.ApplicantUser9;
            importReq.ApplicantUser10 = newApplication.ApplicationUser10;
            var customFieldsXml = "";
            customFieldsXml += "<CustomField>";
            if (newApplication.CustomFields != null)
            {
                foreach (var field in newApplication.CustomFields)
                {
                    if (field != null)
                    {
                        customFieldsXml += "<EntitySchema>" + field.EntitySchema + "</EntitySchema>";
                        customFieldsXml += "<AttributeSchema>" + field.AttributeSchema + "</AttributeSchema>";
                        customFieldsXml += "<Value>" + field.Value + "</Value>";
                    }
                }
            }
            customFieldsXml += "</CustomField>";
            importReq.CustomFieldsXML = customFieldsXml;    
            importReq.Location = newApplication.Location;
            importReq.SubmittedDate = newApplication.SubmittedDate;
            importReq.ApplicationStatus = newApplication.ApplicationStatus;
            importReq.HighSchoolGraduated = newApplication.HighSchoolGraduated;
            importReq.HighSchoolTranscriptStored = newApplication.HighSchoolTranscriptStored;
            importReq.HighSchoolTranscriptLocation = newApplication.HighSchoolTranscriptLocation;
            importReq.HighSchoolTranscriptGpa = newApplication.HighSchoolTranscriptGpa;
            importReq.HighSchoolTranscriptClassPercentage = newApplication.HighSchoolTranscriptClassPercentage;
            importReq.HighSchoolTranscriptClassRank = newApplication.HighSchoolTranscriptClassRank;
            importReq.HighSchoolTranscriptClassSize = newApplication.HighSchoolTranscriptClassSize;
            importReq.CollegeGraduated = newApplication.CollegeGraduated;
            importReq.CollegeTranscriptStored = newApplication.CollegeTranscriptStored;
            importReq.CollegeTranscriptLocation = newApplication.CollegeTranscriptLocation;
            importReq.CollegeTranscriptGpa = newApplication.CollegeTranscriptGpa;
            importReq.CollegeTranscriptClassPercentage = newApplication.CollegeTranscriptClassPercentage;
            importReq.CollegeTranscriptClassRank = newApplication.CollegeTranscriptClassRank;
            importReq.CollegeTranscriptClassSize = newApplication.CollegeTranscriptClassSize;
            importReq.ApplicantCounty = newApplication.ApplicantCounty;
            importReq.ResidencyStatus = newApplication.ResidencyStatus;
            importReq.RecruiterOrganizationName = newApplication.RecruiterOrganizationName;
            importReq.RecruiterOrganizationId = newApplication.RecruiterOrganizationId;
            importReq.ApplicationStatusDate = newApplication.ApplicationStatusDate;

            // Update the application
            var importResponse = new Transactions.ApplicationImportResponse();
            try
            {
                importResponse = await transactionInvoker.ExecuteAsync<Transactions.ApplicationImportRequest, Transactions.ApplicationImportResponse>(importReq);
            }
            catch (Exception)
            {
                var errorText = "Transaction Invoker Execute Error for ApplicationImport";
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            return;
        }

        /// <summary>
        /// Import test scores
        /// </summary>
        /// <param name="newTranscriptCourse">A <see cref="TestScore">test score</param>
        public async Task ImportTestScoresAsync(Ellucian.Colleague.Domain.Student.Entities.TestScore newTestScore)
        {
            Transactions.TestScoreImportRequest importReq = new Transactions.TestScoreImportRequest();
            importReq.ErpProspectId = newTestScore.ErpProspectId;
            importReq.TestType = newTestScore.TestType;
            importReq.TestDate = newTestScore.TestDate;
            importReq.Source = newTestScore.Source;
            importReq.SubtestType = newTestScore.SubtestType;
            importReq.Score = newTestScore.Score;
            var customFieldsXml = "";
            customFieldsXml += "<CustomField>";
            if (newTestScore.CustomFields != null)
            {
                foreach (var field in newTestScore.CustomFields)
                {
                    if (field != null)
                    {
                        customFieldsXml += "<EntitySchema>" + field.EntitySchema + "</EntitySchema>";
                        customFieldsXml += "<AttributeSchema>" + field.AttributeSchema + "</AttributeSchema>";
                        customFieldsXml += "<Value>" + field.Value + "</Value>";
                    }
                }
            }
            customFieldsXml += "</CustomField>";
            importReq.CustomFieldsXML = customFieldsXml;                        
            importReq.RecruiterOrganizationName = newTestScore.RecruiterOrganizationName;
            importReq.RecruiterOrganizationId = newTestScore.RecruiterOrganizationId;
            // Update the test score
            var importResponse = new Transactions.TestScoreImportResponse();
            try
            {
                importResponse = await transactionInvoker.ExecuteAsync<Transactions.TestScoreImportRequest, Transactions.TestScoreImportResponse>(importReq);
            }
            catch (Exception)
            {
                var errorText = "Transaction Invoker Execute Error for TestScoreImport";
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            return;
        }

        /// <summary>
        /// Import transcript courses
        /// </summary>
        /// <param name="newTranscriptCourse">A <see cref="TranscriptCourse">transcript course</param>
        public async Task ImportTranscriptCoursesAsync(Ellucian.Colleague.Domain.Student.Entities.TranscriptCourse newTranscriptCourse)
        {
            Transactions.TranscriptImportRequest importReq = new Transactions.TranscriptImportRequest();
            importReq.ErpProspectId = newTranscriptCourse.ErpProspectId;
            importReq.ErpInstitutionId = newTranscriptCourse.ErpInstitutionId;
            importReq.Title = newTranscriptCourse.Title;
            importReq.Term = newTranscriptCourse.Term;
            importReq.Course = newTranscriptCourse.Course;
            importReq.StartDate = newTranscriptCourse.StartDate;
            importReq.EndDate = newTranscriptCourse.EndDate;
            importReq.Grade = newTranscriptCourse.Grade;
            importReq.InterimGradeFlag = newTranscriptCourse.InterimGradeFlag;
            importReq.Credits = newTranscriptCourse.Credits;
            importReq.Status = newTranscriptCourse.Status;
            importReq.Category = newTranscriptCourse.Category;
            importReq.CreatedOn = newTranscriptCourse.CreatedOn;
            importReq.Source = newTranscriptCourse.Source;
            importReq.Comments = newTranscriptCourse.Comments;
            var customFieldsXml = "";
            customFieldsXml += "<CustomField>";
            if (newTranscriptCourse.CustomFields != null)
            {
                foreach (var field in newTranscriptCourse.CustomFields)
                {
                    if (field != null)
                    {
                        customFieldsXml += "<EntitySchema>" + field.EntitySchema + "</EntitySchema>";
                        customFieldsXml += "<AttributeSchema>" + field.AttributeSchema + "</AttributeSchema>";
                        customFieldsXml += "<Value>" + field.Value + "</Value>";
                    }
                }
            }
            customFieldsXml += "</CustomField>";
            importReq.CustomFieldsXML = customFieldsXml;                        

            importReq.RecruiterOrganizationName = newTranscriptCourse.RecruiterOrganizationName;
            importReq.RecruiterOrganizationId = newTranscriptCourse.RecruiterOrganizationId;
            // Update the transcript course
            var importResponse = new Transactions.TranscriptImportResponse();
            try
            {
                importResponse = await transactionInvoker.ExecuteAsync<Transactions.TranscriptImportRequest, Transactions.TranscriptImportResponse>(importReq);
            }
            catch (Exception)
            {
                var errorText = "Transaction Invoker Execute Error for TranscriptCourseImport";
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            return;
        }

        /// <summary>
        /// Import communication history
        /// </summary>
        /// <param name="newCommunicationHistory">A <see cref="Domain.Base.Entities.CommunicationHistory">communication history</param>
        public async Task ImportCommunicationHistoryAsync(Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory newCommunicationHistory)
        {
            Transactions.CommCodeImportRequest importReq = new Transactions.CommCodeImportRequest();
            importReq.ErpProspectId = newCommunicationHistory.ErpProspectId;
            importReq.CrmActivityId = newCommunicationHistory.CrmActivityId;
            importReq.CommunicationCode = newCommunicationHistory.CommunicationCode;
            importReq.Date = newCommunicationHistory.Date;
            importReq.Subject = newCommunicationHistory.Subject;
            importReq.Status = newCommunicationHistory.Status;
            importReq.Location = newCommunicationHistory.Location;
            importReq.RecruiterOrganizationName = newCommunicationHistory.RecruiterOrganizationName;
            importReq.RecruiterOrganizationId = newCommunicationHistory.RecruiterOrganizationId;
            // Update the communication history
            var importResponse = new Transactions.CommCodeImportResponse();
            try
            {
                importResponse =await transactionInvoker.ExecuteAsync<Transactions.CommCodeImportRequest, Transactions.CommCodeImportResponse>(importReq);
            }
            catch (Exception)
            {
                var errorText = "Transaction Invoker Execute Error for CommunicationHistoryImport";
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            return;
        }

        /// <summary>
        /// Request communication history
        /// </summary>
        /// <param name="newCommunicationHistory">A <see cref="Domain.Base.Entities.CommunicationHistory">communication history</param>
        public async Task RequestCommunicationHistoryAsync(Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory newCommunicationHistory)
        {
            Transactions.CommDataRequestRequest requestReq = new Transactions.CommDataRequestRequest();
            requestReq.ErpProspectId = newCommunicationHistory.ErpProspectId;
            requestReq.CrmProspectId = newCommunicationHistory.CrmProspectId;
            requestReq.RecruiterOrganizationName = newCommunicationHistory.RecruiterOrganizationName;
            requestReq.RecruiterOrganizationId = newCommunicationHistory.RecruiterOrganizationId;
            // iassue the request
            var requestResponse = new Transactions.CommDataRequestResponse();
            try
            {
                requestResponse = await transactionInvoker.ExecuteAsync<Transactions.CommDataRequestRequest, Transactions.CommDataRequestResponse>(requestReq);
            }
            catch (Exception)
            {
                var errorText = "Transaction Invoker Execute Error for CommunicationHistoryRequest";
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            return;
        }

        /// <summary>
        /// Get connection status of Colleague to Recruiter
        /// </summary>
        /// <param name="newConnectionStatus">A <see cref="ConnectionStatus">connection status</see></param>
        /// <returns>A <see cref="ConnectionStatus">connection status</see></returns>
        public async Task<ConnectionStatus> PostConnectionStatusAsync(Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus newConnectionStatus)
        {
            Transactions.ConnectionTestRequest postReq = new Transactions.ConnectionTestRequest();
            postReq.RecruiterOrganizationName = newConnectionStatus.RecruiterOrganizationName;

            var postResponse = new Transactions.ConnectionTestResponse();
            try
            {
                postResponse = await transactionInvoker.ExecuteAsync<Transactions.ConnectionTestRequest, Transactions.ConnectionTestResponse>(postReq);
                var connectionStatus = new ConnectionStatus();
                connectionStatus.ResponseServiceURL = postResponse.ResponseServiceURL;
                connectionStatus.Duration = postResponse.Duration;
                connectionStatus.Message = postResponse.Message;
                connectionStatus.Success = postResponse.Success;
                return connectionStatus;
            }
            catch (Exception)
            {
                var errorText = "Transaction Invoker Execute Error for ConnectionTest";
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
        }

    }
}