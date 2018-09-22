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
    public class StudentPetitionReasonsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// StudentPetitionReasonsController constructor
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentReferenceDataRepository">studentReferenceDataRepository</param>
        /// <param name="logger">logger</param>
        public StudentPetitionReasonsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of all Student Petition Reasons
        /// </summary>
        /// <returns>A list of <see cref="StudentPetitionReason">StudentPetitionReason</see> codes and descriptions</returns>
        public async Task<IEnumerable<StudentPetitionReason>> GetAsync()
        {
            try
            {
                var studentPetitionReasonDtos = new List<StudentPetitionReason>();

                var studentPetitionReasons = await _studentReferenceDataRepository.GetStudentPetitionReasonsAsync();

                //Get the adapter and convert to dto
                var petitonReasonDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentPetitionReason, StudentPetitionReason>();

                if (studentPetitionReasons != null && studentPetitionReasons.Count() > 0)
                {
                    foreach (var petitionReason in studentPetitionReasons)
                    {
                        studentPetitionReasonDtos.Add(petitonReasonDtoAdapter.MapToType(petitionReason));
                    }
                }

                return studentPetitionReasonDtos;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve StudentPetitionReason.");
            }
        }
    }
}