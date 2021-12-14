// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for sstudent petition to retrieve petitions and consents of a student
    /// </summary>
    public interface IStudentPetitionService
    {
        /// <summary>
        /// retrieves student petitions & consent asynchronously
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns><IEnumerable<Dtos.Student.StudentPetition>></returns>
        Task<IEnumerable<Dtos.Student.StudentPetition>> GetAsync(string studentId);

        /// <summary>
        /// retrieves student overload petitions asynchronously
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>A collection of <see cref="Dtos.Student.StudentOverloadPetition"></see> object.</returns>
        Task<IEnumerable<Dtos.Student.StudentOverloadPetition>> GetStudentOverloadPetitionsAsync(string studentId);
    }
}
