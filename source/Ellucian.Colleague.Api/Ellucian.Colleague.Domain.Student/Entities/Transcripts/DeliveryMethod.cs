// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public enum DeliveryMethod
    {
        Electronic,
        Express,
        ExpressCanadaMexico,
        ExpressInternational,
        ExpressUnitedStates,
        Fax,
        FaxExpress,
        FaxMail,
        FaxOvernight,
        HoldForPickup,
        Mail,
        Overnight
    }
}
