﻿// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to student load data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentLoadsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the StudentLoadsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        public StudentLoadsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Student Loads.
        /// </summary>
        /// <returns>All <see cref="StudentLoad">Student Load</see> codes and descriptions.</returns>
        public async Task<IEnumerable<StudentLoad>> GetAsync()
        {
            var studentLoadCollection = await _referenceDataRepository.GetStudentLoadsAsync();

            // Get the right adapter for the type mapping
            var studentLoadDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentLoad, StudentLoad>();

            // Map the StudentLoad entity to the program DTO
            var studentLoadDtoCollection = new List<StudentLoad>();
            foreach (var studentLoad in studentLoadCollection)
            {
                studentLoadDtoCollection.Add(studentLoadDtoAdapter.MapToType(studentLoad));
            }

            return studentLoadDtoCollection;
        }
    }
}