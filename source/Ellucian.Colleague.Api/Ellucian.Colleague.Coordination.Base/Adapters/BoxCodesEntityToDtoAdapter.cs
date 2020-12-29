// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping from the BoxCodes entity to the TaxFormBoxCodes DTO.
    /// </summary>
    public class BoxCodesEntityToDtoAdapter: AutoMapperAdapter<Domain.Base.Entities.BoxCodes, Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes>
    {
        public BoxCodesEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a BoxCodes domain entity and all of its descendent objects into DTOs.
        /// </summary>
        /// <param name="Source">BoxCodes domain entity to be converted.</param>
        /// <returns>TaxFormBoxCodes DTO.</returns>
        public TaxFormBoxCodes MapToType(Domain.Base.Entities.BoxCodes source)
        {
            var boxCodeDto = new Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes();
            if (source != null)
            {
                boxCodeDto.Code = source.Code;
                boxCodeDto.Description = source.Description;
                boxCodeDto.TaxForm = source.TaxCode;
            }
           
            return boxCodeDto;
        }
    }
}
