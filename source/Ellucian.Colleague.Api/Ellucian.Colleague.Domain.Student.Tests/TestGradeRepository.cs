// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestGradeRepository :IGradeRepository
    {
            // first column is both ID, second is grade string, third is "description" fourth is value.  This could be fleshed out
            // more properly

            private String[,] gradeArray = {
                                    //Id, Grade, Description(coll:legend), Value, GradeScheme, Withdraw Grade, Comparison Grade, Guid
                                    {"A","A","A","4","UG","W","1", "d874e05d-9d97-4fa3-8862-5044ef2384d0"},
                                    {"B","B","B","3","UG","W","99", "62b7fa62-5950-46eb-9145-a67e0733af12"}, // used for non-existant comparison grade test validation
                                    {"C","C","C","2","UG","W","D", "aa4c6931-c32b-4664-9903-46ac3432db95"},
                                    {"D","D","D","1","UG","W","", "c3adaf63-7e0f-4480-a0f6-795c368e7a1d"},
                                    {"F","F","F","0","UG","WF","", "d0b21c96-d934-43a7-82d1-9ceda41658a7"},
                                    {"P","P","P","2","UG","W","2", "b9405227-5a2e-4071-bed0-052b0444c335"},
                                    {"S","S","S","2","UG","W","", "39c84b02-0efd-49b0-beb6-4c034942677e"},
                                    {"W","W","W","0","UG","","", "16adeab0-65de-4170-8b91-bbf077f31f87"},
                                    {"WF","WF","WF","0","UG","","", "60636a90-0f59-4e4e-8686-a65646c25354"},
                                    {"I","I","I","0","UG","","", "31161ff8-9c3f-4bf5-9667-304ec27d19c3"}, // used for incomplete grade (adding extra value below)
                                    {"X","Y","Z","6","GR","W","", "58f15d08-0cc4-4e97-b385-257ebda0fdfc"}, // used for test validation
                                    {"AU","AU","AU","0","UG","W","", "9ea07473-b24d-437c-94cf-d6efd2b77640"},
                                    {"WD","WD","WD","0","UG","","", "5a161939-deef-46b7-b74e-19389642be2f"},
                                    {"1","A","A","4","TR","W","A", "b9459074-5de3-460b-a7e7-fafbda932cef"}, // Transfer A
                                    {"2","TP","TP","1","TR","W","P", "dc333822-757c-43d2-8408-37e237ac57d8"} // Transfer pass
                                  };


        public async Task<ICollection<Ellucian.Colleague.Domain.Student.Entities.Grade>> GetAsync()
        {
            int items = gradeArray.Length / 8;
            ICollection<Ellucian.Colleague.Domain.Student.Entities.Grade> Grades = new List<Ellucian.Colleague.Domain.Student.Entities.Grade>();
            Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Grade> grades = new Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Grade>();
            var withdrawGrades = new List<string>();

            for (int x = 0; x < items; x++)
            {
                Ellucian.Colleague.Domain.Student.Entities.Grade grade = new Ellucian.Colleague.Domain.Student.Entities.Grade(gradeArray[x, 0], gradeArray[x, 1], gradeArray[x, 2], gradeArray[x, 4]);
                grade.GradeValue = decimal.Parse(gradeArray[x, 3]);
                withdrawGrades.Add(gradeArray[x, 5]);

                // position 6 is comparison grade
                if (!string.IsNullOrEmpty(gradeArray[x, 6]))
                {
                    // Lookup the row of the comparison grade to retrieve the value and grade scheme
                    for (int y = 0; y < items; y++)
                    {
                        if (gradeArray[x, 6] == gradeArray[y, 0])
                        {
                            // Position 3 is value, 4 is scheme
                            // The repository ignores a comparison grade in the same grade scheme as invalid data
                            if (gradeArray[x, 4] != gradeArray[y, 4])
                            {
                                grade.SetComparisonGrade(gradeArray[x, 6], decimal.Parse(gradeArray[y, 3]), gradeArray[y, 4]);
                            }
                            // Break whether we ignored or not.
                            break;
                        }
                    }
                }
                grade.ExcludeFromFacultyGrading = gradeArray[x, 0] == "AU" || gradeArray[x, 0] == "1" || gradeArray[x, 0] == "2";
                grade.IncompleteGrade = grade.Id == "I" ? "F" : null;
                Grades.Add(grade);
            }

            // Update the IsWithdraw boolean for each grade that is a withdraw grade
            foreach (var withdrawGrade in withdrawGrades.Distinct())
            {
                if (!string.IsNullOrEmpty(withdrawGrade))
                {
                    var grade = Grades.Where(g => g.Id == withdrawGrade).First(); // if this causes an error, there is an invalid withdraw grade in the table
                    if (grade != null) { grade.IsWithdraw = true; }
                }
            }

            // Mark WD as a withdraw grade. In the tests it is defined as a phone reg drop grade
            var phoneDropGrade = Grades.Where(g => g.Id == "WD").First(); // If this causes an error, WD has been removed from the table above.
            phoneDropGrade.IsWithdraw = true;

            var grade1 = Grades.Where(g => g.Id == "1").First();
            if (grade1.Id == "1")
            {
                // Add "priority"
                grade1.GradePriority = 3m;
            }

            return await Task.FromResult(Grades);
        }

        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Grade>> GetHedmAsync(bool bypassCache = false)
        {
            int items = gradeArray.Length / 8;
            ICollection<Ellucian.Colleague.Domain.Student.Entities.Grade> Grades = new List<Ellucian.Colleague.Domain.Student.Entities.Grade>();
            Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Grade> grades = new Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Grade>();
            var withdrawGrades = new List<string>();

            for (int x = 0; x < items; x++)
            {
                Ellucian.Colleague.Domain.Student.Entities.Grade grade =
                    new Ellucian.Colleague.Domain.Student.Entities.Grade(gradeArray[x, 7], gradeArray[x, 0], gradeArray[x, 1], "", gradeArray[x, 2], gradeArray[x, 4]);
                grade.GradeValue = decimal.Parse(gradeArray[x, 3]);
                withdrawGrades.Add(gradeArray[x, 5]);
                // position 6 is comparison grade
                if (!string.IsNullOrEmpty(gradeArray[x, 6]))
                {
                    // Lookup the row of the comparison grade to retrieve the value and grade scheme
                    for (int y = 0; y < items; y++)
                    {
                        if (gradeArray[x,6] == gradeArray[y,0])
                        {
                            // Position 3 is value, 4 is scheme

                            // The repository ignores a comparison grade in the same grade scheme as invalid data
                            if (gradeArray[x,4] != gradeArray[y,4])
                            {
                                grade.SetComparisonGrade(gradeArray[x, 6], decimal.Parse(gradeArray[y, 3]), gradeArray[y, 4]);
                            }
                            // Break whether we ignored or not.
                            break;
                        }
                    }
                }

                Grades.Add(grade);
            }

            // Update the IsWithdraw boolean for each grade that is a withdraw grade
            foreach (var withdrawGrade in withdrawGrades.Distinct())
            {
                if (!string.IsNullOrEmpty(withdrawGrade))
                {
                    var grade = Grades.Where(g => g.Id == withdrawGrade).First(); // if this causes an error, there is an invalid withdraw grade in the table
                    if (grade != null) { grade.IsWithdraw = true; }
                }
            }

            // Mark WD as a withdraw grade. In the tests it is defined as a phone reg drop grade
            var phoneDropGrade = Grades.Where(g => g.Id == "WD").First(); // If this causes an error, WD has been removed from the table above.
            phoneDropGrade.IsWithdraw = true;

            var grade1 = Grades.Where(g => g.Id == "1").First();
            if (grade1.Id == "1")
            {
                // Add "priority"
                grade1.GradePriority = 3m;
            }

            return await Task.FromResult(Grades);
            /*
            int items = gradeArray.Length / 8;
            ICollection<Ellucian.Colleague.Domain.Student.Entities.Grade> Grades = new List<Ellucian.Colleague.Domain.Student.Entities.Grade>();
            Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Grade> grades = new Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Grade>();
            var withdrawGrades = new List<string>();

            for(int x = 0; x < items; x++)
            {
                Ellucian.Colleague.Domain.Student.Entities.Grade grade =
                    new Ellucian.Colleague.Domain.Student.Entities.Grade(gradeArray[x, 7], gradeArray[x, 0], gradeArray[x, 1], "", gradeArray[x, 2], gradeArray[x, 4]);
                grade.ComparisonGrade = gradeArray[x, 6];
                grade.GradePriority = decimal.Parse(gradeArray[x, 3]);
                grade.GradeValue = decimal.Parse(gradeArray[x, 3]);
                grade.IsWithdraw = false;
                withdrawGrades.Add(gradeArray[x, 5]);

                Grades.Add(grade);
            }
            return await Task.FromResult(Grades);*/
        }

        #region IGradeRepository Members


        Task<ICollection<Student.Entities.Grade>> IGradeRepository.GetHedmAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        Task<Student.Entities.Grade> IGradeRepository.GetHedmGradeByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
