/*Copyright 2018 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentTranscriptGradesOptionsRepository : BaseColleagueRepository, IStudentTranscriptGradesOptionsRepository
    {
        private readonly int _readSize;
        private readonly string academicCredentialCriteria = "WITH STC.VERIFIED.GRADE NE '' AND STC.STUDENT.EQUIV.EVAL EQ ''";

        public StudentTranscriptGradesOptionsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        ///  Get all StudentTranscriptGradesOptions
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="offset"></param>
        /// <returns>Collection of StudentTranscriptGradesOptions domain entities</returns>
        public async Task<Tuple<IEnumerable<StudentTranscriptGradesOptions>, int>> GetStudentTranscriptGradesOptionsAsync(int offset, int limit, string student = "", bool bypassCache = false)
        {
            string[] limitingKeys = null;
            if (!string.IsNullOrEmpty(student))
            {
                // student filter only
                var personStDataContract = await DataReader.ReadRecordAsync<Base.DataContracts.PersonSt>("PERSON.ST", student);
                if (personStDataContract == null)
                {
                    return new Tuple<IEnumerable<StudentTranscriptGradesOptions>, int>(null, 0);
                }
                limitingKeys = personStDataContract.PstStudentAcadCred.ToArray();
            }

            var studentAcadCredIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, academicCredentialCriteria);
            var totalCount = studentAcadCredIds.Count();
            Array.Sort(studentAcadCredIds);
            var subList = studentAcadCredIds.Skip(offset).Take(limit).ToArray();
            var studentAcademicCreditData = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", subList);
            var studentAcademicCreditEntities = await BuildStudentTranscriptGradesOptionsAsync(subList, studentAcademicCreditData);
            return new Tuple<IEnumerable<StudentTranscriptGradesOptions>, int>(studentAcademicCreditEntities, totalCount);
        }

        /// <summary>
        /// Get a single StudentTranscriptGradesOptions domain entity from a guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>StudentTranscriptGradesOptions domain entity</returns>
        public async Task<StudentTranscriptGradesOptions> GetStudentTranscriptGradesOptionsByGuidAsync(string guid)
        {
            return await GetStudentTranscriptGradesOptionsByIdAsync(await GetStudentTranscriptGradesOptionsIdFromGuidAsync(guid));
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetStudentTranscriptGradesOptionsIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var guidRecord = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", guid);
            if (guidRecord == null)
            {
                throw new KeyNotFoundException("STUDENT.ACAD.CRED GUID " + guid + " not found.");
            }
            if ((guidRecord.LdmGuidEntity != "STUDENT.ACAD.CRED") || (guidRecord.LdmGuidSecondaryFld != "STC.INTG.KEY.IDX"))
            {
                throw new KeyNotFoundException("GUID " + guid + " is invalid.  Expecting GUID with entity STUDENT.ACAD.CRED with a secondary field equal to STC.INTG.KEY.IDX");
            }            
            return guidRecord.LdmGuidPrimaryKey;
        }

        /// <summary>
        /// Get a single StudentTranscriptGradesOptions domain entity from an StudentAcadCred id.
        /// </summary>
        /// <param name="id">The StudentAcadCred id</param>
        /// <returns>StudentTranscriptGradesOptions domain entity object</returns>
        public async Task<StudentTranscriptGradesOptions> GetStudentTranscriptGradesOptionsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a StudentAcademicCredit.");
            }
            
            var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>(id);
            if (studentAcadCred == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or StudentAcademicCredit with ID ", id, "invalid."));
            }
            if (!string.IsNullOrEmpty(studentAcadCred.StcStudentEquivEval))
            {
                throw new KeyNotFoundException(string.Format("Record has an equivalency (STC.STUDENT.EQUIV.EVAL), Entity: 'STUDENT.ACAD.CRED', Record ID '{0}'", studentAcadCred.Recordkey));
            }
            if (string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade))
            {
                throw new KeyNotFoundException(string.Format("Record has no verified grade (STC.VERIFIED.GRADE), Entity: 'STUDENT.ACAD.CRED', Record ID '{0}'", studentAcadCred.Recordkey));
            }
            return await BuildStudentTranscriptGradeOptionsAsync(studentAcadCred);
        }


        public async Task<IEnumerable<StudentTranscriptGradesOptions>> BuildStudentTranscriptGradesOptionsAsync(string[] studentAcadCredIds, Collection<StudentAcadCred> sources)
        {
            var StudentTranscriptGradesOptionsCollection = new List<StudentTranscriptGradesOptions>();
            
            if (studentAcadCredIds != null && studentAcadCredIds.Any())
            {
                foreach (var source in sources)
                {
                    StudentTranscriptGradesOptionsCollection.Add(await BuildStudentTranscriptGradeOptionsAsync(source));
                }
            }
            return StudentTranscriptGradesOptionsCollection;
        }

        private async Task<StudentTranscriptGradesOptions> BuildStudentTranscriptGradeOptionsAsync(StudentAcadCred source)
        {
            try
            {
                var guidFromRecordInfo = await GetGuidFromRecordInfoAsync("STUDENT.ACAD.CRED", source.Recordkey, "STC.INTG.KEY.IDX", source.StcIntgKeyIdx);

                if (string.IsNullOrEmpty(guidFromRecordInfo))
                {
                    throw new KeyNotFoundException(string.Format("Record not found, Entity: 'STUDENT.ACAD.CRED', Record ID '{0}'", source.Recordkey));
                }
                var studentTranscriptGradeOptions = new StudentTranscriptGradesOptions(source.Recordkey, guidFromRecordInfo)
                {
                    GradeSchemeCode = source.StcGradeScheme
                };
                return studentTranscriptGradeOptions;
            }
            catch
            {
                throw new KeyNotFoundException(string.Format("Record not found, Entity: 'STUDENT.ACAD.CRED', Record ID '{0}'", source.Recordkey));
            }            
        }
    }
}