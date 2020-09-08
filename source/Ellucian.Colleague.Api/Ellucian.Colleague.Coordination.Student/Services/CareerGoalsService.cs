//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CareerGoalsService : BaseCoordinationService, ICareerGoalsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public CareerGoalsService(

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
        /// Gets all career-goals
        /// </summary>
        /// <returns>Collection of CareerGoals DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CareerGoals>> GetCareerGoalsAsync(bool bypassCache = false)
        {
            var careerGoalsCollection = new List<Ellucian.Colleague.Dtos.CareerGoals>();

            var careerGoalsEntities = await _referenceDataRepository.GetCareerGoalsAsync(bypassCache);
            if (careerGoalsEntities != null && careerGoalsEntities.Any())
            {
                foreach (var careerGoals in careerGoalsEntities)
                {
                    careerGoalsCollection.Add(ConvertCareerGoalsEntityToDto(careerGoals));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return careerGoalsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CareerGoals from its GUID
        /// </summary>
        /// <returns>CareerGoals DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CareerGoals> GetCareerGoalsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertCareerGoalsEntityToDto((await _referenceDataRepository.GetCareerGoalsAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No career-goals was found for GUID '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No career-goals was found for GUID '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CareerGoals domain entity to its corresponding CareerGoals DTO
        /// </summary>
        /// <param name="source">CareerGoals domain entity</param>
        /// <returns>CareerGoals DTO</returns>
        private Ellucian.Colleague.Dtos.CareerGoals ConvertCareerGoalsEntityToDto(CareerGoal source)
        {
            var careerGoals = new Ellucian.Colleague.Dtos.CareerGoals();

            careerGoals.Id = source.Guid;
            careerGoals.Code = source.Code;
            careerGoals.Title = source.Description;
            careerGoals.Description = null;

            return careerGoals;
        }
    }
}