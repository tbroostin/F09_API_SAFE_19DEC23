/*Copyright 2018-2022 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentTranscriptGradesOptionsRepository : BaseColleagueRepository, IStudentTranscriptGradesOptionsRepository
    {
        private readonly int _readSize;
        private readonly string academicCredentialCriteria = "WITH STC.VERIFIED.GRADE NE '' AND STC.STUDENT.EQUIV.EVAL EQ ''";
        RepositoryException exception = new RepositoryException();

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
            
            int totalCount = 0;
            IEnumerable<StudentTranscriptGradesOptions> studentAcademicCreditEntities = null;

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
            totalCount = studentAcadCredIds.Count();
            Array.Sort(studentAcadCredIds);
            var subList = studentAcadCredIds.Skip(offset).Take(limit).ToArray();
            var studentAcademicCreditData = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", subList);
            studentAcademicCreditEntities = await BuildStudentTranscriptGradesOptionsAsync(subList, studentAcademicCreditData);

            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
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
                //throw new KeyNotFoundException("No student transcript grades instance was found for guid  " + guid + ".");
                exception.AddError(new RepositoryError("GUID.Not.Found", "No student transcript grades instance was found for guid  '" + guid + "'.") { Id = guid });
                throw exception;
            }
            if ((guidRecord.LdmGuidEntity != "STUDENT.ACAD.CRED") || (guidRecord.LdmGuidSecondaryFld != "STC.INTG.KEY.IDX"))
            {
                //throw new KeyNotFoundException("GUID " + guid + " is invalid.  Expecting GUID with entity STUDENT.ACAD.CRED with a secondary field equal to STC.INTG.KEY.IDX");
                exception.AddError(new RepositoryError("Validation.Exception", "GUID '" + guid + "' is invalid.  Expecting GUID with entity STUDENT.ACAD.CRED with a secondary field equal to STC.INTG.KEY.IDX") { Id = guid });
                throw exception;
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
                // throw new ArgumentNullException("id", "ID is required to get a StudentAcademicCredit.");
                exception.AddError(new RepositoryError("id", "ID is required to get a student transcript grades."));
                throw exception;
            }

            var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>(id);
            if (studentAcadCred == null)
            {
                exception.AddError(new RepositoryError("id", string.Concat("Record not found, or student transcript grades. with ID ", id, "invalid.")));
                throw exception;
            }
            if (!string.IsNullOrEmpty(studentAcadCred.StcStudentEquivEval))
            {
                exception.AddError(new RepositoryError("Validation.Exception", "Record has an equivalency (STC.STUDENT.EQUIV.EVAL)")
                {
                    Id = string.IsNullOrEmpty(studentAcadCred.RecordGuid) ? "" : studentAcadCred.RecordGuid,
                    SourceId = string.IsNullOrEmpty(studentAcadCred.Recordkey) ? "" : studentAcadCred.Recordkey
                });
            }
            if (string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade))
            {
                exception.AddError(new RepositoryError("Validation.Exception", "Record has no verified grade (STC.VERIFIED.GRADE)")
                {
                    Id = string.IsNullOrEmpty(studentAcadCred.RecordGuid) ? "" : studentAcadCred.RecordGuid,
                    SourceId = string.IsNullOrEmpty(studentAcadCred.Recordkey) ? "" : studentAcadCred.Recordkey
                });
            }

            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            var dict = await GetGuidsCollectionAsync(new List<string>() { id });
            var gradeOptions = await BuildStudentTranscriptGradeOptionsAsync(studentAcadCred, dict);
            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return gradeOptions;
        }


        public async Task<IEnumerable<StudentTranscriptGradesOptions>> BuildStudentTranscriptGradesOptionsAsync(string[] studentAcadCredIds, Collection<StudentAcadCred> sources)
        {
            var studentTranscriptGradesOptionsCollection = new List<StudentTranscriptGradesOptions>();

            if (studentAcadCredIds != null && studentAcadCredIds.Any())
            {
                var dict = await GetGuidsCollectionAsync(studentAcadCredIds);

                foreach (var source in sources)
                {
                    studentTranscriptGradesOptionsCollection.Add(await BuildStudentTranscriptGradeOptionsAsync(source, dict));
                }
            }
            return studentTranscriptGradesOptionsCollection;
        }

        /// <summary>
        /// Using a collection of ids with guids
        ///  get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of ids</param>
        /// <returns>Dictionary consisting of a application.id with guids</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new Dictionary<string, string>();
            }
            if (ids == null || !ids.Any())
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(key => new RecordKeyLookup("STUDENT.ACAD.CRED", key,
                    "STC.INTG.KEY.IDX", key, false))
                    .ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occurred while getting guids for {0} with a secondary key of {1}.", "STUDENT.ACAD.CRED", "STC.INTG.KEY.IDX"), ex);
            }

            return guidCollection;
        }

        private async Task<StudentTranscriptGradesOptions> BuildStudentTranscriptGradeOptionsAsync(StudentAcadCred source, Dictionary<string, string> dict)
        {

            if (dict == null)
            {
                exception.AddError(new RepositoryError("GUID.Not.Found", "Guids not found, Entity: 'STUDENT.ACAD.CRED'")
                {
                    Id = string.IsNullOrEmpty(source.RecordGuid) ? "" : source.RecordGuid,
                    SourceId = string.IsNullOrEmpty(source.Recordkey) ? "" : source.Recordkey
                });
                return null;
            }

            var guidFromRecordInfo = string.Empty;
            dict.TryGetValue(source.Recordkey, out guidFromRecordInfo);
            if (string.IsNullOrEmpty(guidFromRecordInfo))
            {
                exception.AddError(new RepositoryError("GUID.Not.Found", "Guid not found, Entity: 'STUDENT.ACAD.CRED'")
                {
                    Id = string.IsNullOrEmpty(source.RecordGuid) ? "" : source.RecordGuid,
                    SourceId = string.IsNullOrEmpty(source.Recordkey) ? "" : source.Recordkey
                });
                return null;
            }


            StudentTranscriptGradesOptions studentTranscriptGradeOptions = null;

            try
            {
                studentTranscriptGradeOptions = new StudentTranscriptGradesOptions(source.Recordkey, guidFromRecordInfo)
                {
                    GradeSchemeCode = source.StcGradeScheme
                };
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    Id = string.IsNullOrEmpty(source.RecordGuid) ? "" : source.RecordGuid,
                    SourceId = string.IsNullOrEmpty(source.Recordkey) ? "" : source.Recordkey
                });
            }
            return studentTranscriptGradeOptions;

        }
    }
}