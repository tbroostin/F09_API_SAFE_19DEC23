// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base
{
    [Serializable]
    public static class PersonHoldsPermissionCodes
    {
        // Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague. 
        public const string ViewPersonHold = "VIEW.PERSON.HOLD";

        // Provides an integration user permission to create/update holds (a.k.a. restrictions) from Colleague. 
        public const string CreateUpdatePersonHold = "UPDATE.PERSON.HOLD";

        // Provides an integration user permission to delete a hold (a.k.a. a record from STUDENT.RESTRICTIONS) in Colleague.
        public const string DeletePersonHold = "DELETE.PERSON.HOLD";
    }
}