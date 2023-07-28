// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentReferenceDataRepository : IStudentReferenceDataRepository
    {
        public Task<IEnumerable<Student.Entities.AcademicDepartment>> GetAcademicDepartmentsAsync()
        {
            return this.GetAcademicDepartmentsAsync(false);
        }

        public Task<IEnumerable<Student.Entities.AcademicDepartment>> GetAcademicDepartmentsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AcademicDepartment>>(new List<Student.Entities.AcademicDepartment>()
                {
                    new Domain.Student.Entities.AcademicDepartment("0500da20-1336-4319-bacf-efe4f4dde018", "AGBU", "Agriculture Business", true),
            new Domain.Student.Entities.AcademicDepartment("62244057-D1EC-4094-A0B7-DE602533E3A6", "COMP", "Computer Science", true),
            new Domain.Student.Entities.AcademicDepartment("fea97622-4445-4666-b673-948227ce7ed2", "ENGL", "English", false),
            new Domain.Student.Entities.AcademicDepartment("6ef164eb-8178-4321-a9f7-24f12d3991d8", "HIST", "History", false)
                });
        }

        public Task<IEnumerable<Student.Entities.AcademicLevel>> GetAcademicLevelsAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.AcademicLevel>>(new List<Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("884a59d1-20e5-43af-94e3-f1504230bbbc", "GR", "Graduate"),
            new Domain.Student.Entities.AcademicLevel("bb336acf-1926-4b12-8daf-d8720280498f", "JD", "Law"),
            new Domain.Student.Entities.AcademicLevel("d118f007-c914-465e-80dc-49d39209b24f", "UG", "Undergraduate")
                });
        }

        public Task<string> GetAcademicLevelsGuidAsync(string code)
        {
            List<Student.Entities.AcademicLevel> acadlevels = new List<Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("884a59d1-20e5-43af-94e3-f1504230bbbc", "GR", "Graduate"),
            new Domain.Student.Entities.AcademicLevel("bb336acf-1926-4b12-8daf-d8720280498f", "JD", "Law"),
            new Domain.Student.Entities.AcademicLevel("d118f007-c914-465e-80dc-49d39209b24f", "UG", "Undergraduate")
                };
            return Task.FromResult(acadlevels.FirstOrDefault(c => c.Code == code).Guid);
        }


        public Task<IEnumerable<Student.Entities.AcademicProgram>> GetAcademicProgramsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AcademicProgram>>(new List<Student.Entities.AcademicProgram>()
                {
                    new Domain.Student.Entities.AcademicProgram("884a59d1-20e5-43af-94e3-f1504230bbbd", "MATH.AA", "Mathematics"),
            new Domain.Student.Entities.AcademicProgram("bb336acf-1926-4b12-8daf-d8720280498e", "CHEM.BA", "Chemistry"),
            new Domain.Student.Entities.AcademicProgram("d118f007-c914-465e-80dc-49d39209b24k", "ENG.BA", "English")
                });

        }

        public Task<Student.Entities.AcademicProgram> GetAcademicProgramByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAcademicProgramsGuidAsync(string code)
        {
            var progs = (new List<Student.Entities.AcademicProgram>()
                {
                    new Domain.Student.Entities.AcademicProgram("884a59d1-20e5-43af-94e3-f1504230bbbd", "MATH.AA", "Mathematics"),
            new Domain.Student.Entities.AcademicProgram("bb336acf-1926-4b12-8daf-d8720280498e", "CHEM.BA", "Chemistry"),
            new Domain.Student.Entities.AcademicProgram("d118f007-c914-465e-80dc-49d39209b24k", "ENG.BA", "English")
                });
            return Task.FromResult(progs.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<Student.Entities.AdvisorType>> GetAdvisorTypesAsync(bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AdvisorType>>(new List<Student.Entities.AdvisorType>()
                {
                    new Domain.Student.Entities.AdvisorType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "MAJ", "Major", "1"),
                    new Domain.Student.Entities.AdvisorType("73244057-D1EC-4094-A0B7-DE602533E3A6", "MIN", "Minor", "2"),
                    new Domain.Student.Entities.AdvisorType("1df164eb-8178-4321-a9f7-24f12d3991d8", "GEN", "General", "3")
                });
        }
        public Task<IEnumerable<Student.Entities.AcademicStanding>> GetAcademicStandingsAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.AcademicStanding>>(new List<Student.Entities.AcademicStanding>()
                {
                    new Domain.Student.Entities.AcademicStanding("DEAN", "Deans List"),
            new Domain.Student.Entities.AcademicStanding("GOOD", "Good Academic Standing"),
            new Domain.Student.Entities.AcademicStanding("PROB", "Probation")
                });
        }

        public Task<IEnumerable<Student.Entities.AcademicStanding2>> GetAcademicStandings2Async(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AcademicStanding2>>(new List<Student.Entities.AcademicStanding2>()
                {
                    new Domain.Student.Entities.AcademicStanding2("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "DEAN", "Deans List"),
            new Domain.Student.Entities.AcademicStanding2("73244057-D1EC-4094-A0B7-DE602533E3A6", "GOOD", "Good Academic Standing"),
            new Domain.Student.Entities.AcademicStanding2("1df164eb-8178-4321-a9f7-24f12d3991d8", "PROB", "Probation")
                });
        }

        public Task<IEnumerable<Student.Entities.AcademicLevel>> GetAcademicLevelsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AcademicLevel>>(new List<Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"),
            new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"),
            new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate")
                 });
        }

        public Task<IEnumerable<AccountReceivableType>> GetAccountReceivableTypesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.AdmissionApplicationType>> GetAdmissionApplicationTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AdmissionApplicationType>>(new List<Student.Entities.AdmissionApplicationType>()
                {
                    new Domain.Student.Entities.AdmissionApplicationType("03ef76f3-41be-1290-8a9d-9a80282fc420", "ST", "Standard")
                });
        }

        public Task<IEnumerable<AdmissionApplicationStatusType>> GetAdmissionApplicationStatusTypesAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.AdmissionPopulation>> GetAdmissionPopulationsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AdmissionPopulation>>(new List<Student.Entities.AdmissionPopulation>()
                {
                    new Domain.Student.Entities.AdmissionPopulation("03ef76f3-61be-4990-8a9d-9a80282fc420", "FR", "First Time Freshman"),
            new Domain.Student.Entities.AdmissionPopulation("d2f4f0af-6714-48c7-88dd-1c40cb407b6c", "GD", "Graduate"),
            new Domain.Student.Entities.AdmissionPopulation("c517d7a5-f06a-42c8-85ad-b6320e1c0c2a", "TR", "Transfer")
                });
        }

        public Task<IEnumerable<Student.Entities.AdmittedStatus>> GetAdmittedStatusesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.AdmittedStatus>>(new List<Student.Entities.AdmittedStatus>()
                {
                    new Domain.Student.Entities.AdmittedStatus("FR", "First Time Freshman", ""),
            new Domain.Student.Entities.AdmittedStatus("GD", "Graduate", ""),
            new Domain.Student.Entities.AdmittedStatus("TR", "Transfer", "Y")
                });
        }

        public Task<IEnumerable<Student.Entities.Affiliation>> GetAffiliationsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.ApplicationInfluence>> GetApplicationInfluencesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.ApplicationInfluence>>(new List<Student.Entities.ApplicationInfluence>()
                {
                    new Domain.Student.Entities.ApplicationInfluence("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "CT", "Campus Tour"),
            new Domain.Student.Entities.ApplicationInfluence("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "BR", "Brochure"),
            new Domain.Student.Entities.ApplicationInfluence("0C3B805D-CFE6-483B-86C3-4C20562F8C15", "WS", "Website")
                });
        }

        public Task<IEnumerable<ApplicationSource>> GetApplicationSourcesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.ApplicationSource>>(new List<Student.Entities.ApplicationSource>()
                {
                    new Domain.Student.Entities.ApplicationSource("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "ONLINE", "Online"),
            new Domain.Student.Entities.ApplicationSource("83244057-D1EC-4094-A0B7-DE602533E3A6", "MAIL", "Mail"),
            new Domain.Student.Entities.ApplicationSource("2df164eb-8178-4321-a9f7-24f12d3991d8", "CB", "College Board")
                 });
        }

        public Task<IEnumerable<Student.Entities.ApplicationStatus>> GetApplicationStatusesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.ApplicationStatusCategory>> GetApplicationStatusCategoriesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.AssessmentSpecialCircumstance>> GetAssessmentSpecialCircumstancesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AssessmentSpecialCircumstance>>(new List<Student.Entities.AssessmentSpecialCircumstance>()
                {
                    new Student.Entities.AssessmentSpecialCircumstance("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.AssessmentSpecialCircumstance("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.AssessmentSpecialCircumstance("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }


        public Task<string> GetAssessmentSpecialCircumstancesGuidAsync(string code)
        {
            var assessmentSpecialCircumstances = (new List<Student.Entities.AssessmentSpecialCircumstance>()
                {
                    new Student.Entities.AssessmentSpecialCircumstance("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.AssessmentSpecialCircumstance("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.AssessmentSpecialCircumstance("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
            return Task.FromResult(assessmentSpecialCircumstances.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<BookOption>> GetBookOptionsAsync()
        {
            return Task.FromResult<IEnumerable<BookOption>>(new List<BookOption>()
                {
                    new BookOption("R", "Required", true),
                    new BookOption("C", "Recommended", false),
                    new BookOption("O", "Optional", false)
                });
        }
        public Task<IEnumerable<Student.Entities.CampusInvRole>> GetCampusInvolvementRolesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CampusInvRole>>(new List<Student.Entities.CampusInvRole>()
                {
                    new Student.Entities.CampusInvRole("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Student.Entities.CampusInvRole("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Student.Entities.CampusInvRole("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                });
        }
        public Task<IEnumerable<Student.Entities.CampusOrganizationType>> GetCampusOrganizationTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CampusOrganizationType>>(new List<Student.Entities.CampusOrganizationType>()
                {
                    new Student.Entities.CampusOrganizationType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Student.Entities.CampusOrganizationType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Student.Entities.CampusOrganizationType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                });
        }

        public Task<IEnumerable<Student.Entities.CareerGoal>> GetCareerGoalsAsync(bool ignoreCache= false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CareerGoal>>(new List<Student.Entities.CareerGoal>()
                {
                    new Student.Entities.CareerGoal("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fba", "ADAS", "Administrative Assistant"),
                    new Student.Entities.CareerGoal("2a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbb", "INFT", "Information Technology"),
                    new Student.Entities.CareerGoal("3a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "PSYC", "Psychology"),
                    new Student.Entities.CareerGoal("4a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbd", "NURS", "Nursing"),
                    new Student.Entities.CareerGoal("5a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbe", "AUTO", "Auto Repair")
                });
        }

        public Task<IEnumerable<Student.Entities.Ccd>> GetCcdsAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.Ccd>>(new List<Student.Entities.Ccd>()
                {
                    new Student.Entities.Ccd("ccd0", "Ccd 0"),
                    new Student.Entities.Ccd("ccd1", "Ccd 1"),
                    new Student.Entities.Ccd("ccd2", "Ccd 2"),
                    new Student.Entities.Ccd("ccd3", "Ccd 3"),
                    new Student.Entities.Ccd("ccd4", "Ccd 4")
                });
        }
        
        /// <summary>
        /// Get All CIP codes for cip-codes API (HEDM)
        /// </summary>
        /// <returns>List of CipCode entities</returns>
        public Task<IEnumerable<CipCode>> GetCipCodesAsync(bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CipCode>>(new List<Student.Entities.CipCode>()
                {
                    new Student.Entities.CipCode("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1", 2020),
                    new Student.Entities.CipCode("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2", 2020),
                    new Student.Entities.CipCode("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3", 2020),
                    new Student.Entities.CipCode("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4", 2020)
                });
        }

        /// <summary>
        /// Get guid for Cip code
        /// </summary>
        /// <param name="code">Cip code</param>
        /// <returns>Guid</returns>
        public Task<string> GetCipCodesGuidAsync(string code)
        {
            var cipCodeList = (new List<Student.Entities.CipCode>()
                {
                    new Student.Entities.CipCode("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1", 2020),
                    new Student.Entities.CipCode("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2", 2020),
                    new Student.Entities.CipCode("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3", 2020),
                    new Student.Entities.CipCode("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4", 2020)
                });
            return Task.FromResult(cipCodeList.FirstOrDefault(c => c.Code == code).Guid);
        }

        /// <summary>
        /// Get code for Cip code guid
        /// </summary>
        /// <param name="code">Cip code guid</param>
        /// <returns>code</returns>
        public Task<string> GetCipCodesFromGuidAsync(string guid)
        {
            var cipCodeList = (new List<Student.Entities.CipCode>()
                {
                    new Student.Entities.CipCode("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1", 2020),
                    new Student.Entities.CipCode("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2", 2020),
                    new Student.Entities.CipCode("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3", 2020),
                    new Student.Entities.CipCode("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4", 2020)
                });
            return Task.FromResult(cipCodeList.FirstOrDefault(c => c.Guid == guid).Code);
        }

        public Task<IEnumerable<Student.Entities.ClassLevel>> GetClassLevelsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.CourseLevel>> GetCourseLevelsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.CourseLevel>> GetCourseLevelsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CourseLevel>>(new List<Student.Entities.CourseLevel>()
                {
                    new Student.Entities.CourseLevel("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "100", "1st Year"),
                    new Student.Entities.CourseLevel("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "200", "2nd Year"),
                    new Student.Entities.CourseLevel("d2253ac7-9931-4560-b42f-1fccd43c952e", "300", "3rd Year")
                });
        }


        public Task<string> GetCourseLevelGuidAsync(string code)
        {
            var courseLevel = (new List<Student.Entities.CourseLevel>()
            {
                    new Student.Entities.CourseLevel("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "100", "1st Year"),
                    new Student.Entities.CourseLevel("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "200", "2nd Year"),
                    new Student.Entities.CourseLevel("d2253ac7-9931-4560-b42f-1fccd43c952e", "300", "3rd Year")
            });
            return Task.FromResult(courseLevel.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<Student.Entities.CourseStatuses>> GetCourseStatusesAsync()
        {
            return this.GetCourseStatusesAsync(false);
        }

        public Task<IEnumerable<Student.Entities.CourseStatuses>> GetCourseStatusesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CourseStatuses>>(new List<Student.Entities.CourseStatuses>()
                {
                    new Student.Entities.CourseStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "A", "Active")    { Status =  CourseStatus.Active },
                    new Student.Entities.CourseStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "I", "Inactive")  { Status =  CourseStatus.Active },
                    new Student.Entities.CourseStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "P", "Pending")   { Status =  CourseStatus.Active },
                    new Student.Entities.CourseStatuses("c04c8158-9ce0-4dd7-bce5-5e7c3e14161c", "O", "Obsolete")  { Status =  CourseStatus.Terminated },
                    new Student.Entities.CourseStatuses("dbd214f8-7dfe-4f8d-bd31-77e8e217f33c", "C", "Cancelled") { Status =  CourseStatus.Unknown }
                });
        }

        public Task<string> GetCourseStatusGuidAsync(string code)
        {
            var courseStatuses = (new List<Student.Entities.CourseStatuses>()
                {
                   new Student.Entities.CourseStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "A", "Active")    { Status =  CourseStatus.Active },
                    new Student.Entities.CourseStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "I", "Inactive")  { Status =  CourseStatus.Active },
                    new Student.Entities.CourseStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "P", "Pending")   { Status =  CourseStatus.Active },
                    new Student.Entities.CourseStatuses("c04c8158-9ce0-4dd7-bce5-5e7c3e14161c", "O", "Obsolete")  { Status =  CourseStatus.Terminated },
                    new Student.Entities.CourseStatuses("dbd214f8-7dfe-4f8d-bd31-77e8e217f33c", "C", "Cancelled") { Status =  CourseStatus.Unknown }
                });
            return Task.FromResult(courseStatuses.FirstOrDefault(c => c.Code == code).Guid);

        }

        public Task<IEnumerable<Student.Entities.CourseTitleType>> GetCourseTitleTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CourseTitleType>>(new List<Student.Entities.CourseTitleType>()
                {
                    new Student.Entities.CourseTitleType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LONG", "Long Title"),
                    new Student.Entities.CourseTitleType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "SHORT", "Short Title")
                });
        }

        public Task<string> GetCourseTitleTypeGuidAsync(string code)
        {
            var courseTitleType = (new List<Student.Entities.CourseTitleType>()
                {
                   new Student.Entities.CourseTitleType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LONG", "Long Title"),
                    new Student.Entities.CourseTitleType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "SHORT", "Short Title")
                });
            return Task.FromResult(courseTitleType.FirstOrDefault(c => c.Code == code).Guid);

        }

        public Task<IEnumerable<Student.Entities.CourseTopic>> GetCourseTopicsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CourseTopic>>(new List<Student.Entities.CourseTopic>()
                {
                    new Student.Entities.CourseTopic("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Student.Entities.CourseTopic("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Student.Entities.CourseTopic("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                });
        }

        public Task<IEnumerable<Student.Entities.CredType>> GetCreditTypesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.CreditCategory>> GetCreditCategoriesAsync()
        {
            return this.GetCreditCategoriesAsync(false);

        }

        public Task<IEnumerable<Student.Entities.CreditCategory>> GetCreditCategoriesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CreditCategory>>(new List<Student.Entities.CreditCategory>()
                {
                    new CreditCategory("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education", CreditType.ContinuingEducation),
                    new CreditCategory("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional", CreditType.Institutional),
                    new CreditCategory("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer", CreditType.Transfer)
                });
        }

        public Task<string> GetCreditCategoriesGuidAsync(string code)
        {
            var creditCategories = (new List<Student.Entities.CreditCategory>()
                {
                    new CreditCategory("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education", CreditType.ContinuingEducation),
                    new CreditCategory("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional", CreditType.Institutional),
                    new CreditCategory("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer", CreditType.Transfer)
                });
            return Task.FromResult(creditCategories.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<Student.Entities.Degree>> GetDegreesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Student.Entities.EnrollmentStatus>> GetEnrollmentStatusesAsync(bool ignoreCache)
        {
            return await Task.FromResult(new List<Student.Entities.EnrollmentStatus>()
            {
                new Student.Entities.EnrollmentStatus("3cf900894jck", "A", "Active", Student.Entities.EnrollmentStatusType.active),
                new Student.Entities.EnrollmentStatus("3cf900894alk", "P", "Potential", Student.Entities.EnrollmentStatusType.inactive),
                new Student.Entities.EnrollmentStatus("3cf900894kkj", "G", "Graduated", Student.Entities.EnrollmentStatusType.complete)

            });
        }

        public Task<IEnumerable<Student.Entities.FederalCourseClassification>> GetFederalCourseClassificationsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.LocalCourseClassification>> GetLocalCourseClassificationsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.ExternalTranscriptStatus>> GetExternalTranscriptStatusesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.ExternalTranscriptStatus>>(new List<Student.Entities.ExternalTranscriptStatus>()
                {
                    new Student.Entities.ExternalTranscriptStatus("R", "Repeated"),
                    new Student.Entities.ExternalTranscriptStatus("W", "Withdrawn"),
                    new Student.Entities.ExternalTranscriptStatus("X", "Not applicable")
                });
        }

        public Task<IEnumerable<GradeSubscheme>> GetGradeSubschemesAsync()
        {
            return this.GetGradeSubschemesAsync(false);
        }

        public Task<IEnumerable<Student.Entities.GradeSubscheme>> GetGradeSubschemesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.GradeSubscheme>>(new List<Student.Entities.GradeSubscheme>()
                {
                    new Student.Entities.GradeSubscheme("CE", "Continuing Education"),
                    new Student.Entities.GradeSubscheme("GR", "Graduate"),
                    new Student.Entities.GradeSubscheme("UG", "Undergraduate"),
                    new Student.Entities.GradeSubscheme("TR", "Transfer")
                });
        }

        public Task<IEnumerable<Student.Entities.GradeScheme>> GetGradeSchemesAsync()
        {
            return this.GetGradeSchemesAsync(false);
        }

        public Task<IEnumerable<Student.Entities.GradeScheme>> GetGradeSchemesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.GradeScheme>>(new List<Student.Entities.GradeScheme>()
                {
                    new Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today },
                    new Student.Entities.GradeScheme("37178aab-a6e8-4d1e-ae27-eca1f7b33363", "TR", "Transfer")
                    { EffectiveStartDate = DateTime.Today }
                });
        }

        public Task<string> GetGradeSchemeGuidAsync(string code)
        {
            var gradeSchemes = (new List<Student.Entities.GradeScheme>()
                {
                    new Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today },
                    new Student.Entities.GradeScheme("37178aab-a6e8-4d1e-ae27-eca1f7b33363", "TR", "Transfer")
                    { EffectiveStartDate = DateTime.Today }
                });
            return Task.FromResult(gradeSchemes.FirstOrDefault(c => c.Code == code).Guid);

        }

        public Task<IEnumerable<Student.Entities.InstructionalMethod>> GetInstructionalMethodsAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.InstructionalMethod>>(new List<Student.Entities.InstructionalMethod>()
                {
                    new Student.Entities.InstructionalMethod("bb66b971-3ee0-4477-9bb7-539721f93434" ,"02", "Lecture And/Or Discussion", false),
                    new Student.Entities.InstructionalMethod("5aeebc5c-c973-4f83-be4b-f64c95002124", "10", "Learning Laboratory", false),
                    new Student.Entities.InstructionalMethod("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "70", "Radio Course", false),
                });
        }

        public Task<IEnumerable<Student.Entities.InstructionalMethod>> GetInstructionalMethodsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.InstructionalMethod>>(new List<Student.Entities.InstructionalMethod>()
                {
                    new Student.Entities.InstructionalMethod("bb66b971-3ee0-4477-9bb7-539721f93434" ,"02", "Lecture And/Or Discussion", false),
                    new Student.Entities.InstructionalMethod("5aeebc5c-c973-4f83-be4b-f64c95002124", "10", "Learning Laboratory", false),
                    new Student.Entities.InstructionalMethod("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "70", "Radio Course", false)
                });
        }

        public Task<string> GetInstructionalMethodGuidAsync(string code)
        {
            var instructionalMethod = (new List<Student.Entities.InstructionalMethod>()
            {
                    new Student.Entities.InstructionalMethod("bb66b971-3ee0-4477-9bb7-539721f93434" ,"02", "Lecture And/Or Discussion", false),
                    new Student.Entities.InstructionalMethod("5aeebc5c-c973-4f83-be4b-f64c95002124", "10", "Learning Laboratory", false),
                    new Student.Entities.InstructionalMethod("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "70", "Radio Course", false)
            });
            return Task.FromResult(instructionalMethod.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<Student.Entities.AdministrativeInstructionalMethod>> GetAdministrativeInstructionalMethodsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AdministrativeInstructionalMethod>>(new List<Student.Entities.AdministrativeInstructionalMethod>()
                {
                    new Student.Entities.AdministrativeInstructionalMethod("bb66b971-3ee0-4477-9bb7-539721f93434" ,"02", "Lecture And/Or Discussion", "D8CED21A-F220-4F79-9544-706E13B51972"),
                    new Student.Entities.AdministrativeInstructionalMethod("5aeebc5c-c973-4f83-be4b-f64c95002124", "10", "Learning Laboratory", "705F052C-7B63-492D-A7CA-5769CE003274"),
                    new Student.Entities.AdministrativeInstructionalMethod("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "70", "Radio Course", "67B0664B-0650-4C88-ACC6-FB0C689CB519")
                });
        }

        public Task<string> GetAdministrativeInstructionalMethodGuidAsync(string code)
        {
            var instrMethods = (new List<Student.Entities.AdministrativeInstructionalMethod>()
                {
                    new Student.Entities.AdministrativeInstructionalMethod("bb66b971-3ee0-4477-9bb7-539721f93434" ,"02", "Lecture And/Or Discussion", "D8CED21A-F220-4F79-9544-706E13B51972"),
                    new Student.Entities.AdministrativeInstructionalMethod("5aeebc5c-c973-4f83-be4b-f64c95002124", "10", "Learning Laboratory", "705F052C-7B63-492D-A7CA-5769CE003274"),
                    new Student.Entities.AdministrativeInstructionalMethod("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "70", "Radio Course", "67B0664B-0650-4C88-ACC6-FB0C689CB519")
                });
            return Task.FromResult(instrMethods.FirstOrDefault(c => c.Code == code).Guid);
        }
    

        public Task<IEnumerable<IntgTestPercentileType>> GetIntgTestPercentileTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.IntgTestPercentileType>>(new List<Student.Entities.IntgTestPercentileType>()
            {
                new IntgTestPercentileType("792b6834-2f9c-409c-8afa-e0081972adb4", "1", "1st percentile"),
                new IntgTestPercentileType("ab8395f3-663d-4d09-b3f6-af28668dc362", "2", "2nd percentile")
            });
        }

        public Task<string> GetIntgTestPercentileTypesGuidAsync(string code)
        {
            var IntgTestPercentileTypes =  (new List<Student.Entities.IntgTestPercentileType>()
            {
                new IntgTestPercentileType("792b6834-2f9c-409c-8afa-e0081972adb4", "1", "1st percentile"),
                new IntgTestPercentileType("ab8395f3-663d-4d09-b3f6-af28668dc362", "2", "2nd percentile")
            });
            return Task.FromResult(IntgTestPercentileTypes.FirstOrDefault(c => c.Code == code).Guid);
        }

        public async Task<IEnumerable<Student.Entities.Major>> GetMajorsAsync(bool bypassCache = false)
        {
            return await Task.FromResult<IEnumerable<Student.Entities.Major>>(new List<Student.Entities.Major>()
            {
                new Student.Entities.Major("MATH", "Mathematics"){FederalCourseClassification="99.9", ActiveFlag = true},
                new Student.Entities.Major("ENGL", "English"){FederalCourseClassification="88.8", ActiveFlag = true},
                new Student.Entities.Major("ROCK", "Geology"){FederalCourseClassification="77.7", ActiveFlag = false}
            });
        }

        public Task<IEnumerable<MealPlan>> GetMealPlansAsync(bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.MealPlan>>(new List<Student.Entities.MealPlan>()
                {
                    new Student.Entities.MealPlan("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.MealPlan("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.MealPlan("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public Task<string> GetMealPlanGuidAsync(string code)
        {
            var mealPlans = (new List<Student.Entities.MealPlan>()
                {
                    new Student.Entities.MealPlan("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.MealPlan("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.MealPlan("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
            return Task.FromResult(mealPlans.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<Student.Entities.MealType>> GetMealTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.MealType>>(new List<Student.Entities.MealType>()
                {
                    new Student.Entities.MealType("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.MealType("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.MealType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public async Task<IEnumerable<Student.Entities.Minor>> GetMinorsAsync(bool bypassCache = false)
        {
            return await Task.FromResult<IEnumerable<Student.Entities.Minor>>(new List<Student.Entities.Minor>()
                {
                    new Student.Entities.Minor("MATH", "Mathematics"){FederalCourseClassification="99.9"},
                    new Student.Entities.Minor("ENGL", "English"){FederalCourseClassification="88.8"},
                    new Student.Entities.Minor("ROCK", "Geology"){FederalCourseClassification="77.7"},
                    new Student.Entities.Minor("HIST", "History"){FederalCourseClassification="66.6"}
                });
        }

        public async Task<IEnumerable<NonCourseCategories>> GetNonCourseCategoriesAsync(bool ignoreCache = false)
        {
            return await Task.FromResult<IEnumerable<Student.Entities.NonCourseCategories>>(new List<Student.Entities.NonCourseCategories>()
                {
                    new Student.Entities.NonCourseCategories("bb66b971-3ee0-4477-9bb7-539721f93434" ,"02", "Lecture And/Or Discussion"),
                    new Student.Entities.NonCourseCategories("5aeebc5c-c973-4f83-be4b-f64c95002124", "10", "Learning Laboratory"),
                    new Student.Entities.NonCourseCategories("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "70", "Radio Course")
                });
        }

        public async Task<IEnumerable<NonCourseGradeUses>> GetNonCourseGradeUsesAsync(bool ignoreCache)
        {
            return await Task.FromResult<IEnumerable<Student.Entities.NonCourseGradeUses>>(new List<Student.Entities.NonCourseGradeUses>()
                {
                    new Student.Entities.NonCourseGradeUses("bb66b971-3ee0-4477-9bb7-539721f93434" ,"LAST", "Use most recent grade"),
                    new Student.Entities.NonCourseGradeUses("5aeebc5c-c973-4f83-be4b-f64c95002124", "BEST", "Use best grade"),
                    new Student.Entities.NonCourseGradeUses("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "SUB", "Use best subtest")
                });
        }

        public Task<IEnumerable<Student.Entities.SectionGradeType>> GetSectionGradeTypesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.SectionGradeType>>(new List<Student.Entities.SectionGradeType>()
                {
                    new Student.Entities.SectionGradeType("bb66b971-3ee0-4477-9bb7-539721f93434" ,"MID1", "Midterm Grade 1"),
                    new Student.Entities.SectionGradeType("5aeebc5c-c973-4f83-be4b-f64c95002124", "MID2", "Midterm Grade 2"),
                    new Student.Entities.SectionGradeType("ed8c08e5-5fcd-4acd-a8df-bcf3d091b7b8", "MID3", "Midterm Grade 2"),
                    new Student.Entities.SectionGradeType("b2f18298-dce9-4702-97f1-7081cb64c9ae", "MID4", "Midterm Grade 2"),
                    new Student.Entities.SectionGradeType("6e33503b-65d2-4e90-a745-eb25ddb59103", "MID5", "Midterm Grade 2"),
                    new Student.Entities.SectionGradeType("59cd09ef-b9d2-4044-9c1b-5e16649cfc9c", "MID6", "Midterm Grade 2"),
                    new Student.Entities.SectionGradeType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "FINAL", "Final Grade"),
                    new Student.Entities.SectionGradeType("ca7f5282-445c-46be-b40e-049768ef6826", "VERIFIED", "Verified Grade"),
                });
        }

        public Task<IEnumerable<Student.Entities.SectionGradeType>> GetSectionGradeTypesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Student.Entities.SectionRegistrationStatusItem>> SectionRegistrationStatusesAsync()
        {
            return await Task.FromResult(new List<Student.Entities.SectionRegistrationStatusItem>()
            {
                new Student.Entities.SectionRegistrationStatusItem("3cf900894jck", "N", "New")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.Registered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Registered
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894alk", "A", "Add")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.Registered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Registered
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894kkj", "D", "Dropped")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Dropped
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894els", "W", "Withdrawn")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Withdrawn
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894srm", "X", "Deleted")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Dropped
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894jck", "C", "Canceled")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Canceled
                    }
                }
            });
        }

        public async Task<IEnumerable<Student.Entities.SectionRegistrationStatusItem>> GetStudentAcademicCreditStatusesAsync(bool ignoreCache = false)
        {
            return await Task.FromResult(new List<Student.Entities.SectionRegistrationStatusItem>()
            {
                new Student.Entities.SectionRegistrationStatusItem("3cf900894jck", "N", "New")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.Registered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Registered
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894alk", "A", "Add")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.Registered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Registered
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894kkj", "D", "Dropped")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Dropped
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894els", "W", "Withdrawn")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Withdrawn
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894srm", "X", "Deleted")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Dropped
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894jck", "C", "Canceled")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Canceled
                    }
                },

            });
        }



        public Task<string> GetStudentAcademicCreditStatusesGuidAsync(string code)
        {
            List<Student.Entities.SectionRegistrationStatusItem> statuses = new List<Student.Entities.SectionRegistrationStatusItem>()
                {
                   new Student.Entities.SectionRegistrationStatusItem("3cf900894jck", "N", "New")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.Registered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Registered
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894alk", "A", "Add")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.Registered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Registered
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894kkj", "D", "Dropped")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Dropped
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894els", "W", "Withdrawn")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Withdrawn
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894srm", "X", "Deleted")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Dropped
                    }
                },
                new Student.Entities.SectionRegistrationStatusItem("3cf900894jck", "C", "Canceled")
                {
                    Status = new Student.Entities.SectionRegistrationStatus()
                    {
                        RegistrationStatus = Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Student.Entities.RegistrationStatusReason.Canceled
                    }
                }
                };
            return Task.FromResult(statuses.FirstOrDefault(c => c.Code == code).Guid);
        }
        public Task<IEnumerable<Student.Entities.SectionStatusCode>> GetSectionStatusCodesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.SectionStatusCode>>(new List<Student.Entities.SectionStatusCode>()
                {
                    new Student.Entities.SectionStatusCode("A", "Active", Student.Entities.SectionStatus.Active),
                    new Student.Entities.SectionStatusCode("I", "Inactive", Student.Entities.SectionStatus.Inactive),
                    new Student.Entities.SectionStatusCode("C", "Cancelled", Student.Entities.SectionStatus.Cancelled),
                     new Student.Entities.SectionStatusCode("O", "Open", Student.Entities.SectionStatus.Active),
                        });
        }

        public Task<IEnumerable<Student.Entities.Specialization>> GetSpecializationsAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.Specialization>>(new List<Student.Entities.Specialization>()
                {
                    new Student.Entities.Specialization("specialization0", "Specialization 0"),
                    new Student.Entities.Specialization("specialization1", "Specialization 1"),
                    new Student.Entities.Specialization("specialization2", "Specialization 2"),
                    new Student.Entities.Specialization("specialization3", "Specialization 3"),
                    new Student.Entities.Specialization("specialization4", "Specialization 4")
                });
        }

        public Task<IEnumerable<Student.Entities.StudentStatus>> GetStudentStatusesAsync(bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.StudentStatus>>(new List<Student.Entities.StudentStatus>()
                {
                    new Student.Entities.StudentStatus("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Student.Entities.StudentStatus("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Student.Entities.StudentStatus("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Student.Entities.StudentStatus("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
        }

        public Task<string> GetStudentStatusesGuidAsync(string code)
        {
            List<Student.Entities.StudentStatus> studentStatuses = new List<Student.Entities.StudentStatus>()
                {
                    new Student.Entities.StudentStatus("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Student.Entities.StudentStatus("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Student.Entities.StudentStatus("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Student.Entities.StudentStatus("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                };
            return Task.FromResult(studentStatuses.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<Student.Entities.StudentType>> GetStudentTypesAsync(bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.StudentType>>(new List<Student.Entities.StudentType>()
                {
                    new Student.Entities.StudentType("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Student.Entities.StudentType("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Student.Entities.StudentType("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Student.Entities.StudentType("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
        }

        public Task<string> GetStudentTypesGuidAsync(string code)
        {
            List<Student.Entities.StudentType> studentTypes = new List<Student.Entities.StudentType>()
                {
                    new Student.Entities.StudentType("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Student.Entities.StudentType("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Student.Entities.StudentType("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Student.Entities.StudentType("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                };
            return Task.FromResult(studentTypes.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<Student.Entities.StudentLoad>> GetStudentLoadsAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.StudentLoad>>(new List<Student.Entities.StudentLoad>()
                {
                    new Student.Entities.StudentLoad("F", "Full Time"),
                    new Student.Entities.StudentLoad("P", "Part Time"),
                    new Student.Entities.StudentLoad("O", "Overload"),
                    new Student.Entities.StudentLoad("H", "Half Time"),
                    new Student.Entities.StudentLoad("L", "Less than Half Time")
                });
        }
        public Task<string> GetUnidataFormattedDate(string date)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.NonAcademicAttendanceEventType>> GetNonAcademicAttendanceEventTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.NonAcademicAttendanceEventType>>(new List<Student.Entities.NonAcademicAttendanceEventType>()
            {
                    new Student.Entities.NonAcademicAttendanceEventType("CHAP", "Chapel"),
                    new Student.Entities.NonAcademicAttendanceEventType("COMM", "Community Service")
                });
        }

        public Task<IEnumerable<Student.Entities.Subject>> GetSubjectsAsync()
        {
            return GetSubjectsAsync(false);
        }

        public Task<IEnumerable<Student.Entities.Subject>> GetSubjectsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.Subject>>(new List<Student.Entities.Subject>()
                {
                    new Student.Entities.Subject("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "AAA", "Hello World", true),
                    new Student.Entities.Subject("bd54668d-50d9-416c-81e9-2318e88571a1", "ACCT", "Accounting", false),
                    new Student.Entities.Subject("5eed2bea-8948-439b-b5c5-779d84724a38", "AGBU", "Agriculture Business", true),
                    new Student.Entities.Subject("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "AGME", "Agriculture Mechanics", false)
                });
        }

        public Task<string> GetSubjectGuidAsync(string code)
        {
            var subjects = (new List<Student.Entities.Subject>()
                {
                    new Student.Entities.Subject("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "AAA", "Hello World", true),
                    new Student.Entities.Subject("bd54668d-50d9-416c-81e9-2318e88571a1", "ACCT", "Accounting", false),
                    new Student.Entities.Subject("5eed2bea-8948-439b-b5c5-779d84724a38", "AGBU", "Agriculture Business", true),
                    new Student.Entities.Subject("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "AGME", "Agriculture Mechanics", false)
                });
            return Task.FromResult(subjects.FirstOrDefault(c => c.Code == code).Guid);
        }



        public Task<IEnumerable<Student.Entities.TopicCode>> GetTopicCodesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.TopicCode>>(new List<Student.Entities.TopicCode>()
                {
                    new Student.Entities.TopicCode("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "REL", "Religion"),
                    new Student.Entities.TopicCode("bd54668d-50d9-416c-81e9-2318e88571a1", "ACCT", "Accounting"),
                    new Student.Entities.TopicCode("5eed2bea-8948-439b-b5c5-779d84724a38", "AGBU", "Agriculture Business"),
                    new Student.Entities.TopicCode("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "AGME", "Agriculture Mechanics")
                });
        }

        public Task<string> GetTopicCodeGuidAsync(string code)
        {
            var topicCode = (new List<Student.Entities.TopicCode>()
                {
                    new Student.Entities.TopicCode("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "REL", "Religion"),
                    new Student.Entities.TopicCode("bd54668d-50d9-416c-81e9-2318e88571a1", "ACCT", "Accounting"),
                    new Student.Entities.TopicCode("5eed2bea-8948-439b-b5c5-779d84724a38", "AGBU", "Agriculture Business"),
                    new Student.Entities.TopicCode("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "AGME", "Agriculture Mechanics")
                });
            return Task.FromResult(topicCode.FirstOrDefault(c => c.Code == code).Guid);
        }


        public Task<IEnumerable<Student.Entities.TranscriptCategory>> GetTranscriptCategoriesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.TranscriptCategory>>(new List<Student.Entities.TranscriptCategory>()
                {
                    new Student.Entities.TranscriptCategory("M", "Major Course"),
                    new Student.Entities.TranscriptCategory("C", "College Prep")
                });
        }

        public Task<IEnumerable<Student.Entities.Test>> GetTestsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.TestSource>> GetTestSourcesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.TestSource>>(new List<Student.Entities.TestSource>()
                {
                    new Student.Entities.TestSource("c5bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "HS", "High School Transcript"),
                    new Student.Entities.TestSource("eh54668d-50d9-416c-81e9-2318e88571a1", "SF", "Self Reported"),
                    new Student.Entities.TestSource("3fgd2bea-8948-439b-b5c5-779d84724a38", "LSAT", "LSAT"),
                    new Student.Entities.TestSource("22f74c63-df5b-4e56-8ef0-e871ccc789e8", "ACT", "ACT")
                });
        }

        //Domain.Student.Entities.StudentCohort
        public Task<IEnumerable<Student.Entities.StudentCohort>> GetAllStudentCohortAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.StudentCohort>>(new List<Student.Entities.StudentCohort>()
                {
                    new Student.Entities.StudentCohort("c5bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "HS", "High School Transcript"){ CohortType = "FED" },
                    new Student.Entities.StudentCohort("eh54668d-50d9-416c-81e9-2318e88571a1", "SF", "Self Reported"),
                    new Student.Entities.StudentCohort("3fgd2bea-8948-439b-b5c5-779d84724a38", "LSAT", "LSAT"),
                    new Student.Entities.StudentCohort("22f74c63-df5b-4e56-8ef0-e871ccc789e8", "ACT", "ACT"){ CohortType = "FED" }
                });
        }

        public Task<string> GetTestSourcesGuidAsync(string code)
        {
            var testSource = (new List<Student.Entities.TestSource>()
                {
                    new Student.Entities.TestSource("c5bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "HS", "High School Transcript"),
                    new Student.Entities.TestSource("eh54668d-50d9-416c-81e9-2318e88571a1", "SF", "Self Reported"),
                    new Student.Entities.TestSource("3fgd2bea-8948-439b-b5c5-779d84724a38", "LSAT", "LSAT"),
                    new Student.Entities.TestSource("22f74c63-df5b-4e56-8ef0-e871ccc789e8", "ACT", "ACT")
                });
            return Task.FromResult(testSource.FirstOrDefault(c => c.Code == code).Guid);
        }


        public Task<IEnumerable<Student.Entities.NoncourseStatus>> GetNoncourseStatusesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.NoncourseStatus>>(new List<Student.Entities.NoncourseStatus>()
                {
                    new Student.Entities.NoncourseStatus("AC", "Accepted", NoncourseStatusType.Accepted),
                    new Student.Entities.NoncourseStatus("NC", "Needs Confirmation", NoncourseStatusType.None),
                    new Student.Entities.NoncourseStatus("NT", "Notational Only", NoncourseStatusType.Notational),
                    new Student.Entities.NoncourseStatus("WD", "Withdrawn", NoncourseStatusType.Withdrawn)
                });
        }

        public Task<IEnumerable<Student.Entities.WaitlistStatusCode>> GetWaitlistStatusCodesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Student.Entities.SectionTransferStatus>> GetSectionTransferStatusesAsync()
        {
            return await Task.FromResult(new List<Student.Entities.SectionTransferStatus>()
                {
                    new Student.Entities.SectionTransferStatus("TS1", "Transfer Status 1"),
                    new Student.Entities.SectionTransferStatus("TS2", "Transfer Status 2"),
                    new Student.Entities.SectionTransferStatus("TS3", "Transfer Status 3")
                });
        }

        public Task<IEnumerable<Student.Entities.StudentWaiverReason>> GetStudentWaiverReasonsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.PetitionStatus>> GetPetitionStatusesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.StudentPetitionReason>> GetStudentPetitionReasonsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.GownSize>> GetGownSizesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.GownSize>>(new List<Student.Entities.GownSize>()
                {
                    new Student.Entities.GownSize("M42", "Men's 42 Regular"),
                    new Student.Entities.GownSize("M48", "Men's 48 Regular"),
                    new Student.Entities.GownSize("W8", "Women's Size 8"),
                    new Student.Entities.GownSize("W10", "Women's Size 10")
                });
        }
        public Task<IEnumerable<CapSize>> GetCapSizesAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.CapSize>>(new List<Student.Entities.CapSize>()
                {
                    new Student.Entities.CapSize("SMALL", "small"),
                    new Student.Entities.CapSize("MEDIUM", "medium"),
                    new Student.Entities.CapSize("LARGE", "large"),
                    new Student.Entities.CapSize("X-LARGE", "extra large")
                });
        }

        public Task<IEnumerable<SessionCycle>> GetSessionCyclesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<YearlyCycle>> GetYearlyCyclesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<HoldRequestType>> GetHoldRequestTypesAsync()
        {
            throw new NotImplementedException();
        }

        #region IStudentReferenceDataRepository Members


        public Task<IEnumerable<AccountingCode>> GetAccountingCodesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DistributionMethod>> GetDistrMethodCodesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        #endregion


        public Task<IEnumerable<CampusOrganization>> GetCampusOrganizationsAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentClassification>> GetAllStudentClassificationAsync(bool bypassCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.StudentClassification>>(new List<Student.Entities.StudentClassification>()
                {
                    new Student.Entities.StudentClassification("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Student.Entities.StudentClassification("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Student.Entities.StudentClassification("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Student.Entities.StudentClassification("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
        }

        public Task<string> GetStudentClassificationGuidAsync(string code)
        {
            List<Student.Entities.StudentClassification> studentClasses = new List<Student.Entities.StudentClassification>()
                {
                    new Student.Entities.StudentClassification ("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Student.Entities.StudentClassification ("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Student.Entities.StudentClassification ("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Student.Entities.StudentClassification ("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                };
            return Task.FromResult(studentClasses.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<ScheduleTerm>> GetAllScheduleTermsAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }


        public Task<IEnumerable<WithdrawReason>> GetWithdrawReasonsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.WithdrawReason>>(new List<Student.Entities.WithdrawReason>()
                {
                    new Student.Entities.WithdrawReason("761597be-0a12-4aa8-8ffe-afc04b62da41", "AC", "Academic Reasons"),
                    new Student.Entities.WithdrawReason("8cc60bb6-1e0e-45f1-bf10-b53d6809275e", "FP", "Financial Problems"),
                    new Student.Entities.WithdrawReason("6196cc8c-6e2c-4bb5-8859-b2553b24c772", "MILIT", "Serve In The Armed Forces"),
               });
        }


        public Task<IEnumerable<AdmissionDecisionType>> GetAdmissionDecisionTypesAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AdmissionResidencyType>> GetAdmissionResidencyTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.AdmissionResidencyType>>(new List<Student.Entities.AdmissionResidencyType>()
                {
                    new Domain.Student.Entities.AdmissionResidencyType("884a59d1-20e5-43af-94e3-f1504230bbbc", "CODE1", "DESC1"),
                    new Domain.Student.Entities.AdmissionResidencyType("bb336acf-1926-4b12-8daf-d8720280498f", "CODE2", "DESC2"),
                    new Domain.Student.Entities.AdmissionResidencyType("d118f007-c914-465e-80dc-49d39209b24f", "CODE3", "DESC3")
                });
        }

        public Task<IEnumerable<FacultySpecialStatuses>> GetFacultySpecialStatusesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FacultySpecialStatuses>>(new List<Student.Entities.FacultySpecialStatuses>()
                {
                    new Domain.Student.Entities.FacultySpecialStatuses("884a59d1-20e5-43af-94e3-f1504230bbbc", "CODE1", "DESC1"),
                    new Domain.Student.Entities.FacultySpecialStatuses("bb336acf-1926-4b12-8daf-d8720280498f", "CODE2", "DESC2"),
                    new Domain.Student.Entities.FacultySpecialStatuses("d118f007-c914-465e-80dc-49d39209b24f", "CODE3", "DESC3")
                });
        }


        public Task<IEnumerable<FacultyContractTypes>> GetFacultyContractTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FacultyContractTypes>>(new List<Student.Entities.FacultyContractTypes>()
                {
                    new Domain.Student.Entities.FacultyContractTypes("884a59d1-20e5-43af-94e3-f1504230bbbc", "CODE1", "DESC1"),
                    new Domain.Student.Entities.FacultyContractTypes("bb336acf-1926-4b12-8daf-d8720280498f", "CODE2", "DESC2"),
                    new Domain.Student.Entities.FacultyContractTypes("d118f007-c914-465e-80dc-49d39209b24f", "CODE3", "DESC3")
                });
        }



        public Task<IEnumerable<BillingOverrideReasons>> GetBillingOverrideReasonsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.BillingOverrideReasons>>(new List<Student.Entities.BillingOverrideReasons>()
                {
                    new Domain.Student.Entities.BillingOverrideReasons("884a59d1-20e5-43af-94e3-f1504230bbbc", "CODE1", "DESC1"),
                    new Domain.Student.Entities.BillingOverrideReasons("bb336acf-1926-4b12-8daf-d8720280498f", "CODE2", "DESC2"),
                    new Domain.Student.Entities.BillingOverrideReasons("d118f007-c914-465e-80dc-49d39209b24f", "CODE3", "DESC3")
                });
        }

        public Task<IEnumerable<FloorCharacteristics>> GetFloorCharacteristicsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FloorCharacteristics>>(new List<Student.Entities.FloorCharacteristics>()
                {
                    new Domain.Student.Entities.FloorCharacteristics("884a59d1-20e5-43af-94e3-f1504230bbbc", "CODE1", "DESC1"),
                    new Domain.Student.Entities.FloorCharacteristics("bb336acf-1926-4b12-8daf-d8720280498f", "CODE2", "DESC2"),
                    new Domain.Student.Entities.FloorCharacteristics("d118f007-c914-465e-80dc-49d39209b24f", "CODE3", "DESC3")
                });
        }

        public Task<IEnumerable<RoommateCharacteristics>> GetRoommateCharacteristicsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.RoommateCharacteristics>>(new List<Student.Entities.RoommateCharacteristics>()
                {
                    new Domain.Student.Entities.RoommateCharacteristics("884a59d1-20e5-43af-94e3-f1504230bbbc", "CODE1", "DESC1"),
                    new Domain.Student.Entities.RoommateCharacteristics("bb336acf-1926-4b12-8daf-d8720280498f", "CODE2", "DESC2"),
                    new Domain.Student.Entities.RoommateCharacteristics("d118f007-c914-465e-80dc-49d39209b24f", "CODE3", "DESC3")
                });
        }
        public Task<IEnumerable<MealPlanRates>> GetMealPlanRatesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetHostCountryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentResidentialCategories>> GetStudentResidentialCategoriesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.StudentResidentialCategories>>(new List<StudentResidentialCategories>()
            {
                    new StudentResidentialCategories("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new StudentResidentialCategories("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new StudentResidentialCategories("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
            });
        }
        public Task<IEnumerable<HousingResidentType>> GetHousingResidentTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.HousingResidentType>>(new List<Student.Entities.HousingResidentType>()
                {
                    new Student.Entities.HousingResidentType("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.HousingResidentType("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.HousingResidentType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }


        public Task<IEnumerable<RoomRate>> GetRoomRatesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AttendanceTypes>> GetAttendanceTypesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CourseType>> GetCourseTypesAsync(bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.CourseType>>(new List<Student.Entities.CourseType>()
                {
                    new Student.Entities.CourseType("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.CourseType("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.CourseType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", " "),
                    new Student.Entities.CourseType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE4", "Desc 4")
                });
        }

        public Task<string> GetCourseTypeGuidAsync(string code)
        {
            var courseType = (new List<Student.Entities.CourseType>()
            {
                    new Student.Entities.CourseType("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.CourseType("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.CourseType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", " "),
                    new Student.Entities.CourseType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE4", "Desc 4")
            });
            return Task.FromResult(courseType.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<SectionStatuses>> GetSectionStatusesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ArCategory>> GetArCategoriesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AccountReceivableDepositType>> GetAccountReceivableDepositTypesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Distribution2>> GetDistributionsAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ContactMeasure>> GetContactMeasuresAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CampusOrgRole>> CampusOrgRolesAsync()
        {
            throw new NotImplementedException();
        }


        public Task<IEnumerable<ChargeAssessmentMethod>> GetChargeAssessmentMethodsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.ChargeAssessmentMethod>>(new List<Student.Entities.ChargeAssessmentMethod>()
                {
                    new Student.Entities.ChargeAssessmentMethod("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.ChargeAssessmentMethod("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.ChargeAssessmentMethod("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }


        public Task<IEnumerable<CourseTransferStatus>> GetCourseTransferStatusesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.DropReason>> GetDropReasonsAsync()
        {
            return Task.FromResult<IEnumerable<Student.Entities.DropReason>>(new List<Student.Entities.DropReason>()
                {
                    new Student.Entities.DropReason("C", "Changed my mind",null),
                    new Student.Entities.DropReason("U", "Undecided","S"),
                    new Student.Entities.DropReason("W", "My Own wish","aaa"),
                });
        }

        public Task<IEnumerable<Student.Entities.SapType>> GetSapTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.SapType>>(new List<Student.Entities.SapType>()
                {
                    new Student.Entities.SapType("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.SapType("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.SapType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", " ")
                });
        }

        public Task<IEnumerable<Student.Entities.SapStatuses>> GetSapStatusesAsync(string restrictedVisibilityValue = "", bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.SapStatuses>>(new List<Student.Entities.SapStatuses>()
                {
                    new Student.Entities.SapStatuses("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.SapStatuses("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.SapStatuses("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", " "),
                    new Student.Entities.SapStatuses("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE4", "Desc 4")
                });
        }

        public Task<IEnumerable<SectionTitleType>> GetSectionTitleTypesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SectionDescriptionType>> GetSectionDescriptionTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.SectionDescriptionType>>(new List<Student.Entities.SectionDescriptionType>()
                {
                    new Student.Entities.SectionDescriptionType("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.SectionDescriptionType("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.SectionDescriptionType("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public Task<IEnumerable<FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FinancialAidFundCategory>>(new List<Student.Entities.FinancialAidFundCategory>()
                {
                    new Student.Entities.FinancialAidFundCategory("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.FinancialAidFundCategory("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.FinancialAidFundCategory("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public Task<IEnumerable<FinancialAidFundClassification>> GetFinancialAidFundClassificationsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FinancialAidFundClassification>>(new List<Student.Entities.FinancialAidFundClassification>()
                {
                    new Student.Entities.FinancialAidFundClassification("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.FinancialAidFundClassification("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.FinancialAidFundClassification("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public Task<IEnumerable<Student.Entities.FinancialAidYear>> GetFinancialAidYearsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FinancialAidYear>>(new List<Student.Entities.FinancialAidYear>()
                {
                    new Student.Entities.FinancialAidYear("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1", "STATUS1"),
                    new Student.Entities.FinancialAidYear("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2", "STATUS2"),
                    new Student.Entities.FinancialAidYear("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3", "STATUS3")
                });
        }
        public Task<IEnumerable<Student.Entities.FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FinancialAidAwardPeriod>>(new List<Student.Entities.FinancialAidAwardPeriod>()
                {
                    new Student.Entities.FinancialAidAwardPeriod("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1", "STATUS1"),
                    new Student.Entities.FinancialAidAwardPeriod("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2", "STATUS2"),
                    new Student.Entities.FinancialAidAwardPeriod("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3", "STATUS3")
                });
        }

        public Task<IEnumerable<FinancialAidFund>> GetFinancialAidFundsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.FinancialAidFund>>(new List<Student.Entities.FinancialAidFund>()
                {
                    new Student.Entities.FinancialAidFund("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new Student.Entities.FinancialAidFund("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new Student.Entities.FinancialAidFund("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public Task<IEnumerable<AwardStatus>> AwardStatusesAsync(bool bypassCache = false)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAdmissionResidencyTypesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAdmissionApplicationTypesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAdmissionApplicationStatusTypesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAdmissionPopulationsGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetWithdrawReasonsGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAdmissionDecisionTypesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAdmissionDecisionTypesSPCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetApplicationInfluenceGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCareerGoalGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetApplicationSourcesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GradingTerm>> GetGradingTermsAsync(bool bypassCache = false)
        {
            throw new NotImplementedException();
        }

        public Task<AdmissionDecisionType> GetAdmissionDecisionTypeByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetStudentCohortGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSectionTitleTypesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSectionDescriptionTypesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all education goals
        /// </summary>
        /// <param name="ignoreCache">Flag indicating whether or not to bypass the cache</param>
        /// <returns>Collection of <see cref="EducationGoal">education goals</see></returns>
        public Task<IEnumerable<EducationGoal>> GetAllEducationGoalsAsync(bool bypassCache = false)
        {
            return Task.FromResult<IEnumerable<EducationGoal>>(new List<EducationGoal>()
                {
                    new EducationGoal("BA", "Bachelor's Degree"),
                    new EducationGoal("MA", "Master's Degree"),
                });
        }

        /// <summary>
        /// Returns all registration reasons
        /// </summary>
        /// <param name="ignoreCache">Flag indicating whether or not to bypass the cache</param>
        /// <returns>Collection of <see cref="RegistrationReason">registration reasons</see></returns>
        public Task<IEnumerable<RegistrationReason>> GetRegistrationReasonsAsync(bool bypassCache = false)
        {
            return Task.FromResult<IEnumerable<RegistrationReason>>(new List<RegistrationReason>()
                {
                    new RegistrationReason("CURRENTJOB", "Need for my current job"),
                    new RegistrationReason("FUN", "Just for fun"),
                });
        }

        /// <summary>
        /// Returns all registration marketing sources
        /// </summary>
        /// <param name="ignoreCache">Flag indicating whether or not to bypass the cache</param>
        /// <returns>Collection of <see cref="RegistrationMarketingSource">registration marrketing sources</see></returns>
        public Task<IEnumerable<RegistrationMarketingSource>> GetRegistrationMarketingSourcesAsync(bool bypassCache = false)
        {
            return Task.FromResult<IEnumerable<RegistrationMarketingSource>>(new List<RegistrationMarketingSource>()
                {
                    new RegistrationMarketingSource("NEWSAD", "From a newspaper ad"),
                    new RegistrationMarketingSource("WEB", "I found it on the web"),
                });
        }

        public Task<string> GetAccountReceivableTypesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAccountReceivableTypesCodeFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDistrMethodGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDistrMethodCodeFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetHeadcountInclusionListAsync(bool ignoreCache = false)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAdvisorTypeGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApplicationInfluence>> GetApplicationInfluencesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<EducationGoals>> GetEducationGoalsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<Student.Entities.EducationGoals>>(new List<Student.Entities.EducationGoals>()
                {
                    new EducationGoals("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new EducationGoals("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new EducationGoals("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                });
        }

        public Task<string> GetEducationGoalGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFinancialAidYearsGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<FinancialAidYear> GetFinancialAidYearAsync( string sourceGuid )
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FinancialAidMaritalStatus>> GetFinancialAidMaritalStatusesAsync( bool bypassCache = false, params string[] years )
        {
            throw new NotImplementedException();
        }

        public Task<FinancialAidMaritalStatus> GetFinancialAidMaritalStatusAsync( string year, string code )
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IntentToWithdrawCode>> GetIntentToWithdrawCodesAsync(bool bypassCache = false)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<StudentReleaseAccess>> GetStudentReleaseAccessCodesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AidApplicationType>> GetAidApplicationTypesAsync(bool bypassCache = false)
        {
            throw new NotImplementedException();
        }
    }
}