﻿// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentAffiliationService
    {
        Task<IEnumerable<Dtos.Student.StudentAffiliation>> QueryStudentAffiliationsAsync(Dtos.Student.StudentAffiliationQueryCriteria criteria);
    }
}
