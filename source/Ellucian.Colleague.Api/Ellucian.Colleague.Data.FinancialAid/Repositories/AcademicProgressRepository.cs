/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Provides methods to interact with Financial Aid Satisfactory Academic Progress
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AcademicProgressRepository : BaseColleagueRepository, IAcademicProgressRepository
    {
        private readonly string colleagueTimeZone;

        /// <summary>
        /// Constructor for the AcademicProgressRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public AcademicProgressRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            :base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get a student's AcademicProgressEvaluation entities
        /// </summary>
        /// <param name="studentId">Required: Colleague PERSON id of the student</param>
        /// <returns>A list of the student's AcademicProgressEvaluation objects</returns>
        public async Task<IEnumerable<AcademicProgressEvaluationResult>> GetStudentAcademicProgressEvaluationResultsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            var financialAidRecord = await DataReader.ReadRecordAsync<FinAid>(studentId);
            if (financialAidRecord == null)
            {
                logger.Info(string.Format("Student {0} has no financial aid data", studentId));
                return new List<AcademicProgressEvaluationResult>();
            }
            if (financialAidRecord.FaSapResultsId == null || financialAidRecord.FaSapResultsId.Count() == 0)
            {
                logger.Info(string.Format("Student {0} has no Academic Progress Evaluations", studentId));
                return new List<AcademicProgressEvaluationResult>();
            }
            
            var sapResultsRecords = await DataReader.BulkReadRecordAsync<SapResults>(financialAidRecord.FaSapResultsId.ToArray());
            if (sapResultsRecords == null || sapResultsRecords.Count() == 0)
            {
                LogDataError("FIN.AID", studentId, financialAidRecord, null, "FaSapResultsId list do not point to existing SapResults records");
                return new List<AcademicProgressEvaluationResult>();
            }

            var academicProgressEvaluations = new List<AcademicProgressEvaluationResult>();
            foreach (var sapResultRecord in sapResultsRecords)
            {
                var index = financialAidRecord.FaSapResultsId.IndexOf(sapResultRecord.Recordkey);

                DateTimeOffset? resultCreateDate = null;
                try
                {
                    int pickTimeStamp;
                    if (int.TryParse(sapResultRecord.SaprBatchId.Split('_')[2], out pickTimeStamp))
                    {
                        //since multiple SAP.RESULTS can be created with the same job id, subtract the index of 
                        //the record id in the Student's FA.SAP.RESULTS.ID list from the pickTimeStamp - the create time
                        //isn't all that important except for finding the most recent evaluation
                        //this will result in a slightly different DateTimeOffset value, which will allow API Clients to
                        //choose the most recently created AcademicProgressEvaluation, instead of finding multiple 
                        //objects with the same DateTimeOffset
                        pickTimeStamp -= index;

                        DateTime? time = new DateTime(DmiString.PickTimeToDateTime(pickTimeStamp).Ticks);
                        resultCreateDate = time.ToPointInTimeDateTimeOffset(sapResultRecord.SapResultsAdddate, colleagueTimeZone);
                    }
                }
                catch(Exception e)
                {
                    LogDataError("SapResults", sapResultRecord.Recordkey, sapResultRecord, e, string.Format("Could not convert PickTime in Job Id {0} to DateTimeOffset", sapResultRecord.SaprBatchId));
                }

                var statusCode = !string.IsNullOrEmpty(sapResultRecord.SaprOvrSapStatus) ? sapResultRecord.SaprOvrSapStatus : sapResultRecord.SaprCalcSapStatus;
                

                try
                {
                    //this is the fallback in case parsing out the time from the batch id
                    if (!resultCreateDate.HasValue)
                    {
                        resultCreateDate = sapResultRecord.SapResultsAdddate.ToPointInTimeDateTimeOffset(sapResultRecord.SapResultsAdddate, colleagueTimeZone);
                    }

                    var evaluation = new AcademicProgressEvaluationResult(sapResultRecord.Recordkey, studentId, statusCode, resultCreateDate.Value, sapResultRecord.SaprAcadProgram)
                    {                        
                        CumulativeAttemptedCredits = sapResultRecord.SaprCumAttCred ?? 0,
                        CumulativeCompletedCredits = sapResultRecord.SaprCumCmplCred ?? 0,
                        CumulativeGpaCredits = sapResultRecord.SaprCumGpaCred ?? 0,
                        CumulativeGpaGradePoints = sapResultRecord.SaprCumGpaGradePts ?? 0,
                        CumulativeCompletedGradePoints = sapResultRecord.SaprCumCmplGradePts ?? 0,
                        EvaluationPeriodAttemptedCredits = sapResultRecord.SaprTrmAttCred ?? 0,
                        EvaluationPeriodCompletedCredits = sapResultRecord.SaprTrmCmplCred ?? 0,
                        EvaluationPeriodGpaCredits = sapResultRecord.SaprTrmGpaCred ?? 0,
                        EvaluationPeriodGpaGradePoints = sapResultRecord.SaprTrmGpaGradePts ?? 0,
                        EvaluationPeriodCompletedGradePoints = sapResultRecord.SaprTrmCmplGradePts ?? 0,
                        CumulativeAttemptedCreditsExcludingRemedial = sapResultRecord.SaprCumAttCredRem ?? 0,
                        CumulativeCompletedCreditsExcludingRemedial = sapResultRecord.SaprCumCmplCredRem ?? 0,
                        EvaluationPeriodStartTerm = sapResultRecord.SaprEvalPdStartTerm,
                        EvaluationPeriodEndTerm = sapResultRecord.SaprEvalPdEndTerm,
                        EvaluationPeriodStartDate = sapResultRecord.SaprEvalPdStartDate,
                        EvaluationPeriodEndDate = sapResultRecord.SaprEvalPdEndDate, 
                        AppealIds = sapResultRecord.SaprAppealsId != null && sapResultRecord.SaprAppealsId.Any() ? sapResultRecord.SaprAppealsId : new List<string>()
                    };
                    evaluation.AcademicProgressTypeCode = sapResultRecord.SaprSapTypeId;
                    academicProgressEvaluations.Add(evaluation);
                }
                catch (Exception e)
                {
                    LogDataError("SapResults", sapResultRecord.Recordkey, sapResultRecord, e);
                }
            }

            return academicProgressEvaluations;
        }

        /// <summary>
        /// Get the student's program detail associated with the academic progress evaluation
        /// </summary>
        /// <param name="programCode">program code</param>
        /// <param name="catalog">program catalog</param>
        /// <returns>AcademicProgressProgramDetail entity</returns>
        public async Task<AcademicProgressProgramDetail> GetStudentAcademicProgressProgramDetailAsync(string programCode, string catalog)
        {
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode");
            }
            if (string.IsNullOrEmpty(catalog))
            {
                throw new ArgumentNullException("catalog");
            }
            string filekey = programCode + "*" + catalog;
            var programDetail = await GetOrAddToCacheAsync<AcademicProgressProgramDetail>("AcademicProgressProgramDetail*" + filekey,
            async () =>
            {
                var acadProgramReqmt = await DataReader.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", filekey);

                if (acadProgramReqmt == null)
                {
                    string message = "Program '" + programCode + "' for catalog '" + catalog + "'" + "is missing ACAD.PROGRAM.REQMTS record.";
                    logger.Info(message);
                    throw new KeyNotFoundException(message);
                }

                return new AcademicProgressProgramDetail(programCode, acadProgramReqmt.AcprMaxCred, acadProgramReqmt.AcprCred);
            }
            );
            return programDetail;
        }
    }
}
