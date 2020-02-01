// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
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
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ProspectOpportunitiesRepository : BaseColleagueRepository, IProspectOpportunitiesRepository
    {
        RepositoryException exception = new RepositoryException();

        #region ..ctor

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public ProspectOpportunitiesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        #endregion

        #region GET Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaObj"></param>
        /// <param name="personFilter"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<ProspectOpportunity>, int>> GetProspectOpportunitiesAsync(int offset, int limit, ProspectOpportunity criteriaObj = null,
            string[] filterPersonIds = null, bool bypassCache = false)
        {
            List<ProspectOpportunity> entities = new List<ProspectOpportunity>();
            Collection<Applications> applications = new Collection<Applications>();

            string criteria = string.Empty;
            string[] limitingKeys = new string[] { };
            int totalCount = 0;
            var applicationLimitingKeys = new List<string>();

            #region  Named query person filter
            if (filterPersonIds != null)
            {
                // Set limiting keys to previously retrieved personIds from SAVE.LIST.PARMS
                limitingKeys = filterPersonIds;
                var applicantApplicationId = (await DataReader.SelectAsync("APPLICANTS", limitingKeys, "WITH APP.APPLICATIONS NE '' BY.EXP APP.APPLICATIONS SAVING APP.APPLICATIONS")).ToList();
                if (applicantApplicationId != null && applicantApplicationId.Any())
                {
                    applicationLimitingKeys.AddRange(applicantApplicationId);
                }
                if (applicationLimitingKeys == null || !applicationLimitingKeys.Any())
                {
                    return new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
                }
                limitingKeys = applicationLimitingKeys.ToArray();
            }

            #endregion

            #region criteria filter

            if (criteriaObj != null)
            {
                if (!string.IsNullOrWhiteSpace(criteriaObj.ProspectId))
                {

                    var record = await DataReader.ReadRecordAsync<Applicants>(criteriaObj.ProspectId);

                    if (record == null)
                    {
                        return new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
                    }

                    if(record.AppApplications == null || !record.AppApplications.Any())
                    {
                        return new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
                    }

                    limitingKeys = record.AppApplications.ToArray();

                    limitingKeys = await DataReader.SelectAsync("APPLICATIONS", limitingKeys, criteria);
                    if (limitingKeys == null || !limitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
                    }
                }

                if (!string.IsNullOrWhiteSpace(criteriaObj.EntryAcademicPeriod))
                {
                    criteria = string.Format("WITH APPL.START.TERM EQ '{0}'", criteriaObj.EntryAcademicPeriod);
                    limitingKeys = await DataReader.SelectAsync("APPLICATIONS", limitingKeys, criteria);
                    if (limitingKeys == null || !limitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
                    }
                }
            }

            #endregion

            string[] applStatusesSpCodeIds = await GetApplStatusSpCodes(bypassCache);
            if (applStatusesSpCodeIds == null || !applStatusesSpCodeIds.Any())
            {
                return new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
            }
            var currStatus = string.Empty;
            foreach (var applStatusesSpCodeId in applStatusesSpCodeIds)
            {
                if (string.IsNullOrEmpty(currStatus))
                {
                    currStatus = string.Format("WITH APPL.INTG.KEY.IDX NE '' AND APPL.STATUS EQ '{0}'", applStatusesSpCodeId);
                }
                else
                {
                    currStatus += string.Format(" '{0}'", applStatusesSpCodeId);
                }
            }

            limitingKeys = await DataReader.SelectAsync("APPLICATIONS", limitingKeys, currStatus);

            if (limitingKeys == null || !limitingKeys.Any())
            {
                return new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
            }

            totalCount = limitingKeys.Count();
            var sublist = limitingKeys.Skip(offset).Take(limit).Distinct().ToArray();

            if(sublist == null || !sublist.Any())
            {
                return new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
            }

            applications = await DataReader.BulkReadRecordAsync<Applications>(sublist);

            Dictionary<string, string> dict = null;

            try
            {
                dict = await GetGuidsCollectionAsync(sublist);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                exception.AddError(new RepositoryError("Bad.Data", "Guids not found for APPLICATION with APPL.INTG.KEY.IDX."));
                throw exception;
            }

            if (dict == null || !dict.Any())
            {
                exception.AddError(new RepositoryError("Bad.Data", "Guids not found for APPLICATION with APPL.INTG.KEY.IDX."));
                throw exception;
            }


            foreach (var application in applications)
            {
                ProspectOpportunity entity = BuildEntity(application, dict);
                entities.Add(entity);
            }

            return entities.Any() ? new Tuple<IEnumerable<ProspectOpportunity>, int>(entities, totalCount) :
                new Tuple<IEnumerable<ProspectOpportunity>, int>(new List<ProspectOpportunity>(), 0);
        }

        /// <summary>
        /// Gets prospect opportunity by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ProspectOpportunity> GetProspectOpportunityByIdAsync(string id, bool bypassCache = false)
        {
            if(string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Prospect opportunity guid is required.");
            }

            var ldmEntity = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", id);
            if (ldmEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("APPLICATIONS '", id, "' not found."));
            }

            if ((!ldmEntity.LdmGuidEntity.Equals("APPLICATIONS", StringComparison.OrdinalIgnoreCase)) || 
                (!ldmEntity.LdmGuidSecondaryFld.Equals("APPL.INTG.KEY.IDX", StringComparison.OrdinalIgnoreCase) &&
                !ldmEntity.LdmGuidPrimaryKey.Equals(ldmEntity.LdmGuidSecondaryKey, StringComparison.OrdinalIgnoreCase)))
            {
                throw new KeyNotFoundException(string.Concat("GUID '", id, "' is invalid. Expecting GUID with entity APPLICATIONS with a secondary field equal to APPL.INTG.KEY.IDX."));
            }

            var prospectOpportunityId = ldmEntity.LdmGuidPrimaryKey;
            if (string.IsNullOrEmpty(prospectOpportunityId))
            {
                throw new RepositoryException(string.Format("No prospect opportunity was found for guid '{0}'", id));
            }

            string[] applStatusesSpCodeIds = await GetApplStatusSpCodes(bypassCache);
            if (applStatusesSpCodeIds == null || !applStatusesSpCodeIds.Any())
            {
                var exception = new RepositoryException(string.Format("No prospect opportunity was found for guid '{0}'", id));
                exception.AddError(new RepositoryError("APPS.SPECIAL.PROCESSING.CODE", "APPLICATION.STATUSES"));
                throw exception;
            }
            var entity = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", prospectOpportunityId);
            if(string.IsNullOrEmpty(entity.ApplIntgKeyIdx) || (entity.ApplStatus == null || !entity.ApplStatus.Any()))
            {
                throw new RepositoryException(string.Format("No prospect opportunity was found for guid '{0}'", id));
            }

            var combinedSocodes = applStatusesSpCodeIds.Intersect(entity.ApplStatus);
            if(combinedSocodes == null || !combinedSocodes.Any())
            {
                throw new RepositoryException(string.Format("No prospect opportunity was found for guid '{0}'", id));
            }

            Dictionary<string, string> dict = null;
            try
            {
                dict = await GetGuidsCollectionAsync(new List<string>() { entity.Recordkey });
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                exception.AddError(new RepositoryError("Bad.Data", "Guids not found for APPLICATION with APPL.INTG.KEY.IDX."));
                throw exception;
            }

            if (dict == null || !dict.Any())
            {
                exception.AddError(new RepositoryError("Bad.Data", "Guids not found for APPLICATION with APPL.INTG.KEY.IDX."));
                throw exception;
            }

            return BuildEntity(entity, dict);
        }

        /// <summary>
        /// Get admission application by Id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication> GetProspectOpportunitiesSubmissionsByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Prospect opportunity guid is required.");
            }

            var ldmEntity = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", guid);
            if (ldmEntity == null)
            {
                throw new KeyNotFoundException(string.Format("No prospect opportunity was found for guid '{0}'", guid));
            }

            if ((!ldmEntity.LdmGuidEntity.Equals("APPLICATIONS", StringComparison.OrdinalIgnoreCase)) ||
                (!ldmEntity.LdmGuidSecondaryFld.Equals("APPL.INTG.KEY.IDX", StringComparison.OrdinalIgnoreCase) &&
                !ldmEntity.LdmGuidPrimaryKey.Equals(ldmEntity.LdmGuidSecondaryKey, StringComparison.OrdinalIgnoreCase)))
            {
                throw new KeyNotFoundException(string.Format("No prospect opportunity was found for guid '{0}'", guid));
            }

            var prospectOpportunityId = ldmEntity.LdmGuidPrimaryKey;
            if (string.IsNullOrEmpty(prospectOpportunityId))
            {
                throw new KeyNotFoundException(string.Format("No prospect opportunity was found for guid '{0}'", guid));
            }

            string[] applStatusesSpCodeIds = await GetApplStatusSpCodes(bypassCache);
            if (applStatusesSpCodeIds == null || !applStatusesSpCodeIds.Any())
            {
                throw new KeyNotFoundException(string.Format("No prospect opportunity was found for guid '{0}'", guid));
            }
            var entity = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", prospectOpportunityId);
            if (string.IsNullOrEmpty(entity.ApplIntgKeyIdx) || (entity.ApplStatus == null || !entity.ApplStatus.Any()))
            {
                throw new KeyNotFoundException(string.Format("No prospect opportunity was found for guid '{0}'", guid));
            }

            var combinedSocodes = applStatusesSpCodeIds.Intersect(entity.ApplStatus);
            if (combinedSocodes == null || !combinedSocodes.Any())
            {
                throw new KeyNotFoundException(string.Format("No prospect opportunity was found for guid '{0}'", guid));
            }

            Dictionary<string, string> dict = null;
            try
            {
                dict = await GetGuidsCollectionAsync(new List<string>() { entity.Recordkey });
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    SourceId = entity.Recordkey,
                    Id = guid
                });
                exception.AddError(new RepositoryError("Bad.Data", "Guids not found for APPLICATIONS with APPL.INTG.KEY.IDX.")
                {
                    SourceId = entity.Recordkey,
                    Id = guid
                });
                throw exception;
            }

            if (dict == null || !dict.Any())
            {
                exception.AddError(new RepositoryError("Bad.Data", "Guids not found for APPLICATIONS with APPL.INTG.KEY.IDX.")
                {
                    SourceId = entity.Recordkey,
                    Id = guid
                });
                throw exception;
            }

            if (string.IsNullOrEmpty(entity.ApplApplicant))
            {
                exception.AddError(new RepositoryError("Bad.Data", "APPLICATION record missing Applicant Reference APPL.APPLICANT")
                {
                    SourceId = entity.Recordkey,
                    Id = guid
                });
                throw exception;
            }

            if (string.IsNullOrEmpty(entity.ApplAcadProgram))
            {
                exception.AddError(new RepositoryError("Bad.Data", "APPLICATION record missing Program Reference APPL.ACAD.PROGRAM")
                {
                    SourceId = entity.Recordkey,
                    Id = guid
                });
                throw exception;
            }

            var studentProgramId = string.Concat(entity.ApplApplicant, "*", entity.ApplAcadProgram);
            var programData = await DataReader.ReadRecordAsync<StudentPrograms>(studentProgramId);

            string personOriginCode = string.Empty;
            string[] columns = new string[] { "PERSON.ORIGIN.CODE" };
            try
            { 
                var personOriginCodes = await DataReader.ReadRecordColumnsAsync("PERSON", entity.ApplApplicant, columns);
                if (personOriginCodes != null && personOriginCodes.Any() && !string.IsNullOrEmpty(personOriginCodes.FirstOrDefault().Value))
                {
                    personOriginCode = personOriginCodes.FirstOrDefault().Value;
                }
            }
            catch
            {
                exception.AddError(new RepositoryError("Bad.Data", string.Format("PERSON record missing or invalid for applicant {0}.", entity.ApplApplicant))
                {
                    SourceId = entity.Recordkey,
                    Id = guid
                });
                throw exception;
            }

            return BuildAdmissionApplication(entity, programData, personOriginCode);
        }

        #endregion

        #region PUT/POST methods for submissions

        /// <summary>
        ///  CreateProspectOpportunitiesSubmissionsAsync
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<ProspectOpportunity> CreateProspectOpportunitiesSubmissionsAsync(AdmissionApplication request)
        {
            return UpdateProspectOpportunitiesSubmissionsAsync(request);
        }

        /// <summary>
        /// UpdateProspectOpportunitiesSubmissionsAsync
        /// </summary>
        /// <param name="request"></param>
        /// <returns>ProspectOpportunities domain object</returns>
        public async Task<ProspectOpportunity> UpdateProspectOpportunitiesSubmissionsAsync(AdmissionApplication entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("prospectOpportunitiesEntity", "Must provide a propspect opportunities to create.");
            }

            if (string.IsNullOrWhiteSpace(entity.Guid))
            {
                throw new ArgumentNullException("prospectOpportunitiesEntity", "Must provide the guid for prospect opportunities to update.");
            }

            var extendedDataTuple = GetEthosExtendedDataLists();

            UpdateProspectOrApplicantRequest updateAdmApplicationRequest = BuildAdmissionApplicationRequest(entity);
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateAdmApplicationRequest.ExtendedNames = extendedDataTuple.Item1;
                updateAdmApplicationRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            UpdateProspectOrApplicantResponse updateAdmApplicationResponse = await transactionInvoker.ExecuteAsync<UpdateProspectOrApplicantRequest, UpdateProspectOrApplicantResponse>(updateAdmApplicationRequest);

            // If there is any error message - throw an exception
            if (updateAdmApplicationResponse.UpdateProspectApplicantErrors != null && updateAdmApplicationResponse.UpdateProspectApplicantErrors.Any())
            {
                var errorMessage = string.Format("Error occurred updating application: '{0}' and AcadCredId: '{1}': ", entity.ApplicantPersonId, entity.ApplicationRecordKey);
                logger.Error(errorMessage);
                foreach (var message in updateAdmApplicationResponse.UpdateProspectApplicantErrors)
                {
                    //collect all the failure messages
                    if (message != null && !string.IsNullOrEmpty(message.ErrorMessages))
                    {
                        exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(message.ErrorCodes, ": ", message.ErrorMessages)) { Id = updateAdmApplicationResponse.Guid, SourceId = entity.ApplicationRecordKey });
                    }
                }
                throw exception;
            }

            // The GUID coming back from the CTX is not always the secondary guid but sometimes is the admission-applications guid.
            var guid = await GetGuidFromRecordInfoAsync("APPLICATIONS", updateAdmApplicationResponse.Application, "APPL.INTG.KEY.IDX", updateAdmApplicationResponse.Application);
            return await GetProspectOpportunityByIdAsync(guid);
        }

        /// <summary>
        /// Builds an  update admApplication request.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private UpdateProspectOrApplicantRequest BuildAdmissionApplicationRequest(AdmissionApplication entity)
        {
            try
            {
                var request = new UpdateProspectOrApplicantRequest()
                {
                    Guid = entity.Guid,
                    Application = entity.ApplicationRecordKey,
                    IsProspect = true
                };
                request.AcademicLoad = string.IsNullOrEmpty(entity.ApplicationStudentLoadIntent) ? string.Empty : entity.ApplicationStudentLoadIntent;
                request.AcademicPeriod = string.IsNullOrEmpty(entity.ApplicationStartTerm) ? string.Empty : entity.ApplicationStartTerm;
                request.AcademicLevel = string.IsNullOrEmpty(entity.ApplicationAcadLevel) ? string.Empty : entity.ApplicationAcadLevel;
                request.AdmissionPopulation = string.IsNullOrEmpty(entity.ApplicationAdmitStatus) ? string.Empty : entity.ApplicationAdmitStatus;
                request.ProgramOwner = string.IsNullOrEmpty(entity.ApplicationOwnerId) ? string.Empty : entity.ApplicationOwnerId;
                request.Type = string.IsNullOrEmpty(entity.ApplicationIntgType) ? string.Empty : entity.ApplicationIntgType;
                request.AcademicProgram = string.IsNullOrEmpty(entity.ApplicationAcadProgram) ? null : entity.ApplicationAcadProgram;
                request.Applicant = string.IsNullOrEmpty(entity.ApplicantPersonId) ? string.Empty : entity.ApplicantPersonId;
                request.AppliedOn = entity.AppliedOn;
                request.Comment = string.IsNullOrEmpty(entity.ApplicationComments) ? string.Empty : entity.ApplicationComments;
                request.Reference = string.IsNullOrEmpty(entity.ApplicationNo) ? string.Empty : entity.ApplicationNo;
                request.ResidencyType = string.IsNullOrEmpty(entity.ApplicationResidencyStatus) ? string.Empty : entity.ApplicationResidencyStatus;
                request.Site = entity.ApplicationLocations != null && entity.ApplicationLocations.Any() ? entity.ApplicationLocations.FirstOrDefault() : string.Empty;
                request.ApplicationSource = string.IsNullOrEmpty(entity.ApplicationSource) ? string.Empty : entity.ApplicationSource;
                request.WithdrawalInstitutionAttended = string.IsNullOrEmpty(entity.ApplicationAttendedInstead) ? string.Empty : entity.ApplicationAttendedInstead;
                request.WithdrawalReason = string.IsNullOrEmpty(entity.ApplicationWithdrawReason) ? string.Empty : entity.ApplicationWithdrawReason;
                request.WithdrawnOn = entity.WithdrawnOn;
                request.ccds = entity.ApplicationCredentials;
                request.PersonSource = entity.PersonSource;

                // Majors, Minors, and Concentrations (specializations)
                if (entity.ApplicationDisciplines != null && entity.ApplicationDisciplines.Any())
                {
                    foreach (var discipline in entity.ApplicationDisciplines)
                    {
                        switch (discipline.DisciplineType)
                        {
                            case Domain.Base.Entities.AcademicDisciplineType.Major:
                                {
                                    request.UpdateProspectApplicantMajors.Add(new UpdateProspectApplicantMajors()
                                    {
                                        Majors = discipline.Code,
                                        MajorDepts = discipline.AdministeringInstitutionUnit,
                                        MajorStartDates = discipline.StartOn
                                    });
                                    break;
                                }
                            case Domain.Base.Entities.AcademicDisciplineType.Minor:
                                {
                                    request.UpdateProspectApplicantMinors.Add(new UpdateProspectApplicantMinors()
                                    {
                                        Minors = discipline.Code,
                                        MinorDepts = discipline.AdministeringInstitutionUnit,
                                        MinorStartDates = discipline.StartOn
                                    });
                                    break;
                                }
                            case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                                {
                                    request.UpdateProspectApplicantSpecials.Add(new UpdateProspectApplicantSpecials()
                                    {
                                        Specials = discipline.Code,
                                        SpecialDepts = discipline.AdministeringInstitutionUnit,
                                        SpecialStartDates = discipline.StartOn
                                    });
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }

                return request;
            }
            catch (Exception e)
            {
                exception.AddError(new RepositoryError("Bad.Data", e.Message) { Id = entity.Guid, SourceId = entity.ApplicationRecordKey });
                throw exception;
            }
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetProspectOpportunityIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var guidRecord = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", guid);
            if (guidRecord == null)
            {
                throw new KeyNotFoundException("No prospect opportunty was found for guid " + guid + ".");
            }
            if (guidRecord.LdmGuidEntity != "APPLICATIONS")
            {
                throw new KeyNotFoundException("GUID " + guid + " has different entity, than expected, APPLICATIONS");
            }
            return guidRecord.LdmGuidPrimaryKey;
        }

        #endregion

        #region Build method

        /// <summary>
        /// BuildAdmissionApplication
        /// </summary>
        /// <param name="application"></param>
        /// <param name="programData"></param>
        /// <returns>AdmissionApplication domain entity</returns>
        private AdmissionApplication BuildAdmissionApplication(Applications application,
            StudentPrograms programData, string personOriginCode)
        {
            try
            {
                var applicationEntity = new AdmissionApplication(application.RecordGuid, application.Recordkey)
                {
                    ApplicantPersonId = application.ApplApplicant,
                    ApplicationNo = application.ApplNo,
                    ApplicationStartTerm = application.ApplStartTerm,
                    AdmissionApplicationStatuses = BuildAdmissionApplicationStatuses(application.ApplStatusesEntityAssociation),
                    ApplicationSource = application.ApplSource,
                    ApplicationAdmissionsRep = application.ApplAdmissionsRep,
                    ApplicationAdmitStatus = application.ApplAdmitStatus,
                    ApplicationLocations = application.ApplLocations,
                    ApplicationResidencyStatus = application.ApplResidencyStatus,
                    ApplicationStudentLoadIntent = application.ApplStudentLoadIntent,
                    ApplicationAcadProgram = application.ApplAcadProgram,
                    ApplicationAcadProgramGuid = programData.RecordGuid,
                    ApplicationStprAcadPrograms = BuildProgramCodeList(application, programData),
                    ApplicationDisciplines = BuildAcademicDiscipline(application, programData),
                    ApplicationCredentials = programData.StprCcds,
                    ApplicationWithdrawReason = application.ApplWithdrawReason,
                    ApplicationAttendedInstead = application.ApplAttendedInstead,
                    ApplicationComments = application.ApplComments,
                    ApplicationWithdrawDate = application.ApplWithdrawDate,
                    ApplicationSchool = programData.StprSchool,
                    ApplicationProgramOwner = programData.StprDept,
                    PersonSource = personOriginCode
                };

                return applicationEntity;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format(ex.Message, "admission application guid: {0}.", application.RecordGuid), ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException(string.Format(ex.Message, "admission application guid: {0}.", application.RecordGuid), ex);
            }
        }

        /// <summary>
        /// STPR_MAJOR_LIST, STPR_MINOR_LIST, STPR_SPECIALTIES
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        private List<string> BuildProgramCodeList(Applications application, StudentPrograms programData)
        {
            var programs = new List<string>();

            if (programData == null)
            {
                return programs;
            }

            //STPR_MAJOR_LIST
            if (programData.StprMajorListEntityAssociation != null && programData.StprMajorListEntityAssociation.Any())
            {
                foreach (var major in programData.StprMajorListEntityAssociation)
                {
                    if (!programs.Contains(major.StprAddnlMajorsAssocMember))
                    {
                        programs.Add(major.StprAddnlMajorsAssocMember);
                    }
                }
            }

            //STPR_MINOR_LIST
            if (programData.StprMinorListEntityAssociation != null && programData.StprMinorListEntityAssociation.Any())
            {
                foreach (var minor in programData.StprMinorListEntityAssociation)
                {
                    if (!programs.Contains(minor.StprMinorsAssocMember))
                    {
                        programs.Add(minor.StprMinorsAssocMember);
                    }
                }
            }

            //STPR_SPECIALTIES
            if (programData.StprSpecialtiesEntityAssociation != null && programData.StprSpecialtiesEntityAssociation.Any())
            {
                foreach (var specialty in programData.StprSpecialtiesEntityAssociation)
                {
                    if (!programs.Contains(specialty.StprSpecializationsAssocMember))
                    {
                        programs.Add(specialty.StprSpecializationsAssocMember);
                    }
                }
            }
            return programs.Any() ? programs : null;
        }

        /// <summary>
        /// STPR_MAJOR_LIST, STPR_MINOR_LIST, STPR_SPECIALTIES
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        private List<ApplicationDiscipline> BuildAcademicDiscipline(Applications application, StudentPrograms programData)
        {
            var discipline = new List<ApplicationDiscipline>();

            if (programData == null)
            {
                return discipline;
            }

            //STPR_MAJOR_LIST
            if (programData.StprMajorListEntityAssociation != null && programData.StprMajorListEntityAssociation.Any())
            {
                foreach (var major in programData.StprMajorListEntityAssociation)
                {
                    var existingMajor = discipline.FirstOrDefault(i => i.DisciplineType == Domain.Base.Entities.AcademicDisciplineType.Major && i.Code == major.StprAddnlMajorsAssocMember);
                    if (existingMajor == null)
                    {
                        discipline.Add(new ApplicationDiscipline()
                        {
                            Code = major.StprAddnlMajorsAssocMember,
                            DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Major,
                            StartOn = major.StprAddnlMajorStartDateAssocMember,
                            AdministeringInstitutionUnit = programData.StprDept
                        });
                    }
                }
            }

            //STPR_MINOR_LIST
            if (programData.StprMinorListEntityAssociation != null && programData.StprMinorListEntityAssociation.Any())
            {
                foreach (var minor in programData.StprMinorListEntityAssociation)
                {
                    var existingMinor = discipline.FirstOrDefault(i => i.DisciplineType == Domain.Base.Entities.AcademicDisciplineType.Minor && i.Code == minor.StprMinorsAssocMember);
                    if (existingMinor == null)
                    {
                        discipline.Add(new ApplicationDiscipline()
                        {
                            Code = minor.StprMinorsAssocMember,
                            DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Minor,
                            StartOn = minor.StprMinorStartDateAssocMember,
                            AdministeringInstitutionUnit = programData.StprDept
                        });
                    }
                }
            }

            //STPR_SPECIALTIES
            if (programData.StprSpecialtiesEntityAssociation != null && programData.StprSpecialtiesEntityAssociation.Any())
            {
                foreach (var specialty in programData.StprSpecialtiesEntityAssociation)
                {
                    var existingSpecialty = discipline.FirstOrDefault(i => i.DisciplineType == Domain.Base.Entities.AcademicDisciplineType.Concentration && i.Code == specialty.StprSpecializationsAssocMember);
                    if (existingSpecialty == null)
                    {
                        discipline.Add(new ApplicationDiscipline()
                        {
                            Code = specialty.StprSpecializationsAssocMember,
                            DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Concentration,
                            StartOn = specialty.StprSpecializationStartAssocMember,
                            AdministeringInstitutionUnit = programData.StprDept
                        });
                    }
                }
            }
            return discipline.Any() ? discipline : null;
        }

        /// <summary>
        /// Builds application statuses.
        /// </summary>
        /// <param name="applStatusesAssociation"></param>
        /// <returns></returns>
        private List<Domain.Student.Entities.AdmissionApplicationStatus> BuildAdmissionApplicationStatuses(List<ApplicationsApplStatuses> applStatusesAssociation)
        {
            var applStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>();
            if (applStatusesAssociation != null && applStatusesAssociation.Any())
            {
                foreach (var item in applStatusesAssociation)
                {
                    var admApplStatus = new Domain.Student.Entities.AdmissionApplicationStatus()
                    {
                        ApplicationDecisionBy = item.ApplDecisionByAssocMember,
                        ApplicationStatus = item.ApplStatusAssocMember,
                        ApplicationStatusDate = item.ApplStatusDateAssocMember,
                        ApplicationStatusTime = item.ApplStatusTimeAssocMember
                    };
                    applStatuses.Add(admApplStatus);
                }
            }
            return applStatuses;
        }

        /// <summary>
        /// Builds decision by.
        /// </summary>
        /// <param name="applicationsApplStatuses"></param>
        /// <returns></returns>
        private List<string> BuildApplicationDecisionBy(List<ApplicationsApplStatuses> applicationsApplStatuses)
        {
            var decisionBy = new List<string>();

            var items = applicationsApplStatuses.OrderByDescending(i => i.ApplStatusDateAssocMember);
            items.ToList().ForEach(i => decisionBy.Add(i.ApplDecisionByAssocMember));

            return decisionBy.Any() ? decisionBy : null;
        }

        /// <summary>
        /// Gets dictionary with colleague id and guid key pair for APPLICATIONS.
        /// </summary>
        /// <param name="applicationIds"></param>
        /// <returns>Dictionary containing entity PK (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetAdmissionApplicationGuidDictionary(IEnumerable<string> applicationIds)
        {
            var ldmGuidCollection = new Dictionary<string, string>();

            if ((applicationIds == null) || (!applicationIds.Any()))
            {
                return ldmGuidCollection;
            }

            //The data contract can sometimes get the wrong guid
            var guidList = await DataReader.SelectAsync("LDM.GUID", "WITH LDM.GUID.PRIMARY.KEY EQ '?' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.LDM.NAME EQ 'admission-applications' AND LDM.GUID.REPLACED.BY EQ ''", applicationIds.ToArray());
            var ldmGuidRecords = await DataReader.BulkReadRecordAsync<LdmGuid>(guidList);

            var exception = new RepositoryException();

            foreach (var e in ldmGuidRecords)
            {
                var output = string.Empty;
                if (ldmGuidCollection.TryGetValue(e.LdmGuidPrimaryKey, out output))
                {
                    var errorMessage = string.Format("Duplicate key found in LDM.GUID '{0}':", e.LdmGuidPrimaryKey);
                    exception.AddError(new RepositoryError("invalid.key", errorMessage));
                }
                else
                    ldmGuidCollection.Add(e.LdmGuidPrimaryKey, e.Recordkey);
            }

            if (exception.Errors.Any())
            {
                throw exception;
            }
            return ldmGuidCollection;
        }


        /// <summary>
        /// Gets the record key.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<string> GetRecordKeyAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Builds ProspectOpportunity entity.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private ProspectOpportunity BuildEntity(Applications source, Dictionary<string, string> dict)
        {
            ProspectOpportunity entity = null;
            string guid = string.Empty;
            string key = string.Concat(source.Recordkey, "|", source.Recordkey);
            dict.TryGetValue(key, out guid);
            entity = new ProspectOpportunity(guid, source.Recordkey)
            {
                ProspectId = source.ApplApplicant,
                StudentAcadProgId = string.Concat(source.ApplApplicant, "*", source.ApplAcadProgram),
                EntryAcademicPeriod = source.ApplStartTerm,
                AdmissionPopulation = source.ApplAdmitStatus,
                Site = (source.ApplLocations != null && source.ApplLocations.Any()) ? source.ApplLocations.FirstOrDefault() : string.Empty
            };

            return entity;
        }

        /// <summary>
        /// Using a collection of ids with guids
        ///  get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of ids</param>
        /// <returns>Dictionary consisting of a application.id with guids</returns>
        private async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(key => new RecordKeyLookup("APPLICATIONS", key,
                    "APPL.INTG.KEY.IDX", key, false))
                    .ToArray();                

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(string.Concat(splitKeys[1], "|", splitKeys[2]), recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error occured while getting guids for {0}.", "APPLICATIONS"), ex);
            }

            return guidCollection;
        }

        /// <summary>
        /// Gets application statuses either from cache or builds and then add to caches.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<string[]> GetApplStatusSpCodes(bool bypassCache)
        {
            if (bypassCache)
            {
                var applStatuses = await BuildApplStatusesAsync();
                return await AddOrUpdateCacheAsync<string[]>("AllStatusesWithSpProcCodeNull", applStatuses);

            }
            else
            {
                return await GetOrAddToCacheAsync("AllStatusesWithSpProcCodeNull", (Func<Task<string[]>>)(async () => await BuildApplStatusesAsync()), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Builds Application Statuses.
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> BuildApplStatusesAsync()
        {
            return await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");
        }

        #endregion
    }
}