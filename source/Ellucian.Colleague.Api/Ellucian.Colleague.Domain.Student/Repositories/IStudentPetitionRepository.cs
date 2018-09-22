﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// interface for section permission
    /// </summary>
    public interface IStudentPetitionRepository
    {
        /// <summary>
        /// get petitions & faculty consents for a given student asynchronously
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns><IEnumerable<StudentPetition>></returns>
        Task<IEnumerable<StudentPetition>> GetStudentPetitionsAsync(string studentId);
    }
}
