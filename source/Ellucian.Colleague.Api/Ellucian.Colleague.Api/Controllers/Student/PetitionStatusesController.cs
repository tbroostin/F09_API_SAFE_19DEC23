// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Petition Statuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class PetitionStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// PetitionStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentReferenceDataRepository">studentReferenceDataRepository</param>
        /// <param name="logger">logger</param>
        public PetitionStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of all Petition Statuses
        /// </summary>
        /// <returns>A list of <see cref="PetitionStatus">PetitionStatus</see> codes and descriptions</returns>
        public async Task<IEnumerable<PetitionStatus>> GetAsync()
        {
            try
            {
                var petitionStatusDtos = new List<PetitionStatus>();

                var petitionStatuses =await _studentReferenceDataRepository.GetPetitionStatusesAsync();

                //Get the adapter and convert to dto
                var petitionStatusDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.PetitionStatus, PetitionStatus>();

                if (petitionStatuses != null && petitionStatuses.Count() > 0)
                {
                    foreach (var status in petitionStatuses)
                    {
                        petitionStatusDtos.Add(petitionStatusDtoAdapter.MapToType(status));
                    }
                }

                return petitionStatusDtos;
                ;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve PetitionStatuses.");
            }
        }
    }
}

