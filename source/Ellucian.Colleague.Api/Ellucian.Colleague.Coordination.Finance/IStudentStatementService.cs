// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Base.Reports;
using Ellucian.Colleague.Dtos.Finance;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Finance
{
    /// <summary>
    /// Interface for a StudentStatementService
    /// </summary>
    public interface IStudentStatementService
    {
        /// <summary>
        /// Get an account holder's statement for a term or period.  
        /// </summary>
        /// <param name="accountHolderId">ID of the student for whom the statement will be generated</param>
        /// <param name="timeframeId">ID of the timeframe for which the statement will be generated</param>
        /// <param name="startDate">Date on which the supplied timeframe starts</param>
        /// <param name="endDate">Date on which the supplied timeframe ends</param>
        /// <returns>A student statement</returns>
        Task<StudentStatement> GetStudentStatementAsync(string accountHolderId, string timeframeId, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Get a student's accounts receivable statement as a byte array representation of a PDF file.  
        /// </summary>
        /// <param name="statementDto">StudentStatement DTO to use as the data source for producing the student statement report.</param>
        /// <param name="pathToReport">The path on the server to the report template</param>
        /// <param name="pathToResourceFile">The path on the server to the resource file</param>
        /// <param name="pathToLogo">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF student statement report.</returns>
        byte[] GetStudentStatementReport(StudentStatement statementDto, string pathToReport, string pathToResourceFile, string pathToLogo);
    }
}
