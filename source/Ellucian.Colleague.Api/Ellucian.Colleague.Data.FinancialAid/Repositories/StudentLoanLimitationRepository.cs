//Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Repository class gets and updates StudentLoanLimitations to the colleague database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentLoanLimitationRepository : BaseColleagueRepository, IStudentLoanLimitationRepository
    {
        
        public StudentLoanLimitationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// This gets all of the student's loan limitation objects from Colleague for all
        /// StudentAwardYears passed in
        /// </summary>
        /// <param name="studentId">The id of the student for whom to retrieve the loan limitations</param>
        /// <returns>A list of StudentLoanLimitation objects</returns>
        /// <exception cref="ArgumentNullException">Thrown when studentId argument is null or empty</exception>
        public async Task<IEnumerable<StudentLoanLimitation>> GetStudentLoanLimitationsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (studentAwardYears == null || !studentAwardYears.Any())
            {
                return new List<StudentLoanLimitation>();
            }

            //Init the return arg
            var studentLoanLimitations = new List<StudentLoanLimitation>();

            //for each year, create a StudentLoanLimitation object, and populate it with data from colleague, and add the object to the return list.
            Stopwatch sw1 = new Stopwatch();
            //sw1.Start();
            foreach (var year in studentAwardYears)
            {                
                sw1.Restart();
                var studentLoanLimitation = await BuildLoanLimitationAsync(studentId, year);
                sw1.Stop();
                logger.Info(string.Format("Time elapsed to get one student loan limitation for {0} year (repo): {1}", year.Code, sw1.ElapsedMilliseconds));

                studentLoanLimitations.Add(studentLoanLimitation);
            }
            sw.Stop();
            logger.Info(string.Format("Time elapsed to get all student loan limitations (repo): {0}", sw.ElapsedMilliseconds));
            return studentLoanLimitations;
        }

        /// <summary>
        /// Gets student loan limitation information
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve the information</param>
        /// <param name="studentAwardYear">student award year fr which to retrieve the information</param>
        /// <returns></returns>
        private async Task<StudentLoanLimitation> BuildLoanLimitationAsync(string studentId, StudentAwardYear studentAwardYear)
        {
            //read SA.ACYR to get max loan limit supression flag
            string saAcyrFile = "SA." + studentAwardYear.Code;
            var studentAwardData = await DataReader.ReadRecordAsync<SaAcyr>(saAcyrFile, studentId); 
            bool suppressStudentMaximumAmounts = studentAwardData != null && !string.IsNullOrEmpty(studentAwardData.SaOverLoanMax) && studentAwardData.SaOverLoanMax.ToUpper() == "Y";

            Transactions.GetLoanLimitationsRequest request = new Transactions.GetLoanLimitationsRequest();
            request.Year = studentAwardYear.Code;
            request.StudentId = studentId;
            request.AllowUnmetNeedBorrowing = (studentAwardYear.CurrentConfiguration != null) ? studentAwardYear.CurrentConfiguration.AllowNegativeUnmetNeedBorrowing : false;

            Transactions.GetLoanLimitationsResponse response = await transactionInvoker.ExecuteAsync<Transactions.GetLoanLimitationsRequest, Transactions.GetLoanLimitationsResponse>(request);

            var limitation = new StudentLoanLimitation(studentAwardYear.Code, studentId, suppressStudentMaximumAmounts);
            limitation.SubsidizedMaximumAmount = (response.SubEligAmount.HasValue) ? response.SubEligAmount.Value : 0;
            limitation.UnsubsidizedMaximumAmount = (response.UnsubEligAmount.HasValue) ? response.UnsubEligAmount.Value : 0;
            limitation.GradPlusMaximumAmount = (response.GplusEligAmount.HasValue) ? response.GplusEligAmount.Value : 0;
            return limitation;
        }
    }
}
