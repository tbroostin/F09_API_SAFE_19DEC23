// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Term data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class TermsController : BaseCompressedApiController
    {
        private readonly ITermRepository _termRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TermsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="termRepository">Repository of type <see cref="ITermRepository">ITermRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        public TermsController(IAdapterRegistry adapterRegistry, ITermRepository termRepository, ILogger logger)
        {
            _termRepository = termRepository;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Terms that are available to add to a Degree Plan for planning purposes.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.Student.Term">Terms</see> that are available to add to a Degree Plan for planning purposes.</returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Term>> GetPlanningTermsAsync()
        {
            var termDtoList = new List<Ellucian.Colleague.Dtos.Student.Term>();

            var termCollection =await _termRepository.GetAsync();
            termCollection = termCollection.Where(t => t.ForPlanning == true);

            // Get the entity to Dto adapter
            var termDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Ellucian.Colleague.Dtos.Student.Term>();
            foreach (var term in termCollection)
            {
                termDtoList.Add(termDtoAdapter.MapToType(term));
            }
            
            return termDtoList;
        }

        /// <summary>
        /// Retrieves all Terms that are open for pre-registration or registration.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.Student.Term">Terms</see> that are open for pre-registration or registration.</returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Term>> GetRegistrationTermsAsync()
        {
            List<Ellucian.Colleague.Dtos.Student.Term> termDtoList = new List<Ellucian.Colleague.Dtos.Student.Term>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> termCollection = await _termRepository.GetRegistrationTermsAsync();
            // Get the entity to Dto adapter
            var termDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Ellucian.Colleague.Dtos.Student.Term>();
            foreach (var term in termCollection)
            {
                termDtoList.Add(termDtoAdapter.MapToType(term));
            }
            return termDtoList;
        }

        /// <summary>
        /// Retrieves all terms with the option to limit by start date.
        /// </summary>
        /// <param name="startsOnOrAfter">The earliest start date of terms to retrieve. May be null to get all terms.</param>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.Student.Term">Terms</see>. If a start date was specified, then all Terms starting on or after the start date.</returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Term>> GetAllTermsAsync(DateTime? startsOnOrAfter = null)
        {
            List<Ellucian.Colleague.Dtos.Student.Term> termDtoList = new List<Ellucian.Colleague.Dtos.Student.Term>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> termCollection =await _termRepository.GetAsync();

            // Get the entity to Dto adapter
            var termDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Ellucian.Colleague.Dtos.Student.Term>();
            foreach (var term in termCollection)
            {
                termDtoList.Add(termDtoAdapter.MapToType(term));
            }

            if (startsOnOrAfter.HasValue)
            {
                return termDtoList.Where(t => t.StartDate >= startsOnOrAfter);
            }
            
            return termDtoList;
        }

        /// <summary>
        /// Retrieves a specific Term.
        /// </summary>
        /// <param name="id">Id of the term desired</param>
        /// <returns>The requested <see cref="Ellucian.Colleague.Dtos.Student.Term">Term</see></returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        public async Task<Ellucian.Colleague.Dtos.Student.Term> GetAsync(string id)
        {
            Ellucian.Colleague.Dtos.Student.Term termDto = null;

            var termEntity =await _termRepository.GetAsync(id);
            if (termEntity == null)
            {
                throw CreateHttpResponseException();
            }
            var termDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Ellucian.Colleague.Dtos.Student.Term>();
            termDto = termDtoAdapter.MapToType(termEntity);
            return termDto;
        }
    }
}