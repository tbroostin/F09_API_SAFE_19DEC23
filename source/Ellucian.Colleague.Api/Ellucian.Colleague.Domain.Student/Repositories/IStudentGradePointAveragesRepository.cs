// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentGradePointAveragesRepository
    {
        /// <summary>
        /// Date to be formatted.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<string> GetUnidataFormattedDate(string date);

        /// <summary>
        /// Gets entities to calculate student GPA's.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="sGpa"></param>
        /// <param name="acadPeriodId"></param>
        /// <param name="gradeDate"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<StudentAcademicCredit>, int>> GetStudentGpasAsync(int offset, int limit, StudentAcademicCredit sGpa, string gradeDate);

        /// <summary>
        /// Returns the value of STWEB.TRAN.ALTCUM.FLAG
        /// </summary>
        /// <returns></returns>
        Task<bool> UseAlternativeCumulativeValuesAsync();

        /// <summary>
        /// Gets student program info based om marked credentials ids in student acad cred file/table.
        /// </summary>
        /// <param name="markAcadCredIds"></param>
        /// <returns></returns>
        Task<IEnumerable<StudentAcademicCredentialProgramInfo>> GetStudentCredProgramInfoAsync(IEnumerable<string> markAcadCredIds);

        /// <summary>
        /// Gets record key for student grade point average based on guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<string> GetStudentGradePointAverageIdFromGuidAsync(string guid);

        /// <summary>
        /// Gets student acad credit record based on an id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<StudentAcademicCredit> GetStudentCredProgramInfoByIdAsync(string id);
    }
}
