//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;
using System;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for a StudentDocumentService
    /// </summary>
    public interface IStudentDocumentService
    {
        /// <summary>
        /// Get all of a student's financial aid documents across all award years.
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to get documents</param>
        /// <returns>A list of StudentDocument DTO objects</returns>        
        Task<IEnumerable<StudentDocument>> GetStudentDocumentsAsync(string studentId);        
    }
}
