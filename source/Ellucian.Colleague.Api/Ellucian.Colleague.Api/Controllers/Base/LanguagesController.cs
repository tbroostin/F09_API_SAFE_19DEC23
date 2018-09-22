// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Language data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class LanguagesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the LanguagesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        public LanguagesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Languages.
        /// </summary>
        /// <returns>All <see cref="Language">Language codes and descriptions.</see></returns>
        public IEnumerable<Language> Get()
        {
            var languageCollection = _referenceDataRepository.Languages;

            // Get the right adapter for the type mapping
            var languageDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Language, Language>();

            // Map the language entity to the program DTO
            var languageDtoCollection = new List<Language>();
            foreach (var language in languageCollection)
            {
                languageDtoCollection.Add(languageDtoAdapter.MapToType(language));
            }

            return languageDtoCollection;
        }
    }
}
