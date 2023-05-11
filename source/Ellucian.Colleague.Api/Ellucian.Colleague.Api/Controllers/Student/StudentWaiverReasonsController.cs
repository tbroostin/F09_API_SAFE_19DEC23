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
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Waiver Reasons
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentWaiverReasonsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// WaiverReasonsController constructor
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentReferenceDataRepository">studentReferenceDataRepository</param>
        /// <param name="logger">logger</param>
        public StudentWaiverReasonsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of all Waiver Reasons
        /// </summary>
        /// <returns>A list of <see cref="StudentWaiverReason">WaiverReason</see> codes and descriptions</returns>
        public async Task<IEnumerable<StudentWaiverReason>> GetAsync()
        {
            try
            {
                var waiverDtos = new List<StudentWaiverReason>();

                var waiverReasons = await _studentReferenceDataRepository.GetStudentWaiverReasonsAsync();

                //Get the adapter and convert to dto
                var waiverReasonDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentWaiverReason, StudentWaiverReason>();

                if (waiverReasons != null && waiverReasons.Count() > 0)
                {
                    foreach (var waiverReason in waiverReasons)
                    {
                        waiverDtos.Add(waiverReasonDtoAdapter.MapToType(waiverReason));
                    }
                }

                return waiverDtos;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving student section waiver reasons.";
                _logger.Error(csse, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                var message = "Unable to retrieve StudentWaiverReasons.";
                _logger.Error(e, message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}

