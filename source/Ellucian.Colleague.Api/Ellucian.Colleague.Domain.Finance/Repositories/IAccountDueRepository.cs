// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    /// <summary>
    /// Interface to the Accounts Due repository
    /// </summary>
    public interface IAccountDueRepository
    {
        /// <summary>
        /// Get a student's term-based account due information
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account due detail</returns>
        AccountDue Get(string studentId);

        /// <summary>
        /// Get a student's period-based account due information
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account due detail</returns>
        AccountDuePeriod GetPeriods(string studentId);

        /// <summary>
        /// Get a payer's demographic data
        /// </summary>
        /// <param name="personId">Payer ID</param>
        /// <returns>Demographic details</returns>
        ElectronicCheckPayer GetCheckPayerInformation(string personId);

        /// <summary>
        /// Process an e-check
        /// </summary>
        /// <param name="paymentDetails">Payment details</param>
        /// <returns>Processing results</returns>
        ElectronicCheckProcessingResult ProcessCheck(Payment paymentDetails);
    }
}
