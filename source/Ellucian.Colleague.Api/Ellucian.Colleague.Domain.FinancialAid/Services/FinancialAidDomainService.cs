/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Services
{
    /// <summary>
    /// Provide domain utilities that don't fit in other classes
    /// </summary>
    public static class FinancialAidDomainService
    {
        /// <summary>
        /// Get the Id of the student/applicant's financial aid counselor.
        /// </summary>
        /// <param name="personBasedStudentOrApplicant"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="personBasedStudentOrApplicant"/> argument is null</exception>
        /// <exception cref="ApplicationException">Thrown if the <paramref name="personBasedStudentOrApplicant"/> is not a Student or Applicant type</exception>
        public static string GetFinancialAidCounselorId(Person personBasedStudentOrApplicant)
        {
            if (personBasedStudentOrApplicant == null)
            {
                throw new ArgumentNullException("personBasedStudentOrApplicant");
            }
            if (personBasedStudentOrApplicant.GetType() == typeof(Domain.Student.Entities.Student))
            {
                var student = personBasedStudentOrApplicant as Domain.Student.Entities.Student;
                return student.FinancialAidCounselorId;
            }
            else if (personBasedStudentOrApplicant.GetType() == typeof(Domain.Student.Entities.Applicant))
            {
                var applicant = personBasedStudentOrApplicant as Domain.Student.Entities.Applicant;
                return applicant.FinancialAidCounselorId;
            }
            else
            {
                var message = string.Format("Cannot get financial aid counselor id for non-student and non-applicant person id {0}", personBasedStudentOrApplicant.Id);
                throw new ApplicationException(message);
            }
        }
    }
}
