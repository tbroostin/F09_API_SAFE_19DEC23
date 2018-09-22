// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.License;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to career goal data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CareerGoalsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the CareerGoalsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        public CareerGoalsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Career Goals.
        /// </summary>
        /// <returns>All <see cref="CareerGoal">Career Goal</see> codes and descriptions.</returns>
        public async Task<IEnumerable<CareerGoal>> GetAsync()
        {
            var careerGoalCollection = await _referenceDataRepository.GetCareerGoalsAsync();

            // Get the right adapter for the type mapping
            var careerGoalDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CareerGoal, CareerGoal>();

            // Map the CareerGoal entity to the program DTO
            var careerGoalDtoCollection = new List<CareerGoal>();
            foreach (var careerGoal in careerGoalCollection)
            {
                careerGoalDtoCollection.Add(careerGoalDtoAdapter.MapToType(careerGoal));
            }

            return careerGoalDtoCollection;
        }
    }
}