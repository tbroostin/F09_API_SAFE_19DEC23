// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;


namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Applicant Service
    /// </summary>
    [RegisterType]
    public class ApplicantService : StudentCoordinationService, IApplicantService
    {

        private readonly IApplicantRepository applicantRepository;
        private readonly IConfigurationRepository configurationRepository;
        private readonly IPersonBaseRepository personBaseRepository;


        public ApplicantService(IAdapterRegistry adapterRegistry,
            IApplicantRepository applicantRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IStudentRepository studentRepository,
            IPersonBaseRepository personBaseRepository,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            this.configurationRepository = configurationRepository;
            this.applicantRepository = applicantRepository;
            this.personBaseRepository = personBaseRepository;
        }

        /// <summary>
        /// Get an Applicant by id
        /// </summary>
        /// <param name="applicantId">Applicant's Colleague PERSON id</param>
        /// <returns>An Applicant DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown if the applicantId argument is null or empty</exception>
        /// <exception cref="PermissionsException">Thrown if the current user is attempting to access data for an applicant other than self,
        /// does not have specific permissions to view their data and not acting as proxy</exception>
        public async Task<Dtos.Student.Applicant> GetApplicantAsync(string applicantId)
        {
            if (string.IsNullOrEmpty(applicantId))
            {
                throw new ArgumentNullException("applicantId");
            }

            //Or CheckUserAccess(applicantId);
            if (!UserIsSelf(applicantId) && !HasPermission(StudentPermissionCodes.ViewFinancialAidInformation) && !HasProxyAccessForPerson(applicantId))
            {
                var message = string.Format("User {0} does not have permission to access data for applicant {1}", CurrentUser.PersonId, applicantId);
                logger.Error(message);
                throw new PermissionsException(message);
            }


            var applicantEntity =await applicantRepository.GetApplicantAsync(applicantId);

            Domain.Base.Entities.NameAddressHierarchy nameHierarchy = null;
            var hierarchyNameAsString = await applicantRepository.GetStwebDefaultsHierarchyAsync();
            if (!string.IsNullOrEmpty(hierarchyNameAsString))
            {
                try
                {
                    nameHierarchy = await personBaseRepository.GetCachedNameAddressHierarchyAsync(hierarchyNameAsString);
                }

                catch (Exception ex)
                {
                    logger.Error(ex, string.Format("Unable to find name address hierarchy with ID {0}. Not calculating hierarchy name.", hierarchyNameAsString));

                }
            }

            //If we have a valid name hierarchy from SPWP (verified existence on NAHM) get the student/applicant's display name
            if (nameHierarchy != null)
            {
                applicantEntity.PersonDisplayName = Domain.Base.Services.PersonNameService.GetHierarchyName(applicantEntity, nameHierarchy);
            }

            var applicantDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Applicant, Dtos.Student.Applicant>();

            return applicantDtoAdapter.MapToType(applicantEntity);

        }
    }
}
