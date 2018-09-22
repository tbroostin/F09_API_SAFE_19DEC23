// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Colleague ImportantNumbers
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ImportantNumbersController : BaseCompressedApiController 
    {
        private readonly IImportantNumberRepository _impRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the ImportantNumbersController class
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="impRepository">Repository of type <see cref="IImportantNumberRepository">IImportantNumberRepository</see></param>
        public ImportantNumbersController(IAdapterRegistry adapterRegistry, IImportantNumberRepository impRepository) {
            _adapterRegistry = adapterRegistry;
            _impRepository = impRepository;
        }

        /// <summary>
        /// Retrieves all Important Numbers.
        /// </summary>
        /// <returns>All <see cref="ImportantNumber">Important Numbers.</see></returns>
        public IEnumerable<ImportantNumber> Get() {
            try
            {
                List<ImportantNumber> impDtoList = new List<ImportantNumber>();
                IEnumerable<Ellucian.Colleague.Domain.Base.Entities.ImportantNumber> impCollection = _impRepository.Get();
                var impDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.ImportantNumber, ImportantNumber>();
                foreach (var imp in impCollection)
                {
                    impDtoList.Add(impDtoAdapter.MapToType(imp));
                }
                return impDtoList;
            }
            catch (System.Exception ex)
            {
                throw CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.Forbidden);
            }
        }

        //[CacheControlfilter(MaxAgeHours = 1, Public = true, Revalidate = true)]
        /// <summary>
        /// Retrieves all Important Number Categories.
        /// </summary>
        /// <returns>All Important Number Category codes and descriptions.</returns>
        public IEnumerable<ImportantNumberCategory> GetImportantNumberCategories()
        {
            var numberCategories = _impRepository.ImportantNumberCategories;
            var numberCategoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.ImportantNumberCategory, ImportantNumberCategory>();
            var numberCategoryDtoCollection = new List<ImportantNumberCategory>();
            foreach (var numCat in numberCategories)
            {
                numberCategoryDtoCollection.Add(numberCategoryDtoAdapter.MapToType(numCat));
            }
            return numberCategoryDtoCollection.OrderBy(s => s.Code);
        }
    }
}
