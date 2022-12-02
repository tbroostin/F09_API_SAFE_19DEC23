// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;
using System;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Requirements data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RequirementsController : BaseCompressedApiController
    {
        private readonly IRequirementRepository _RequirementRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RequirementsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="requirementRepository">Repository of type <see cref="IRequirementRepository">IRequirementRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public RequirementsController(IAdapterRegistry adapterRegistry, IRequirementRepository requirementRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _RequirementRepository = requirementRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the requirement details for a specific requirement code.
        /// </summary>
        /// <param name="id">Requirement Code</param>
        /// <returns>The requested <see cref="Requirement">Requirement</see></returns>
        [ParameterSubstitutionFilter]
        public async Task<Ellucian.Colleague.Dtos.Student.Requirements.Requirement> GetAsync(string id)
        {
            try
            {
                var requirementEntity = await _RequirementRepository.GetAsync(id);

                var requirementDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Requirement>();

                Requirement requirementDto = requirementDtoAdapter.MapToType(requirementEntity);
                return requirementDto;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving requirement details";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while retrieving requirement details";
                _logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the requirement details for the provided requirement codes.
        /// </summary>
        /// <param name="criteria">Criteria, including Requirement Ids, to use to request Requirements</param>
        /// <returns>List of the requested <see cref="Requirement">Requirement</see> objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Requirements.Requirement>> QueryRequirementsByPostAsync([FromBody] RequirementQueryCriteria criteria )
        {
            try
            {
                var requirementDtos = new List<Dtos.Student.Requirements.Requirement>();

                var requirementEntities = await _RequirementRepository.GetAsync(criteria.RequirementIds);

                var requirementDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Requirement>();

                foreach (var requirementEntity in requirementEntities)
                {
                    requirementDtos.Add(requirementDtoAdapter.MapToType(requirementEntity));
                }
                return requirementDtos;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while querying requirements";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while querying requirements";
                _logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

    }
}

