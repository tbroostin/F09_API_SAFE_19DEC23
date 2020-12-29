//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Ellucian.Colleague.Domain.Base.Services;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Higher education institution admission applications.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentUnverifiedGradesRepository : BaseColleagueRepository, IStudentUnverifiedGradesRepository
    {
        private RepositoryException exception;
        const string AllStudentUnverifiedGradesCache = "AllStudentUnverifiedGradesKeys";
        const int AllStudentUnverifiedGradesCacheTimeout = 20;

        private Dictionary<string, string> _admissionApplicationDict = new Dictionary<string, string>();

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public StudentUnverifiedGradesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
        }

        /// <summary>
        /// Get student unverified grades
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int>> GetStudentUnverifiedGradesAsync(int offset, int limit, bool bypassCache,
            string student = "", string studentAcadCredId = "", string section = "")
        {
            List<Domain.Student.Entities.StudentUnverifiedGrades> studentUnverifiedGradesEntities = new List<Domain.Student.Entities.StudentUnverifiedGrades>();
            string studentUnverifiedGradesKeys = CacheSupport.BuildCacheKey(AllStudentUnverifiedGradesCache,
                    student, studentAcadCredId, section);

            int totalCount = 0;
            string[] limitingKeys = null;

            var criteria = new StringBuilder();
            criteria.Append("WITH STC.FINAL.GRADE");
            criteria.Append(" OR WITH SCS.MID.TERM.GRADE1");
            criteria.Append(" OR WITH SCS.MID.TERM.GRADE2");
            criteria.Append(" OR WITH SCS.MID.TERM.GRADE3");
            criteria.Append(" OR WITH SCS.MID.TERM.GRADE4");
            criteria.Append(" OR WITH SCS.MID.TERM.GRADE5");
            criteria.Append(" OR WITH SCS.MID.TERM.GRADE6");
            criteria.Append(" OR WITH SCS.LAST.ATTEND.DATE");
            criteria.Append(" OR WITH SCS.NEVER.ATTENDED.FLAG");

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                studentUnverifiedGradesKeys,
                "STUDENT.COURSE.SEC",
                offset,
                limit,
                AllStudentUnverifiedGradesCacheTimeout,
                async () =>
                {
                    if ((!string.IsNullOrEmpty(studentAcadCredId)) && (string.IsNullOrEmpty(student)) && (string.IsNullOrEmpty(section)))
                    {
                            // sectionRegistration filter only
                            var studentAcadCredDataContract = await DataReader.ReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredId);
                        if (studentAcadCredDataContract == null)
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        if (string.IsNullOrEmpty(studentAcadCredDataContract.StcStudentCourseSec))
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        limitingKeys = new string[] { studentAcadCredDataContract.StcStudentCourseSec };
                    }

                    if ((!string.IsNullOrEmpty(studentAcadCredId)) && (!string.IsNullOrEmpty(student)) && (string.IsNullOrEmpty(section)))
                    {
                            // sectionRegistration filter and student filter
                            var studentAcadCredDataContract = await DataReader.ReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredId);
                        if (studentAcadCredDataContract == null)
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        if (string.IsNullOrEmpty(studentAcadCredDataContract.StcStudentCourseSec))
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        if (student == studentAcadCredDataContract.StcPersonId)
                        {
                            limitingKeys = new string[] { studentAcadCredDataContract.StcStudentCourseSec };
                        }
                        else
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                    }

                    if ((string.IsNullOrEmpty(studentAcadCredId)) && (!string.IsNullOrEmpty(student)) && (string.IsNullOrEmpty(section)))
                    {
                            // student filter only
                            var personStDataContract = await DataReader.ReadRecordAsync<Base.DataContracts.PersonSt>("PERSON.ST", student);
                        if (personStDataContract == null)
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        limitingKeys = personStDataContract.PstStudentCourseSec.ToArray();
                    }

                    if ((string.IsNullOrEmpty(studentAcadCredId)) && (string.IsNullOrEmpty(student)) && (!string.IsNullOrEmpty(section)))
                    {
                            // section filter only
                            limitingKeys = await DataReader.SelectAsync("STUDENT.COURSE.SEC", "WITH SCS.COURSE.SECTION = '" + section + "'");
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                    }

                    //  StudentUnverifiedGradesController will not allow these filter combinations, but adding support from repo directly

                    if ((!string.IsNullOrEmpty(studentAcadCredId)) && (string.IsNullOrEmpty(student)) && (!string.IsNullOrEmpty(section)))
                    {
                            // sectionRegistration filter and section filter
                            var studentAcadCredDataContract = await DataReader.ReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredId);
                        if (studentAcadCredDataContract == null)
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        if (string.IsNullOrEmpty(studentAcadCredDataContract.StcStudentCourseSec))
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        var scsLimitingKeys = new string[] { studentAcadCredDataContract.StcStudentCourseSec };
                        limitingKeys = await DataReader.SelectAsync("STUDENT.COURSE.SEC", scsLimitingKeys, "WITH SCS.COURSE.SECTION = '" + section + "'");
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                    }

                    if ((string.IsNullOrEmpty(studentAcadCredId)) && (!string.IsNullOrEmpty(student)) && (!string.IsNullOrEmpty(section)))
                    {
                            // student filter and section filter
                            var personStDataContract = await DataReader.ReadRecordAsync<Base.DataContracts.PersonSt>("PERSON.ST", student);
                        if (personStDataContract == null)
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        var scsLimitingKeys = personStDataContract.PstStudentCourseSec.ToArray();
                        limitingKeys = await DataReader.SelectAsync("STUDENT.COURSE.SEC", scsLimitingKeys, "WITH SCS.COURSE.SECTION = '" + section + "'");
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                    }

                    if ((!string.IsNullOrEmpty(studentAcadCredId)) && (!string.IsNullOrEmpty(student)) && (!string.IsNullOrEmpty(section)))
                    {
                            // sectionRegistration filter, student filter, and section filter
                            var studentAcadCredDataContract = await DataReader.ReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredId);
                        if (studentAcadCredDataContract == null)
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        if (string.IsNullOrEmpty(studentAcadCredDataContract.StcStudentCourseSec))
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        if (student == studentAcadCredDataContract.StcPersonId)
                        {
                            var scsLimitingKeys = new string[] { studentAcadCredDataContract.StcStudentCourseSec };
                            limitingKeys = await DataReader.SelectAsync("STUDENT.COURSE.SEC", scsLimitingKeys, "WITH SCS.COURSE.SECTION = '" + section + "'");
                            if (limitingKeys == null || !limitingKeys.Any())
                            {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            }
                        }
                        else
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }

                    }

                    return new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = limitingKeys != null && limitingKeys.Any() ? limitingKeys.Distinct().ToList() : null,
                        criteria = criteria.ToString(),
                    };
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<StudentUnverifiedGrades>, int>(new List<StudentUnverifiedGrades>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            //Array.Sort(limitingKeys);
            var subList = keyCacheObject.Sublist.ToArray();

            // Now we have criteria, so we can select and read the records
            Collection<DataContracts.StudentCourseSec> studentCourseSecDataContracts = null;
            Collection<DataContracts.StudentAcadCred> studentAcadCredDataContracts = null;
            studentCourseSecDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.StudentCourseSec>("STUDENT.COURSE.SEC", subList);

            var studentAcadCredIds = studentCourseSecDataContracts.Select(s => s.ScsStudentAcadCred).Distinct();
            studentAcadCredDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredIds.ToArray());

            if (studentCourseSecDataContracts != null && studentCourseSecDataContracts.Any())
            {
                foreach (var studentCourseSecDataContract in studentCourseSecDataContracts)
                {
                    List<string> studentCourseSecKey = new List<string>();
                    studentCourseSecKey.Add(studentCourseSecDataContract.Recordkey);
                    var studentAcadCredDataContract = studentAcadCredDataContracts.Where(s => s.Recordkey == studentCourseSecDataContract.ScsStudentAcadCred).FirstOrDefault();
                    if (studentAcadCredDataContract == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Format("Could not read STUDENT.ACAD.CRED with ID : '{0}'.", studentCourseSecDataContract.ScsStudentAcadCred))
                        {
                            Id = studentCourseSecDataContract.RecordGuid,
                            SourceId = studentCourseSecDataContract.Recordkey
                        });
                    }
                    StudentUnverifiedGrades studentUnverifiedGrades = null;
                    try
                    {
                        studentUnverifiedGrades = new StudentUnverifiedGrades(studentCourseSecDataContract.RecordGuid, studentCourseSecDataContract.Recordkey);
                        if (studentUnverifiedGrades != null)
                        {
                            StudentUnverifiedGrades studentUnverifiedGradesEntity = BuildStudentUnverifiedGrades(studentUnverifiedGrades, studentCourseSecDataContract, studentAcadCredDataContract);
                            studentUnverifiedGradesEntities.Add(studentUnverifiedGradesEntity);
                        }
                    }
                    catch (Exception ex)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                        {
                            Id = studentCourseSecDataContract.RecordGuid,
                            SourceId = studentCourseSecDataContract.Recordkey
                        });
                    }
                }
                if (exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
            }

            return studentUnverifiedGradesEntities.Any() ?
                new Tuple<IEnumerable<StudentUnverifiedGrades>, int>(studentUnverifiedGradesEntities, totalCount) :
                new Tuple<IEnumerable<StudentUnverifiedGrades>, int>(new List<StudentUnverifiedGrades>(), 0);
        }

        /// <summary>
        /// Get admission application by Id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Domain.Student.Entities.StudentUnverifiedGrades> GetStudentUnverifiedGradeByGuidAsync(string guid)
        {
            return await GetStudentUnverifiedGradesByIdAsync(await GetStudentUnverifiedGradesIdFromGuidAsync(guid));
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetStudentUnverifiedGradesIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var guidRecord = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", guid);
            if (guidRecord == null)
            {
                throw new KeyNotFoundException("No student-unverified-grades was found for GUID '" + guid + "'.");
            }
            if (guidRecord.LdmGuidEntity != "STUDENT.COURSE.SEC")
            {
                throw new KeyNotFoundException("GUID " + guid + " has different entity, expecting STUDENT.COURSE.SEC");
            }
            return guidRecord.LdmGuidPrimaryKey;
        }

        /// <summary>
        /// Get a single StudentUnverifiedGrades domain entity from an StudentCourseSec id.
        /// </summary>
        /// <param name="id">The StudentCourseSec id</param>
        /// <returns>StudentUnverifiedGrades domain entity object</returns>
        public async Task<StudentUnverifiedGrades> GetStudentUnverifiedGradesByIdAsync(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a StudentCourseSec.");
            }

            var studentCourseSecDataContract = await DataReader.ReadRecordAsync<StudentCourseSec>(id);
            if (studentCourseSecDataContract == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or StudentCourseSec with ID ", id, " is invalid."));
            }

            var studentAcadCredId = studentCourseSecDataContract.ScsStudentAcadCred;
            if (studentAcadCredId == null)
            {
                var errorMessage = string.Format("No STUDENT.ACAD.CRED found for STUDENT.COURSE.SEC with ID : '{0}'.", id);
                throw new ArgumentException(errorMessage);
            }

            StudentAcadCred studentAcadCredDataContract = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredId);
            if (studentAcadCredDataContract == null)
            {
                var errorMessage = string.Format("Could not read STUDENT.ACAD.CRED with ID : '{0}'.", studentAcadCredId);
                throw new ArgumentException(errorMessage);
            }

            StudentUnverifiedGrades studentUnverifiedGrades = null;
            try
            {
                studentUnverifiedGrades = new StudentUnverifiedGrades(studentCourseSecDataContract.RecordGuid, id);                
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
            }

            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            else
            {                
                studentUnverifiedGrades = BuildStudentUnverifiedGrades(studentUnverifiedGrades, studentCourseSecDataContract, studentAcadCredDataContract);
            }
            return studentUnverifiedGrades;

        }

        /// <summary>
        /// Build StudentUnverifiedGrades domain entity from STUDENT.COURSE.SEC and STUDENT.ACAD.CRED
        /// </summary>
        /// <param name="studentUnverifiedGrades"></param>
        /// <param name="studentCourseSecDataContract"></param>
        /// <param name="studentAcadCredDataContract"></param>
        /// <returns></returns>
        private StudentUnverifiedGrades BuildStudentUnverifiedGrades(StudentUnverifiedGrades studentUnverifiedGrades,
            StudentCourseSec studentCourseSecDataContract,
            StudentAcadCred studentAcadCredDataContract)
        {
            studentUnverifiedGrades.StudentId = studentCourseSecDataContract.ScsStudent;
            studentUnverifiedGrades.MidtermGrade1 = studentCourseSecDataContract.ScsMidTermGrade1;
            studentUnverifiedGrades.MidtermGrade2 = studentCourseSecDataContract.ScsMidTermGrade2;
            studentUnverifiedGrades.MidtermGrade3 = studentCourseSecDataContract.ScsMidTermGrade3;
            studentUnverifiedGrades.MidtermGrade4 = studentCourseSecDataContract.ScsMidTermGrade4;
            studentUnverifiedGrades.MidtermGrade5 = studentCourseSecDataContract.ScsMidTermGrade5;
            studentUnverifiedGrades.MidtermGrade6 = studentCourseSecDataContract.ScsMidTermGrade6;
            studentUnverifiedGrades.MidtermGradeDate1 = studentCourseSecDataContract.ScsMidGradeDate1;
            studentUnverifiedGrades.MidtermGradeDate2 = studentCourseSecDataContract.ScsMidGradeDate2;
            studentUnverifiedGrades.MidtermGradeDate3 = studentCourseSecDataContract.ScsMidGradeDate3;
            studentUnverifiedGrades.MidtermGradeDate4 = studentCourseSecDataContract.ScsMidGradeDate4;
            studentUnverifiedGrades.MidtermGradeDate5 = studentCourseSecDataContract.ScsMidGradeDate5;
            studentUnverifiedGrades.MidtermGradeDate6 = studentCourseSecDataContract.ScsMidGradeDate6;
            if ((studentCourseSecDataContract.ScsLastAttendDate.HasValue) && 
                (studentCourseSecDataContract.ScsLastAttendDate != Dmi.Runtime.DmiString.PickDateToDateTime(0)))
                studentUnverifiedGrades.LastAttendDate =  studentCourseSecDataContract.ScsLastAttendDate;
            studentUnverifiedGrades.HasNeverAttended = studentCourseSecDataContract.ScsNeverAttendedFlag.ToUpper() == "Y";

            if (studentAcadCredDataContract != null)
            {
                studentUnverifiedGrades.StudentAcadaCredId = studentAcadCredDataContract.Recordkey;

                studentUnverifiedGrades.GradeScheme = studentAcadCredDataContract.StcGradeScheme;
                studentUnverifiedGrades.IncompleteGradeExtensionDate = studentAcadCredDataContract.StcGradeExpireDate;
                studentUnverifiedGrades.FinalGrade = studentAcadCredDataContract.StcFinalGrade;

                // For final grade date, use verified grade date if it exists, otherwise STUDENT.ACAD.CRED change date.
                if (studentAcadCredDataContract.StcVerifiedGradeDate != null)
                {
                    studentUnverifiedGrades.FinalGradeDate = studentAcadCredDataContract.StcVerifiedGradeDate;
                }
                else
                {
                    studentUnverifiedGrades.FinalGradeDate = studentAcadCredDataContract.StudentAcadCredChgdate;
                }
            }
            return studentUnverifiedGrades;
        }

        /// <summary>
        /// Using a collection of  ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup(filename, p, false)).ToArray();

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
                throw new RepositoryException(string.Format("Error occured while getting guids for {0}.", filename), ex);
            }

            return guidCollection;
        }

        /// <summary>
        ///  CreateStudentUnverifiedGradesSubmissionsAsync
        /// </summary>
        /// <param name="studentUnverifiedGradesEntity"></param>
        /// <returns></returns>
        public Task<StudentUnverifiedGrades> CreateStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGrades studentUnverifiedGradesEntity)
        {
            return UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesEntity);
        }

        /// <summary>
        /// UpdateStudentUnverifiedGradesSubmissionsAsync
        /// </summary>
        /// <param name="request"></param>
        /// <returns>StudentUnverifiedGrades domain object</returns>
        public async Task<StudentUnverifiedGrades> UpdateStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGrades request)
        {
            ImportGrades2Request importGradesRequest = new ImportGrades2Request();
            importGradesRequest.Guid = request.Guid;
            importGradesRequest.SectionRegId = request.StudentAcadaCredId;


            DataContracts.Grades gradeDataContract = null;
            if (!string.IsNullOrEmpty(request.FinalGrade))
            {
                gradeDataContract = await DataReader.ReadRecordAsync<DataContracts.Grades>(request.FinalGrade);
                if (gradeDataContract == null)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Format("Unable to locate final grade: '{0}'", request.FinalGrade))
                    {
                        Id = request.Guid,
                        SourceId = request.StudentCourseSecId
                    });
                }
            }
            else if (!string.IsNullOrEmpty(request.MidtermGrade1))
            {
                gradeDataContract = await DataReader.ReadRecordAsync<DataContracts.Grades>(request.MidtermGrade1);
                if (gradeDataContract == null)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Format("Unable to locate midterm grade 1: '{0}'", request.MidtermGrade1))
                    {
                        Id = request.Guid,
                        SourceId = request.StudentCourseSecId
                    });
                }
            }
            else if (!string.IsNullOrEmpty(request.MidtermGrade2))
            {
                gradeDataContract = await DataReader.ReadRecordAsync<DataContracts.Grades>(request.MidtermGrade2);
                if (gradeDataContract == null)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Format("Unable to locate midterm grade 2: '{0}'", request.MidtermGrade2))
                    {
                        Id = request.Guid,
                        SourceId = request.StudentCourseSecId
                    });
                }
            }
            else if (!string.IsNullOrEmpty(request.MidtermGrade3))
            {
                gradeDataContract = await DataReader.ReadRecordAsync<DataContracts.Grades>(request.MidtermGrade3);
                if (gradeDataContract == null)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Format("Unable to locate midterm grade 3: '{0}'", request.MidtermGrade3))
                    {
                        Id = request.Guid,
                        SourceId = request.StudentCourseSecId
                    });
                }
            }
            else if (!string.IsNullOrEmpty(request.MidtermGrade4))
            {
                gradeDataContract = await DataReader.ReadRecordAsync<DataContracts.Grades>(request.MidtermGrade4);
                if (gradeDataContract == null)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Format("Unable to locate midterm grade 4: '{0}'", request.MidtermGrade4))
                    {
                        Id = request.Guid,
                        SourceId = request.StudentCourseSecId
                    });
                }
            }
            else if (!string.IsNullOrEmpty(request.MidtermGrade5))
            {
                gradeDataContract = await DataReader.ReadRecordAsync<DataContracts.Grades>(request.MidtermGrade5);
                if (gradeDataContract == null)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Format("Unable to locate midterm grade 5: '{0}'", request.MidtermGrade5))
                    {
                        Id = request.Guid,
                        SourceId = request.StudentCourseSecId
                    });
                }
            }
            else if (!string.IsNullOrEmpty(request.MidtermGrade6))
            {
                gradeDataContract = await DataReader.ReadRecordAsync<DataContracts.Grades>(request.MidtermGrade6);
                if (gradeDataContract == null)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Format("Unable to locate midterm grade 6: '{0}'", request.MidtermGrade6))
                    {
                        Id = request.Guid,
                        SourceId = request.StudentCourseSecId
                    });
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            importGradesRequest.Grade = request.GradeId;
            importGradesRequest.GradeType = request.GradeType;
            importGradesRequest.Grade = gradeDataContract != null ? gradeDataContract.GrdGrade : string.Empty;
            importGradesRequest.NeverAttend = request.HasNeverAttended ? "Y" : "N";
            importGradesRequest.LastDayAttendDate = request.LastAttendDate.HasValue
                        ? request.LastAttendDate.Value.ToString("yyyy/MM/dd")
                        : string.Empty;
            importGradesRequest.GradeExpiry = request.FinalGradeDate.HasValue ?
                        request.FinalGradeDate.Value.ToString("yyyy/MM/dd")
                         : string.Empty;
            
            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                importGradesRequest.ExtendedNames = extendedDataTuple.Item1;
                importGradesRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            var importGradesResponse = await transactionInvoker.ExecuteAsync<ImportGrades2Request, ImportGrades2Response>(importGradesRequest);

            // If there is any error message - throw an exception
            if (importGradesResponse.GradeMessages2 != null && importGradesResponse.GradeMessages2.Any())
            {
                var errorMessage = string.Format("Error occurred updating grade for Student: '{0}' and AcadCredId: '{1}': ", request.StudentId, request.StudentAcadaCredId);
                logger.Error(errorMessage);
                var repositoryException = new RepositoryException();
                foreach (var message in importGradesResponse.GradeMessages2)
                { 
                    //collect all the failure messages
                    if (message != null && !string.IsNullOrEmpty(message.ErrorMessge))
                    {
                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(string.IsNullOrEmpty(message.StatusCode) ? "" : message.StatusCode + ": ", message.ErrorMessge)) 
                        { 
                            Id = importGradesResponse.Guid, 
                            SourceId = request.StudentCourseSecId 
                        });
                    }
                }
                throw repositoryException;
            }
        
            return await GetStudentUnverifiedGradesByIdAsync(await GetStudentUnverifiedGradesIdFromGuidAsync(importGradesResponse.Guid));
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetStudentAcademicCredIdFromGuidAsync(string guid)
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
            if (guidRecord.LdmGuidEntity != "STUDENT.ACAD.CRED")
            {
                throw new KeyNotFoundException("GUID " + guid + " has different entity, than expected, STUDENT.ACAD.CRED");
            }
            return guidRecord.LdmGuidPrimaryKey;
        }

        /// <summary>
        /// Get the GUID for a StudentAcadCred using its ID
        /// </summary>
        /// <param name="id">StudentAcadCred ID</param>
        /// <returns>StudentAcadCred GUID</returns>
        public async Task<string> GetStudentAcadCredGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("STUDENT.ACAD.CRED", id);
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Guid.NotFound", "GUID not found for STUDENT.ACAD.CRED id: " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Get the grade scheme for a StudentAcadCred using its ID
        /// </summary>
        /// <param name="id">StudentAcadCred ID</param>
        /// <returns>grade scheme code</returns>
        public async Task<string> GetStudentAcadCredGradeSchemeFromIdAsync(string id)
        {
            var retVal = string.Empty;
            var data = await DataReader.BatchReadRecordColumnsAsync("STUDENT.ACAD.CRED", new string[] { id }, new string[] { "STC.GRADE.SCHEME" });
            if (data != null && data.Any())
            {
                var dict = data.FirstOrDefault().Value;
                if (dict != null && dict.Any())
                {
                    return dict.FirstOrDefault().Value;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Get the grade scheme for a StudentAcadCred using its ID
        /// </summary>
        /// <param name="id">StudentAcadCred ID</param>
        /// <returns>grade scheme code</returns>
        public async Task<Tuple<string, string, string>> GetStudentAcadCredDataFromIdAsync(string id)
        {
            var retVal = new Tuple<string, string, string>(string.Empty, string.Empty, string.Empty);
            var stcPersonId = string.Empty;
            var stcStudentCourseSec = string.Empty;
            var stcGradeScheme = string.Empty;

            try
            {
                var data = await DataReader.BatchReadRecordColumnsAsync("STUDENT.ACAD.CRED", new string[] { id }, new string[] { "STC.PERSON.ID", "STC.STUDENT.COURSE.SEC", "STC.GRADE.SCHEME" });
                if (data != null && data.Any())
                {
                    var dataValues = data.FirstOrDefault().Value;
                    foreach (var studentAcadCred in dataValues)
                    {
                        if (studentAcadCred.Key == "STC.PERSON.ID")
                            stcPersonId = studentAcadCred.Value;
                        if (studentAcadCred.Key == "STC.STUDENT.COURSE.SEC")
                            stcStudentCourseSec = studentAcadCred.Value;
                        if (studentAcadCred.Key == "STC.GRADE.SCHEME")
                            stcGradeScheme = studentAcadCred.Value;
                    }
                }
                return new Tuple<string, string, string>(stcPersonId, stcStudentCourseSec, stcGradeScheme);
            }
            catch (Exception)
            {
                throw new RepositoryException(string.Format("Unexpected error encountered when trying to read STUDENT.ACAD.CRED '{0}'.", id));
            }    
        }

    }
}