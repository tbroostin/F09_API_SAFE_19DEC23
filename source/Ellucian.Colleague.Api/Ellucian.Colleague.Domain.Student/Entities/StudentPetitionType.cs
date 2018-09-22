// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Describes the type of student petition, either faculty consent or regular student petition
    /// </summary>
    [Serializable]
    public enum StudentPetitionType
    {
        StudentPetition,
        FacultyConsent
    }
}
