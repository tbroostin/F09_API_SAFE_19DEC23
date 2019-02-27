/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.FinancialAid.Transactions;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Creates StudentBudgetComponents from database records
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentBudgetComponentRepository : BaseColleagueRepository, IStudentBudgetComponentRepository
    {
        /// <summary>
        /// Constructor for the StudentBudgetComponent Repository
        /// </summary>
        /// <param name="cacheProvider">cacheProvider</param>
        /// <param name="transactionFactory">transactionFactory</param>
        /// <param name="logger">logger</param>
        public StudentBudgetComponentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get StudentBudgetComponents for the given award years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get budgets</param>
        /// <param name="studentAwardYears">The StudentAwardYears for which to get budgets</param>
        /// <returns>A list of StudentBudgetComponent objects for the given student id and award years</returns>
        public async Task<IEnumerable<StudentBudgetComponent>> GetStudentBudgetComponentsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null || !studentAwardYears.Any())
            {
                logger.Info(string.Format("Cannot get budget components for student {0} with no studentAwardYears", studentId));
                return new List<StudentBudgetComponent>();
            }

            var studentBudgetComponents = new List<StudentBudgetComponent>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var request = new GetStuBudgetComponentsRequest()
                {
                    StudentId = studentId,
                    Year = studentAwardYear.Code
                };
                try
                {
                    var budgetComponents = await transactionInvoker.ExecuteAsync<GetStuBudgetComponentsRequest, GetStuBudgetComponentsResponse>(request);
                    if(budgetComponents != null && budgetComponents.StudentBudgetComponents.Any())
                    {
                        for(int i = 0; i < budgetComponents.StudentBudgetComponents.Count; i++)
                        {
                            int origAmt = 0, overwriteAmt = 0;
                            bool origAmtSuccess = false, overwriteAmtSuccess = false;

                            //Sometimes there can be less original or ovewrite amount values in the associated lists than component codes, skip if so
                            try { origAmtSuccess = int.TryParse(budgetComponents.StuBgtComponentOrigAmts[i], out origAmt); } catch { /*just skip*/ }
                            try { overwriteAmtSuccess = int.TryParse(budgetComponents.StuBgtComponentOvrAmts[i], out overwriteAmt); } catch { /*just skip*/ }

                            try
                            {
                                studentBudgetComponents.Add(
                                    new StudentBudgetComponent(
                                        studentAwardYear.Code,
                                        studentId,
                                        budgetComponents.StudentBudgetComponents[i],
                                        origAmtSuccess ? origAmt : 0)
                                    {
                                        CampusBasedOverrideAmount = overwriteAmtSuccess ? overwriteAmt : (int?)null
                                    });
                            }
                            catch (Exception e)
                            {
                                var message =
                                    string.Format("Unable to create budget component code {0} for student {1}, award year {2}", budgetComponents.StudentBudgetComponents[i], studentId, studentAwardYear.Code);
                                logger.Error(e, message);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    var message = string.Format("Could not retrieve budget components for student {0}, award year {1}", studentId, studentAwardYear.Code);
                    logger.Error(ex, message);
                }                
            }

            return studentBudgetComponents;
        }
    }
}
