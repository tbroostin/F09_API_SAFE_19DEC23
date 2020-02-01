// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EducationHistoryRepository : BaseColleagueRepository, IEducationHistoryRepository
    {
        private ApplValcodes InstitutionTypes;
        private ApplValcodes GraduationTypes;

        public EducationHistoryRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }
        #region Validation Tables
        /// <summary>
        /// Return the Validation Table InstTypes for determination of High School or College
        /// within the Institutions Attended data.
        /// </summary>
        /// <returns>Validation Table Object for Institution Types</returns>
        private async Task<ApplValcodes> GetInstitutionTypesAsync()
        {
            if (InstitutionTypes != null)
            {
                return InstitutionTypes;
            }

            InstitutionTypes =await GetOrAddToCacheAsync<ApplValcodes>("InstitutionTypes",
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
            return InstitutionTypes;
        }
        /// <summary>
        /// Return GraduationTypes validation table
        /// </summary>
        /// <returns>Validation Table Object for Graduation Types</returns>
        private async Task<ApplValcodes> GetGraduationTypesAsync()
        {
            if (GraduationTypes != null)
            {
                return GraduationTypes;
            }

            GraduationTypes = await GetOrAddToCacheAsync<ApplValcodes>("GraduationTypes",
                async () =>
                {
                    ApplValcodes typeTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "GRADUATION.TYPES");
                    if (typeTable == null)
                    {
                        var errorMessage = "Unable to access GRADUATION.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return typeTable;
                }, Level1CacheTimeoutValue);
            return GraduationTypes;
        }
        public async Task<IEnumerable<OtherHonor>> GetOtherHonorsAsync()
        {
            return await GetCodeItemAsync<OtherHonors, OtherHonor>("AllOtherHonors", "OTHER.HONORS",
               a => new OtherHonor(a.Recordkey, a.OhonDesc));
        }
        public async Task<IEnumerable<OtherCcd>> GetOtherCcdsAsync()
        {
            return await GetCodeItemAsync<OtherCcds, OtherCcd>("AllOtherCcds", "OTHER.CCDS",
               a => new OtherCcd(a.Recordkey, a.OccdDesc));
        }
        public async Task<IEnumerable<OtherDegree>> GetOtherDegreesAsync()
        {
            return await GetCodeItemAsync<OtherDegrees, OtherDegree>("AllOtherDegrees", "OTHER.DEGREES",
               a => new OtherDegree(a.Recordkey, a.OdegDesc));
        }
        public async Task<IEnumerable<OtherMajor>> GetOtherMajorsAsync()
        {
            return await GetCodeItemAsync<OtherMajors, OtherMajor>("AllOtherMajors", "OTHER.MAJORS",
               a => new OtherMajor(a.Recordkey, a.OmajDesc));
        }
        public async Task<IEnumerable<OtherMinor>> GetOtherMinorsAsync()
        {
            return await GetCodeItemAsync<OtherMinors, OtherMinor>("AllOtherMinors", "OTHER.MINORS",
               a => new OtherMinor(a.Recordkey, a.OminDesc));
        }
        public async Task<IEnumerable<OtherSpecialization>> GetOtherSpecializationsAsync()
        {
            return await GetCodeItemAsync<OtherSpecials, OtherSpecialization>("AllOtherSpecials", "OTHER.SPECIALS",
               a => new OtherSpecialization(a.Recordkey, a.OspecDesc));
        }
        #endregion

        /// <summary>
        /// Get Education History for multiple students
        /// </summary>
        /// <param name="ids">List of student Ids to get Student Standings for</param>
        /// <returns>Returns a list of Student Standing Entities</returns>
        public async Task<IEnumerable<EducationHistory>> GetAsync(IEnumerable<string> ids)
        {
            List<EducationHistory> educationHistories = new List<EducationHistory>();
            if (ids != null && ids.Count() > 0)
            {
                // Get PERSON data
                Collection<Person> personContracts = await DataReader.BulkReadRecordAsync<Person>(ids.ToArray());
                if (personContracts != null)
                {
                    var institutionsAttendIds = new List<string>();
                    foreach (var person in personContracts)
                    {
                        if (person.PersonInstitutionsAttend != null)
                        {
                            foreach (var instAttendId in person.PersonInstitutionsAttend)
                            {
                                institutionsAttendIds.Add(person.Recordkey + "*" + instAttendId);
                            }
                        }
                    }
                    // get Institutions Attend data
                    Collection<InstitutionsAttend> institutionsAttendData =await DataReader.BulkReadRecordAsync<InstitutionsAttend>(institutionsAttendIds.ToArray());

                    // Get Institutions data
                    Collection<Institutions> institutionsData =await DataReader.BulkReadRecordAsync<Institutions>(personContracts.SelectMany(p => p.PersonInstitutionsAttend).ToArray());
                    Collection<Person> institutionsPersonData = await DataReader.BulkReadRecordAsync<Person>(personContracts.SelectMany(p => p.PersonInstitutionsAttend).ToArray());
                    Collection<AcadCredentials> acadCredentialsData =await DataReader.BulkReadRecordAsync<AcadCredentials>(institutionsAttendData.SelectMany(a => a.InstaAcadCredentials).ToArray());

                    // Go through each Institutions Attended record and build the EducationHistory
                    foreach (var person in personContracts)
                    {
                        if (person != null)
                        {
                            var educationHistoryEntity =await BuildEducationHistoryAsync(person, institutionsAttendData, acadCredentialsData, institutionsData, institutionsPersonData);
                            if (educationHistoryEntity != null)
                            {
                                educationHistories.Add(educationHistoryEntity);
                            }
                        }
                    }
                }
            }
            return educationHistories;
        }
        /// <summary>
        /// Build the Education History Entity from the Data Contract
        /// </summary>
        /// <param name="acadCredentialData">Data Contract object for Student Standings</param>
        /// <returns>A single Entity object for Student Standings</returns>
        private async Task<EducationHistory> BuildEducationHistoryAsync(Person personData, Collection<InstitutionsAttend> institutionsAttendData, Collection<AcadCredentials> acadCredentialData, Collection<Institutions> institutionData, Collection<Person> institutionsPersonData)
        {
            EducationHistory educationHistoryEntity = null;
            if (personData != null)
            {
                var personId = personData.Recordkey;
                educationHistoryEntity = new EducationHistory(personId);

                foreach (var instId in personData.PersonInstitutionsAttend)
                {
                    var instAttendId = personId + "*" + instId;
                    var institutionsAttend = institutionsAttendData.Where(i => i.Recordkey == instAttendId).FirstOrDefault();
                    if (institutionsAttend != null)
                    {
                        try
                        {
                            Institutions institutionContract = institutionData.Where(p => p.Recordkey == instId).FirstOrDefault();
                            Person institutionPerson = institutionsPersonData.Where(p => p.Recordkey == instId).FirstOrDefault();

                            HighSchool highSchoolEntity = null;
                            College collegeEntity = null;
                            if (institutionContract != null && institutionContract.InstType != null)
                            {
                                var codeAssoc = (await GetInstitutionTypesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == institutionContract.InstType).FirstOrDefault();
                                if (codeAssoc != null && (codeAssoc.ValActionCode1AssocMember == "H"))
                                {
                                    highSchoolEntity = new HighSchool(instId);
                                    highSchoolEntity.CredentialsEndDate = institutionsAttend.InstaCredentialsEndDate;
                                    highSchoolEntity.Gpa = institutionsAttend.InstaExtGpa;
                                    highSchoolEntity.SummaryCredits = institutionsAttend.InstaExtCredits;
                                    highSchoolEntity.GraduationType = institutionsAttend.InstaGradType;
                                    var typeAssoc = (await GetGraduationTypesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == institutionsAttend.InstaGradType).FirstOrDefault();
                                    if (typeAssoc != null)
                                    {
                                        highSchoolEntity.GraduationType = typeAssoc.ValExternalRepresentationAssocMember;
                                    }
                                    highSchoolEntity.CredentialsEndDate = institutionsAttend.InstaCredentialsEndDate;
                                    highSchoolEntity.Comments = institutionsAttend.InstaComments;
                                    highSchoolEntity.HighSchoolName = institutionPerson.LastName;
                                    if (!string.IsNullOrEmpty(institutionPerson.PreferredName))
                                    {
                                        highSchoolEntity.HighSchoolName = institutionPerson.PreferredName;
                                    }
                                    var lastYearAttend = institutionsAttend.YearsAttendedEntityAssociation.OrderByDescending(i => i.InstaYearAttendEndAssocMember).Select(i => i.InstaYearAttendEndAssocMember).FirstOrDefault();
                                    highSchoolEntity.LastAttendedYear = lastYearAttend;
                                }
                                else
                                {
                                    collegeEntity = new College(instId);
                                    collegeEntity.CredentialsEndDate = institutionsAttend.InstaCredentialsEndDate;
                                    collegeEntity.Gpa = institutionsAttend.InstaExtGpa;
                                    collegeEntity.SummaryCredits = institutionsAttend.InstaExtCredits;
                                    collegeEntity.CredentialsEndDate = institutionsAttend.InstaCredentialsEndDate;
                                    collegeEntity.Comments = institutionsAttend.InstaComments;
                                    collegeEntity.CollegeName = institutionPerson.LastName;
                                    if (!string.IsNullOrEmpty(institutionPerson.PreferredName))
                                    {
                                        collegeEntity.CollegeName = institutionPerson.PreferredName;
                                    }
                                    var lastYearAttend = institutionsAttend.YearsAttendedEntityAssociation.OrderByDescending(i => i.InstaYearAttendEndAssocMember).Select(i => i.InstaYearAttendEndAssocMember).FirstOrDefault();
                                    collegeEntity.LastAttendedYear = lastYearAttend;
                                }
                                // Now go through each Acad Credentials and build College and HighSchool
                                foreach (var acadCredentialId in institutionsAttend.InstaAcadCredentials)
                                {
                                    AcadCredentials acadCredential = acadCredentialData.Where(a => a.Recordkey == acadCredentialId).FirstOrDefault();
                                    Credential credentialEntity = new Credential();

                                    var degreeAssoc = (await GetOtherDegreesAsync()).Where(d => d.Code == acadCredential.AcadDegree).FirstOrDefault();
                                    if (degreeAssoc != null)
                                    {
                                        credentialEntity.Degree = degreeAssoc.Description;
                                    }
                                    credentialEntity.DegreeDate = acadCredential.AcadDegreeDate;
                                    credentialEntity.StartDate = acadCredential.AcadStartDate;
                                    credentialEntity.EndDate = acadCredential.AcadEndDate;
                                    credentialEntity.NumberOfYears = acadCredential.AcadNoYears;

                                    // Award Codes are not validated
                                    foreach (var award in acadCredential.AcadAwards)
                                    {
                                        credentialEntity.AddAward(award);
                                    }

                                    foreach (var honor in acadCredential.AcadHonors)
                                    {
                                        var honorAssoc = (await GetOtherHonorsAsync()).Where(h => h.Code == honor).FirstOrDefault();
                                        if (honorAssoc != null)
                                        {
                                            var description = honorAssoc.Description;
                                            credentialEntity.AddHonor(description);
                                        }
                                    }
                                    foreach (var certificate in acadCredential.AcadCcd)
                                    {
                                        var certificateAssoc = (await GetOtherCcdsAsync()).Where(c => c.Code == certificate).FirstOrDefault();
                                        if (certificateAssoc != null)
                                        {
                                            var description = certificateAssoc.Description;
                                            credentialEntity.AddCertificate(description);
                                        }
                                    }
                                    foreach (var major in acadCredential.AcadMajors)
                                    {
                                        var MajorAssoc = (await GetOtherMajorsAsync()).Where(m => m.Code == major).FirstOrDefault();
                                        if (MajorAssoc != null)
                                        {
                                            var description = MajorAssoc.Description;
                                            credentialEntity.AddMajor(description);
                                        }
                                    }
                                    foreach (var minor in acadCredential.AcadMinors)
                                    {
                                        var minorAssoc = (await GetOtherMinorsAsync()).Where(m => m.Code == minor).FirstOrDefault();
                                        if (minorAssoc != null)
                                        {
                                            var description = minorAssoc.Description;
                                            credentialEntity.AddMinor(description);
                                        }
                                    }
                                    foreach (var specialization in acadCredential.AcadSpecialization)
                                    {
                                        var specializationAssoc = (await GetOtherSpecializationsAsync()).Where(s => s.Code == specialization).FirstOrDefault();
                                        if (specializationAssoc != null)
                                        {
                                            var description = specializationAssoc.Description;
                                            credentialEntity.AddSpecialization(description);
                                        }
                                    }
                                    if (codeAssoc != null && (codeAssoc.ValActionCode1AssocMember == "H"))
                                    {
                                        highSchoolEntity.Credentials.Add(credentialEntity);
                                    }
                                    else
                                    {
                                        collegeEntity.Credentials.Add(credentialEntity);
                                    }
                                }
                                if (collegeEntity != null && !string.IsNullOrEmpty(collegeEntity.CollegeId))
                                {
                                    educationHistoryEntity.Colleges.Add(collegeEntity);
                                }
                                if (highSchoolEntity != null && !string.IsNullOrEmpty(highSchoolEntity.HighSchoolId))
                                {
                                    educationHistoryEntity.HighSchools.Add(highSchoolEntity);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, string.Format("An error occurred building education history from institutions attended record {0}", institutionsAttend.Recordkey));
                            return null;
                        }
                    }
                }
            }
            return educationHistoryEntity;
        }
    }
}
