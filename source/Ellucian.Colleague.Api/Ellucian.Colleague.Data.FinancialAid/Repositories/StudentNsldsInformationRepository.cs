//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// StudentNsldsInformationRepository class
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentNsldsInformationRepository : BaseColleagueRepository, IStudentNsldsInformationRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public StudentNsldsInformationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Gets StudentNsldsInformation for the specified student
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>StudentNsldsInformation entity</returns>
        public async Task<StudentNsldsInformation> GetStudentNsldsInformationAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }


            //  03/30/17 Add in new transaction to get the pell leu instead of reading to get data.

            Transactions.GetPellLeuRequest request = new Transactions.GetPellLeuRequest();
            request.StudentId = studentId;

            Transactions.GetPellLeuResponse response = await transactionInvoker.ExecuteAsync<Transactions.GetPellLeuRequest, Transactions.GetPellLeuResponse>(request);

            StudentNsldsInformation nsldsInformation;
            nsldsInformation = new StudentNsldsInformation(studentId, response.PellLeu);
           
            return nsldsInformation;
        }
    }
}
