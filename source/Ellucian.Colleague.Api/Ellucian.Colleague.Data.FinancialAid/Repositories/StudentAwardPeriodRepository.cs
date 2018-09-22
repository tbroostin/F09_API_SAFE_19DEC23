using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Utility.Dependency;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Utility.Caching;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Contains method to get StudentAwardPeriod data from TA.ACYR
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAwardPeriodRepository : BaseColleagueRepository, IStudentAwardPeriodRepository
    {
        /// <summary>
        /// Constructor for StudentAwardPeriod
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public StudentAwardPeriodRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Method returns a list of StudentAwardPeriod objects for a given student id. This list is analogous to
        /// all of a student's TA.ACYR records across FA Years.
        /// </summary>
        /// <param name="studentId">Required: Student ID for which to retreive award period award data</param>
        /// <returns>List of StudentAwardPeriod objects for all FA Years a student has</returns>
        public IEnumerable<StudentAwardPeriod> Get(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            var studentAwardYearsData = dataReader.ReadRecord<FinAid>("FIN.AID", studentId);

            if (studentAwardYearsData == null)
            {
                throw new KeyNotFoundException(string.Format("Student Id {0} does not have Financial Aid records", studentId));
            }

            // Get years with awards from FIN.AID
            var studentAwardYearsList = studentAwardYearsData.FaSaYears;

            var studentAwardPeriods = new List<StudentAwardPeriod>();
            foreach (var year in studentAwardYearsList)
            {
                string acyrFile = "TA." + year;
                string criteria = "WITH TA.STUDENT.ID EQ '" + studentId + "'";
                var studentAwardPeriodData = dataReader.BulkReadRecord<TaAcyr>(acyrFile, criteria);

                var studentAwardPeriodList = BuildStudentAwardPeriods(studentAwardPeriodData, year);
                studentAwardPeriods.AddRange(studentAwardPeriodList);
            }

            return studentAwardPeriods;
        }

        /// <summary>
        /// Build a list of StudentAwardPeriod domain objects from the given collection of TaAcyr data contracts
        /// </summary>
        /// <param name="studentAwardPeriodData">Collection of TaAcyr data contracts</param>
        /// <param name="awardYear">The award year used to retreive the physical TA.ACYR records</param>
        /// <returns>List of StudentAwardPeriod domain objects</returns>
        private IEnumerable<StudentAwardPeriod> BuildStudentAwardPeriods(Collection<TaAcyr> studentAwardPeriodData, string awardYear)
        {
            var studentAwardPeriodList = new List<StudentAwardPeriod>();
            foreach (var studentAwardPeriod in studentAwardPeriodData)
            {
                try
                {
                    //TaAcyr recordkey is StudentId*Award*AwardPeriod
                    string[] keyFields = studentAwardPeriod.Recordkey.Split('*');
                    string studentId = keyFields[0];
                    string award = keyFields[1];
                    string awardPeriod = keyFields[2];

                    var studentAwardPeriodObj = new StudentAwardPeriod(studentId, awardYear, award, awardPeriod, studentAwardPeriod.TaTermAmount, studentAwardPeriod.TaTermAction);
                    studentAwardPeriodList.Add(studentAwardPeriodObj);
                }
                catch (Exception e)
                {
                    logger.Info(e, string.Format("Unable to create StudentAwardPeriod from data. AwardYear: {0} RecordKey {1}", awardYear, studentAwardPeriod.Recordkey));
                }
            }
            return studentAwardPeriodList;
        }

        /*
        /// <summary>
        /// Accept the given StudentAwardPeriod. This method updates the Colleague DB to accept
        /// the award period based on the default or year-specific "web Accept Status" defined by the institution
        /// It also adds/updates a CM Code (if specified by the institution)
        /// on the student records.
        /// </summary>
        /// <param name="studentAwardPeriod">StudentAwardPeriod object to Accept</param>
        /// <returns>The StudentAwardPeriod object with a Accepted status</returns>
        public StudentAwardPeriod AcceptStudentAwardPeriod(StudentAwardPeriod studentAwardPeriod)
        {

                       

            //Get action status to use
            var actionStatusData = dataReader.ReadRecord<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
            if (actionStatusData == null)
            {
                var errorMessage = "Unable to access Financial Aid web defaults from ST.PARMS -> STWEB.DEFAULTS";
                logger.Info(errorMessage);
                throw new Exception(errorMessage);
            }

            var acceptStatusCode = actionStatusData.StwebFaAccActionCode;
            var acceptCmCode = actionStatusData.StwebFaAccCommCode;
            var acceptCmCodeStatus = actionStatusData.StwebFaAccCommStatus;

            //pull out the association record that matches the studentAwardPeriod's fa year
            var yearBasedConfig = actionStatusData.StwebAcceptRejectEntityAssociation.FirstOrDefault(a => a.StwebFaAccRejYearsAssocMember == studentAwardPeriod.AwardYear);
            if (yearBasedConfig != null)
            {
                acceptStatusCode = yearBasedConfig.StwebFaYrAccActCodesAssocMember;
                acceptCmCode = yearBasedConfig.StwebFaYrAccCommCodesAssocMember;
                acceptCmCodeStatus = yearBasedConfig.StwebFaYrAccCommStatAssocMember;
            }

            //Set up request transaction
            Transactions.UpdateStuAwardPeriodStatusRequest request = new Transactions.UpdateStuAwardPeriodStatusRequest();
            request.Year = studentAwardPeriod.AwardYear;
            request.StudentId = studentAwardPeriod.StudentId;
            request.AwardId = studentAwardPeriod.AwardId;
            request.AwardPeriodId = studentAwardPeriod.AwardPeriod;
            request.NewActionStatus = acceptStatusCode;
            request.CmCode = acceptCmCode;
            request.CmStatus = acceptCmCodeStatus;

            //Invoke transaction
            Transactions.UpdateStuAwardPeriodStatusResponse response = transactionInvoker.Execute<Transactions.UpdateStuAwardPeriodStatusRequest, Transactions.UpdateStuAwardPeriodStatusResponse>(request);


            //catch errors 
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                
                throw new Exception(response.ErrorMessage);
            }

            //if no errors, update studentAwardPeriod action
            studentAwardPeriod.AwardStatus = acceptStatusCode;
            return studentAwardPeriod;
        }

        
        /// <summary>
        /// Reject the given StudentAwardPeriod. This method updates the Colleague DB to reject
        /// the award period based on the default or year-specific "web Reject Status" defined by the institution
        /// It also adds/updates a CM Code (if specified by the institution)
        /// on the student records.
        /// </summary>
        /// <param name="studentAwardPeriod">StudentAwardPeriod object to Reject</param>
        /// <returns>The StudentAwardPeriod object with an rejected status</returns>
        public StudentAwardPeriod RejectStudentAwardPeriod(StudentAwardPeriod studentAwardPeriod)
        {
            

            //Get action status to use
            var actionStatusData = dataReader.ReadRecord<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
            if (actionStatusData == null)
            {
                var errorMessage = "Unable to access Financial Aid web defaults from ST.PARMS -> STWEB.DEFAULTS";
                logger.Info(errorMessage);
                throw new Exception(errorMessage);
            }

            var rejectStatusCode = actionStatusData.StwebFaRejActionCode;
            var rejectCmCode = actionStatusData.StwebFaRejCommCode;
            var rejectCmStatus = actionStatusData.StwebFaRejCommStatus;

            //pull out the association record that matches the studentAwardPeriod's fa year
            var yearBasedConfig = actionStatusData.StwebAcceptRejectEntityAssociation.FirstOrDefault(a => a.StwebFaAccRejYearsAssocMember == studentAwardPeriod.AwardYear);
            if (yearBasedConfig != null)
            {
                rejectStatusCode = yearBasedConfig.StwebFaYrRejActCodesAssocMember;
                rejectCmCode = yearBasedConfig.StwebFaYrRejCommCodesAssocMember;
                rejectCmStatus = yearBasedConfig.StwebFaYrRejCommStatAssocMember;

            }

            //Set up request transaction
            Transactions.UpdateStuAwardPeriodStatusRequest request = new Transactions.UpdateStuAwardPeriodStatusRequest();
            request.Year = studentAwardPeriod.AwardYear;
            request.StudentId = studentAwardPeriod.StudentId;
            request.AwardId = studentAwardPeriod.AwardId;
            request.AwardPeriodId = studentAwardPeriod.AwardPeriod;
            request.NewActionStatus = rejectStatusCode;
            request.CmCode = rejectCmCode;
            request.CmStatus = rejectCmStatus;

            //Invoke transaction
            Transactions.UpdateStuAwardPeriodStatusResponse response = transactionInvoker.Execute<Transactions.UpdateStuAwardPeriodStatusRequest, Transactions.UpdateStuAwardPeriodStatusResponse>(request);


            //catch errors 
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                
                throw new Exception(response.ErrorMessage);
            }

            //if no errors, update studentAwardPeriod action
            studentAwardPeriod.AwardStatus = rejectStatusCode;
            return studentAwardPeriod;
        }*/
    }
}
