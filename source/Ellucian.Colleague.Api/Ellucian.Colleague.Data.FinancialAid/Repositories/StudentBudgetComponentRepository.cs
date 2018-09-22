/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
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
using System.Threading.Tasks;

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
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                logger.Info(string.Format("Cannot get budget components for student {0} with no studentAwardYears", studentId));
                return new List<StudentBudgetComponent>();
            }

            var studentBudgetComponents = new List<StudentBudgetComponent>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var csAcyrFile = "CS." + studentAwardYear.Code;
                var csRecord = await DataReader.ReadRecordAsync<CsAcyr>(csAcyrFile, studentId);
                if (csRecord != null && csRecord.CsCompEntityAssociation != null)
                {
                    foreach (var csCompEntity in csRecord.CsCompEntityAssociation)
                    {
                        try
                        {
                            studentBudgetComponents.Add(
                                new StudentBudgetComponent(
                                    studentAwardYear.Code,
                                    studentId,
                                    csCompEntity.CsCompIdAssocMember,
                                    csCompEntity.CsCompCbOrigAmtAssocMember.HasValue ? csCompEntity.CsCompCbOrigAmtAssocMember.Value : 0)
                                    {
                                        CampusBasedOverrideAmount = csCompEntity.CsCompCbOvrAmtAssocMember
                                    });
                        }
                        catch (Exception e)
                        {
                            var message =
                                string.Format("Unable to create budget component code {0} for student {1}, award year {2}", csCompEntity.CsCompIdAssocMember, studentId, studentAwardYear.Code);
                            logger.Error(e, message);
                        }
                    }
                }
            }

            return studentBudgetComponents;
        }
    }
}
