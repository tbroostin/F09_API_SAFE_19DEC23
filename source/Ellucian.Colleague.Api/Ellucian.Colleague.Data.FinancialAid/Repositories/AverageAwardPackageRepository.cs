//Copyright 2014-2017 Ellucian Company L.P. and its affiliates
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
    /// Repository for the Award Category Averages
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AverageAwardPackageRepository : BaseColleagueRepository, IAverageAwardPackageRepository
    {
        private const string GraduateStudentCode = "GR";
        private const string UndergraduateStudentCode = "UG";

        /// <summary>
        /// Constructor for the AverageAwardPackage Repository
        /// </summary>
        /// <param name="cacheProvider">cacheProvider</param>
        /// <param name="transactionFactory">transactionFactory</param>
        /// <param name="logger">logger</param>
        public AverageAwardPackageRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get a set of average award packages based on data for all active studentAwardYears
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of the student</param>
        /// <param name="studentAwardYears">StudentAwardYear objects describing the award years to retrieve AverageAwardPackages for</param>
        /// <returns>A IEnumerable of AverageAwardPackage objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if either the studentId or the studentAwardYears are null</exception>
        public async Task<IEnumerable<AverageAwardPackage>> GetAverageAwardPackagesAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (studentAwardYears == null || studentAwardYears.Count() <= 0)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            var averageAwardPackages = new List<AverageAwardPackage>();

            //get the student's CS.ACYR records
            foreach (var studentAwardYear in studentAwardYears)
            {

                if (studentAwardYear.CurrentConfiguration == null)
                {
                    var message = string.Format("StudentAwardYear has no configuration for student id {0}, awardYear {1}. Cannot retrieve average award package.", studentId, studentAwardYear.Code);
                    logger.Debug(message);
                }
                else
                {
                    var csAcyrFile = "CS." + studentAwardYear.Code;
                    var csDataRecord = await DataReader.ReadRecordAsync<CsAcyr>(csAcyrFile, studentId);
                    if (csDataRecord == null)
                    {
                        logger.Debug(string.Format("Student {0} has no {1} record", studentId, csAcyrFile));
                    }
                    else
                    {
                        var csFedIsirKey = csDataRecord.CsFedIsirId;

                        var studentLevel = await GetStudentLevelAsync(studentId, csFedIsirKey);

                        if (studentLevel == GraduateStudentCode)
                        {
                            averageAwardPackages.Add(studentAwardYear.CurrentConfiguration.GraduatePackage);
                        }
                        else
                        {
                            averageAwardPackages.Add(studentAwardYear.CurrentConfiguration.UndergraduatePackage);
                        }

                    }
                }
            }

            return averageAwardPackages;
        }

        #region Helper methods

        /// <summary>
        /// Call a Colleague Transaction to determine the student's graduate level.
        /// If the graduate level cannot be determined by the Transaction, analyze some data points 
        /// to determine the graduate level. If all else fails, assume the student is an undergraduate.
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of the student to analyze</param>
        /// <param name="isirKey">The Id of the IsirFafsa record to analyze</param>
        /// <returns>A string representing the student's graduate level</returns>
        private async Task<string> GetStudentLevelAsync(string studentId, string isirKey)
        {
            // The transaction can return a Y or N value if it finds data to use; if the value remains a null
            // we know the transaction couldn't determine the student level

            var studentLevel = "";

            Transactions.GetGraduateLevelRequest request = new Transactions.GetGraduateLevelRequest();
            request.StudentId = studentId;

            Transactions.GetGraduateLevelResponse response = await transactionInvoker.ExecuteAsync<Transactions.GetGraduateLevelRequest, Transactions.GetGraduateLevelResponse>(request);
            if (response != null && !string.IsNullOrEmpty(response.GraduateLevel))
            {
                if (response.GraduateLevel.ToUpper() == "Y")
                {
                    studentLevel = GraduateStudentCode;
                }
                else if (response.GraduateLevel.ToUpper() == "N")
                {
                    studentLevel = UndergraduateStudentCode;
                }
            }

            // if transaction couldn't determine student level and there is an ISIR on file look
            // there to see if they are a Grad/Prof student

            if (string.IsNullOrEmpty(studentLevel) && !string.IsNullOrEmpty(isirKey))
            {
                var isirFafsaRecord = DataReader.ReadRecord<IsirFafsa>(isirKey);
                if (isirFafsaRecord != null &&
                    (isirFafsaRecord.IfafIsirType.ToUpper() == "ISIR" || isirFafsaRecord.IfafIsirType.ToUpper() == "CPSSG") &&
                    (isirFafsaRecord.IfafGradProf.ToUpper() == "Y"))
                {
                    studentLevel = GraduateStudentCode;
                }
            }

            // If we still can't determine a student level then default to Undergrad

            if (string.IsNullOrEmpty(studentLevel))
            {
                studentLevel = UndergraduateStudentCode;
            }
            return studentLevel;
        }

        #endregion
    }
}