// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping the Tax Form Configuration entity to the DTO
    /// </summary>
    public class TaxFormConfigurationEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.TaxFormConfiguration, Ellucian.Colleague.Dtos.Base.TaxFormConfiguration>
    {
        public TaxFormConfigurationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a tax form configuration domain entity into a DTO
        /// </summary>
        /// <param name="Source">Tax Form Configuration entity to be converted</param>
        /// <returns>TaxFormConfiguration DTO</returns>
        public TaxFormConfiguration MapToType(Domain.Base.Entities.TaxFormConfiguration Source)
        {
            var taxFormConfigurationDto = new TaxFormConfiguration();
            switch (Source.TaxFormId)
            {
                case Domain.Base.Entities.TaxForms.Form1095C:
                    taxFormConfigurationDto.TaxFormId = TaxForms.Form1095C;
                    break;
                case Domain.Base.Entities.TaxForms.FormW2:
                    taxFormConfigurationDto.TaxFormId = TaxForms.FormW2;
                    break;
                case Domain.Base.Entities.TaxForms.Form1098:
                    taxFormConfigurationDto.TaxFormId = TaxForms.Form1098;
                    break;
                case Domain.Base.Entities.TaxForms.Form1098T:
                    taxFormConfigurationDto.TaxFormId = TaxForms.Form1098T;
                    break;
                case Domain.Base.Entities.TaxForms.Form1098E:
                    taxFormConfigurationDto.TaxFormId = TaxForms.Form1098E;
                    break;
                case Domain.Base.Entities.TaxForms.FormT4:
                    taxFormConfigurationDto.TaxFormId = TaxForms.FormT4;
                    break;
                case Domain.Base.Entities.TaxForms.FormT4A:
                    taxFormConfigurationDto.TaxFormId = TaxForms.FormT4A;
                    break;
                case Domain.Base.Entities.TaxForms.FormT2202A:
                    taxFormConfigurationDto.TaxFormId = TaxForms.FormT2202A;
                    break;
                case Domain.Base.Entities.TaxForms.Form1099MI:
                    taxFormConfigurationDto.TaxFormId = TaxForms.Form1099MI;
                    break;
            }

            taxFormConfigurationDto.ConsentText = Source.ConsentParagraphs.ConsentText;
            taxFormConfigurationDto.ConsentWithheldText = Source.ConsentParagraphs.ConsentWithheldText;

            return taxFormConfigurationDto;
        }
    }
}
