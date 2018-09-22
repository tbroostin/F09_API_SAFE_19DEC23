//Copyright 2015 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentDefaultAwardPeriodRepository : BaseColleagueRepository, IStudentDefaultAwardPeriodRepository
    {
        public StudentDefaultAwardPeriodRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// This method will get the student award periods for all active years.
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns>The Colleague student id</returns>
        public async Task<IEnumerable<StudentDefaultAwardPeriod>> GetStudentDefaultAwardPeriodsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null)
            {
                throw new ArgumentNullException("studentAwardYears");
            }

            //Create the return list
            var studentDefaultAwardPeriods = new List<StudentDefaultAwardPeriod>();

            //Loop thru active years
            foreach (var year in studentAwardYears)
            {
                try
                {
                    var studentAwardPeriod = await BuildAwardPeriod(studentId, year);
                    studentDefaultAwardPeriods.Add(studentAwardPeriod);
                }
                catch (Exception e)
                {
                    logger.Error(e, e.Message);
                }
            }
           
            return studentDefaultAwardPeriods;

        }

        private async Task<StudentDefaultAwardPeriod> BuildAwardPeriod(string studentId, StudentAwardYear studentAwardYear)
        {
            
            var studentAwardPeriod = new StudentDefaultAwardPeriod(studentId, studentAwardYear.Code);

            var request = new GetDefaultAwardPeriodsRequest();
            request.StudentId = studentId;
            request.Year = studentAwardYear.Code;

            //Execute the transaction to get the award periods for a given year
            var response = await transactionInvoker.ExecuteAsync<GetDefaultAwardPeriodsRequest, GetDefaultAwardPeriodsResponse>(request);

            if (response.AwardPeriodsList == null)
            {
                var message = string.Format("No default Award Periods defined");
                logger.Info(message);
            }

            var awardPeriods = new List<string>();
            foreach (var awardPeriod in response.AwardPeriodsList)
            {
                awardPeriods.Add(awardPeriod.ToString());
            }

            studentAwardPeriod.DefaultAwardPeriods = awardPeriods;

            return studentAwardPeriod;
        }

    }
}
