// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    public class PersonRestrictionEntityAdapter
    {
        private IAdapterRegistry AdapterRegistry;
        private ILogger Logger;

        public PersonRestrictionEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            Logger = logger;
        }

        public Dtos.Base.PersonRestriction MapToType(Ellucian.Colleague.Domain.Base.Entities.PersonRestriction source, Ellucian.Colleague.Domain.Base.Entities.Restriction restriction)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Must provide source to convert");
            }
            if (restriction == null)
            {
                throw new ArgumentNullException("restriction", "Must provide restriction to convert");
            }
            Dtos.Base.PersonRestriction srDto = new Dtos.Base.PersonRestriction();
            srDto.Id = source.Id;
            srDto.StudentId = source.StudentId;
            srDto.RestrictionId = source.RestrictionId;
            srDto.StartDate = source.StartDate;
            srDto.EndDate = source.EndDate;
            srDto.Severity = source.Severity;
            srDto.OfficeUseOnly = source.OfficeUseOnly;
            srDto.Title = restriction.Title;
            srDto.Details = restriction.Details;
            srDto.Hyperlink = restriction.Hyperlink;
            srDto.HyperlinkText = restriction.FollowUpLabel;
            return srDto;
        }
    }
}
