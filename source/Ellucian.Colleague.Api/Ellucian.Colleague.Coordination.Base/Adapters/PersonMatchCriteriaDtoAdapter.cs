// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    public class PersonMatchCriteriaDtoAdapter : AutoMapperAdapter<Dtos.Base.PersonMatchCriteria, Domain.Base.Entities.PersonMatchCriteria>
    {
        public PersonMatchCriteriaDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Domain.Base.Entities.PersonMatchCriteria MapToType(Dtos.Base.PersonMatchCriteria source)
        {
            List<Domain.Base.Entities.PersonName> names = new List<Domain.Base.Entities.PersonName>();
            if (source.MatchNames != null)
            {
                var mapper = new AutoMapperAdapter<Dtos.Base.PersonName, Domain.Base.Entities.PersonName>(adapterRegistry, logger);
                source.MatchNames.ToList().ForEach(mn => names.Add(new PersonName(mn.GivenName, mn.MiddleName, mn.FamilyName)));
            }
            var result = new Domain.Base.Entities.PersonMatchCriteria(source.MatchCriteriaIdentifier, names);
            result.BirthDate = source.BirthDate;
            result.EmailAddress = source.EmailAddress;
            result.Gender = source.Gender;
            result.GovernmentId = source.GovernmentId;
            result.Phone = source.Phone;
            result.PhoneExtension = source.PhoneExtension;
            result.Prefix = source.Prefix;
            result.Suffix = source.Suffix;

            return result;
        }
    }
}
