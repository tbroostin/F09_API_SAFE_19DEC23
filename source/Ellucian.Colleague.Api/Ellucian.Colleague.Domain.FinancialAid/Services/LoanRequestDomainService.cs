/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Services
{
    /// <summary>
    /// Provides Financial Aid business logic for servicing LoanRequests
    /// </summary>
    public class LoanRequestDomainService
    {

        /// <summary>
        /// Get the Colleague PERSON id of the loan counselor that should be assigned to a new LoanRequest. This method
        /// also validates that the student identified by the personBasedObject argument is able to submit a new LoanRequest
        /// for the given award year.
        /// </summary>
        /// <param name="personBasedObject">A Student or an Applicant object, both of which inherit Person. Method will throw exception if object is neither Student nor Applicant.</param>
        /// <param name="awardYear">The awardYear the student is trying to submit a loan request for</param>
        /// <param name="studentAwardYears">A list of the StudentAwardYear objects for the given Student or Applicant</param>
        /// <returns>The Colleague PERSON id of the loan counselor that should be assigned to a new LoanRequest for the student</returns>
        //public static string GetLoanRequestAssignment(Person personBasedObject, string awardYear, IEnumerable<StudentAwardYear> studentAwardYears)
        public static string GetLoanRequestAssignment(Person personBasedObject, StudentAwardYear studentAwardYear)
        {
            if (personBasedObject == null)
            {
                throw new ArgumentNullException("personBasedObject", "Cannot create loan request for non-student/non-applicant person.");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear", "studentAwardYear is required to Get a LoanRequest assignment id");
            }
            if (studentAwardYear.StudentId != personBasedObject.Id)
            {
                throw new ArgumentException("studentAwardYear", "studentAwardYear is not assigned to the personBasedObject id " + personBasedObject.Id);
            }

            //Validate that the student can submit a new loan request
            if (!studentAwardYear.CanRequestLoan)
            {
                var message = string.Format("Loan requests are not valid for year {0}", studentAwardYear.Code);
                throw new InvalidOperationException(message);
            }

            //check if student already has pending loan request for the year
            if (!string.IsNullOrEmpty(studentAwardYear.PendingLoanRequestId))
            {
                var message = string.Format("Student {0} already has a pending loan request with id {1} for award year {2}",
                   personBasedObject.Id, studentAwardYear.PendingLoanRequestId, studentAwardYear.Code);

                throw new ExistingResourceException(message, studentAwardYear.PendingLoanRequestId);
            }

            var assignRequestToId = string.Empty;

            //At this point the student can submit the request. Set the AssignRequestToId attribute
            if (personBasedObject.GetType() == typeof(Domain.Student.Entities.Student))
            {
                var student = personBasedObject as Domain.Student.Entities.Student;
                assignRequestToId = student.FinancialAidCounselorId;
            }
            else if (personBasedObject.GetType() == typeof(Domain.Student.Entities.Applicant))
            {
                var applicant = personBasedObject as Domain.Student.Entities.Applicant;
                assignRequestToId = applicant.FinancialAidCounselorId;
            }
            else
            {
                var message = string.Format("Cannot create loan request for non-student and non-applicant person id {0}", personBasedObject.Id);
                throw new InvalidOperationException(message);
            }

            return assignRequestToId;

        }
    }
}
