/* Copyright 2017-2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// PayStatementService Interface
    /// </summary>
    public interface IPayStatementService
    {
        /// <summary>
        /// Get a PDF report of a single pay statement
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pathToReportTemplate"></param>
        /// <returns></returns>
        Task<Tuple<string,byte[]>> GetPayStatementPdf(string id, string pathToReportTemplate, string pathToLogo);

        /// <summary>
        /// Get a PDF report for multiple pay statements
        /// </summary>
        /// <param name="payStatementIds"></param>
        /// <param name="pathToReportTemplate"></param>
        /// <param name="pathToLogo"></param>
        /// <returns></returns>
        Task<byte[]> GetPayStatementPdf(IEnumerable<string> payStatementIds, string pathToReportTemplate, string pathToLogo);

        /// <summary>
        /// Get a collection of PayStatementSummary objects for the current user
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PayStatementSummary>> GetPayStatementSummariesAsync(IEnumerable<string> employeeIdsFilter = null, 
            bool? hasOnlineConsentFilter = null, 
            DateTime? payDateFilter = null, 
            string payCycleIdFilter = null,
            DateTime? startDateFilter = null,
            DateTime? endDateFilter = null);

        /// <summary>
        /// Get the pay statement information for a given id.
        /// </summary>
        /// <param name="id">The id of the requested pay statement.</param>      
        /// <returns>The requested PayStatementInformation DTO</returns>
        Task<PayStatementInformation> GetPayStatementInformationAsync(string id);
    }
}
