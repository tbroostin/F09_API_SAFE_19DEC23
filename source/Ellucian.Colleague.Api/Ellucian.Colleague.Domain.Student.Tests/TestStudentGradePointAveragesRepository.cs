//Copyright 2018 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentGradePointAveragesRepository
    {

        public Task<IEnumerable<StudentAcademicCredit>> GetStudentGradePointAveragesAsync()
        {
            return Task.FromResult<IEnumerable<StudentAcademicCredit>>(new List<Student.Entities.StudentAcademicCredit>()
                {
                    new Student.Entities.StudentAcademicCredit("bb66b971-3ee0-4477-9bb7-539721f93434", "1"),
                    new Student.Entities.StudentAcademicCredit("5aeebc5c-c973-4f83-be4b-f64c95002124", "2"),
                    new Student.Entities.StudentAcademicCredit("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "3"),
                });
        }
        
    }
}