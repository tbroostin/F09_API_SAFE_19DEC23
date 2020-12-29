// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student.QuickRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentQuickRegistrationService
    {
        /// <summary>
        /// Retrieves a given student's quick registration information for the provided academic term codes
        /// </summary>
        /// <param name="studentId">ID of the student for whom Colleague Self-Service Quick Registration data will be retrieved</param>
        /// <returns>A <see cref="StudentQuickRegistration"/> object</returns>
        Task<StudentQuickRegistration> GetStudentQuickRegistrationAsync(string studentId);
    }
}
