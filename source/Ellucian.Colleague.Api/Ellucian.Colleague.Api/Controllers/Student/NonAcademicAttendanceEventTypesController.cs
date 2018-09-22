// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to nonacademic attendance event type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class NonAcademicAttendanceEventTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the NonAcademicAttendanceEventTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public NonAcademicAttendanceEventTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository,
            ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all nonacademic attendanc event types.
        /// </summary>
        /// <returns>All <see cref="NonAcademicAttendanceEventType">nonacademic attendance event type</see> codes and descriptions.</returns>
        public async Task<IEnumerable<NonAcademicAttendanceEventType>> GetAsync()
        {
            try
            {
                var nonAcademicAttendanceEventTypeCollection = await _studentReferenceDataRepository.GetNonAcademicAttendanceEventTypesAsync();

                // Get the right adapter for the type mapping
                var nonAcademicAttendanceEventTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.NonAcademicAttendanceEventType, NonAcademicAttendanceEventType>();

                // Map the StudentLoad entity to the program DTO
                var nonAcademicAttendanceEventTypeDtoCollection = new List<NonAcademicAttendanceEventType>();
                foreach (var nonAcademicAttendanceEventType in nonAcademicAttendanceEventTypeCollection)
                {
                    nonAcademicAttendanceEventTypeDtoCollection.Add(nonAcademicAttendanceEventTypeDtoAdapter.MapToType(nonAcademicAttendanceEventType));
                }

                return nonAcademicAttendanceEventTypeDtoCollection;
            }
            catch (ColleagueDataReaderException cdre)
            {
                string message = "An error occurred while trying to read nonacademic attendance event type data from the database.";
                _logger.Error(message, cdre.ToString());
                throw CreateHttpResponseException(message);
            }
            catch (Exception ex)
            {
                string message = "An error occurred while trying to retrieve nonacademic attendance event type information.";
                _logger.Error(message, ex.ToString());
                throw CreateHttpResponseException(message);
            }
        }
    }
}