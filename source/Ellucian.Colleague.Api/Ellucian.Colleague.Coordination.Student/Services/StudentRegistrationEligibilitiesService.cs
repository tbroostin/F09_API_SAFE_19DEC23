//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentRegistrationEligibilitiesService : BaseCoordinationService, IStudentRegistrationEligibilitiesService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ITermRepository _termRepository;
        private readonly IRegistrationPriorityRepository _registrationPriorityRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentRegistrationEligibilitiesService(

            IPersonRepository personRepository,
            IStudentRepository studentRepository,
            IRegistrationPriorityRepository registrationPriorityRepository,
            ITermRepository termRepository,
            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {
            _personRepository = personRepository;
            _studentRepository = studentRepository;
            _registrationPriorityRepository = registrationPriorityRepository;
            _termRepository = termRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets a single student-registration-eligibilities for filter of student and term.
        /// </summary>
        /// <returns>A single StudentRegistrationEligibilities DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentRegistrationEligibilities> GetStudentRegistrationEligibilitiesAsync(string studentId, string academicPeriodId, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                IntegrationApiExceptionAddError("Both student and academicPeriod filters are required on GET operation.", "Validation.Exception");
                throw IntegrationApiException;
            }
            if (string.IsNullOrEmpty(academicPeriodId))
            {
                IntegrationApiExceptionAddError("Both student and academicPeriod filters are required on GET operation.", "Validation.Exception");
                throw IntegrationApiException;
            }

           
            var personId = string.Empty;
            try
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(studentId);
                if (string.IsNullOrEmpty(personId))
                {
                    return null;
                }
            }
            catch (Exception)
            {            
                return null;
            }

            var studentRegistrationEligibilitiesCollection = new List<Ellucian.Colleague.Dtos.StudentRegistrationEligibilities>();

            // Retrieve the term for the priority checking (cannot just pull the registration ones due to the reporting term check).
            Term academicPeriod = null;
            try
            {
                var allTerms = await _termRepository.GetAsync();
                if (allTerms == null || allTerms.Count() == 0)
                {           
                    return null;
                }
                else
                {
                    academicPeriod = allTerms.FirstOrDefault(rt => rt.RecordGuid == academicPeriodId);
                    if (academicPeriod == null || string.IsNullOrEmpty(academicPeriod.Code))
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
               return null;
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
          
            IEnumerable<RegistrationPriority> studentRegistrationPriorities = null;
            RegistrationEligibility registrationEligibility = null;
            try
            {
                registrationEligibility = await _studentRepository.CheckRegistrationEligibilityEthosAsync(personId, new List<string>() { academicPeriod.Code });

                // Next determine if the student has any registration priority (or is missing one where required).
                // Registration priorities can affect the registration eligibility status for a term and the anticipated add date.
                studentRegistrationPriorities = await _registrationPriorityRepository.GetAsync(personId);

                // Next deal with any registration priorities - these may override the information above.
                // Even if the student has no priorities you still need to do this update because if the term
                // requires priorities and they don't have any then it changes their registration status.
                registrationEligibility.UpdateRegistrationPriorities(studentRegistrationPriorities, new List<Term>() { academicPeriod });
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, "Bad.Data");
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                throw IntegrationApiException;
            }
            var retval = ConvertStudentRegistrationEligibilitiesEntityToDto(registrationEligibility, studentRegistrationPriorities, studentId, academicPeriod);

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return retval;

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentRegistrationEligibilities domain entity to its corresponding StudentRegistrationEligibilities DTO
        /// </summary>
        /// <param name="registrationEligibility">The eligibility data for registration</param>
        /// <param name="studentRegistrationPriorities">List of registration priorities for this student</param>
        /// <param name="studentGuid">GUID for the student we are processing</param>
        /// <param name="academicPeriod">Academic Period object (term)</param>
        /// <returns>StudentRegistrationEligibilities DTO</returns>
        private Ellucian.Colleague.Dtos.StudentRegistrationEligibilities ConvertStudentRegistrationEligibilitiesEntityToDto(RegistrationEligibility registrationEligibility, IEnumerable<RegistrationPriority> studentRegistrationPriorities, string studentGuid, Term academicPeriod)
        {
            var studentRegistrationEligibilities = new Ellucian.Colleague.Dtos.StudentRegistrationEligibilities();

            studentRegistrationEligibilities.Student = new GuidObject2(studentGuid);
            studentRegistrationEligibilities.AcademicPeriod = new GuidObject2(academicPeriod.RecordGuid);
            if (studentRegistrationPriorities != null)
            {
                var registrationPriority = studentRegistrationPriorities.FirstOrDefault(rp => rp.TermCode == academicPeriod.Code);
                if (registrationPriority != null && !string.IsNullOrEmpty(registrationPriority.StudentId))
                {
                    // Only include optional properties for dates if we have values coming in
                    // from reg priorities records.
                    if (registrationPriority.Start.HasValue && registrationPriority.End.HasValue)
                    {
                        studentRegistrationEligibilities.PriorityRegistrationTimeSlots = new List<Dtos.DtoProperties.StudentRegistrationEligibilitiesPriorityRegistrationTimeSlots>();
                        var registrationTimeSlot = new Dtos.DtoProperties.StudentRegistrationEligibilitiesPriorityRegistrationTimeSlots();
                        studentRegistrationEligibilities.StartOn = registrationPriority.Start.Value.DateTime;
                        registrationTimeSlot.StartOn = registrationPriority.Start.Value.DateTime.ToLocalTime();
                        studentRegistrationEligibilities.EndOn = registrationPriority.End.Value.DateTime;
                        registrationTimeSlot.EndOn = registrationPriority.End.Value.DateTime.ToLocalTime();
                        studentRegistrationEligibilities.PriorityRegistrationTimeSlots.Add(registrationTimeSlot);
                    }
                }
            }
            // If the term requires that registration priorities be setup and this student doesn't have one,
            // then the status is Ineligible without further verification.
            if (academicPeriod.RegistrationPriorityRequired && (studentRegistrationPriorities == null || !studentRegistrationPriorities.Any()))
            {
                studentRegistrationEligibilities.EligibilityStatus = StudentRegistrationEligibilitiesEligibilityStatus.Ineligible;
            }
            else
            {
                if (registrationEligibility.IsEligible)
                {
                    studentRegistrationEligibilities.EligibilityStatus = StudentRegistrationEligibilitiesEligibilityStatus.Eligible;
                }
                else
                {
                    studentRegistrationEligibilities.EligibilityStatus = StudentRegistrationEligibilitiesEligibilityStatus.Ineligible;
                }
            }
            // Look at the terms to see if we have a specific start on outside of reg priority
            // or any ineligible message to be returned.
            if (registrationEligibility.Terms != null && registrationEligibility.Terms.Any())
            {
                var termEligibility = registrationEligibility.Terms.FirstOrDefault(st => st.TermCode == academicPeriod.Code);
                if (termEligibility == null)
                {
                    studentRegistrationEligibilities.EligibilityStatus = StudentRegistrationEligibilitiesEligibilityStatus.Ineligible;
                }
                else
                {
                    if (termEligibility.AnticipatedTimeForAdds != null && termEligibility.AnticipatedTimeForAdds.HasValue)
                    {
                        studentRegistrationEligibilities.StartOn = termEligibility.AnticipatedTimeForAdds.Value.DateTime;
                    }
                    if (termEligibility.Status == RegistrationEligibilityTermStatus.NotEligible)
                    {
                        studentRegistrationEligibilities.EligibilityStatus = StudentRegistrationEligibilitiesEligibilityStatus.Ineligible;
                    }
                    if (studentRegistrationEligibilities.EligibilityStatus == StudentRegistrationEligibilitiesEligibilityStatus.Ineligible)
                    {
                        if (!string.IsNullOrEmpty(termEligibility.Message))
                        {
                            studentRegistrationEligibilities.IneligibilityReasons = new List<string>();
                            studentRegistrationEligibilities.IneligibilityReasons.Add(termEligibility.Message);
                        }
                    }
                }
            }
            if (studentRegistrationEligibilities.EligibilityStatus == StudentRegistrationEligibilitiesEligibilityStatus.Ineligible && registrationEligibility.Messages != null && registrationEligibility.Messages.Any())
            {
                if (studentRegistrationEligibilities.IneligibilityReasons == null)
                {
                    studentRegistrationEligibilities.IneligibilityReasons = new List<string>();
                }
                foreach (var msg in registrationEligibility.Messages)
                {
                    studentRegistrationEligibilities.IneligibilityReasons.Add(msg.Message);
                }
            }

            return studentRegistrationEligibilities;
        }
    }
}