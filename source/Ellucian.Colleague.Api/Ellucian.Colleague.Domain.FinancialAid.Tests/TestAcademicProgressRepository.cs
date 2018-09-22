using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestAcademicProgressRepository : IAcademicProgressRepository
    {

        public class FinancialAidStudentRecord
        {
            public List<string> sapResultsIds;
        }

        public FinancialAidStudentRecord financialAidStudentData = new FinancialAidStudentRecord()
        {
            sapResultsIds = new List<string>() { "1234", "4321" }
        };

        public List<string> academicProgramCodes = new List<string>()
        {
            "MATH.BS",
            "ECON.BA",
            "MATH.FA"
        };

        public string catalogCode = "CATALOG";

        public class ProgramRequirementsRecord
        {
            public string programCode;
            public string catalogCode;
            public decimal? maxCredits;
            public decimal? minCredits;
        }

        public List<ProgramRequirementsRecord> programRequirementsData = new List<ProgramRequirementsRecord>()
        {
            new ProgramRequirementsRecord()
            {
                programCode = "MATH.BS",
                catalogCode = "CATALOG",
                maxCredits = 120m,
                minCredits = 100m,
            },
            new ProgramRequirementsRecord()
            {
                programCode = "ECON.BA",
                catalogCode = "CATALOG",
                maxCredits = null,
                minCredits = 100m,
            },
            new ProgramRequirementsRecord()
            {
                programCode = "MATH.FA",
                catalogCode = "CATALOG",
                maxCredits = 111m,
                minCredits = 99m,
            },
        };

        public class SapResultsRecord
        {
            public string id;
            public string academicProgram;
            public string originalStatusCode;
            public string overrideStatusCode;
            public DateTime? evaluationDate;
            public string batchId;
            public string evaluationPeriodStartTerm;
            public string evaluationPeriodEndTerm;
            public DateTime? evaluationPeriodStartDate;
            public DateTime? evaluationPeriodEndDate;
            public decimal? evaluationPeriodAttemptedCredits;
            public decimal? evaluationPeriodCompletedCredits;
            public decimal? evaluationPeriodCompletedGradePoints;
            public decimal? cumulativeAttemptedCredits;
            public decimal? cumulativeCompletedCredits;
            public decimal? cumulativeCompletedGradePoints;
            public decimal? cumulativeAttemptedCreditsNoRemedial;
            public decimal? cumulativeCompletedCreditsNoRemedial;
        }

        public List<SapResultsRecord> SapResultsData = new List<SapResultsRecord>()
        {
            new SapResultsRecord()
            {
                id = "1234",
                originalStatusCode = "U",
                overrideStatusCode = "S",
                evaluationDate = DateTime.Today,
                evaluationPeriodStartTerm = "2015/FALL",
                evaluationPeriodEndTerm = "2015/FALL",
                evaluationPeriodStartDate = null,
                evaluationPeriodEndDate = null,
                evaluationPeriodAttemptedCredits = 16m,
                evaluationPeriodCompletedCredits = 16m,
                evaluationPeriodCompletedGradePoints = 64m,
                cumulativeAttemptedCredits = 80m,
                cumulativeCompletedCredits = 76m,
                cumulativeCompletedGradePoints = 320m,
                academicProgram = "MATH.BS",
                batchId = "SAPC_MCD_55555_17802",
                cumulativeAttemptedCreditsNoRemedial = 70m,
                cumulativeCompletedCreditsNoRemedial = 70m,
            },
            new SapResultsRecord()
            {
                id = "4321",
                originalStatusCode = "S",
                overrideStatusCode = "",
                evaluationDate = DateTime.Today,
                evaluationPeriodStartTerm = "",
                evaluationPeriodEndTerm = "",
                evaluationPeriodStartDate = new DateTime(2015, 9, 1),
                evaluationPeriodEndDate = new DateTime(2015, 12, 31),
                evaluationPeriodAttemptedCredits = 16m,
                evaluationPeriodCompletedCredits = 15m,
                evaluationPeriodCompletedGradePoints = 60m,
                cumulativeAttemptedCredits = 76m,
                cumulativeCompletedCredits = 76m,
                cumulativeCompletedGradePoints = 290m,
                academicProgram = "MATH.BS",
                batchId = "SAPC_MCD_55555_17802",
                cumulativeAttemptedCreditsNoRemedial = 70m,
                cumulativeCompletedCreditsNoRemedial = 70m,                
            }
        };

        public Task<IEnumerable<AcademicProgressEvaluationResult>> GetStudentAcademicProgressEvaluationResultsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (financialAidStudentData == null || financialAidStudentData.sapResultsIds == null || financialAidStudentData.sapResultsIds.Count() == 0)
            {
                //log error
                return Task.FromResult(new List<AcademicProgressEvaluationResult>().AsEnumerable());
            }

            var studentSapResults = SapResultsData.Where(r => financialAidStudentData.sapResultsIds.Contains(r.id));

            var academicProgressEvaluations = new List<AcademicProgressEvaluationResult>();
            foreach (var resultRecord in studentSapResults)
            {
                var index = financialAidStudentData.sapResultsIds.IndexOf(resultRecord.id);
                DateTimeOffset? resultCreateDate = null;
                try
                {
                    int pickTimeStamp;
                    if (int.TryParse(resultRecord.batchId.Split('_')[2], out pickTimeStamp))
                    {
                        pickTimeStamp -= index;

                        DateTime? time = new DateTime(DmiString.PickTimeToDateTime(pickTimeStamp).Ticks);
                        resultCreateDate = time.ToPointInTimeDateTimeOffset(resultRecord.evaluationDate, TimeZoneInfo.Local.Id);
                    }
                }
                catch (Exception) { }

                try
                {
                    if (!resultCreateDate.HasValue)
                    {
                        resultCreateDate = resultRecord.evaluationDate.ToPointInTimeDateTimeOffset(resultRecord.evaluationDate, TimeZoneInfo.Local.Id);
                    }

                    var statusCode = (!string.IsNullOrEmpty(resultRecord.overrideStatusCode)) ? resultRecord.overrideStatusCode : resultRecord.originalStatusCode;
                    var evaluation = new AcademicProgressEvaluationResult(resultRecord.id, studentId, statusCode, resultCreateDate.Value, resultRecord.academicProgram)
                    {
                        CumulativeAttemptedCredits = resultRecord.cumulativeAttemptedCredits ?? 0,
                        CumulativeCompletedCredits = resultRecord.cumulativeCompletedCredits ?? 0,
                        CumulativeCompletedGradePoints = resultRecord.cumulativeCompletedGradePoints ?? 0,
                        CumulativeAttemptedCreditsExcludingRemedial = resultRecord.cumulativeAttemptedCreditsNoRemedial ?? 0,
                        CumulativeCompletedCreditsExcludingRemedial = resultRecord.cumulativeCompletedCreditsNoRemedial ?? 0,
                        EvaluationPeriodAttemptedCredits = resultRecord.evaluationPeriodAttemptedCredits ?? 0,
                        EvaluationPeriodCompletedCredits = resultRecord.evaluationPeriodCompletedCredits ?? 0,
                        EvaluationPeriodCompletedGradePoints = resultRecord.evaluationPeriodCompletedGradePoints ?? 0,
                        EvaluationPeriodStartTerm = resultRecord.evaluationPeriodStartTerm,
                        EvaluationPeriodEndTerm = resultRecord.evaluationPeriodEndTerm,
                        EvaluationPeriodStartDate = resultRecord.evaluationPeriodStartDate,
                        EvaluationPeriodEndDate = resultRecord.evaluationPeriodEndDate,
                    };

                    academicProgressEvaluations.Add(evaluation);
                    
                }
                catch (Exception)
                {

                }
            }

            return Task.FromResult(academicProgressEvaluations.AsEnumerable());
        }

        public Task<IEnumerable<StudentProgram>> GetStudentProgramsAsync(string studentId)
        {
            return Task.FromResult(academicProgramCodes.Select(code => new StudentProgram(studentId, code, catalogCode)));
        }

        public Task<ProgramRequirements> GetProgramRequirementsAsync(string progCode, string catCode)
        {
            var req = new ProgramRequirements(progCode, catCode);
            var dataRecord = programRequirementsData.FirstOrDefault(p => p.programCode == progCode && p.catalogCode == catCode);
            if (dataRecord != null)
            {
                req.MaximumCredits = dataRecord.maxCredits;
                req.MinimumCredits = dataRecord.minCredits;
            }

            return Task.FromResult(req);
        }

        public Task<AcademicProgressProgramDetail> GetStudentAcademicProgressProgramDetailAsync(string programCode, string catalog)
        {
            AcademicProgressProgramDetail detail = null;
            var dataRecord = programRequirementsData.FirstOrDefault(p => p.programCode == programCode && p.catalogCode == catalog);
            if (dataRecord != null)
            {
                detail = new AcademicProgressProgramDetail(programCode, dataRecord.maxCredits, dataRecord.minCredits);
            }

            return Task.FromResult(detail);
        }
    }
}
