/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Provides methods for interacting with Academic Progress Appeals for a student
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AcademicProgressAppealRepository : BaseColleagueRepository, IAcademicProgressAppealRepository
    {
        private readonly string colleagueTimeZone;
        
        /// <summary>
        /// Constructor for the AcademicProgressAppealRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public AcademicProgressAppealRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            :base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        public async Task<IEnumerable<AcademicProgressAppeal>> GetStudentAcademicProgressAppealsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            // Read all records for a student from SAP.APPEALS
            string criteria = "WITH SAPA.STUDENT.ID EQ '" + studentId + "'";
            var studentAppealRecords = await DataReader.BulkReadRecordAsync<SapAppeals>("SAP.APPEALS",criteria);

            //Get the year's awardPeriod level data from Colleague
            //string acyrFile = "TA." + studentAwardYear.Code;
            //string criteria = "WITH TA.STUDENT.ID EQ '" + studentId + "'";
            //var studentAwardPeriodData = DataReader.BulkReadRecord<TaAcyr>(acyrFile, criteria);

            if (studentAppealRecords == null || studentAppealRecords.Count() == 0)
            {
                logger.Info(string.Format("Student {0} has no Academic Progress Appeals", studentId));
                return new List<AcademicProgressAppeal>();
            }

            var academicProgressAppeals = new List<AcademicProgressAppeal>();
            foreach (var appeal in studentAppealRecords)
            {
                var appealRecord = new AcademicProgressAppeal(appeal.SapaStudentId, appeal.Recordkey)
                {
                    AppealStatusCode = appeal.SapaAction,
                    AppealDate = appeal.SapaDate,
                    AppealCounselorId = appeal.SapaCounselor,
                    AcademicProgressEvaluationId = appeal.SapaSapResultsId,
                };
                academicProgressAppeals.Add(appealRecord);
            };

            return academicProgressAppeals;
        }

    }
}
