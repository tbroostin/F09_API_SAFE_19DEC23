/*Copyright 2014-2022 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{

    /// <summary>
    /// LoanRequestRepository 
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class LoanRequestRepository : BaseColleagueRepository, ILoanRequestRepository
    {
        /// <summary>
        /// Instantiate a new LoanRequestRepository
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public LoanRequestRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get a LoanRequest with the given id
        /// </summary>
        /// <param name="id">Id of the LoanRequest to get</param>
        /// <returns>LoanRequest object with the given id.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<LoanRequest> GetLoanRequestAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var loanRequestRecord = await DataReader.ReadRecordAsync<NewLoanRequest>(id);
            if (loanRequestRecord == null)
            {
                throw new KeyNotFoundException(string.Format("NewLoanRequest record {0} does not exist.", id));
            }

            LoanRequestStatus status;

            switch (loanRequestRecord.NlrCurrentStatus.ToUpper())
            {
                case "A":
                    status = LoanRequestStatus.Accepted;
                    break;
                case "P":
                    status = LoanRequestStatus.Pending;
                    break;
                case "R":
                    status = LoanRequestStatus.Rejected;
                    break;
                default:
                    logger.Error(string.Format("LoanRequestStatus does not exist for NewLoanRequest record id {0}, status {1}. Setting to Pending.", id, loanRequestRecord.NlrCurrentStatus));
                    status = LoanRequestStatus.Pending;
                    break;
            }

            try
            {
                var loanRequest = new LoanRequest(loanRequestRecord.Recordkey,
                    loanRequestRecord.NlrStudentId,
                    loanRequestRecord.NlrAwardYear,
                    loanRequestRecord.NewLoanRequestAdddate.Value,
                    loanRequestRecord.NlrTotalRequestAmount.Value,
                    loanRequestRecord.NlrAssignedToId,
                    status,
                    loanRequestRecord.NlrCurrentStatusDate.Value,
                    loanRequestRecord.NlrModifierId);

                foreach (var loanPeriod in loanRequestRecord.AwardPeriodEntityAssociation)
                {
                    if (!loanRequest.AddLoanPeriod(loanPeriod.NlrAwardPeriodsAssocMember, loanPeriod.NlrAwardPeriodAmountsAssocMember ?? 0))
                    {
                        throw new ApplicationException(string.Format("Could not add loan period {0}", loanPeriod.NlrAwardPeriodsAssocMember));
                    }
                }

                loanRequest.StudentComments = loanRequestRecord.NlrStudentComments;
                loanRequest.ModifierComments = loanRequestRecord.NlrModifierComments;

                return loanRequest;
            }
            catch (Exception e)
            {
                var message = string.Format("Error creating loan request object from NewLoanRequest record id {0}", id);
                logger.Error(e, message);
                throw new ApplicationException(message, e);
            }

        }

        /// <summary>
        /// Create a LoanRequest record in the database. 
        /// </summary>
        /// <param name="loanRequest">The LoanRequest object containing the data with which to update the database</param>
        /// <param name="studentAwardYear">The StudentAwardYear that the loan request will be created for.</param>
        /// <returns>A new LoanRequest object successfully created from the new loan request database record</returns>       
        /// <exception cref="ArgumentNullException">Thrown if the loanRequest or studentAwardYear argument is null</exception>
        /// <exception cref="ExistingResourceException">Thrown if a loanRequest record already exists in the db with the StudentId and AwardYear from the loanRequest argument</exception>
        /// <exception cref="OperationCanceledException">Thrown if the Colleague Transaction was unable to create the LoanRequest due to a locked record</exception>
        public async Task<LoanRequest> CreateLoanRequestAsync(LoanRequest loanRequest, StudentAwardYear studentAwardYear)
        {
            if (loanRequest == null)
            {
                throw new ArgumentNullException("loanRequest");
            }

            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            //create transaction object
            var createRequest = new CreateLoanRequestRequest()
            {
                StudentId = loanRequest.StudentId,
                AwardYear = loanRequest.AwardYear,
                RequestDate = loanRequest.RequestDate,
                TotalRequestAmount = loanRequest.TotalRequestAmount,
                AssignedToId = loanRequest.AssignedToId,
                Status = loanRequest.Status.ToString()[0].ToString(),
                StatusDate = loanRequest.StatusDate,
                StudentComments = loanRequest.StudentComments,
                CmCode = (studentAwardYear.CurrentConfiguration != null) ? studentAwardYear.CurrentConfiguration.NewLoanCommunicationCode : string.Empty,
                CmStatus = (studentAwardYear.CurrentConfiguration != null) ? studentAwardYear.CurrentConfiguration.NewLoanCommunicationStatus : string.Empty
            };

            createRequest.LoanAwardPeriods = new List<LoanAwardPeriods>();

            if (loanRequest.LoanRequestPeriods == null || loanRequest.LoanRequestPeriods.Count == 0)
            {
                throw new ArgumentException("loanRequestPeriods list cannot be empty", "loanRequest");
            }

            var total = 0;
            foreach (var period in loanRequest.LoanRequestPeriods)
            {
                total += period.LoanAmount;
            }
            if (total != loanRequest.TotalRequestAmount)
            {
                throw new ArgumentException("loanRequest.TotalRequestAmount must be equal to the sum of all loan period amounts", "loanRequest");
            }

            foreach (var loanPeriod in loanRequest.LoanRequestPeriods)
            {
                createRequest.LoanAwardPeriods.Add(new LoanAwardPeriods()
                {
                    LoanPeriodIds = loanPeriod.Code,
                    LoanPeriodAmounts = loanPeriod.LoanAmount

                });
            }

            //execute transaction
            var createResponse = await transactionInvoker.ExecuteAsync<CreateLoanRequestRequest, CreateLoanRequestResponse>(createRequest);

            //check for errors
            if (!string.IsNullOrEmpty(createResponse.ErrorMessage))
            {
                logger.Error(createResponse.ErrorMessage);

                if (createResponse.ErrorMessage.StartsWith("ExistingResource", true, CultureInfo.InvariantCulture))
                {
                    throw new ExistingResourceException(createResponse.ErrorMessage, createResponse.ExistingRequestId);
                }
                else if (createResponse.ErrorMessage.StartsWith("Conflict", true, CultureInfo.InvariantCulture))
                {
                    throw new OperationCanceledException(createResponse.ErrorMessage);
                }
                else
                {
                    throw new ColleagueWebApiException("Unknown error: " + createResponse.ErrorMessage);
                }
            }

            //Get new LoanRequest object
            return await GetLoanRequestAsync(createResponse.OutLoanRequestId);
        }
    }
}
