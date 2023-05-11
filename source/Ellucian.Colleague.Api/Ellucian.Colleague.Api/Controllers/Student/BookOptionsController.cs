﻿// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Data.Colleague.Exceptions;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to book option data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class BookOptionsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BookOptionsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>

        public BookOptionsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Book Options.
        /// </summary>
        /// <returns>All <see cref="BookOption">Book Option</see> codes and descriptions.</returns>
        [HttpGet]
        public async Task<IEnumerable<BookOption>> GetAsync()
        {
            try
            {
                var bookOptionCollection = await _referenceDataRepository.GetBookOptionsAsync();

                // Get the right adapter for the type mapping
                var bookOptionDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.BookOption, BookOption>();

                // Map the BookOption entity to the program DTO
                var bookOptionDtoCollection = new List<BookOption>();
                foreach (var bookOption in bookOptionCollection)
                {
                    bookOptionDtoCollection.Add(bookOptionDtoAdapter.MapToType(bookOption));
                }
                return bookOptionDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
                _logger.Error(csse, invalidSessionErrorMessage);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
           
            catch (Exception ex)
            {
                string errorMessage = "Exception occurred while retrieving books options";
                _logger.Error(ex, errorMessage);
                throw CreateHttpResponseException(errorMessage, HttpStatusCode.BadRequest);
            }


        }
    }
}