// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Planning.Services
{
    [RegisterType]
    public class PlanningStudentService : StudentCoordinationService, IPlanningStudentService
    {
        private readonly IPlanningStudentRepository _planningStudentRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger logger;
        private readonly IConfigurationRepository _configurationRepository;

        public PlanningStudentService(IAdapterRegistry adapterRegistry, IPlanningStudentRepository planningStudentRepository, 
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentRepository studentRepository,
            IConfigurationRepository configurationRepository, IStaffRepository staffRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository, staffRepository)
        {
            _configurationRepository = configurationRepository;
            _planningStudentRepository = planningStudentRepository;
            _adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Get the list of skinny PlanningStudents information for the list of student ids
        /// </summary>
        /// <param name="studentIds">list of student Ids</param>
        /// <returns>list of planning students Dtos</returns>
        public async Task<PrivacyWrapper<IEnumerable<Dtos.Student.PlanningStudent>>> QueryPlanningStudentsAsync(IEnumerable<string> studentIds)
        {
            if (null == studentIds || studentIds.Count() == 0)
            { 
                throw new ArgumentException("ids", "You must specify at least 1 id to retrieve.");
            }
            List<Dtos.Student.PlanningStudent> planningStudents = new List<Dtos.Student.PlanningStudent>();
            string requestor = CurrentUser.PersonId;
            bool hasPrivacyRestriction = false;
            //Should have ViewPersonInformation as well as ViewStudentInformation permissions OR have advisors permissions
            if (!(HasPermission(StudentPermissionCodes.ViewPersonInformation) && HasPermission(StudentPermissionCodes.ViewStudentInformation)) && !(await UserIsAdvisorAsync(requestor)))
            {
                throw new PermissionsException("User does not have permissions to query students");
            }

            var planningStudentsEntity = await _planningStudentRepository.GetAsync(studentIds);
            var adapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Student.PlanningStudent>();
            foreach (var planningStudentEntity in planningStudentsEntity)
            {
                try
                {
                    // Before doing anything, check the current advisor's privacy code settings (on their staff record)
                    // against any privacy code on the student's record
                    var adviseeHasPrivacyRestriction = string.IsNullOrEmpty(planningStudentEntity.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(planningStudentEntity.PrivacyStatusCode);
                    Dtos.Student.PlanningStudent planningStudentDto;
                    // If a privacy restriction exists (staff record doesn't contain student's privacy code)
                    // then blank out the record, except for name, id, and privacy code
                    if (adviseeHasPrivacyRestriction)
                    {
                        hasPrivacyRestriction = true;
                        planningStudentDto = new Dtos.Student.PlanningStudent()
                        {
                            LastName = planningStudentEntity.LastName,
                            FirstName = planningStudentEntity.FirstName,
                            MiddleName = planningStudentEntity.MiddleName,
                            Id = planningStudentEntity.Id,
                            PrivacyStatusCode = planningStudentEntity.PrivacyStatusCode
                        };
                    }
                    else
                    {
                        planningStudentDto = adapter.MapToType(planningStudentEntity);

                        //Add all the email addresses
                        if (planningStudentEntity.EmailAddresses != null)
                        {
                            if (planningStudentEntity.EmailAddresses.Any())
                            {
                                planningStudentDto.EmailAddresses = new List<EmailAddress>();
                                foreach (var email in planningStudentEntity.EmailAddresses)
                                {
                                    EmailAddress emailAddress = new EmailAddress()
                                    {
                                        Value = email.Value,
                                        TypeCode = email.TypeCode,
                                        IsPreferred = email.IsPreferred
                                    };
                                    planningStudentDto.EmailAddresses.Add(emailAddress);
                                }
                            }
                        }

                        //Get phone types heirarchy order
                        if (planningStudentEntity.PhoneTypesHierarchy != null)
                        {
                            if (planningStudentEntity.PhoneTypesHierarchy.Any())
                            {
                                planningStudentDto.PhoneTypesHierarchy = planningStudentEntity.PhoneTypesHierarchy;
                            }
                        }
                    }
                    planningStudents.Add(planningStudentDto);
                }
                catch (Ellucian.Data.Colleague.Exceptions.ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, string.Format("Failed to build PlanningStudent DTO for student {0}", planningStudentEntity.Id));
                    logger.Error(ex, ex.Message);
                }
            }
            return new PrivacyWrapper<IEnumerable<Dtos.Student.PlanningStudent>>(planningStudents, hasPrivacyRestriction);
        }
    }
}
