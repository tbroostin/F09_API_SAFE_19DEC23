//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class EducationalGoalsService : BaseCoordinationService, IEducationalGoalsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public EducationalGoalsService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all educational-goals
        /// </summary>
        /// <returns>Collection of EducationalGoals DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalGoals>> GetEducationalGoalsAsync(bool bypassCache = false)
        {
            var educationalGoalsCollection = new List<Ellucian.Colleague.Dtos.EducationalGoals>();

            var educationalGoalsEntities = await _referenceDataRepository.GetEducationGoalsAsync(bypassCache);
            if (educationalGoalsEntities != null && educationalGoalsEntities.Any())
            {
                foreach (var educationalGoals in educationalGoalsEntities)
                {
                    educationalGoalsCollection.Add(ConvertEducationalGoalsEntityToDto(educationalGoals));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return educationalGoalsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EducationalGoals from its GUID
        /// </summary>
        /// <returns>EducationalGoals DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EducationalGoals> GetEducationalGoalsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertEducationalGoalsEntityToDto((await _referenceDataRepository.GetEducationGoalsAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No educational-goals was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No educational-goals was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EducationGoals domain entity to its corresponding EducationalGoals DTO
        /// </summary>
        /// <param name="source">EducationGoals domain entity</param>
        /// <returns>EducationalGoals DTO</returns>
        private Ellucian.Colleague.Dtos.EducationalGoals ConvertEducationalGoalsEntityToDto(EducationGoals source)
        {
            var educationalGoals = new Ellucian.Colleague.Dtos.EducationalGoals();

            educationalGoals.Id = source.Guid;
            educationalGoals.Code = source.Code;
            educationalGoals.Title = source.Description;
            educationalGoals.Description = null;

            return educationalGoals;
        }
    }
}