//Copyright 2018 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentUnverifiedGradesRepository
    {
        public Task<StudentUnverifiedGrades> GetStudentUnverifiedGradeByGuidAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentUnverifiedGrades>> GetStudentUnverifiedGradesAsync()
        {
            return Task.FromResult<IEnumerable<StudentUnverifiedGrades>>(new List<Student.Entities.StudentUnverifiedGrades>()
                {
                    new Student.Entities.StudentUnverifiedGrades("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1")
                    {
                        StudentId = "stud1",
                        StudentAcadaCredId = "studentAcadCredId1",
                        GradeScheme = "gradeScheme1",
                        GradeType = "gradeType1",
                        GradeId = "gradeId1"
                    },
                    new Student.Entities.StudentUnverifiedGrades("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2")
                    {
                        StudentId = "stud2",
                        StudentAcadaCredId = "studentAcadCredId2",
                        GradeScheme = "gradeScheme2",
                        GradeType = "gradeType2",
                        GradeId = "gradeId2"
                    },
                    new Student.Entities.StudentUnverifiedGrades("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3")
                    {
                        StudentId = "stud3",
                        StudentAcadaCredId = "studentAcadCredId3",
                        GradeScheme = "gradeScheme3",
                        GradeType = "gradeType3",
                        GradeId = "gradeId3"
                    },
                });
        }
        
    }
}