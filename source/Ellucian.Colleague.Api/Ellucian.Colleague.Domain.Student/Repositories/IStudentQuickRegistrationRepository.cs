// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.QuickRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentQuickRegistrationRepository
    {
        Task<StudentQuickRegistration> GetStudentQuickRegistrationAsync(string studentId);
    }
}
