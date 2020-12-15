// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping the Tax Form Configuration version 2 entity to the DTO
    /// </summary>
    public class TaxFormConfiguration2EntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.TaxFormConfiguration2, Ellucian.Colleague.Dtos.Base.TaxFormConfiguration2>
    {
        public TaxFormConfiguration2EntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a tax form configuration domain entity into a DTO
        /// </summary>
        /// <param name="Source">Tax Form Configuration2 entity to be converted</param>
        /// <returns>TaxFormConfiguration2 DTO</returns>
        public TaxFormConfiguration2 MapToType(Domain.Base.Entities.TaxFormConfiguration2 Source)
        {
            var taxFormConfigurationDto = new TaxFormConfiguration2();
 
            taxFormConfigurationDto.TaxForm = Source.TaxForm;
            taxFormConfigurationDto.ConsentText = Source.ConsentParagraphs.ConsentText;
            taxFormConfigurationDto.ConsentWithheldText = Source.ConsentParagraphs.ConsentWithheldText;
            taxFormConfigurationDto.IsBypassingConsentPermitted = Source.IsBypassingConsentPermitted;

            return taxFormConfigurationDto;
        }
    }
}
