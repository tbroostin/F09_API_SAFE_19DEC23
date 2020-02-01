//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 16.0.0</remarks>
        /// <summary>
        /// Gets all admission-applications
        /// </summary>
        /// <returns>Collection of AdmissionApplications DTO objects</returns>

        public async Task<Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>> GetAdmissionApplications3Async(int offset, int limit, Dtos.AdmissionApplication3 filterCriteria = null, Dtos.Filters.PersonFilterFilter2 personFilterCriteria = null, bool bypassCache = false)
        {
            CheckUserAdmissionApplicationsViewPermissions2();

            var admissionApplicationsCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplication3>();

            string applicant = "", academicPeriod = "", personFilter = "";
            if (filterCriteria != null)
            {
                if (filterCriteria.Applicant != null && !string.IsNullOrEmpty(filterCriteria.Applicant.Id))
                {
                    try
                    {
                        applicant = await _personRepository.GetPersonIdFromGuidAsync(filterCriteria.Applicant.Id);
                        if (string.IsNullOrEmpty(applicant))
                        {
                            return new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(new List<Dtos.AdmissionApplication3>(), 0);
                        }
                    }
                    catch
                    {
                        return new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(new List<Dtos.AdmissionApplication3>(), 0);
                    }
                }
                if (filterCriteria.AcademicPeriod != null && !string.IsNullOrEmpty(filterCriteria.AcademicPeriod.Id))
                {
                    try
                    {
                        academicPeriod = await _termRepository.GetAcademicPeriodsCodeFromGuidAsync(filterCriteria.AcademicPeriod.Id);
                        if (string.IsNullOrEmpty(academicPeriod))
                        {
                            return new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(new List<Dtos.AdmissionApplication3>(), 0);
                        }
                    }
                    catch
                    {
                        return new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(new List<Dtos.AdmissionApplication3>(), 0);
                    }
                }
            }

            //convert person filter named query.
            string[] filterPersonIds = new List<string>().ToArray();

            if (personFilterCriteria != null && personFilterCriteria.personFilter != null && !string.IsNullOrEmpty(personFilterCriteria.personFilter.Id))
            {
                personFilter = personFilterCriteria.personFilter.Id;
                if (!string.IsNullOrEmpty(personFilter))
                {
                    var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                    if (personFilterKeys != null)
                    {
                        filterPersonIds = personFilterKeys;
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(new List<Dtos.AdmissionApplication3>(), 0);
                    }
                }
            }

            Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int> admissionApplicationsEntities = null;
            try
            {
                admissionApplicationsEntities = await _admissionApplicationsRepository.GetAdmissionApplications2Async(offset, limit, applicant, academicPeriod, personFilter, filterPersonIds, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (admissionApplicationsEntities != null && admissionApplicationsEntities.Item1.Any())
            {
                var ownerIds = await GetLocalOwnerIdsAsync(admissionApplicationsEntities.Item1);
                var personIds = BuildLocalPersonGuids(admissionApplicationsEntities.Item1, ownerIds);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);
                foreach (var admissionApplications in admissionApplicationsEntities.Item1)
                {
                    admissionApplicationsCollection.Add(await ConvertAdmissionApplicationsEntityToDto3Async(admissionApplications, personGuidCollection, ownerIds, bypassCache));
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return admissionApplicationsCollection.Any() ? new Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication3>, int>(admissionApplicationsCollection, admissionApplicationsEntities.Item2) :
                new Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication3>, int>(new List<Ellucian.Colleague.Dtos.AdmissionApplication3>(), 0);
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 16.0.0</remarks>
        /// <summary>
        /// Get a AdmissionApplications from its GUID
        /// </summary>
        /// <returns>AdmissionApplications2 DTO object</returns>
        public async Task<AdmissionApplication3> GetAdmissionApplicationsByGuid3Async(string guid)
        {
            CheckUserAdmissionApplicationsViewPermissions2();

            Domain.Student.Entities.AdmissionApplication admissionApplication = null;
            try
            {
                admissionApplication = await _admissionApplicationsRepository.GetAdmissionApplicationById2Async(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (admissionApplication == null)
            {
                var error = string.Concat("Unable to build admission application for guid ", guid); ;
                logger.Error(error);
                throw new KeyNotFoundException(error);
            }
            var admissionApplications = new List<Ellucian.Colleague.Domain.Student.Entities.AdmissionApplication>() { admissionApplication };

            var ownerIds = await GetLocalOwnerIdsAsync(admissionApplications);
            var personIds = BuildLocalPersonGuids(admissionApplications, ownerIds);
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

            var admissionsApplicationDto3 = await ConvertAdmissionApplicationsEntityToDto3Async(admissionApplication, personGuidCollection, ownerIds, true);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;

            }

            return admissionsApplicationDto3;
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view applications.
        /// </summary>
        private void CheckUserAdmissionApplicationsViewPermissions()
        {
            // access is ok if the current user has the view registrations permission
            if (!HasPermission(StudentPermissionCodes.ViewApplications) && !HasPermission(StudentPermissionCodes.UpdateApplications))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view admission-applications.");
                throw new PermissionsException("User is not authorized to view admission-applications.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view applications.
        /// </summary>
        private void CheckUserAdmissionApplicationsViewPermissions2()
        {
            // access is ok if the current user has the view registrations permission
            if (!HasPermission(StudentPermissionCodes.ViewApplications) && !HasPermission(StudentPermissionCodes.UpdateApplications))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view admission-applications.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to view admission-applications.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
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
                admissionApplication.AcademicLoad = await ConvertEntityToAcademicLoadTypeDto(source.ApplicationStudentLoadIntent);
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
                admissionApplication.AcademicLoad = await ConvertEntityToAcademicLoadTypeDto(source.ApplicationStudentLoadIntent);
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdmissionApplications domain entity to its corresponding AdmissionApplications DTO
        /// </summary>
        /// <param name="source">AdmissionApplications domain entity</param>
        /// <returns>AdmissionApplications DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.AdmissionApplication3> ConvertAdmissionApplicationsEntityToDto3Async(Domain.Student.Entities.AdmissionApplication source,
            Dictionary<string, string> personGuidCollection, Dictionary<string, string> ownerIdCollection, bool bypassCache)
        {
                var admissionApplication = new Ellucian.Colleague.Dtos.AdmissionApplication3();
                //Required fields
                admissionApplication.Id = source.Guid;

                if (string.IsNullOrEmpty(source.ApplicantPersonId))
                {
                    IntegrationApiExceptionAddError("Person key is required.", "applicant.id", source.Guid, source.ApplicationRecordKey);
                }

                var personGuid = string.Empty;
                personGuidCollection.TryGetValue(source.ApplicantPersonId, out personGuid);
                if (string.IsNullOrEmpty(personGuid))
                {                    
                    IntegrationApiExceptionAddError(string.Concat("Person guid not found, PersonId: '", source.ApplicantPersonId, "'"), "applicant.id", source.Guid, source.ApplicationRecordKey);
                }
                admissionApplication.Applicant = new GuidObject2(personGuid);
                admissionApplication.ApplicationAcademicPrograms = new List<GuidObject2>() { new GuidObject2(source.ApplicationAcadProgramGuid) };

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

                admissionApplication.ReferenceID = string.IsNullOrEmpty(source.ApplicationNo) ? null : source.ApplicationNo;
                admissionApplication.Type = await ConvertEntityToTypeGuidObjectDto3Async(source.ApplicationIntgType, bypassCache, source.Guid, source.ApplicationRecordKey);
                admissionApplication.AcademicPeriod = await ConvertEntityToAcademicPeriodGuidObject2Async(source.ApplicationStartTerm, source.Guid, source.ApplicationRecordKey);
                admissionApplication.ApplicationSource = await ConvertEntityToApplicationSourceGuidObjectDto2Async(source.ApplicationSource, source.Guid, source.ApplicationRecordKey);
                admissionApplication.AdmissionPopulation = await ConvertEntityToAdmitStatusGuidObjectDto2Async(source.ApplicationAdmitStatus, source.Guid, source.ApplicationRecordKey);
                admissionApplication.Site = await ConvertEntityToApplLocationGuidObjectDto2Async(source.ApplicationLocations, source.Guid, source.ApplicationRecordKey);
                admissionApplication.ResidencyType = await ConvertEntityToApplResidencyStatusGuidObjectDto2Async(source.ApplicationResidencyStatus, source.Guid, source.ApplicationRecordKey);
                admissionApplication.AcademicLoad = await ConvertEntityToAcademicLoadTypeDto(source.ApplicationStudentLoadIntent);
                admissionApplication.Withdrawal = await ConvertEntityToWithdrawlDto3Async(source.ApplicationAttendedInstead, source.ApplicationWithdrawReason,
                    source.ApplicationWithdrawDate, withdrawnOn, personGuidCollection, bypassCache, source.Guid, source.ApplicationRecordKey);
                admissionApplication.Comment = string.IsNullOrEmpty(source.ApplicationComments) ? null : source.ApplicationComments;

            return admissionApplication;
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
        private async Task<GuidObject2> ConvertEntityToAcademicPeriodGuidObject2Async(string sourceCode, string sourceGuid, string sourceRecordKey)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            //var source = (await Terms(bypassCache)).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
            var termsGuid = string.Empty;
            try
            {
                termsGuid = await _termRepository.GetAcademicPeriodsGuidAsync(sourceCode);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: sourceGuid, id: sourceRecordKey);
            }
            if (string.IsNullOrEmpty(termsGuid))
            {
                IntegrationApiExceptionAddError(string.Concat("Term not found for code '", sourceCode, "'"), "academicPeriod.id", sourceGuid, sourceRecordKey);
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
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToApplicationSourceGuidObjectDto2Async(string sourceCode, string sourceGuid, string sourceRecordKey)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            var applicationSourcesGuid = string.Empty;
            try
            {
                applicationSourcesGuid = await _studentReferenceDataRepository.GetApplicationSourcesGuidAsync(sourceCode);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: sourceGuid, id: sourceRecordKey);
            }
            if (string.IsNullOrEmpty(applicationSourcesGuid))
            {
                IntegrationApiExceptionAddError(string.Concat("Application source not found for code '", sourceCode, "'"), "source.id", sourceGuid, sourceRecordKey);
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
        /// Convert code to guid.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns>GuidObject2</returns>
        private async Task<GuidObject2> ConvertEntityToAdmitStatusGuidObjectDto2Async(string sourceCode, string sourceGuid, string sourceRecordKey)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            var admissionPopulationsGuid = string.Empty;
            try
            {
                admissionPopulationsGuid = await _studentReferenceDataRepository.GetAdmissionPopulationsGuidAsync(sourceCode);
            }
            catch (RepositoryException ex)
            {
                // if we encountered a repo error here, we want to abort immediately.  An error means we have an incomplete 
                // list of admit statuses.  So we may have false positives for admit statuses in next error message about an admit status not found for code <admit status>
                IntegrationApiExceptionAddError(ex, guid: sourceGuid, id: sourceRecordKey);
                throw IntegrationApiException;
            }
            if (string.IsNullOrEmpty(admissionPopulationsGuid))
            {
                IntegrationApiExceptionAddError(string.Concat("Application admit status not found for code '", sourceCode, "'"), "admissionPopulation.id", sourceGuid, sourceRecordKey);
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
        /// Convert location code to guid.
        /// </summary>
        /// <param name="list">list of locations.  Only the first value in the list will be used.</param>
        /// <returns>GuidObject2</returns>
        private async Task<GuidObject2> ConvertEntityToApplLocationGuidObjectDto2Async(List<string> list, string sourceGuid, string sourceRecordKey
)
        {
            if (list == null || (!list.Any()))
            {
                return null;
            }

            var site = list.FirstOrDefault();
            var siteGuid = string.Empty;
            try
            {
                siteGuid = await _referenceDataRepository.GetLocationsGuidAsync(site);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: sourceGuid, id: sourceRecordKey);
            }
            if (string.IsNullOrEmpty(siteGuid))
            {
                IntegrationApiExceptionAddError(string.Concat("Site not found for code '", site, "'"), "site.id", sourceGuid, sourceRecordKey);
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
        /// <returns>GuidObject2</returns>
        private async Task<GuidObject2> ConvertEntityToApplResidencyStatusGuidObjectDto2Async(string sourceCode, string sourceGuid, string sourceRecordKey
)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var admissionResidencyTypesGuid = string.Empty;
            try
            {
                admissionResidencyTypesGuid = await _studentReferenceDataRepository.GetAdmissionResidencyTypesGuidAsync(sourceCode);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: sourceGuid, id: sourceRecordKey);
            }

            if (string.IsNullOrEmpty(admissionResidencyTypesGuid))
            {
                IntegrationApiExceptionAddError(string.Concat("Residency type not found for code '", sourceCode, "'"), "residencyType.id", sourceGuid, sourceRecordKey);
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

            var levelSourceGuid = string.Empty;
            var academicPrograms = await AcademicProgramsAsync(bypassCache);
            if (academicPrograms != null && academicPrograms.Any())
            {
                var academicProgram = academicPrograms.FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
                if (academicProgram == null)
                {
                    var error = string.Format("Academic program not found for code {0}. ", sourceCode);
                    throw new KeyNotFoundException(error);
                }

                if (!string.IsNullOrEmpty(academicProgram.AcadLevelCode))
                {
                    levelSourceGuid = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(academicProgram.AcadLevelCode);
                    if (string.IsNullOrEmpty(levelSourceGuid))
                    {
                        var error = string.Format("Academic level not found for code {0}. ", academicProgram.AcadLevelCode);
                        throw new KeyNotFoundException(error);
                    }
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

            var academicPrograms = await AcademicProgramsAsync(bypassCache);
            if (academicPrograms != null && academicPrograms.Any())
            {
                var source = academicPrograms.FirstOrDefault(i => i.Code.Equals(program, StringComparison.OrdinalIgnoreCase));
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
                            var disciplines = await AcademicDisciplinesAsync(bypassCache);
                            if (disciplines != null && disciplines.Any())
                            {
                                var acadDiscSource = disciplines.FirstOrDefault(disc => disc.Code.Equals(item, StringComparison.OrdinalIgnoreCase));
                                if (acadDiscSource == null)
                                {
                                    var error = string.Format("Academic disciplines not found for code {0}. ", item);
                                    throw new KeyNotFoundException(error);
                                }
                                academicDisciplineList.Add(new AdmissionApplicationDiscipline() { Discipline = new GuidObject2(acadDiscSource.Guid) });
                            }
                        }
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
        /// Gets admission application withdrawl
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
        /// Gets admission applicaion withdrawl
        /// </summary>
        /// <param name="applicationAttendedInstead"></param>
        /// <param name="applicationWithdrawReason"></param>
        /// <returns></returns>
        private async Task<Dtos.DtoProperties.AdmissionApplicationsWithdrawal3> ConvertEntityToWithdrawlDto3Async(string applicationAttendedInstead, string applicationWithdrawReason,
            DateTime? applicationWithdrawDate, DateTime? withdrawnOn, Dictionary<string, string> personGuidCollection, bool bypassCache, string sourceGuid, string sourceRecordKey)
        {
            if (string.IsNullOrWhiteSpace(applicationAttendedInstead + applicationWithdrawReason) && applicationWithdrawDate == null
                  && withdrawnOn == null)
                return null;

            var admApplWithdrawReason = new Dtos.DtoProperties.AdmissionApplicationsWithdrawal3();
            if (!string.IsNullOrEmpty(applicationWithdrawReason))
            {
                var applicationWithdrawReasonGuid = string.Empty;
                try
                {
                    applicationWithdrawReasonGuid = await _studentReferenceDataRepository.GetWithdrawReasonsGuidAsync(applicationWithdrawReason);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, guid: sourceGuid, id: sourceRecordKey);
                }

                if (string.IsNullOrEmpty(applicationWithdrawReasonGuid))
                {
                    IntegrationApiExceptionAddError(string.Concat("Withdraw reason not found for code '", applicationWithdrawReason, "'"), "withdrawal.reason.id", sourceGuid, sourceRecordKey);
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
                    IntegrationApiExceptionAddError(string.Concat("Institution attended guid not found for id '", applicationAttendedInstead, "'"), "withdrawal.institutionAttended.id", sourceGuid, sourceRecordKey);
                }

                personGuidCollection.TryGetValue(applicationAttendedInstead, out institutionGuid);
                if (string.IsNullOrEmpty(institutionGuid))
                {
                    IntegrationApiExceptionAddError(string.Concat("Institution attended guid not found for id '", applicationAttendedInstead, "'"), "withdrawal.institutionAttended.id", sourceGuid, sourceRecordKey);
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
        private async Task<AdmissionApplicationsAcademicLoadType> ConvertEntityToAcademicLoadTypeDto(string applicationStudentLoadIntent)
        {
            if (string.IsNullOrEmpty(applicationStudentLoadIntent))
            {
                return AdmissionApplicationsAcademicLoadType.NotSet;
            }
            
            var studentLoads = await StudentLoadsAsync();
            if (studentLoads != null && studentLoads.Any())
            {
                var load = studentLoads.FirstOrDefault(l => l.Code.Equals(applicationStudentLoadIntent));
                if (load != null && load.Sp1 != null)
                {
                    if (load.Sp1 == "1")
                    {
                        return AdmissionApplicationsAcademicLoadType.PartTime;
                    }
                    if (load.Sp1 == "2" || load.Sp1 == "3")
                    {
                        return AdmissionApplicationsAcademicLoadType.FullTime;
                    }
                }
            }
            return AdmissionApplicationsAcademicLoadType.NotSet;
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
        /// Gets AdmissionApplicationTypes guid objects
        /// </summary>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToTypeGuidObjectDto3Async(string intgType, bool bypassCache, string sourceGuid, string sourceRecordKey)
        {
            if (string.IsNullOrEmpty(intgType))
            {
                var source = (await AdmissionApplicationTypesAsync(bypassCache)).FirstOrDefault();
                if (source != null)
                {
                    return string.IsNullOrEmpty(source.Guid) ? null : new GuidObject2(source.Guid);
                }
                else
                {
                    //  will only occur if missing GUID for code ST (standard) in Ellucian-maintained INTG.APPLICATION.TYPES valcode.
                    IntegrationApiExceptionAddError("Admission application type not found for 'ST' (or any other code)", "type.id", sourceGuid, sourceRecordKey);
                    return null;
                }
            }
            else
            {
                var intgTypeGuid = await _studentReferenceDataRepository.GetAdmissionApplicationTypesGuidAsync(intgType);
                if (string.IsNullOrEmpty(intgTypeGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Admission application type not found for '{0}'", intgTypeGuid), "type.id", sourceGuid, sourceRecordKey);
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
        private IEnumerable<Domain.Student.Entities.ApplicationSource> _applicationSources;
        private async Task<IEnumerable<Domain.Student.Entities.ApplicationSource>> ApplicationSourcesAsync(bool bypassCache)
        {
            if (_applicationSources == null)
            {
                _applicationSources = await _studentReferenceDataRepository.GetApplicationSourcesAsync(bypassCache);
            }
            return _applicationSources;
        }

        /// <summary>
        /// PersonSources
        /// </summary>
        private IEnumerable<Domain.Base.Entities.PersonOriginCodes> _personSources;
        private async Task<IEnumerable<Domain.Base.Entities.PersonOriginCodes>> PersonSourcesAsync(bool bypassCache)
        {
            if (_personSources == null)
            {
                _personSources = await _referenceDataRepository.GetPersonOriginCodesAsync(bypassCache);
            }
            return _personSources;
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
        private IEnumerable<Domain.Student.Entities.AcademicDepartment> _academicDepartments;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicDepartment>> AcademicDepartmentsAsync(bool bypassCache)
        {
            if (_academicDepartments == null)
            {
                _academicDepartments = await _studentReferenceDataRepository.GetAcademicDepartmentsAsync(bypassCache);
            }
            return _academicDepartments;
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
        
        /// <summary>
        /// Acad Credential
        /// </summary>
        private IEnumerable<Domain.Base.Entities.AcadCredential> _acadCredential;
        private async Task<IEnumerable<Domain.Base.Entities.AcadCredential>> AcademicCredentialsAsync(bool bypassCache)
        {
            if (_acadCredential == null)
            {
                _acadCredential = await _referenceDataRepository.GetAcadCredentialsAsync(bypassCache);
            }
            return _acadCredential;
        }

        /// <summary>
        /// Acad Credential
        /// </summary>
        private IEnumerable<StudentLoad> _studentLoads;
        private async Task<IEnumerable<StudentLoad>> StudentLoadsAsync()
        {
            if (_studentLoads == null)
            {
                _studentLoads = await _studentReferenceDataRepository.GetStudentLoadsAsync();
            }
            return _studentLoads;
        }

        //private IEnumerable<Domain.Base.Entities.Department> _departments = null;
        //private async Task<IEnumerable<Domain.Base.Entities.Department>> DepartmentsAsync(bool bypassCache = false)
        //{
        //    if (_departments == null)
        //    {
        //        _departments = await _referenceDataRepository.GetDepartmentsAsync(bypassCache);
        //    }
        //    return _departments;
        //}
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

        #region admission-applications-submissions
        public async Task<Dtos.AdmissionApplicationSubmission> GetAdmissionApplicationsSubmissionsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid");
                }
                // access is ok if the current user has the view, or create, permission
                if (!await CheckAdmissionApplicationCreateUpdatePermAsync())
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create/update admission applications.");
                    throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to create/update admission applications.");
                }
                var admissionApplication = await _admissionApplicationsRepository.GetAdmissionApplicationSubmissionByIdAsync(guid);
                if (admissionApplication == null)
                {
                    throw new KeyNotFoundException(string.Format("No admission-application was found for guid '{0}'", guid));
                }
                var admissionApplications = new List<Ellucian.Colleague.Domain.Student.Entities.AdmissionApplication>() { admissionApplication };

                var ownerIds = await GetLocalOwnerIdsAsync(admissionApplications);
                var personIds = BuildLocalPersonGuids(admissionApplications, ownerIds);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                Dtos.AdmissionApplicationSubmission dto = await ConvertAdmissionApplicationsSubmissionsEntityToDtoAsync(admissionApplication, personGuidCollection, ownerIds, bypassCache);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return dto ?? null;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No admission-application was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No admission-application was found for guid '{0}'", guid), ex);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Updates admission application.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="dto"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Dtos.AdmissionApplication3> UpdateAdmissionApplicationsSubmissionAsync(string guid, Dtos.AdmissionApplicationSubmission dto, bool bypassCache)
        {
            if (!await CheckAdmissionApplicationCreateUpdatePermAsync())
            {                
                logger.Error(string.Format("User '{0}' is not authorized to create admission-applications-submissions.", CurrentUser.UserId));
                IntegrationApiExceptionAddError(string.Format("User '{0}' is not authorized to view section-registrations.", CurrentUser.UserId), "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }

            try
            {
                var applicationsId = await _admissionApplicationsRepository.GetRecordKeyAsync(dto.Id);
                if (!string.IsNullOrEmpty(applicationsId))
                {
                    _admissionApplicationsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    ValidatePayload(dto);
                    Domain.Student.Entities.AdmissionApplication admissionApplicationEntity = await ConvertDtoToEntityAsync(guid, dto, bypassCache);                                       

                    Domain.Student.Entities.AdmissionApplication admissionApplication = await _admissionApplicationsRepository.UpdateAdmissionApplicationSubmissionAsync(admissionApplicationEntity);

                    var admApplEntities = new List<Domain.Student.Entities.AdmissionApplication>() { admissionApplication };
                    var ownerIds = await GetLocalOwnerIdsAsync(admApplEntities);
                    var personIds = BuildLocalPersonGuids(admApplEntities, ownerIds);
                    var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                    return await ConvertAdmissionApplicationsEntityToDto3Async(admissionApplication, personGuidCollection, ownerIds, bypassCache);
                }
            }
            catch (IntegrationApiException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw e;
            }
            return await CreateAdmissionApplicationsSubmissionAsync(dto, bypassCache);
        }

        /// <summary>
        /// Creates admission application.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Dtos.AdmissionApplication3> CreateAdmissionApplicationsSubmissionAsync(Dtos.AdmissionApplicationSubmission dto, bool bypassCache)
        {
            if (!await CheckAdmissionApplicationCreateUpdatePermAsync())
            {
                logger.Error(string.Format("User '{0}' is not authorized to create admission-applications-submissions.", CurrentUser.UserId));
                IntegrationApiExceptionAddError(string.Format("User '{0}' is not authorized to view section-registrations.", CurrentUser.UserId), "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }

            try
            {
                _admissionApplicationsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                ValidatePayload(dto);
                Domain.Student.Entities.AdmissionApplication admissionApplicationEntity = await ConvertDtoToEntityAsync(null, dto, bypassCache);
                
                Domain.Student.Entities.AdmissionApplication admissionApplication = await _admissionApplicationsRepository.CreateAdmissionApplicationSubmissionAsync(admissionApplicationEntity);

                var admApplEntities = new List<Domain.Student.Entities.AdmissionApplication>() { admissionApplication };
                var ownerIds = await GetLocalOwnerIdsAsync(admApplEntities);
                var personIds = BuildLocalPersonGuids(admApplEntities, ownerIds);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                return await ConvertAdmissionApplicationsEntityToDto3Async(admissionApplication, personGuidCollection, ownerIds, bypassCache);
            }
            catch (IntegrationApiException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw e;
            }
        }        

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdmissionApplications domain entity to its corresponding AdmissionApplications DTO
        /// </summary>
        /// <param name="source">AdmissionApplications domain entity</param>
        /// <returns>AdmissionApplications DTO</returns>
        private async Task<Dtos.AdmissionApplicationSubmission> ConvertAdmissionApplicationsSubmissionsEntityToDtoAsync(Domain.Student.Entities.AdmissionApplication source,
            Dictionary<string, string> personGuidCollection, Dictionary<string, string> ownerIdCollection, bool bypassCache)
        {
            try
            {
                var dto = new Dtos.AdmissionApplicationSubmission();
                //Required fields
                if (string.IsNullOrEmpty(source.Guid))
                {
                    IntegrationApiExceptionAddError("Could not find a GUID for admission-applications entity.",
                        "admission-applications.id", id: source.ApplicationRecordKey);
                }
                dto.Id = source.Guid;

                //referenceId
                dto.ReferenceID = string.IsNullOrEmpty(source.ApplicationNo) ? null : source.ApplicationNo;

                //applicant.id
                var personGuid = string.Empty;
                if (!personGuidCollection.TryGetValue(source.ApplicantPersonId, out personGuid) || string.IsNullOrEmpty(source.ApplicantPersonId))
                {
                    IntegrationApiExceptionAddError(string.Format("Person guid not found, PersonId: '{0}', Record ID:'{1}", source.ApplicantPersonId, source.ApplicationRecordKey),
                        "applicant.id", source.Guid, source.ApplicationRecordKey);
                }
                dto.Applicant = new GuidObject2(personGuid);

                //type.id
                if (!string.IsNullOrEmpty(source.ApplicationIntgType))
                {                    
                    var intgTypeGuid = await _studentReferenceDataRepository.GetAdmissionApplicationTypesGuidAsync(source.ApplicationIntgType);
                    if (string.IsNullOrEmpty(intgTypeGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to locate guid for type ID '{0}'", source.ApplicationIntgType),
                        "type.id", source.Guid, source.ApplicationRecordKey);
                    }
                    dto.Type = new GuidObject2(intgTypeGuid);
                }

                //academicPeriod.id
                if (!string.IsNullOrEmpty(source.ApplicationStartTerm))
                {
                    var termsGuid = await _termRepository.GetAcademicPeriodsGuidAsync(source.ApplicationStartTerm);
                    if (string.IsNullOrEmpty(termsGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Term not found for code '{0}'. ", source.ApplicationStartTerm),
                        "academicPeriod.id", source.Guid, source.ApplicationRecordKey);
                    }
                    dto.AcademicPeriod = new GuidObject2(termsGuid);
                }

                //source.id
                if (!string.IsNullOrEmpty(source.ApplicationSource))
                {
                    var applicationSourcesGuid = await _studentReferenceDataRepository.GetApplicationSourcesGuidAsync(source.ApplicationSource);
                    if (string.IsNullOrEmpty(applicationSourcesGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Application source not found for code '{0}'. ", source.ApplicationSource),
                        "source.id", source.Guid, source.ApplicationRecordKey);
                    }
                    dto.ApplicationSource = new GuidObject2(applicationSourcesGuid);
                }

                //owner.id
                if (ownerIdCollection != null && (!string.IsNullOrEmpty(source.ApplicationAdmissionsRep)))
                {
                    var id = string.Empty;
                    //Check first if admin rep has value
                    var ownerGuid = string.Empty;
                    if (!personGuidCollection.TryGetValue(source.ApplicationAdmissionsRep, out ownerGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Person guid not found, PersonId: '{0}'", source.ApplicationAdmissionsRep),
                        "owner.id", source.Guid, source.ApplicationRecordKey);
                    }
                    dto.Owner = new GuidObject2(ownerGuid);

                }

                //admissionPopulation.id
                if (!string.IsNullOrEmpty(source.ApplicationAdmitStatus))
                {
                    var admissionPopulationsGuid = await _studentReferenceDataRepository.GetAdmissionPopulationsGuidAsync(source.ApplicationAdmitStatus);
                    if (string.IsNullOrEmpty(admissionPopulationsGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Application admit status not found for code {0}. ", source.ApplicationAdmitStatus),
                        "admissionPopulation.id", source.Guid, source.ApplicationRecordKey);
                    }
                    dto.AdmissionPopulation = new GuidObject2(admissionPopulationsGuid);
                }

                //site.id
                if (source.ApplicationLocations != null && source.ApplicationLocations.Any())
                {
                    string location = source.ApplicationLocations.FirstOrDefault();
                    if (!string.IsNullOrEmpty(location))
                    {
                        var siteGuid = await _referenceDataRepository.GetLocationsGuidAsync(location);
                        if (string.IsNullOrEmpty(siteGuid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for site ID '{0}'", location),
                                "site.id", source.Guid, source.ApplicationRecordKey);
                        }
                        dto.Site = new GuidObject2(siteGuid);
                    }
                }

                //residencyType.id
                if (!string.IsNullOrEmpty(source.ApplicationResidencyStatus))
                {
                    var residencyTypeGuid = await _studentReferenceDataRepository.GetAdmissionResidencyTypesGuidAsync(source.ApplicationResidencyStatus);
                    if (string.IsNullOrEmpty(residencyTypeGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Residency type not found for code {0}. ", source.ApplicationResidencyStatus),
                        "residencyType.id", source.Guid, source.ApplicationRecordKey);
                    }
                    dto.ResidencyType = new GuidObject2(residencyTypeGuid);
                }

                //academicLoad
                dto.AcademicLoad = await ConvertEntityToAcademicLoadTypeDto(source.ApplicationStudentLoadIntent);

                //applicationAcademicPrograms
                var applicationAcademicPrograms = new List<ApplicationAcademicProgram>();
                if (string.IsNullOrEmpty(source.ApplicationAcadProgram))
                {
                    IntegrationApiExceptionAddError("Unable to locate guid for academic program.",
                        "applicationAcademicPrograms.program.id", source.Guid, source.ApplicationRecordKey);
                }
                string programGuid = "";
                string academicLevelGuid = "";
                var programEntity = (await AcademicProgramsAsync(false)).FirstOrDefault(i => i.Code.Equals(source.ApplicationAcadProgram, StringComparison.OrdinalIgnoreCase));
                if (programEntity == null)
                {
                    IntegrationApiExceptionAddError("Unable to locate guid for academic program.",
                        "applicationAcademicPrograms[0].program.id", source.Guid, source.ApplicationRecordKey);
                }
                else
                {
                    programGuid = programEntity.Guid;
                    var academicLevel = programEntity.AcadLevelCode;
                    if (!string.IsNullOrEmpty(academicLevel))
                    {
                        var acadLevelEntity = (await AcademicLevelsAsync(false)).FirstOrDefault(i => i.Code.Equals(academicLevel, StringComparison.OrdinalIgnoreCase));
                        if (acadLevelEntity == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Academic level not found for code {0}. ", academicLevel),
                                "applicationAcademicPrograms[0].academicLevel.id", source.Guid, source.ApplicationRecordKey);
                        }
                        else
                        {
                            academicLevelGuid = acadLevelEntity.Guid;
                        }
                    }
                }
                var applicationAcademicProgram = new ApplicationAcademicProgram()
                {
                    AcademicProgram = new GuidObject2(programGuid),
                    AcademicLevel = new GuidObject2(academicLevelGuid)
                };

                // applicationAcademicPrograms.disciplines
                if (source.ApplicationDisciplines != null && source.ApplicationDisciplines.Any())
                {
                    foreach (var discipline in source.ApplicationDisciplines)
                    {
                        var disciplineEntity = (await AcademicDisciplinesAsync(false)).FirstOrDefault(i => i.Code.Equals(discipline.Code, StringComparison.OrdinalIgnoreCase));
                        if (disciplineEntity == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for academic discipline '{0}'.", discipline),
                                "applicationPrograms[0].disciplines", source.Guid, source.ApplicationRecordKey);
                        }
                        else
                        {
                            string deptGuid = "";
                            if (!string.IsNullOrEmpty(discipline.AdministeringInstitutionUnit))
                            {
                                var acadDeptEntity = (await AcademicDepartmentsAsync(false)).FirstOrDefault(i => i.Code.Equals(discipline.AdministeringInstitutionUnit, StringComparison.OrdinalIgnoreCase));
                                if (acadDeptEntity != null && !string.IsNullOrEmpty(acadDeptEntity.Guid))
                                {
                                    deptGuid = acadDeptEntity.Guid;
                                }
                            }
                            if (applicationAcademicProgram.Disciplines == null)
                            {
                                applicationAcademicProgram.Disciplines = new List<AdmissionApplicationSubmissionDiscipline>();
                            }
                            applicationAcademicProgram.Disciplines.Add(new AdmissionApplicationSubmissionDiscipline()
                            {
                                Discipline = new GuidObject2(disciplineEntity.Guid),
                                StartOn = discipline.StartOn,
                                AdministeringInstitutionUnit = new GuidObject2(deptGuid)
                            });
                        }
                    }
                }
                //applicationAcademicPrograms.credentials
                if (source.ApplicationCredentials != null && source.ApplicationCredentials.Any())
                {
                    foreach (var credential in source.ApplicationCredentials)
                    {
                        var credentialEntity = (await AcademicCredentialsAsync(false)).FirstOrDefault(i => i.Code.Equals(credential, StringComparison.OrdinalIgnoreCase));
                        if (credentialEntity == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for academic credential '{0}'.", credential),
                                "applicationAcademicPrograms[0].disciplines", source.Guid, source.ApplicationRecordKey);
                        }
                        else
                        {
                            if (applicationAcademicProgram.AcademicCredentials == null)
                            {
                                applicationAcademicProgram.AcademicCredentials = new List<GuidObject2>();
                            }
                            applicationAcademicProgram.AcademicCredentials.Add(new GuidObject2(credentialEntity.Guid));
                        }
                    }
                }

                //applicationAcademicPrograms.programOwner.id
                if (!string.IsNullOrEmpty(source.ApplicationProgramOwner))
                {
                    var departmentEntity = (await AcademicDepartmentsAsync(false)).FirstOrDefault(i => i.Code.Equals(source.ApplicationProgramOwner, StringComparison.OrdinalIgnoreCase));
                    if (departmentEntity == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to locate guid for academic department '{0}'.", source.ApplicationOwnerId),
                                "applicationAcademicPrograms[0].programOwner.id", source.Guid, source.ApplicationRecordKey);
                    }
                    else
                    {
                        applicationAcademicProgram.ProgramOwner = new GuidObject2(departmentEntity.Guid);
                    }
                }
                if (applicationAcademicProgram != null)
                {
                    applicationAcademicPrograms.Add(applicationAcademicProgram);
                }

                //withdrawal
                DateTime? withdrawnOn = null;
                var applicationStatuses = await GetApplicationStatusesAsync(bypassCache);
                var appliedStatuses = applicationStatuses.Where(ap => !ap.SpecialProcessingCode.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
                                                         .Select(ast => ast.Code).Distinct();
                var admittedStatuses = applicationStatuses.Where(ap => ap.SpecialProcessingCode == "AC")
                                                         .Select(ast => ast.Code).Distinct();
                var matriculatedStatuses = applicationStatuses.Where(ap => ap.SpecialProcessingCode == "MS")
                                                         .Select(ast => ast.Code).Distinct();
                var withdrawnStatuses = applicationStatuses.Where(ap => ap.SpecialProcessingCode == "WI")
                                                         .Select(ast => ast.Code).Distinct();
                if (source.AdmissionApplicationStatuses != null && source.AdmissionApplicationStatuses.Any())
                {
                    foreach (var statusEntity in source.AdmissionApplicationStatuses)
                    {
                        if ((appliedStatuses != null) && (appliedStatuses.Any())
                            && (appliedStatuses.Contains(statusEntity.ApplicationStatus)))
                        {
                            //appliedOn
                            dto.AppliedOn = statusEntity.ApplicationStatusDate;
                        }

                        if ((withdrawnStatuses != null) && (withdrawnStatuses.Any())
                            && (withdrawnStatuses.Contains(statusEntity.ApplicationStatus)))
                        {
                            withdrawnOn = statusEntity.ApplicationStatusDate;
                        }
                    }
                }
                dto.Withdrawal = await ConvertEntityToSubmissionWithdrawlDtoAsync(source.ApplicationAttendedInstead, source.ApplicationWithdrawReason,
                                        source.ApplicationWithdrawDate, withdrawnOn, personGuidCollection, source.Guid, source.ApplicationRecordKey, bypassCache);

                //comment
                dto.Comment = source.ApplicationComments ?? null;

                return dto;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(string.Concat(ex.Message, "admission application guid: ", source.Guid), ex);
            }
            catch (Exception ex)
            {
                var error = string.Concat("Something unexpected happened for guid ", source.Guid);
                throw new Exception(error, ex);
            }
        }

        /// <summary>
        /// Gets admission applicaion withdrawl
        /// </summary>
        /// <param name="applicationAttendedInstead"></param>
        /// <param name="applicationWithdrawReason"></param>
        /// <returns></returns>
        private async Task<Dtos.DtoProperties.AdmissionApplicationsWithdrawal2> ConvertEntityToSubmissionWithdrawlDtoAsync(string applicationAttendedInstead, string applicationWithdrawReason,
            DateTime? applicationWithdrawDate, DateTime? withdrawnOn, Dictionary<string, string> personGuidCollection, string sourceGuid, string recordKey, bool bypassCache)
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
                    IntegrationApiExceptionAddError(string.Format("Withdraw reason not found for reason {0}. ", applicationWithdrawReason),
                    "withdrawal.reason ", sourceGuid, recordKey);
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
                    IntegrationApiExceptionAddError(string.Format("Institution attended guid not found for id {0}. ", applicationAttendedInstead),
                    "withdrawal.institutionAttended.id", sourceGuid, recordKey);
                }
                
                if (!personGuidCollection.TryGetValue(applicationAttendedInstead, out institutionGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Institution attended guid not found for id {0}. ", applicationAttendedInstead),
                    "withdrawal.institutionAttended.id", sourceGuid, recordKey);
                }

                admApplWithdrawReason.InstitutionAttended = new AdmissionApplicationInstitutionAttendedDtoProperty()
                {
                    Id = institutionGuid,
                };
            }

            return admApplWithdrawReason;
        }

        /// <summary>
        /// Converts dto to an entity.
        /// </summary>
        /// <param name="recordKey"></param>
        /// <param name="dto"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.AdmissionApplication> ConvertDtoToEntityAsync(string guid, Dtos.AdmissionApplicationSubmission dto, bool bypassCache = false)
        {
            Domain.Student.Entities.AdmissionApplication entity = null;

            if (string.IsNullOrEmpty(guid))
            {
                entity = new Domain.Student.Entities.AdmissionApplication(dto.Id);
            }
            else
            {
                var recordKey = await _admissionApplicationsRepository.GetRecordKeyAsync(dto.Id);
                entity = new Domain.Student.Entities.AdmissionApplication(dto.Id, recordKey);
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
                if (string.IsNullOrEmpty(applicantKey))
                {
                    IntegrationApiExceptionAddError(string.Format("Person guid '{0}' is not valid for the admission application.", dto.Applicant.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicantPersonId = applicantKey;
                }
            }

            //type.id
            if (dto.Type != null && !string.IsNullOrEmpty(dto.Type.Id))
            {
                var typeKey = (await AdmissionApplicationTypesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.Type.Id, StringComparison.OrdinalIgnoreCase));
                if (typeKey == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Admission application submission type not found for guid '{0}'. ", dto.Type.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicationIntgType = typeKey.Code;
                }
            }
            else
            {
                var typeKey = (await AdmissionApplicationTypesAsync(bypassCache)).FirstOrDefault(i => i.Code.ToUpper().Equals("ST", StringComparison.OrdinalIgnoreCase));
                if (typeKey == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Admission application submission type not found for guid '{0}'. ", dto.Type.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                entity.ApplicationIntgType = typeKey.Code;
            }

            //academicPeriod.id
            if (dto.AcademicPeriod != null && !string.IsNullOrEmpty(dto.AcademicPeriod.Id))
            {
                var source = (await Terms(bypassCache)).FirstOrDefault(i => i.RecordGuid.Equals(dto.AcademicPeriod.Id, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Academic period not found for guid '{0}'. ", dto.AcademicPeriod.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else if (string.IsNullOrEmpty(source.Code))
                {
                    IntegrationApiExceptionAddError(string.Format("Academic period code not found for guid '{0}'. ", dto.AcademicPeriod.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicationStartTerm = source.Code;
                }
            }

            //applicationSource.id
            if (dto.ApplicationSource != null && !string.IsNullOrEmpty(dto.ApplicationSource.Id))
            {
                var source = (await ApplicationSourcesAsync(true)).FirstOrDefault(i => i.Guid.Equals(dto.ApplicationSource.Id, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Application source not found for guid '{0}'. ", dto.ApplicationSource.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicationSource = source.Code;
                }
            }

            //personSource.id
            if (dto.PersonSource != null && !string.IsNullOrEmpty(dto.PersonSource.Id))
            {
                var source = (await PersonSourcesAsync(true)).FirstOrDefault(i => i.Guid.Equals(dto.PersonSource.Id, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Person source not found for guid '{0}'. ", dto.PersonSource.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.PersonSource = source.Code;
                }
            }

            //owner.id persons.json
            if (dto.Owner != null && !string.IsNullOrEmpty(dto.Owner.Id))
            {
                var owner = await _personRepository.GetPersonIdFromGuidAsync(dto.Owner.Id);
                if (string.IsNullOrEmpty(owner))
                {
                    IntegrationApiExceptionAddError(string.Format("Application owner not found for guid '{0}'. ", dto.Owner.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicationAdmissionsRep = owner;
                }
            }

            //admissionPopulation.id
            if (dto.AdmissionPopulation != null && !string.IsNullOrEmpty(dto.AdmissionPopulation.Id))
            {
                var admPopulation = (await AdmissionPopulationsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.AdmissionPopulation.Id, StringComparison.OrdinalIgnoreCase));
                if (admPopulation == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Application admit status not found for guid '{0}'. ", dto.AdmissionPopulation.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicationAdmitStatus = admPopulation.Code;
                }
            }

            //site.id
            if (dto.Site != null && !string.IsNullOrEmpty(dto.Site.Id))
            {
                var site = (await SitesAsync(true)).FirstOrDefault(i => i.Guid.Equals(dto.Site.Id, StringComparison.OrdinalIgnoreCase));
                if (site == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Site not found for guid '{0}'.", dto.Site.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicationLocations = new List<string>() { site.Code };
                }
            }

            //residencyType.id
            if (dto.ResidencyType != null && !string.IsNullOrEmpty(dto.ResidencyType.Id))
            {
                //var residencyType = 
                var residencyType = (await AdmissionResidencyTypesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.ResidencyType.Id, StringComparison.OrdinalIgnoreCase));
                if (residencyType == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Residency type not found for guid '{0}'.", dto.ResidencyType.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicationResidencyStatus = residencyType.Code;
                }
            }

            //academicLoad
            if (dto.AcademicLoad != AdmissionApplicationsAcademicLoadType.NotSet)
            {
                entity.ApplicationStudentLoadIntent = dto.AcademicLoad.ToString().ToUpperInvariant();
            }

            //applicationAcademicPrograms
            Domain.Student.Entities.AcademicProgram applAcadProgram = null;
            AcademicLevel acadLevel = null;
            string acadLevelCode = string.Empty;

            if (dto.ApplicationAcademicPrograms != null && dto.ApplicationAcademicPrograms.Any())
            {
                //Will only contain 1 item in the array. In validation, we throw error if there are more than one 
                //(Per specs: Issue an error if there is more than one entry in the array).
                var acdProgDto = dto.ApplicationAcademicPrograms.FirstOrDefault();

                //applicationAcademicPrograms.program.id && applicationAcademicPrograms.academicLevel.id
                if (acdProgDto != null && acdProgDto.AcademicProgram != null && !string.IsNullOrEmpty(acdProgDto.AcademicProgram.Id))
                {
                    applAcadProgram = (await AcademicProgramsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(acdProgDto.AcademicProgram.Id, StringComparison.OrdinalIgnoreCase));
                    if (applAcadProgram == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic program not found for guid '{0}'.", acdProgDto.AcademicProgram.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                    }
                    else
                    {
                        acadLevelCode = applAcadProgram.AcadLevelCode;
                    }
                }

                //applicationAcademicPrograms.academicLevel.id
                if (acdProgDto != null && acdProgDto.AcademicLevel != null && !string.IsNullOrEmpty(acdProgDto.AcademicLevel.Id))
                {
                    acadLevel = (await AcademicLevelsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(acdProgDto.AcademicLevel.Id));
                    if (acadLevel == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic level not found for guid {0}.", acdProgDto.AcademicLevel.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                    }
                    else if (!acadLevel.Code.Equals(acadLevelCode, StringComparison.OrdinalIgnoreCase))
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic level '{0}' is not valid for the academic program '{1}'.", acadLevel.Code, applAcadProgram.Code),
                            "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                    }
                    else
                    {
                        entity.ApplicationAcadLevel = acadLevel.Code;
                    }
                }

                //ApplicationAcadProgram
                entity.ApplicationAcadProgram = (applAcadProgram != null && !string.IsNullOrEmpty(applAcadProgram.Code)) ? applAcadProgram.Code : string.Empty;

                //applicationAcademicPrograms.disciplines
                List<ApplicationDiscipline> disciplines = new List<ApplicationDiscipline>();

                if (acdProgDto.Disciplines != null && acdProgDto.Disciplines.Any())
                {
                    foreach (var disciplineDto in acdProgDto.Disciplines)
                    {
                        if (disciplineDto != null)
                        {
                            var acadDiscipline = (await AcademicDisciplinesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(disciplineDto.Discipline.Id));
                            if (acadDiscipline == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Academic discipline not found for guid '{0}'.", disciplineDto.Discipline.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                            }
                            else
                            {
                                string deptCode = "";
                                if (disciplineDto.AdministeringInstitutionUnit != null && !string.IsNullOrEmpty(disciplineDto.AdministeringInstitutionUnit.Id))
                                {
                                    var acadDeptEntity = (await AcademicDepartmentsAsync(false)).FirstOrDefault(i => i.Guid.Equals(disciplineDto.AdministeringInstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                                    if (acadDeptEntity != null && !string.IsNullOrEmpty(acadDeptEntity.Guid))
                                    {
                                        deptCode = acadDeptEntity.Code;
                                    }
                                }
                                if (entity.ApplicationStprAcadPrograms == null)
                                {
                                    entity.ApplicationStprAcadPrograms = new List<string>();
                                }
                                if (entity.ApplicationDisciplines == null)
                                {
                                    entity.ApplicationDisciplines = new List<ApplicationDiscipline>();
                                }
                                entity.ApplicationStprAcadPrograms.Add(acadDiscipline.Code);
                                entity.ApplicationDisciplines.Add(new ApplicationDiscipline()
                                {
                                    Code = acadDiscipline.Code,
                                    AdministeringInstitutionUnit = deptCode,
                                    DisciplineType = acadDiscipline.AcademicDisciplineType,
                                    StartOn = disciplineDto.StartOn ?? default(DateTime?)
                                });
                            }
                        }
                    }
                }

                //applicationAcademicPrograms.credentials
                if (acdProgDto.AcademicCredentials != null && acdProgDto.AcademicCredentials.Any())
                {
                    entity.ApplicationCredentials = new List<string>();
                    foreach (var credentialDto in acdProgDto.AcademicCredentials)
                    {
                        if (credentialDto != null && !string.IsNullOrEmpty(credentialDto.Id))
                        {
                            var credential = (await AcademicCredentialsAsync(bypassCache)).FirstOrDefault(c => c.Guid.Equals(credentialDto.Id));
                            if (credential == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Acdemic credential not found for guid '{0}'. ", credentialDto.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                            }
                            else
                            {
                                entity.ApplicationCredentials.Add(credential.Code);
                            }
                        }
                    }
                }

                //applicationAcademicPrograms.programOwner.id
                if (acdProgDto != null && acdProgDto.ProgramOwner != null && !string.IsNullOrEmpty(acdProgDto.ProgramOwner.Id))
                {
                    var departmentEntity = (await AcademicDepartmentsAsync(false)).FirstOrDefault(i => i.Guid.Equals(acdProgDto.ProgramOwner.Id,
                        StringComparison.OrdinalIgnoreCase));
                    if (departmentEntity == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Program owner not found for guid '{0}'. ", acdProgDto.ProgramOwner.Id), "Bad.Data",
                            dto.Applicant.Id, entity.ApplicationRecordKey);
                    }
                    else
                    {
                        entity.ApplicationProgramOwner = departmentEntity.Code;
                    }
                }
            }

            //appliedOn
            if (dto.AppliedOn.HasValue)
            {
                entity.AppliedOn = dto.AppliedOn.Value;
            }

            //withdrawal.WithdrawnOn
            if (dto.Withdrawal != null && dto.Withdrawal.WithdrawnOn.HasValue)
            {
                entity.WithdrawnOn = dto.Withdrawal.WithdrawnOn.Value;
            }

            //withdrawal.WithdrawalReason.id
            if (dto.Withdrawal != null && dto.Withdrawal.WithdrawalReason != null && !string.IsNullOrEmpty(dto.Withdrawal.WithdrawalReason.Id))
            {
                var withdrwlReason = (await WithdrawReasonsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.Withdrawal.WithdrawalReason.Id, StringComparison.OrdinalIgnoreCase));
                if (withdrwlReason == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Withdraw reason not found for guid '{0}'. ", dto.Withdrawal.WithdrawalReason.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                else
                {
                    entity.ApplicationWithdrawReason = withdrwlReason.Code;
                }
            }

            //withdrawal.InstitutionAttended.Id
            if (dto.Withdrawal != null && dto.Withdrawal.InstitutionAttended != null && !string.IsNullOrEmpty(dto.Withdrawal.InstitutionAttended.Id))
            {
                string instAttended = null;
                try
                {
                    instAttended = await _personRepository.GetPersonIdFromGuidAsync(dto.Withdrawal.InstitutionAttended.Id);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Institution attended not found for guid '{0}'. ", dto.Withdrawal.InstitutionAttended.Id), "Bad.Data", dto.Applicant.Id, entity.ApplicationRecordKey);
                }
                entity.ApplicationAttendedInstead = instAttended ?? string.Empty;
            }

            //comment
            entity.ApplicationComments = dto.Comment ?? dto.Comment;

            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return entity;
        }

        /// <summary>
        /// Validates request body.
        /// </summary>
        /// <param name="dto"></param>
        private void ValidatePayload(Dtos.AdmissionApplicationSubmission dto)
        {
            if (dto == null)
            {
                IntegrationApiExceptionAddError("AdmissionApplicationsSubmissions body is required.");
                throw IntegrationApiException;
            }

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                IntegrationApiExceptionAddError("id is required.");
                throw IntegrationApiException;
            }

            //Required fields.
            if (dto.Applicant == null)
            {
                IntegrationApiExceptionAddError("Applicant is required.");
                throw IntegrationApiException;
            }

            if (dto.ApplicationAcademicPrograms == null || !dto.ApplicationAcademicPrograms.Any())
            {
                IntegrationApiExceptionAddError("ApplicationAcademicPrograms is required.");
                throw IntegrationApiException;
            }

            if (dto.Applicant != null && string.IsNullOrWhiteSpace(dto.Applicant.Id))
            {
                IntegrationApiExceptionAddError("Applicant id is required.");
            }


            if (dto.ApplicationAcademicPrograms != null && dto.ApplicationAcademicPrograms.Any())
            {
                if(dto.ApplicationAcademicPrograms.Count() > 1)
                {
                    IntegrationApiExceptionAddError("Only one program per application can be submitted.");
                }
                
                var prgm = dto.ApplicationAcademicPrograms.Where(i => i.AcademicProgram != null);
                if (prgm == null || !prgm.Any())
                {
                    IntegrationApiExceptionAddError("Programs are required.");
                }

                foreach (var program in dto.ApplicationAcademicPrograms)
                {
                    //program
                    if (program == null)
                    {
                        IntegrationApiExceptionAddError("applicationAcademicPrograms.program is required.");
                    }

                    //program.AcademicProgram.Id
                    if (program != null && program.AcademicProgram != null && string.IsNullOrEmpty(program.AcademicProgram.Id))
                    {
                        IntegrationApiExceptionAddError("applicationAcademicPrograms.program.id is required.");
                    }

                    //academicLevel.id
                    if (program != null && program.AcademicLevel != null && string.IsNullOrEmpty(program.AcademicLevel.Id))
                    {
                        IntegrationApiExceptionAddError("applicationAcademicPrograms.academicLevel.id is required.");
                    }

                    //credentials
                    if (program != null && program.AcademicCredentials != null && program.AcademicCredentials.Any())
                    {
                        program.AcademicCredentials.ToList().ForEach(cred =>
                        {
                            if (cred != null && string.IsNullOrEmpty(cred.Id))
                            {
                                IntegrationApiExceptionAddError("applicationAcademicPrograms.credential.id is required.");
                            }
                        });
                    }

                    //programOwner
                    if (program != null && program.ProgramOwner != null && string.IsNullOrEmpty(program.ProgramOwner.Id))
                    {
                        IntegrationApiExceptionAddError("applicationAcademicPrograms.programOwner.id is required.");
                    }

                    if (program != null && program.Disciplines != null && program.Disciplines.Any())
                    {
                        program.Disciplines.ToList().ForEach(discipline =>
                        {
                            if (discipline != null && discipline.Discipline == null)
                            {
                                IntegrationApiExceptionAddError("applicationAcademicPrograms.disciplines.discipline is required.");
                            }

                            if (discipline != null && discipline.Discipline != null && string.IsNullOrEmpty(discipline.Discipline.Id))
                            {
                                IntegrationApiExceptionAddError("applicationAcademicPrograms.disciplines.discipline.id is required.");
                            }

                            if (discipline != null && discipline.AdministeringInstitutionUnit != null && string.IsNullOrEmpty(discipline.AdministeringInstitutionUnit.Id))
                            {
                                IntegrationApiExceptionAddError("applicationAcademicPrograms.disciplines.administeringInstitutionUnit.id is required.");
                            }
                        });
                    }
                }

                //applicationAcademicPrograms.withdrawal.withdrawalReason.id
                if (dto != null && dto.Withdrawal != null &&
                    dto.Withdrawal.WithdrawalReason != null && string.IsNullOrEmpty(dto.Withdrawal.WithdrawalReason.Id))
                {
                    IntegrationApiExceptionAddError("applicationAcademicPrograms.withdrawal.withdrawalReason.id is required.");
                }

                //applicationAcademicPrograms.withdrawal.institutionAttended.id
                if (dto != null && dto.Withdrawal != null &&
                    dto.Withdrawal.InstitutionAttended != null && string.IsNullOrEmpty(dto.Withdrawal.InstitutionAttended.Id))
                {
                    IntegrationApiExceptionAddError("applicationAcademicPrograms.withdrawal.institutionAttended.id is required.");
                }
            }

            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
        }

        #endregion
    }
}