//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Defines how a StudentDocumentRepository must provide access to Student Documents from Colleague.
    /// </summary>
    public interface IStudentDocumentRepository
    {
        /// <summary>
        /// Get all of a student's financial aid documents across all award years.
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to get documents</param>
        /// <returns>A list of StudentDocument objects</returns>
        Task<IEnumerable<StudentDocument>> GetDocumentsAsync(string studentId);
        
    }
}
