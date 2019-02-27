//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdmissionApplicationsService : BaseCoordinationService, IAdmissionApplicationsService
    {
        private readonly IAdmissionApplicationsRepository _admissionApplicationsRepository;
        private readonly ITermRepository _termRepository;
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IPersonRepository _personRepository;

        /// <summary>
        /// ...ctor
        /// </summary>
        /// <param name="admissionApplicationsRepository"></param>
        /// <param name="termRepository"></param>
        /// <param name="iInstitutionRepository"></param>
        /// <param name="studentReferenceDataRepository"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="configurationRepository">The configuration repository</param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public AdmissionApplicationsService(
            IAdmissionApplicationsRepository admissionApplicationsRepository,
            ITermRepository termRepository,
            IInstitutionRepository iInstitutionRepository,
            IPersonRepository personRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _admissionApplicationsRepository = admissionApplicationsRepository;
            _termRepository = termRepository;
            _institutionRepository = iInstitutionRepository;
            _personRepository = personRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        #region GetAll, GetById

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Gets all admission-applications
        /// </summary>
        /// <returns>Collection of AdmissionApplications DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication>, int>> GetAdmissionApplicationsAsync(int offset, int limit, bool bypassCache = false)
        {
            try
            {
                CheckUserAdmissionApplicationsViewPermissions();

                var admissionApplicationsCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplication>();

                var admissionApplicationsEntities = await _admissionApplicationsRepository.GetAdmissionApplicationsAsync(offset, limit, bypassCache);
                if (admissionApplicationsEntities != null && admissionApplicationsEntities.Item1.Any())
                {
                    var ownerIds = await GetLocalOwnerIdsAsync(admissionApplicationsEntities.Item1);
                    var personIds = BuildLocalPersonGuids(admissionApplicationsEntities.Item1, ownerIds);
                    var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);
                    foreach (var admissionApplications in admissionApplicationsEntities.Item1)
                    {
                        admissionApplicationsCollection.Add(await ConvertAdmissionApplicationsEntityToDto(admissionApplications, personGuidCollection, ownerIds, bypassCache));
                    }
                }
                return admissionApplicationsCollection.Any() ? new Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication>, int>(admissionApplicationsCollection, admissionApplicationsEntities.Item2) :
                    new Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication>, int>(new List<Ellucian.Colleague.Dtos.AdmissionApplication>(), 0);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.Message);
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw new PermissionsException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex.Message);
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 11</remarks>
        /// <summary>
        /// Gets all admission-applications
        /// </summary>
        /// <returns>Collection of AdmissionApplications DTO objects</returns>

        public async Task<Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>> GetAdmissionApplications2Async(int offset, int limit, bool bypassCache = false)
        {
            try
            {
                CheckUserAdmissionApplicationsViewPermissions();

                var admissionApplicationsCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplication2>();

                var admissionApplicationsEntities = await _admissionApplicationsRepository.GetAdmissionApplicationsAsync(offset, limit, bypassCache);
                if (admissionApplicationsEntities != null && admissionApplicationsEntities.Item1.Any())
                {
                    var ownerIds = await GetLocalOwnerIdsAsync(admissionApplicationsEntities.Item1);
                    var personIds = BuildLocalPersonGuids(admissionApplicationsEntities.Item1, ownerIds);
                    var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);
                    foreach (var admissionApplications in admissionApplicationsEntities.Item1)
                    {
                        admissionApplicationsCollection.Add(await ConvertAdmissionApplicationsEntityToDto2Async(admissionApplications, personGuidCollection, ownerIds, bypassCache));
                    }
                }
                return admissionApplicationsCollection.Any() ? new Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication2>, int>(admissionApplicationsCollection, admissionApplicationsEntities.Item2) :
                    new Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication2>, int>(new List<Ellucian.Colleague.Dtos.AdmissionApplication2>(), 0);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.Message);
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw new PermissionsException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex.Message);
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Get a AdmissionApplications from its GUID
        /// </summary>
        /// <returns>AdmissionApplications DTO object</returns>
        public async Task<Dtos.AdmissionApplication> GetAdmissionApplicationsByGuidAsync(string guid)
        {
            try
            {
                CheckUserAdmissionApplicationsViewPermissions();

                var admissionApplication = await _admissionApplicationsRepository.GetAdmissionApplicationByIdAsync(guid);
                var admissionApplications = new List<Domain.Student.Entities.AdmissionApplication>() { admissionApplication };

                var ownerIds = await GetLocalOwnerIdsAsync(admissionApplications);
                var personIds = BuildLocalPersonGuids(admissionApplications, ownerIds);  
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                return await ConvertAdmissionApplicationsEntityToDto(admissionApplication, personGuidCollection, ownerIds, true);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.Message);
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw new PermissionsException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex.Message);
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 11</remarks>
        /// <summary>
        /// Get a AdmissionApplications from its GUID
        /// </summary>
        /// <returns>AdmissionApplications2 DTO object</returns>
        public async Task<AdmissionApplication2> GetAdmissionApplicationsByGuid2Async(string guid)
        {
            try
            {
                CheckUserAdmissionApplicationsViewPermissions();

                var admissionApplication = await _admissionApplicationsRepository.GetAdmissionApplicationByIdAsync(guid);
                if (admissionApplication == null)
                {
                    return new AdmissionApplication2();
                }
                var admissionApplications = new List<Ellucian.Colleague.Domain.Student.Entities.AdmissionApplication>() { admissionApplication };

                var ownerIds = await GetLocalOwnerIdsAsync(admissionApplications);
                var personIds = BuildLocalPersonGuids(admissionApplications, ownerIds);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                return await ConvertAdmissionApplicationsEntityToDto2Async(admissionApplication, personGuidCollection, ownerIds, true);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.Message);
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.Message);
                throw;
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw new PermissionsException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex.Message);
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view applications.
        /// </summary>
        private void CheckUserAdmissionApplicationsViewPermissions()
        {
            // access is ok if the current user has the view registrations permission
            if (!HasPermission(StudentPermissionCodes.ViewApplications))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view admission-applications.");
                throw new PermissionsException("User is not authorized to view admission-applications.");
            }
        }
        #endregion             

        #region Convert Methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdmissionApplications domain entity to its corresponding AdmissionApplications DTO
        /// </summary>
        /// <param name="source">AdmissionApplications domain entity</param>
        /// <returns>AdmissionApplications DTO</returns>
        private async Task<Dtos.AdmissionApplication> ConvertAdmissionApplicationsEntityToDto(Domain.Student.Entities.AdmissionApplication source, 
            Dictionary<string, string> personGuidCollection, Dictionary<string, string> ownerIdCollection, bool bypassCache)
        {
            try
            {
                var admissionApplication = new Dtos.AdmissionApplication();
                //Required fields
                admissionApplication.Id = source.Guid;                              

                if (string.IsNullOrEmpty(source.ApplicantPersonId))
                {
                    var error = string.Concat("Admission application source is required for guid ", source.Guid);
                    logger.Error(error);
                    throw new InvalidOperationException(error);
                }
                admissionApplication.Applicant = ConvertEntityKeyToPersonGuidObject(source.ApplicantPersonId, personGuidCollection);

                if (source.AdmissionApplicationStatuses == null)
                {
                    var error = string.Concat("Admission application statuses is required for guid ", source.Guid);
                    logger.Error(error);
                    throw new InvalidOperationException(error);
                }

                if (source.AdmissionApplicationStatuses != null && source.AdmissionApplicationStatuses.Any())
                {
                    var statuses = source.AdmissionApplicationStatuses.Select(i => i.ApplicationStatus);
                    if (statuses != null && !statuses.Any())
                    {
                        var error = string.Concat("Admission application statuses are required for guid ", source.Guid); ;
                        logger.Error(error);
                        throw new KeyNotFoundException(error);
                    }
                    admissionApplication.Statuses = await ConvertEntityToStatusesDtoAsync(source.AdmissionApplicationStatuses, bypassCache);

                    var admissionApplicationStatus = source.AdmissionApplicationStatuses
                        .Where(i => i.ApplicationStatus != null)
                        .OrderByDescending(dt => dt.ApplicationStatusDate)
                        .FirstOrDefault();

                    try
                    {
                        var id = string.Empty;
                        if (ownerIdCollection != null && (!string.IsNullOrEmpty(source.ApplicationAdmissionsRep) || !string.IsNullOrEmpty(admissionApplicationStatus.ApplicationDecisionBy)))
                        {
                            //Check first if admin rep has value
                            if (!string.IsNullOrEmpty(source.ApplicationAdmissionsRep))
                            {
                                admissionApplication.Owner = ConvertEntityKeyToPersonGuidObject(source.ApplicationAdmissionsRep, personGuidCollection);
                            }
                            else if ((ownerIdCollection.TryGetValue(source.ApplicationAdmissionsRep, out id)) && (!string.IsNullOrEmpty(id)))
                            {

                            }
                            // else check if _staffOperIdsDict has the value
                            else if ((ownerIdCollection.TryGetValue(admissionApplicationStatus.ApplicationDecisionBy, out id)) && (!string.IsNullOrEmpty(id)))
                            {

                                admissionApplication.Owner = ConvertEntityKeyToPersonGuidObject(id, personGuidCollection);
                            }
                            else
                            {
                                admissionApplication.Owner = ConvertEntityKeyToPersonGuidObject(admissionApplicationStatus.ApplicationDecisionBy, personGuidCollection);
                            }
                        }
                    }
                    catch
                    {
                        //Owner id is not required so no need to throw error
                    }
                }

                admissionApplication.School = await ConvertEntityToSchoolGuidObjectAsync(source.ApplicationSchool);
                admissionApplication.ReferenceID = string.IsNullOrEmpty(source.ApplicationNo) ? null : source.ApplicationNo;
                admissionApplication.Type = await ConvertEntityToTypeGuidObjectDtoAsync(bypassCache);
                admissionApplication.AcademicPeriod = await ConvertEntityToAcademicPeriodGuidObjectAsync(source.ApplicationStartTerm);
                admissionApplication.Source = await ConvertEntityToApplicationSourceGuidObjectDtoAsync(source.ApplicationSource);
                admissionApplication.AdmissionPopulation = await ConvertEntityToAdmitStatusGuidObjectDtoAsync(source.ApplicationAdmitStatus);
                admissionApplication.Site = await ConvertEntityToApplLocationGuidObjectDtoAsync(source.ApplicationLocations);
                admissionApplication.ResidencyType = await ConvertEntityToApplResidencyStatusGuidObjectDtoAsync(source.ApplicationResidencyStatus);
                admissionApplication.Program = await ConvertEntityToAcadProgGuidObjectDtoAsync(source.ApplicationAcadProgram);
                admissionApplication.Level = await ConvertEntityToAcadProgLevelGuidObjectDtoAsync(source.ApplicationAcadProgram, bypassCache);
                admissionApplication.Disciplines = await ConvertEntityToDisciplinesDtoAsync(source.ApplicationAcadProgram, source.ApplicationStprAcadPrograms, bypassCache);
                admissionApplication.AcademicLoad = ConvertEntityToAcademicLoadTypeDto(source.ApplicationStudentLoadIntent);
                admissionApplication.Withdrawal = await ConvertEntityToWithdrawlDtoAsync(source.ApplicationAttendedInstead, source.ApplicationWithdrawReason, personGuidCollection, bypassCache);
                admissionApplication.Comment = string.IsNullOrEmpty(source.ApplicationComments) ? null : source.ApplicationComments;

                return admissionApplication;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(string.Concat(ex.Message, "admission application guid: ", source.Guid), ex);
            }
            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Concat(ex.Message, "admission application guid: ", source.Guid), ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Concat(ex.Message, "admission application guid: ", source.Guid), ex);
            }
            catch (Exception ex)
            {
                var error = string.Concat("Something unexpected happened for guid ", source.Guid);
                throw new Exception(error, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdmissionApplications domain entity to its corresponding AdmissionApplications DTO
        /// </summary>
        /// <param name="source">AdmissionApplications domain entity</param>
        /// <returns>AdmissionApplications DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.AdmissionApplication2> ConvertAdmissionApplicationsEntityToDto2Async(Domain.Student.Entities.AdmissionApplication source,
            Dictionary<string, string> personGuidCollection, Dictionary<string, string> ownerIdCollection, bool bypassCache)
        {
            try
            {
                var admissionApplication = new Ellucian.Colleague.Dtos.AdmissionApplication2();
                //Required fields
                admissionApplication.Id = source.Guid;
                                                
                if (string.IsNullOrEmpty(source.ApplicantPersonId))
                {
                    throw new ArgumentNullException("Person key is required. ");
                }
               
                var personGuid = string.Empty;
                personGuidCollection.TryGetValue(source.ApplicantPersonId, out personGuid);
                if (string.IsNullOrEmpty(personGuid))
                {
                    throw new KeyNotFoundException(string.Concat("Person guid not found, PersonId: '", source.ApplicantPersonId, "', Record ID: '", source.ApplicationRecordKey, "'"));
                }
                admissionApplication.Applicant = new GuidObject2(personGuid);
                
                
                DateTime? withdrawnOn = null;
                var applicationStatuses = await GetApplicationStatusesAsync(bypassCache);
                var appliedStatuses = applicationStatuses.Where(ap => !ap.SpecialProcessingCode.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
                                                         .Select(ast => ast.Code)
                                                         .Distinct();
                var admittedStatuses = applicationStatuses.Where(ap => ap.SpecialProcessingCode == "AC")
                                                         .Select(ast => ast.Code)
                                                         .Distinct();
                var matriculatedStatuses = applicationStatuses.Where(ap => ap.SpecialProcessingCode == "MS")
                                                         .Select(ast => ast.Code)
                                                         .Distinct();
                var withdrawnStatuses = applicationStatuses.Where(ap => ap.SpecialProcessingCode == "WI")
                                                         .Select(ast => ast.Code)
                                                         .Distinct();
                if (source.AdmissionApplicationStatuses != null && source.AdmissionApplicationStatuses.Any())
                {
                    foreach (var statusEntity in source.AdmissionApplicationStatuses)
                    {
                        if ((appliedStatuses != null) && (appliedStatuses.Any()) 
                            && (appliedStatuses.Contains(statusEntity.ApplicationStatus)))
                        {
                            admissionApplication.AppliedOn = statusEntity.ApplicationStatusDate;
                        }


                        if ((admittedStatuses != null) && (admittedStatuses.Any()) 
                            && (admittedStatuses.Contains(statusEntity.ApplicationStatus)))
                        {
                            admissionApplication.AdmittedOn = statusEntity.ApplicationStatusDate;
                        }

                        if ((matriculatedStatuses != null) && (matriculatedStatuses.Any())
                            && (matriculatedStatuses.Contains(statusEntity.ApplicationStatus)))
                        {
                            admissionApplication.MatriculatedOn = statusEntity.ApplicationStatusDate;
                        }

                        if ((withdrawnStatuses != null) && (withdrawnStatuses.Any())
                            && (withdrawnStatuses.Contains(statusEntity.ApplicationStatus)))
                        {
                            withdrawnOn = statusEntity.ApplicationStatusDate;
                        }
                    }
                                   
                    try
                    {                      
                        if (ownerIdCollection != null && (!string.IsNullOrEmpty(source.ApplicationAdmissionsRep)))
                        {
                            var id = string.Empty;
                            //Check first if admin rep has value
                            if (!string.IsNullOrEmpty(source.ApplicationAdmissionsRep))
                            {
                                admissionApplication.Owner = ConvertEntityKeyToPersonGuidObject(source.ApplicationAdmissionsRep, personGuidCollection);
                            }
                            else if (ownerIdCollection.TryGetValue(source.ApplicationAdmissionsRep, out id))
                            {
                                if (!string.IsNullOrEmpty(id))
                                {
                                    admissionApplication.Owner = ConvertEntityKeyToPersonGuidObject(id, personGuidCollection);
                                }
                            }
                        }
                    }
                    catch
                    {
                        //Owner id is not required so no need to throw error
                    }
                }

                admissionApplication.School = await ConvertEntityToSchoolGuidObjectAsync(source.ApplicationSchool);
                admissionApplication.ReferenceID = string.IsNullOrEmpty(source.ApplicationNo) ? null : source.ApplicationNo;
                admissionApplication.Type = await ConvertEntityToTypeGuidObjectDto2Async(source.ApplicationIntgType, bypassCache);
                admissionApplication.AcademicPeriod = await ConvertEntityToAcademicPeriodGuidObjectAsync(source.ApplicationStartTerm);
                admissionApplication.Source = await ConvertEntityToApplicationSourceGuidObjectDtoAsync(source.ApplicationSource);
                admissionApplication.AdmissionPopulation = await ConvertEntityToAdmitStatusGuidObjectDtoAsync(source.ApplicationAdmitStatus);
                admissionApplication.Site = await ConvertEntityToApplLocationGuidObjectDtoAsync(source.ApplicationLocations);
                admissionApplication.ResidencyType = await ConvertEntityToApplResidencyStatusGuidObjectDtoAsync(source.ApplicationResidencyStatus);
                admissionApplication.Program = await ConvertEntityToAcadProgGuidObjectDtoAsync(source.ApplicationAcadProgram);
                admissionApplication.Level = await ConvertEntityToAcadProgLevelGuidObjectDtoAsync(source.ApplicationAcadProgram, bypassCache);
                admissionApplication.AcademicLoad = ConvertEntityToAcademicLoadTypeDto(source.ApplicationStudentLoadIntent);
                admissionApplication.Withdrawal = await ConvertEntityToWithdrawl2DtoAsync(source.ApplicationAttendedInstead, source.ApplicationWithdrawReason, 
                    source.ApplicationWithdrawDate, withdrawnOn, personGuidCollection, bypassCache);
                admissionApplication.Comment = string.IsNullOrEmpty(source.ApplicationComments) ? null : source.ApplicationComments;

                return admissionApplication;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(string.Concat(ex.Message, "admission application guid: ", source.Guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Concat(ex.Message, "admission application guid: ", source.Guid), ex);
            }

            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Concat(ex.Message, "admission application guid: ", source.Guid), ex);
            }
            catch (Exception ex)
            {
                var error = string.Concat("Something unexpected happened for guid ", source.Guid);
                throw new Exception(error, ex);
            }
        }
        
        /// <summary>
        /// Gets person guid object
        /// </summary>
        /// <param name="personRecordKey"></param>
        /// <returns></returns>
        private GuidObject2 ConvertEntityKeyToPersonGuidObject(string personRecordKey, Dictionary<string, string> personGuidCollection)
        {
            if (string.IsNullOrEmpty(personRecordKey))
            {
                throw new ArgumentNullException("Person key is required. ");
            }
            var personGuid = string.Empty;
            personGuidCollection.TryGetValue(personRecordKey, out personGuid);
            if (string.IsNullOrEmpty(personGuid))
            {
                throw new KeyNotFoundException(string.Concat("Person guid not found, PersonId: '", personRecordKey));
            }
            return  new GuidObject2(personGuid);
        }

        /// <summary>
        /// Gets status object
        /// </summary>
        /// <param name="admissionApplicationStatuses"></param>
        /// <returns></returns>
        private async Task<List<Dtos.DtoProperties.AdmissionApplicationsStatus>> ConvertEntityToStatusesDtoAsync(List<AdmissionApplicationStatus> admissionApplicationStatuses, bool bypassCache)
        {
            if (admissionApplicationStatuses == null || (admissionApplicationStatuses != null && !admissionApplicationStatuses.Any()))
            {
                throw new ArgumentNullException("Statuses are required. ");
            }

            var statusesList = new List<Dtos.DtoProperties.AdmissionApplicationsStatus>();

            foreach (var admissionApplicationStatus in admissionApplicationStatuses)
            {
                var status = new Dtos.DtoProperties.AdmissionApplicationsStatus()
                {
                    AdmissionApplicationsStatusDetail = await ConvertEntityToStatusDetailGuidObjectAsync(admissionApplicationStatus.ApplicationStatus, bypassCache),
                    AdmissionApplicationsStatusType = await ConvertEntityToStatusType(admissionApplicationStatus.ApplicationStatus, bypassCache),
                    AdmissionApplicationsStatusStartOn = ConvertEntityDateTimeSpanToDate(admissionApplicationStatus.ApplicationStatusDate, admissionApplicationStatus.ApplicationStatusTime)
                };
                statusesList.Add(status);
            }
            return statusesList.Any() ? statusesList : null;
        }

        /// <summary>
        /// Gets status object
        /// </summary>
        /// <param name="admissionApplicationStatuses"></param>
        /// <returns></returns>
        private async Task<List<Dtos.DtoProperties.AdmissionApplicationsStatus2>> ConvertEntityToStatuses2DtoAsync(List<AdmissionApplicationStatus> admissionApplicationStatuses, bool bypassCache)
        {
            if (admissionApplicationStatuses == null || (admissionApplicationStatuses != null && !admissionApplicationStatuses.Any()))
            {
                throw new ArgumentNullException("Statuses are required. ");
            }

            List<Dtos.DtoProperties.AdmissionApplicationsStatus2> statusesList = new List<Dtos.DtoProperties.AdmissionApplicationsStatus2>();

            foreach (var admissionApplicationStatus in admissionApplicationStatuses)
            {
                var status = new Dtos.DtoProperties.AdmissionApplicationsStatus2()
                {
                    AdmissionApplicationsStatusDetail = await ConvertEntityToStatusDetailGuidObjectAsync(admissionApplicationStatus.ApplicationStatus, bypassCache),
                    AdmissionApplicationsStatusType = await ConvertEntityToStatusType2(admissionApplicationStatus.ApplicationStatus, bypassCache),
                    AdmissionApplicationsStatusStartOn = ConvertEntityDateTimeSpanToDate(admissionApplicationStatus.ApplicationStatusDate, admissionApplicationStatus.ApplicationStatusTime)
                };
                statusesList.Add(status);
            }
            return statusesList.Any() ? statusesList : null;
        }

        /// <summary>
        /// Gets date with time
        /// </summary>
        /// <param name="statusDate"></param>
        /// <param name="statusTime"></param>
        /// <returns></returns>
        private DateTime ConvertEntityDateTimeSpanToDate(DateTime? statusDate, DateTime? statusTime)
        {
            DateTime dateOnly;
            DateTime timeOnly;
            DateTime combined;

            if (!statusDate.HasValue)
            {
                throw new ArgumentNullException("Application status date cannot be null. ");
            }

            if (DateTime.TryParse(statusDate.Value.ToShortDateString(), out dateOnly))
            {
                if (statusTime.HasValue)
                {
                    if (DateTime.TryParse(statusTime.Value.ToLongTimeString(), out timeOnly))
                    {
                        return combined = dateOnly.Date.Add(timeOnly.TimeOfDay);
                    }
                    else
                    {
                        return combined = dateOnly;
                    }
                }
                else
                {
                    return combined = dateOnly;
                }
            }
            return statusDate.Value;
        }

        /// <summary>
        /// Gets admission applications status type
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns>AdmissionApplicationsStatusType enumeration</returns>
        private async Task<AdmissionApplicationsStatusType> ConvertEntityToStatusType(string sourceCode, bool bypassCache)
        {
            var status = string.Empty;            
            if (!string.IsNullOrEmpty(sourceCode))
            {
                try
                {
                    status = await _studentReferenceDataRepository.GetAdmissionDecisionTypesSPCodeAsync(sourceCode);
                }
                catch (Exception)
                {
                    status = sourceCode;
                }               
            }
            else
            {
                status = sourceCode;
            }

            switch (status)
            {
                case "AP":
                    return AdmissionApplicationsStatusType.Submitted;
                case "CO":
                    return AdmissionApplicationsStatusType.ReadyForReview;
                case "AC":
                case "WL":
                case "RE":
                case "WI":
                    return AdmissionApplicationsStatusType.DecisionMade;
                case "MS":
                    return AdmissionApplicationsStatusType.EnrollmentCompleted;
                default:
                    return Dtos.EnumProperties.AdmissionApplicationsStatusType.Started;
            }
        }

        /// <summary>
        /// Gets admission applications status type
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns>AdmissionApplicationsStatusType2 enumeration</returns>
        private async Task<AdmissionApplicationsStatusType2> ConvertEntityToStatusType2(string sourceCode, bool bypassCache)
        {
            var status = string.Empty;
            if (!string.IsNullOrEmpty(sourceCode))
            {
                try
                {
                    status = await _studentReferenceDataRepository.GetAdmissionDecisionTypesSPCodeAsync(sourceCode);
                }
                catch (Exception)
                {
                    status = sourceCode;
                }
            }
            else
            {
                status = sourceCode;
            }

            switch (status)
            {
                case "AP":
                    return AdmissionApplicationsStatusType2.Submitted;
                case "CO":
                    return AdmissionApplicationsStatusType2.ReadyForReview;
                case "AC":
                    return AdmissionApplicationsStatusType2.Admitted;
                case "WL":
                case "RE":
                case "WI":
                    return AdmissionApplicationsStatusType2.DecisionMade;
                case "MS":
                    return AdmissionApplicationsStatusType2.MovedToStudentSystem;
                default:
                    return AdmissionApplicationsStatusType2.Started;
            }
        }

        /// <summary>
        /// Gets status guid object
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToStatusDetailGuidObjectAsync(string sourceCode, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            var admissionDecisionTypesGuid = string.Empty;
            try
            {
                admissionDecisionTypesGuid = await _studentReferenceDataRepository.GetAdmissionDecisionTypesGuidAsync(sourceCode);
            }
            catch (Exception)
            {
                return null;
            }
            if (string.IsNullOrEmpty(admissionDecisionTypesGuid))
            {
                return null;
            }
            return new GuidObject2(admissionDecisionTypesGuid);
        }

        /// <summary>
        /// Convert code to guid.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToAcademicPeriodGuidObjectAsync(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            //var source = (await Terms(bypassCache)).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
            var termsGuid = await _termRepository.GetAcademicPeriodsGuidAsync(sourceCode);
            if (string.IsNullOrEmpty(termsGuid))
            {
                var error = string.Format("Term not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
            return new GuidObject2(termsGuid);
        }

        /// <summary>
        /// Convert code to guid.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToApplicationSourceGuidObjectDtoAsync(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            var applicationSourcesGuid = await _studentReferenceDataRepository.GetApplicationSourcesGuidAsync(sourceCode);
            if (string.IsNullOrEmpty(applicationSourcesGuid))
            {
                var error = string.Format("Application source not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
            return new GuidObject2(applicationSourcesGuid);
        }

        /// <summary>
        /// Convert code to guid.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns>GuidObject2</returns>
        private async Task<GuidObject2> ConvertEntityToAdmitStatusGuidObjectDtoAsync(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            var admissionPopulationsGuid = await _studentReferenceDataRepository.GetAdmissionPopulationsGuidAsync(sourceCode);
                     
           if (string.IsNullOrEmpty(admissionPopulationsGuid))
            {
                var error = string.Format("Application admit status not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
            return new GuidObject2(admissionPopulationsGuid);
        }

        /// <summary>
        /// Convert location code to guid.
        /// </summary>
        /// <param name="list">list of locations.  Only the first value in the list will be used.</param>
        /// <returns>GuidObject2</returns>
        private async Task<GuidObject2> ConvertEntityToApplLocationGuidObjectDtoAsync(List<string> list)
        {
            if (list == null || (!list.Any()))
            {
                return null;
            }

            var site = list.FirstOrDefault();
            var siteGuid = await _referenceDataRepository.GetLocationsGuidAsync(site);
            if (string.IsNullOrEmpty(siteGuid))
            {
                throw new KeyNotFoundException(string.Format("Site not found for code '{0}'.", site));
            }
            return new GuidObject2(siteGuid);
        }

        /// <summary>
        /// Convert code to guid.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns>GuidObject2</returns>
        private async Task<GuidObject2> ConvertEntityToApplResidencyStatusGuidObjectDtoAsync(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var admissionResidencyTypesGuid = await _studentReferenceDataRepository.GetAdmissionResidencyTypesGuidAsync(sourceCode);

            if (string.IsNullOrEmpty(admissionResidencyTypesGuid))
            {
                var error = string.Format("Residency type not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
            return new GuidObject2(admissionResidencyTypesGuid);
        }
               
        /// <summary>
        /// Convert code to guid.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<ResidencyTypeDtoProperty> ConvertEntityToApplResidencyStatusStudentDtoAsync(string sourceCode, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var admissionResidencyTypesGuid = await _studentReferenceDataRepository.GetAdmissionResidencyTypesGuidAsync(sourceCode);

            if (string.IsNullOrEmpty(admissionResidencyTypesGuid))
            {
                var error = string.Format("Residency type not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
            return new ResidencyTypeDtoProperty()
            {
                Student = new GuidObject2(admissionResidencyTypesGuid)
            };
        }

        /// <summary>
        /// Convert code to guid.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToAcadProgGuidObjectDtoAsync(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var academicProgramGuid = await _studentReferenceDataRepository.GetAcademicProgramsGuidAsync(sourceCode);

            if (string.IsNullOrEmpty(academicProgramGuid))
            {
                var error = string.Format("Academic program not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
            return new GuidObject2(academicProgramGuid);
        }

        /// <summary>
        /// Convert code to guid.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToAcadProgLevelGuidObjectDtoAsync(string sourceCode, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var academicProgram = (await AcademicProgramsAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
            if (academicProgram == null)
            {
                var error = string.Format("Academic program not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
          
            var levelSourceGuid = string.Empty;
            if (!string.IsNullOrEmpty(academicProgram.AcadLevelCode))
            {              
                levelSourceGuid = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(academicProgram.AcadLevelCode);
                if (string.IsNullOrEmpty(levelSourceGuid))
                {
                    var error = string.Format("Academic level not found for code {0}. ", academicProgram.AcadLevelCode);
                    throw new KeyNotFoundException(error);
                }
            }
            return string.IsNullOrEmpty(levelSourceGuid) ? null : new GuidObject2(levelSourceGuid);
        }

        /// <summary>
        /// Gets discipline guid object
        /// </summary>
        /// <param name="sourceCodes"></param>
        /// <returns></returns>
        private async Task<IEnumerable<AdmissionApplicationDiscipline>> ConvertEntityToDisciplinesDtoAsync(string program, List<string> sourceCodes, bool bypassCache)
        {
            if (sourceCodes == null || (sourceCodes != null && !sourceCodes.Any()))
            {
                return null;
            }

            var academicDisciplineList = new List<AdmissionApplicationDiscipline>();
            var disciplineCodeList = new List<string>();

            disciplineCodeList.AddRange(sourceCodes);

            var source = (await AcademicProgramsAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(program, StringComparison.OrdinalIgnoreCase));
            if (source == null)
            {
                var error = string.Format("Academic program not found for code {0}. ", sourceCodes);
                throw new KeyNotFoundException(error);
            }

            //Major
            source.MajorCodes.ForEach(i =>
            {
                if ((!string.IsNullOrEmpty(i)) && (!disciplineCodeList.Contains(i)))
                {
                    disciplineCodeList.Add(i);
                }
            });

            //Minor
            source.MinorCodes.ForEach(i =>
            {
                if ((!string.IsNullOrEmpty(i)) && (!disciplineCodeList.Contains(i)))
                {
                    disciplineCodeList.Add(i);
                }
            });

            //Specializations
            source.SpecializationCodes.ForEach(i =>
            {
                if ((!string.IsNullOrEmpty(i)) && (!disciplineCodeList.Contains(i)))
                {
                    disciplineCodeList.Add(i);
                }
            });


            //Now create guid objects
            if (disciplineCodeList.Any())
            {
                foreach (var item in disciplineCodeList)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var acadDiscSource = (await AcademicDisciplinesAsync(bypassCache)).FirstOrDefault(disc => disc.Code.Equals(item, StringComparison.OrdinalIgnoreCase));
                        if (acadDiscSource == null)
                        {
                            var error = string.Format("Academic disciplines not found for code {0}. ", item);
                            throw new KeyNotFoundException(error);
                        }
                        academicDisciplineList.Add(new AdmissionApplicationDiscipline() { Discipline = new GuidObject2(acadDiscSource.Guid) });
                    }
                }
            }

            return academicDisciplineList.Any() ? academicDisciplineList : null;
        }

        /// <summary>
        /// Gets admission applicaion withdrawl
        /// </summary>
        /// <param name="applicationAttendedInstead"></param>
        /// <param name="applicationWithdrawReason"></param>
        /// <returns></returns>
        private async Task<Dtos.DtoProperties.AdmissionApplicationsWithdrawal> ConvertEntityToWithdrawlDtoAsync(string applicationAttendedInstead, string applicationWithdrawReason,
            Dictionary<string, string> personGuidCollection, bool bypassCache)
        {
            if (string.IsNullOrEmpty(applicationWithdrawReason))
            {
                return null;
            }

           var admApplWithdrawReason = new Dtos.DtoProperties.AdmissionApplicationsWithdrawal();

            var applicationWithdrawReasonGuid = await _studentReferenceDataRepository.GetWithdrawReasonsGuidAsync(applicationWithdrawReason);
            if (string.IsNullOrEmpty(applicationWithdrawReasonGuid))
            {
                var error = string.Format("Withdraw reason not found for reason {0}. ", applicationWithdrawReason);
                throw new KeyNotFoundException(error);
            }
            admApplWithdrawReason.WithdrawalReason = new GuidObject2(applicationWithdrawReasonGuid);
            if (!string.IsNullOrEmpty(applicationAttendedInstead))
            {
                if (personGuidCollection == null)
                {
                    var error = string.Format("Institution attended guid not found for id {0}. ", applicationAttendedInstead);
                    throw new KeyNotFoundException(error);
                }
                var institutionGuid = string.Empty;
                personGuidCollection.TryGetValue(applicationAttendedInstead, out institutionGuid);
                
                if (string.IsNullOrEmpty(institutionGuid))
                {
                    var error = string.Format("Institution attended guid not found for id {0}. ", applicationAttendedInstead);
                    throw new KeyNotFoundException(error);
                }

                admApplWithdrawReason.InstitutionAttended = institutionGuid;
            }
            
            return admApplWithdrawReason;
        }

        /// <summary>
        /// Gets admission applicaion withdrawl
        /// </summary>
        /// <param name="applicationAttendedInstead"></param>
        /// <param name="applicationWithdrawReason"></param>
        /// <returns></returns>
        private async Task<Dtos.DtoProperties.AdmissionApplicationsWithdrawal2> ConvertEntityToWithdrawl2DtoAsync(string applicationAttendedInstead, string applicationWithdrawReason, 
            DateTime? applicationWithdrawDate, DateTime? withdrawnOn, Dictionary<string, string> personGuidCollection,  bool bypassCache)
        {
            if (string.IsNullOrWhiteSpace(applicationAttendedInstead + applicationWithdrawReason) && applicationWithdrawDate == null
                  && withdrawnOn == null)
                return null;

            var admApplWithdrawReason = new Dtos.DtoProperties.AdmissionApplicationsWithdrawal2();
            if (!string.IsNullOrEmpty(applicationWithdrawReason))
            {
                var applicationWithdrawReasonGuid = await _studentReferenceDataRepository.GetWithdrawReasonsGuidAsync(applicationWithdrawReason);
                if (string.IsNullOrEmpty(applicationWithdrawReasonGuid))
                {
                    var error = string.Format("Withdraw reason not found for reason {0}. ", applicationWithdrawReason);
                    throw new KeyNotFoundException(error);
                }
                admApplWithdrawReason.WithdrawalReason = new GuidObject2(applicationWithdrawReasonGuid);
            }
            
            admApplWithdrawReason.WithdrawnOn = applicationWithdrawDate == null ? null : applicationWithdrawDate;
            if (applicationWithdrawDate == null)
            {
                admApplWithdrawReason.WithdrawnOn = withdrawnOn;
            }

            if (!string.IsNullOrEmpty(applicationAttendedInstead))
            {
                var institutionGuid = string.Empty;
                if (personGuidCollection == null)
                {
                    var error = string.Format("Institution attended guid not found for id {0}. ", applicationAttendedInstead);
                    throw new KeyNotFoundException(error);
                }

                personGuidCollection.TryGetValue(applicationAttendedInstead, out institutionGuid);
                if (string.IsNullOrEmpty(institutionGuid))
                {
                    var error = string.Format("Institution attended guid not found for id {0}. ", applicationAttendedInstead);
                    throw new KeyNotFoundException(error);
                }

                admApplWithdrawReason.InstitutionAttended = new AdmissionApplicationInstitutionAttendedDtoProperty()
                {
                    Id = institutionGuid,
                };
            }

            return admApplWithdrawReason;
        }

        /// <summary>
        /// Gets AdmissionApplicationsAcademicLoadType enumeration from applicationStudentLoadIntent
        /// </summary>
        /// <param name="applicationStudentLoadIntent"></param>
        /// <returns>AdmissionApplicationsAcademicLoadType enumeration</returns>
        private AdmissionApplicationsAcademicLoadType ConvertEntityToAcademicLoadTypeDto(string applicationStudentLoadIntent)
        {
            if (string.IsNullOrEmpty(applicationStudentLoadIntent))
            {
                return AdmissionApplicationsAcademicLoadType.NotSet;
            }

            if (applicationStudentLoadIntent.ToUpperInvariant().Equals("F", StringComparison.OrdinalIgnoreCase) ||
                applicationStudentLoadIntent.ToUpperInvariant().Equals("O", StringComparison.OrdinalIgnoreCase))
            {
                return AdmissionApplicationsAcademicLoadType.FullTime;
            }
            else if (applicationStudentLoadIntent.ToUpperInvariant().Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                return AdmissionApplicationsAcademicLoadType.PartTime;
            }
            else
            {
                return AdmissionApplicationsAcademicLoadType.NotSet;
            }
        }

        /// <summary>
        /// Gets guid objects
        /// </summary>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToTypeGuidObjectDtoAsync(bool bypassCache)
        {
            var source = (await AdmissionApplicationTypesAsync(bypassCache)).FirstOrDefault();
            return string.IsNullOrEmpty(source.Guid) ? null : new GuidObject2(source.Guid);
        }

        /// <summary>
        /// Gets AdmissionApplicationTypes guid objects
        /// </summary>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToTypeGuidObjectDto2Async(string intgType, bool bypassCache)
        {
            if (string.IsNullOrEmpty(intgType))
            {
                var source = (await AdmissionApplicationTypesAsync(bypassCache)).FirstOrDefault();
                return string.IsNullOrEmpty(source.Guid) ? null : new GuidObject2(source.Guid);
            }
            else
            {
                var intgTypeGuid = await _studentReferenceDataRepository.GetAdmissionApplicationTypesGuidAsync(intgType);
                if (string.IsNullOrEmpty(intgTypeGuid))
                {
                    var error = string.Format("Admission application types not found for code {0}. ", intgType);
                    throw new KeyNotFoundException(error);
                }
                return new GuidObject2(intgTypeGuid);
            }
        }

        /// <summary>
        /// Gets guid objects
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToSchoolGuidObjectAsync(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            var schoolsGuid = await _referenceDataRepository.GetSchoolsGuidAsync(sourceCode);
            if (string.IsNullOrEmpty(schoolsGuid))
            {
                var error = string.Format("School not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
            return new GuidObject2(schoolsGuid);
        }

        #endregion

        #region Reference Methods

        /// <summary>
        /// Get all person ids associated with a collection of AdmissionApplication domain entities.
        /// Used to perform a bulk guid lookup
        /// </summary>
        private IEnumerable<string> BuildLocalPersonGuids(IEnumerable<Domain.Student.Entities.AdmissionApplication> admissionApplicationsEntities,
            Dictionary<string, string> staffOperIdsDict)
        {
            var personIds = new List<string>();

            personIds.AddRange(admissionApplicationsEntities
                     .Where(i => !string.IsNullOrEmpty(i.ApplicantPersonId) && !personIds.Contains(i.ApplicantPersonId))
                     .Select(i => i.ApplicantPersonId));
            personIds.AddRange(admissionApplicationsEntities
                     .Where(i => !string.IsNullOrEmpty(i.ApplicationAdmissionsRep) && !personIds.Contains(i.ApplicationAdmissionsRep))
                     .Select(i => i.ApplicationAdmissionsRep));
            personIds.AddRange(admissionApplicationsEntities
                     .Where(i => !string.IsNullOrEmpty(i.ApplicationAttendedInstead) && !personIds.Contains(i.ApplicationAttendedInstead))
                     .Select(i => i.ApplicationAttendedInstead));

          
            if (staffOperIdsDict != null)
            {
                foreach (var item in staffOperIdsDict)
                {
                    if (!personIds.Contains(item.Value))
                    {
                        personIds.Add(item.Value);
                    }
                }
            }

            return personIds.Distinct().ToList();
        }

        private async Task<Dictionary<string, string>> GetLocalOwnerIdsAsync(IEnumerable<Domain.Student.Entities.AdmissionApplication> admissionApplicationsEntities)
        {
           
            var ownerIds = new List<string>();

       
            admissionApplicationsEntities.ToList().ForEach(admissionApplicationsEntity =>
            {
                if (!string.IsNullOrEmpty(admissionApplicationsEntity.ApplicationAdmissionsRep))
                {
                    if (!ownerIds.Contains(admissionApplicationsEntity.ApplicationAdmissionsRep))
                    {
                        ownerIds.Add(admissionApplicationsEntity.ApplicationAdmissionsRep);
                    }
                }

                if (admissionApplicationsEntity.AdmissionApplicationStatuses != null && admissionApplicationsEntity.AdmissionApplicationStatuses.Any())
                {
                    var admissionApplicationStatus = admissionApplicationsEntity.AdmissionApplicationStatuses
                        .Where(i => i.ApplicationStatus != null)
                        .OrderByDescending(dt => dt.ApplicationStatusDate)
                        .FirstOrDefault();
                    if (admissionApplicationStatus != null && !ownerIds.Contains(admissionApplicationStatus.ApplicationDecisionBy))
                    {
                        ownerIds.Add(admissionApplicationStatus.ApplicationDecisionBy);
                    }
                }
            });

            return await _admissionApplicationsRepository.GetStaffOperIdsAsync(ownerIds);
            
        }

        /// <summary>
        /// Gets admission application key.
        /// </summary>
        /// <param name="admissionApplicationDto"></param>
        /// <returns></returns>
        private async Task<string> GetAdmissionApplicationKeyAsync(Dtos.AdmissionApplication2 admissionApplicationDto)
        {
            return await _admissionApplicationsRepository.GetRecordKeyAsync(admissionApplicationDto.Id);
        }

        /// <summary>
        /// AdmissionApplicationType
        /// </summary>
        private IEnumerable<AdmissionApplicationType> _admissionApplicationTypes;
        private async Task<IEnumerable<AdmissionApplicationType>> AdmissionApplicationTypesAsync(bool bypassCache)
        {
            if (_admissionApplicationTypes == null)
            {
                _admissionApplicationTypes = await _studentReferenceDataRepository.GetAdmissionApplicationTypesAsync(bypassCache);
            }
            return _admissionApplicationTypes;
        }

        /// <summary>
        /// Terms
        /// </summary>
        private IEnumerable<Term> _terms;
        private async Task<IEnumerable<Term>> Terms(bool bypassCache)
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync(bypassCache);
            }
            return _terms;
        }

        /// <summary>
        /// ApplicationSources
        /// </summary>
        private IEnumerable<Domain.Student.Entities.ApplicationSource> _sources;
        private async Task<IEnumerable<Domain.Student.Entities.ApplicationSource>> ApplicationSourcesAsync(bool bypassCache)
        {
            if (_sources == null)
            {
                _sources = await _studentReferenceDataRepository.GetApplicationSourcesAsync(bypassCache);
            }
            return _sources;
        }

        /// <summary>
        /// AdmissionPopulation
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AdmissionPopulation> _admissionPopulations;
        private async Task<IEnumerable<Domain.Student.Entities.AdmissionPopulation>> AdmissionPopulationsAsync(bool bypassCache)
        {
            if (_admissionPopulations == null)
            {
                _admissionPopulations = await _studentReferenceDataRepository.GetAdmissionPopulationsAsync(bypassCache);
            }
            return _admissionPopulations;
        } 

        /// <summary>
        /// Sites
        /// </summary>
        private IEnumerable<Domain.Base.Entities.Location> _locations;
        private async Task<IEnumerable<Domain.Base.Entities.Location>> SitesAsync(bool bypassCache)
        {
            if (_locations == null)
            {
                _locations = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _locations;
        }

        /// <summary>
        /// AdmissionResidencyType
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AdmissionResidencyType> _admissionResidencyTypes;
        private async Task<IEnumerable<Domain.Student.Entities.AdmissionResidencyType>> AdmissionResidencyTypesAsync(bool bypassCache)
        {
            if (_admissionResidencyTypes == null)
            {
                _admissionResidencyTypes = await _studentReferenceDataRepository.GetAdmissionResidencyTypesAsync(bypassCache);
            }
            return _admissionResidencyTypes;
        }

        /// <summary>
        /// AcademicLevels
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> AcademicLevelsAsync(bool bypassCache)
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache);
            }
            return _academicLevels;
        }

        /// <summary>
        /// AcademicDisciplines
        /// </summary>
        private IEnumerable<Domain.Base.Entities.AcademicDiscipline> _academicDisciplines;
        private async Task<IEnumerable<Domain.Base.Entities.AcademicDiscipline>> AcademicDisciplinesAsync(bool bypassCache)
        {
            if (_academicDisciplines == null)
            {
                _academicDisciplines = await _referenceDataRepository.GetAcademicDisciplinesAsync(bypassCache);
            }
            return _academicDisciplines;
        }

        /// <summary>
        /// AcademicProgram
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicProgram> _academicPrograms;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicProgram>> AcademicProgramsAsync(bool bypassCache)
        {
            if (_academicPrograms == null)
            {
                _academicPrograms = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache);
            }
            return _academicPrograms;
        }

        /// <summary>
        /// WithdrawReason
        /// </summary>
        private IEnumerable<Domain.Student.Entities.WithdrawReason> _withdrawReasons;
        private async Task<IEnumerable<Domain.Student.Entities.WithdrawReason>> WithdrawReasonsAsync(bool bypassCache)
        {
            if (_withdrawReasons == null)
            {
                _withdrawReasons = await _studentReferenceDataRepository.GetWithdrawReasonsAsync(bypassCache);
            }
            return _withdrawReasons;
        }

        /// <summary>
        /// Schools
        /// </summary>
        private IEnumerable<Domain.Base.Entities.School> _schools;
        private async Task<IEnumerable<Domain.Base.Entities.School>> SchoolsAsync(bool bypassCache)
        {
            if (_schools == null)
            {
                _schools = await _referenceDataRepository.GetSchoolsAsync(bypassCache);
            }
            return _schools;
        }

        private IEnumerable<Domain.Student.Entities.AdmissionApplicationStatusType> _applicationStatuses = null;
        private async Task<IEnumerable<Domain.Student.Entities.AdmissionApplicationStatusType>> GetApplicationStatusesAsync(bool bypassCache)
        {
            if (_applicationStatuses == null)
            {
                _applicationStatuses = await _studentReferenceDataRepository.GetAdmissionApplicationStatusTypesAsync(bypassCache);
            }
            return _applicationStatuses;
        }

        #endregion

        #region PUT/POST V11

        /// <summary>
        /// Update an existing admission application.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="admissionApplicationDto"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication2> UpdateAdmissionApplicationAsync(string guid, AdmissionApplication2 admissionApplicationDto, bool bypassCache = true)
        {
            if (admissionApplicationDto == null)
            {
                throw new ArgumentNullException("admissionApplication", "Must provide an admission applications for update.");
            }

            if (string.IsNullOrEmpty(admissionApplicationDto.Id))
            {
                throw new ArgumentNullException("admissionApplication", "Must provide a guid for admission applications update.");
            }

            ValidatePayload(admissionApplicationDto);

            if (!await CheckAdmissionApplicationCreateUpdatePermAsync())
            {
                logger.Error(string.Format("User '{0}' is not authorized to create admission-applications.", CurrentUser.UserId));
                throw new PermissionsException("User is not authorized to create admission-applications.");
            }
                        
            var applicationsId = await _admissionApplicationsRepository.GetRecordKeyAsync(admissionApplicationDto.Id);
            if (!string.IsNullOrEmpty(applicationsId))
            {
                _admissionApplicationsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                Domain.Student.Entities.AdmissionApplication admissionApplicationEntity = await ConvertDtoToEntityAsync(guid, admissionApplicationDto, bypassCache);
                Domain.Student.Entities.AdmissionApplication admApplEntity = await _admissionApplicationsRepository.UpdateAdmissionApplicationAsync(admissionApplicationEntity);

                var admApplEntities = new List<Domain.Student.Entities.AdmissionApplication>() { admApplEntity };
                var ownerIds = await GetLocalOwnerIdsAsync(admApplEntities);
                var personIds = BuildLocalPersonGuids(admApplEntities, ownerIds);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                return await ConvertAdmissionApplicationsEntityToDto2Async(admApplEntity, personGuidCollection, ownerIds, bypassCache);
            }
            return await CreateAdmissionApplicationAsync(admissionApplicationDto);
        }

        /// <summary>
        /// Create an admission application.
        /// </summary>
        /// <param name="admissionApplication"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication2> CreateAdmissionApplicationAsync(Dtos.AdmissionApplication2 admissionApplication, bool bypassCache = true)
        {
            if (admissionApplication == null)
            {
                throw new ArgumentNullException("admissionApplication", "Must provide an admission application for create.");
            }

            if (string.IsNullOrEmpty(admissionApplication.Id))
            {
                throw new ArgumentNullException("admissionApplication", "Must provide a guid for admission application create.");
            }

            ValidatePayload(admissionApplication);

            if (!await CheckAdmissionApplicationCreateUpdatePermAsync())
            {
                logger.Error(string.Format("User '{0}' is not authorized to create admission-applications.", CurrentUser.UserId));
                throw new PermissionsException("User is not authorized to create admission-applications.");
            }

            try
            {
                _admissionApplicationsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                Domain.Student.Entities.AdmissionApplication admissionApplicationEntity = await ConvertDtoToEntityAsync(null, admissionApplication, bypassCache);

                Domain.Student.Entities.AdmissionApplication admApplEntity = await _admissionApplicationsRepository.CreateAdmissionApplicationAsync(admissionApplicationEntity);

                var admApplEntities = new List<Domain.Student.Entities.AdmissionApplication>() { admApplEntity };
                var ownerIds = await GetLocalOwnerIdsAsync(admApplEntities);
                var personIds = BuildLocalPersonGuids(admApplEntities, ownerIds);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                return await ConvertAdmissionApplicationsEntityToDto2Async(admApplEntity, personGuidCollection, ownerIds, bypassCache);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

     
        /// <summary>
        /// Converts dto to an entity.
        /// </summary>
        /// <param name="recordKey"></param>
        /// <param name="dto"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.AdmissionApplication> ConvertDtoToEntityAsync(string guid, Dtos.AdmissionApplication2 dto, bool bypassCache = false)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Id))
            {
                throw new ArgumentNullException("admissionApplications", "Must provide guid for an admission application.");
            }
            Domain.Student.Entities.AdmissionApplication entity = null;

            if (string.IsNullOrEmpty(guid))
            {
                entity = new Domain.Student.Entities.AdmissionApplication(dto.Id);
            }
            else
            {
                entity = new Domain.Student.Entities.AdmissionApplication(dto.Id, await GetAdmissionApplicationKeyAsync(dto));
            }

            //referenceId
            if (!string.IsNullOrEmpty(dto.ReferenceID))
            {
                entity.ApplicationNo = dto.ReferenceID;
            }

            //applicant.id persons.json
            if (dto.Applicant != null && !string.IsNullOrEmpty(dto.Applicant.Id))
            {
                var applicantKey = await _personRepository.GetPersonIdFromGuidAsync(dto.Applicant.Id);
                if(string.IsNullOrEmpty(applicantKey))
                {
                    throw new KeyNotFoundException(string.Format("Applicant not found for guid {0}. ", dto.Applicant.Id));
                }
                entity.ApplicantPersonId = applicantKey;
            }

            //type.id
            if(dto.Type != null && !string.IsNullOrEmpty(dto.Type.Id))
            {
                var typeKey = (await AdmissionApplicationTypesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.Type.Id, StringComparison.OrdinalIgnoreCase));
                if (typeKey == null)
                {
                    throw new KeyNotFoundException(string.Format("Admission application type not found for guid {0}. ", dto.Type.Id));
                }
                entity.ApplicationIntgType = typeKey.Code;
            }
            else
            {
                var typeKey = (await AdmissionApplicationTypesAsync(bypassCache)).FirstOrDefault(i => i.Code.ToUpper().Equals("ST", StringComparison.OrdinalIgnoreCase));
                if (typeKey == null)
                {
                    throw new KeyNotFoundException(string.Format("Admission application type not found for guid {0}. ", dto.Type.Id));
                }
                entity.ApplicationIntgType = typeKey.Code;
            }

            //academicPeriod.id
            if (dto.AcademicPeriod != null && !string.IsNullOrEmpty(dto.AcademicPeriod.Id))
            {
                var source = (await Terms(bypassCache)).FirstOrDefault(i => i.RecordGuid.Equals(dto.AcademicPeriod.Id, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    var error = string.Format("Term not found for code {0}. ", dto.AcademicPeriod.Id);
                    throw new KeyNotFoundException(error);
                }
                if (string.IsNullOrEmpty(source.Code))
                {
                    throw new KeyNotFoundException(string.Format("Academic period not found for guid {0}. ", dto.AcademicPeriod.Id));
                }
                entity.ApplicationStartTerm = source.Code;
            }
            //source.id
            if(dto.Source != null && !string.IsNullOrEmpty(dto.Source.Id))
            {
                var source = (await ApplicationSourcesAsync(true)).FirstOrDefault(i => i.Guid.Equals(dto.Source.Id, StringComparison.OrdinalIgnoreCase));
                if(source == null)
                {
                    throw new KeyNotFoundException(string.Format("Application source not found for guid {0}. ", dto.Source.Id));
                }
                entity.ApplicationSource = source.Code;
            }

            //owner.id persons.json
            if (dto.Owner != null && !string.IsNullOrEmpty(dto.Owner.Id))
            {
                var owner = await _personRepository.GetPersonIdFromGuidAsync(dto.Owner.Id);
                if(owner == null)
                {
                    throw new KeyNotFoundException(string.Format("Application owner not found for guid {0}. ", dto.Owner.Id));
                }
                entity.ApplicationOwnerId = owner;
            }

            //admissionPopulation.id
            if(dto.AdmissionPopulation != null && !string.IsNullOrEmpty(dto.AdmissionPopulation.Id))
            {
                var admPopulation = (await AdmissionPopulationsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.AdmissionPopulation.Id, StringComparison.OrdinalIgnoreCase));
                if(admPopulation == null)
                {
                    throw new KeyNotFoundException(string.Format("Application admit status not found for guid {0}. ", dto.AdmissionPopulation.Id));
                }
                entity.ApplicationAdmitStatus = admPopulation.Code;
            }

            //site.id
            if(dto.Site != null && !string.IsNullOrEmpty(dto.Site.Id))
            {
                var site = (await SitesAsync(true)).FirstOrDefault(i => i.Guid.Equals(dto.Site.Id, StringComparison.OrdinalIgnoreCase));
                if (site == null)
                {
                    throw new KeyNotFoundException(string.Format("Site not found for guid {0}.", dto.Site.Id));
                }
                entity.ApplicationLocations = new List<string>() { site.Code };
            }

            //residencyType.id
            if(dto.ResidencyType != null && !string.IsNullOrEmpty(dto.ResidencyType.Id))
            {
                //var residencyType = 
                var residencyType = (await AdmissionResidencyTypesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.ResidencyType.Id, StringComparison.OrdinalIgnoreCase));
                if (residencyType == null)
                {
                    throw new KeyNotFoundException(string.Format("Residency type not found for guid {0}.", dto.ResidencyType.Id));
                }
                entity.ApplicationResidencyStatus = residencyType.Code;
            }

            //academicLoad
            if(dto.AcademicLoad !=  AdmissionApplicationsAcademicLoadType.NotSet)
            {
                entity.ApplicationStudentLoadIntent = dto.AcademicLoad.ToString().ToUpperInvariant();
            }

            //program.id
            if(dto.Program != null && !string.IsNullOrEmpty(dto.Program.Id))
            {
                var program = (await AcademicProgramsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.Program.Id, StringComparison.OrdinalIgnoreCase));
                if (program == null)
                {
                    throw new KeyNotFoundException(string.Format("Academic program not found for guid {0}. ", dto.Program.Id));
                }
                entity.ApplicationAcadProgram = program.Code;
            }

            //level.id
            if(dto.Level != null && !string.IsNullOrEmpty(dto.Level.Id))
            {
                //This property really is not used for PUT/POST since it is derived from Acad program in GET
                var acadLevel = (await AcademicLevelsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.Level.Id, StringComparison.OrdinalIgnoreCase));
                if (acadLevel == null)
                {
                    throw new KeyNotFoundException(string.Format("Academic level not found for guid {0}. ", dto.Level.Id));
                }                
            }

            //school.id
            if (dto.School != null && !string.IsNullOrEmpty(dto.School.Id))
            {
                var school = (await SchoolsAsync(bypassCache)).FirstOrDefault(s => s.Guid.Equals(dto.School.Id, StringComparison.OrdinalIgnoreCase));
                if(school == null)
                {
                    throw new KeyNotFoundException(string.Format("School not found for guid {0}. ", dto.School.Id));
                }
                entity.ApplicationSchool = school.Code;
            }

            //appliedOn
            if (dto.AppliedOn.HasValue)
            {
                entity.AppliedOn = dto.AppliedOn.Value;
            }

            //admittedOn
            if (dto.AdmittedOn.HasValue)
            {
                entity.AdmittedOn = dto.AdmittedOn.Value;
            }

            //matriculatedOn
            if (dto.MatriculatedOn.HasValue)
            {
                entity.MatriculatedOn = dto.MatriculatedOn.Value;
            }

            //withdrawal.WithdrawalReason.id
            if (dto.Withdrawal != null && dto.Withdrawal.WithdrawalReason != null && !string.IsNullOrEmpty(dto.Withdrawal.WithdrawalReason.Id))
            {
                var withdrwlReason = (await WithdrawReasonsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.Withdrawal.WithdrawalReason.Id, StringComparison.OrdinalIgnoreCase));
                if (withdrwlReason == null)
                {
                    throw new KeyNotFoundException(string.Format("Withdraw reason not found for reason {0}. ", dto.Withdrawal.WithdrawalReason.Id));
                }
                entity.ApplicationWithdrawReason = withdrwlReason.Code;
            }

            //withdrawal.InstitutionAttended.Id
            if (dto.Withdrawal != null && dto.Withdrawal.InstitutionAttended != null && !string.IsNullOrEmpty(dto.Withdrawal.InstitutionAttended.Id))
            {
                var instAttended = await _personRepository.GetPersonIdFromGuidAsync(dto.Withdrawal.InstitutionAttended.Id);
                if(string.IsNullOrEmpty(instAttended))
                {
                    throw new KeyNotFoundException(string.Format("Institution attended not found for guid {0}. ", dto.Withdrawal.InstitutionAttended.Id));
                }
                entity.ApplicationAttendedInstead = instAttended;
            }                     

            //withdrawal.WithdrawnOn
            if (dto.Withdrawal != null && dto.Withdrawal.WithdrawnOn.HasValue)
            {
                entity.WithdrawnOn = dto.Withdrawal.WithdrawnOn.Value;
            }

            //comment
            if (!string.IsNullOrEmpty(dto.Comment))
            {
                entity.ApplicationComments = dto.Comment;
            }

            return entity;
        }

        /// <summary>
        /// Permissions code that allows an external system to create/update admission applications.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckAdmissionApplicationCreateUpdatePermAsync()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.UpdateApplications))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validate AdmissionApplication2 DTO
        /// </summary>
        /// <param name="source">AdmissionApplication2 DTO</param>
        private void ValidatePayload(AdmissionApplication2 source)
        {
            if (source.Applicant == null)
            {
                throw new ArgumentNullException("applicant", "Applicant is required.");
            }

            if (source.Applicant != null && string.IsNullOrEmpty(source.Applicant.Id))
            {
                throw new ArgumentNullException("applicant.id", "Applicant id is required.");
            }

            if (source.Type != null && string.IsNullOrEmpty(source.Type.Id))
            {
                throw new ArgumentNullException("type.Id", "Type id is required.");
            }

            if (source.AcademicPeriod != null && string.IsNullOrEmpty(source.AcademicPeriod.Id))
            {
                throw new ArgumentNullException("academicPeriod.Id", "Academic period id is required.");
            }

            if (source.Source != null && string.IsNullOrEmpty(source.Source.Id))
            {
                throw new ArgumentNullException("source.Id", "Source id is required.");
            }

            if (source.Owner != null && string.IsNullOrEmpty(source.Owner.Id))
            {
                throw new ArgumentNullException("owner.Id", "Owner id is required.");
            }

            if (source.AdmissionPopulation != null && string.IsNullOrEmpty(source.AdmissionPopulation.Id))
            {
                throw new ArgumentNullException("admissionPopulation.Id", "Admission population id is required.");
            }

            if (source.Site != null && string.IsNullOrEmpty(source.Site.Id))
            {
                throw new ArgumentNullException("site.Id", "Site id is required.");
            }

            if (source.ResidencyType != null && string.IsNullOrEmpty(source.ResidencyType.Id))
            {
                throw new ArgumentNullException("residencyType.Id", "Residency type id is required.");
            }

            if (source.Program != null && string.IsNullOrEmpty(source.Program.Id))
            {
                throw new ArgumentNullException("program.Id", "Program id is required.");
            }

            if (source.Level != null && string.IsNullOrEmpty(source.Level.Id))
            {
                throw new ArgumentNullException("level.Id", "Level id is required.");
            }

            if (source.School != null && string.IsNullOrEmpty(source.School.Id))
            {
                throw new ArgumentNullException("school.Id", "School id is required.");
            }

            if (source.Withdrawal != null && source.Withdrawal.WithdrawalReason != null && string.IsNullOrEmpty(source.Withdrawal.WithdrawalReason.Id))
            {
                throw new ArgumentNullException("withdrawal.withdrawalReason.Id", "Withdrawal reason id is required.");
            }
            else if (source.Withdrawal != null && source.Withdrawal.WithdrawnOn.HasValue && source.Withdrawal.WithdrawalReason == null)
            {
                throw new ArgumentNullException("withdrawal.withdrawnOn", "Withdrwawl reason must be provided if withdrawl date is provided.");
            }

            if (source.Withdrawal != null && source.Withdrawal.InstitutionAttended != null && string.IsNullOrEmpty(source.Withdrawal.InstitutionAttended.Id))
            {
                throw new ArgumentNullException("withdrawal.institutionAttended.Id", "Institution attended reason id is required.");
            }
        }

        #endregion
    }
}