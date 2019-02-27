﻿/*Copyright 2018 Ellucian Company L.P. and its affiliates. */

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
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentTranscriptGradesRepository : BaseColleagueRepository, IStudentTranscriptGradesRepository, IEthosExtended
    {
        private readonly int _readSize;
        private readonly string academicCredentialCriteria = "WITH STC.VERIFIED.GRADE NE '' AND STC.STUDENT.EQUIV.EVAL EQ ''";
        private RepositoryException exception;


        public StudentTranscriptGradesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
            exception = new RepositoryException();
        }

        /// <summary>
        ///  Get all StudentTranscriptGrades
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="offset"></param>
        /// <returns>Collection of StudentTranscriptGrades domain entities</returns>
        public async Task<Tuple<IEnumerable<StudentTranscriptGrades>, int>> GetStudentTranscriptGradesAsync(int offset, int limit, string student = "", bool bypassCache = false)
        {
            string[] limitingKeys = null;
            if (!string.IsNullOrEmpty(student))
            {
                // student filter only
                var personStDataContract = await DataReader.ReadRecordAsync<Base.DataContracts.PersonSt>("PERSON.ST", student);
                if (personStDataContract == null)
                {
                    return new Tuple<IEnumerable<StudentTranscriptGrades>, int>(null, 0);
                }
                limitingKeys = personStDataContract.PstStudentAcadCred.ToArray();
            }

            var studentAcadCredIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, academicCredentialCriteria);
            var totalCount = studentAcadCredIds.Count();
            Array.Sort(studentAcadCredIds);
            var subList = studentAcadCredIds.Skip(offset).Take(limit).ToArray();
            var studentAcademicCreditData = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", subList);

            var studentAcademicCreditEntities = await BuildStudentTranscriptGradesAsync(subList, studentAcademicCreditData);
            return new Tuple<IEnumerable<StudentTranscriptGrades>, int>(studentAcademicCreditEntities, totalCount);
        }

        /// <summary>
        /// Get a single StudentTranscriptGrades domain entity from a guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>StudentTranscriptGrades domain entity</returns>
        public async Task<StudentTranscriptGrades> GetStudentTranscriptGradesByGuidAsync(string guid)
        {
            return await GetStudentTranscriptGradesByIdAsync(await GetStudentTranscriptGradesIdFromGuidAsync(guid));
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetStudentTranscriptGradesIdFromGuidAsync(string guid)
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
                throw new KeyNotFoundException("GUID " + guid + " has different entity, than expected, STUDENT.ACAD.CRED with a secondary field equal to STC.INTG.KEY.IDX");
            }
            return guidRecord.LdmGuidPrimaryKey;
        }

        /// <summary>
        /// Get a single StudentTranscriptGrades domain entity from an StudentAcadCred id.
        /// </summary>
        /// <param name="id">The StudentAcadCred id</param>
        /// <returns>StudentTranscriptGrades domain entity object</returns>
        public async Task<StudentTranscriptGrades> GetStudentTranscriptGradesByIdAsync(string id)
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

            var stwebDefaults = await GetStwebDefaultsAsync();
            if (stwebDefaults == null)
            {
                throw new Exception("Unable to access STWEB.DEFAULTS values");

            }
            var stwebTranAltcumFlag = !string.IsNullOrEmpty(stwebDefaults.StwebTranAltcumFlag) && stwebDefaults.StwebTranAltcumFlag == "Y" ? true : false;

            Dictionary<string, string> scsCourseSectionDict = null;
            var studentCourseSec = await DataReader.SelectAsync("STUDENT.COURSE.SEC", new string[] { studentAcadCred.StcStudentCourseSec }, "");
            var records = await DataReader.BulkReadRecordAsync<StudentCourseSec>(studentCourseSec);
            if (records != null && records.Any())
            {
                scsCourseSectionDict =
                    records.Where(scs => scs.ScsCourseSection != null)
                        .ToDictionary(scs => scs.Recordkey.ToString(),
                                     scs => scs.ScsCourseSection.ToString());
            }

            var criteria = "WITH HL.RECORD.ID EQ '" + id + "'";
            var studentAcadCredHistLogRecords = await DataReader.BulkReadRecordAsync<StcHistLog>("STUDENT.ACAD.CRED.HIST.LOG", criteria);
            Collection<Hist> studentAcadCredHistRecords = null;
            if ((studentAcadCredHistLogRecords != null) && (studentAcadCredHistLogRecords.Any()))
            {
                var histIdCollection = studentAcadCredHistLogRecords.SelectMany(hl => hl.StchlHist).ToArray();
                // use @ID since this is a multi-part key and HIST.ID is not defined properly
                var histCriteria = "WITH @ID EQ '" + (string.Join(" ", histIdCollection.ToArray())).Replace(" ", "' '") + "' AND WITH HIST.FIELD.NAME EQ 'STC.VERIFIED.GRADE'";
                studentAcadCredHistRecords = await DataReader.BulkReadRecordAsync<Hist>("STUDENT.ACAD.CRED.HIST", histCriteria);
            }

            return await BuildStudentTranscriptGradeAsync(studentAcadCred, scsCourseSectionDict, studentAcadCredHistLogRecords, studentAcadCredHistRecords, stwebTranAltcumFlag);
        }

        public async Task<StudentTranscriptGrades> UpdateStudentTranscriptGradesAdjustmentsAsync(StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustment)
        {
            var id = studentTranscriptGradesAdjustment.Id;
            var guid = studentTranscriptGradesAdjustment.Guid;

            var updateRequest = new UpdateTranscriptGradeRequest();
            updateRequest.StudentAcadCredId = id;
            updateRequest.VerifiedGrade = studentTranscriptGradesAdjustment.VerifiedGrade;
            updateRequest.IncompleteGrade = studentTranscriptGradesAdjustment.IncompleteGrade;
            updateRequest.ExtensionDate = studentTranscriptGradesAdjustment.ExtensionDate;
            updateRequest.ChangeReason = studentTranscriptGradesAdjustment.ChangeReason;
           
            // Get Extended Data names and values
            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // Submit the registration
            var updateResponse = await transactionInvoker.ExecuteAsync<UpdateTranscriptGradeRequest, UpdateTranscriptGradeResponse>(updateRequest);

            // If there is any error message - throw an exception 
            if (updateResponse.UpdateTranscriptGradeErrors != null && updateResponse.UpdateTranscriptGradeErrors.Count > 0)
            {
                // Register repository errors and throw an exception
                //var exception = new RepositoryException(string.Format("Errors encountered while updating student-transcript-grades-adjustments '{0}'.", guid));
                exception.AddErrors(updateResponse.UpdateTranscriptGradeErrors.ConvertAll(x => (new RepositoryError(x.ErrorCodes, x.ErrorMessages))));
                throw exception;
            }

            return await GetStudentTranscriptGradesByIdAsync(id);
        }


        private async Task<IEnumerable<StudentTranscriptGrades>> BuildStudentTranscriptGradesAsync(string[] studentAcadCredIds, Collection<StudentAcadCred> sources)
        {
            var studentTranscriptGradesCollection = new List<StudentTranscriptGrades>();

            var stwebDefaults = await GetStwebDefaultsAsync();
            if (stwebDefaults == null)
            {
                throw new Exception("Unable to access STWEB.DEFAULTS values");

            }
            var stwebTranAltcumFlag = !string.IsNullOrEmpty(stwebDefaults.StwebTranAltcumFlag) && stwebDefaults.StwebTranAltcumFlag == "Y" ? true : false;


            Dictionary<string, string> scsCourseSectionDict = null;
            var studentCourseSecIds = sources.Where(cs => cs.StcStudentCourseSec != null)
                      .Select(cs => cs.StcStudentCourseSec).Distinct().ToArray();
            var studentCourseSec = await DataReader.SelectAsync("STUDENT.COURSE.SEC", studentCourseSecIds, "");
            var records = await DataReader.BulkReadRecordAsync<StudentCourseSec>(studentCourseSec);
            if (records != null && records.Any())
            {
                scsCourseSectionDict =
                    records.Where(scs => scs.ScsCourseSection != null)
                        .ToDictionary(scs => scs.Recordkey.ToString(),
                                     scs => scs.ScsCourseSection.ToString());
            }

            var criteria = "WITH HL.RECORD.ID EQ '" + (string.Join(" ", studentAcadCredIds.ToArray())).Replace(" ", "' '") + "'";
            var studentAcadCredHistLogRecords = await DataReader.BulkReadRecordAsync<StcHistLog>("STUDENT.ACAD.CRED.HIST.LOG", criteria);

            Collection<Hist> studentAcadCredHistRecords = null;
            if ((studentAcadCredHistLogRecords != null) && (studentAcadCredHistLogRecords.Any()))
            {
                var histIdCollection = studentAcadCredHistLogRecords.SelectMany(hl => hl.StchlHist).ToArray();
                // use @ID since this is a multi-part key and HIST.ID is not defined properly
                var histCriteria = "WITH @ID EQ '" + (string.Join(" ", histIdCollection.ToArray())).Replace(" ", "' '") + "' AND WITH HIST.FIELD.NAME EQ 'STC.VERIFIED.GRADE'";
                studentAcadCredHistRecords = await DataReader.BulkReadRecordAsync<Hist>("STUDENT.ACAD.CRED.HIST", histCriteria);
            }

            if (studentAcadCredIds != null && studentAcadCredIds.Any())
            {
                foreach (var source in sources)
                {
                    studentTranscriptGradesCollection.Add(await BuildStudentTranscriptGradeAsync(source, scsCourseSectionDict, studentAcadCredHistLogRecords, studentAcadCredHistRecords, stwebTranAltcumFlag));// academicCredentialsGuid));
                }
            }

            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return studentTranscriptGradesCollection.AsEnumerable();
        }

        private async Task<StudentTranscriptGrades> BuildStudentTranscriptGradeAsync(StudentAcadCred source, Dictionary<string, string> scsCourseSectionDict,
            Collection<StcHistLog> studentAcadCredHistLogCollection, Collection<Hist> studentAcadCredHistCollection, bool stwebTranAltcumFlag = false)
        {
            StudentTranscriptGrades studentTranscriptGrade = null;
            if (source == null)
            {
                //throw new ArgumentNullException("source", "source required to build StudentAcademicCredit.");
                exception.AddError(new RepositoryError("BuildStudentTranscriptGrade", "Source required to build student transcript grade."));
                return studentTranscriptGrade;
            }
            
            string guidFromRecordInfo = string.Empty;

            try {
                guidFromRecordInfo = await GetGuidFromRecordInfoAsync("STUDENT.ACAD.CRED", source.Recordkey, "STC.INTG.KEY.IDX", source.StcIntgKeyIdx);

                studentTranscriptGrade = new StudentTranscriptGrades(source.Recordkey, guidFromRecordInfo)
                {
                    StudentId = source.StcPersonId,
                    StudentCourseSectionId = source.StcStudentCourseSec,
                    Course = source.StcCourse,
                    CourseName = source.StcCourseName,
                    Title = source.StcTitle,
                    FinalGradeExpirationDate = source.StcGradeExpireDate,
                    GradeSchemeCode = source.StcGradeScheme,
                    CreditType = source.StcCredType,
                    AttemptedCredit = source.StcAttCred,
                    CompletedCredit = source.StcCmplCred,
                    AttemptedCeus = source.StcAttCeus,
                    CompletedCeus = source.StcCmplCeus,
                    GradePoints = source.StcGradePts,

                    AltcumContribGradePts = source.StcAltcumContribGradePts,
                    AltcumContribGpaCredits = source.StcAltcumContribGpaCred,
                    AltcumContribCmplCredits = source.StcAltcumContribCmplCred,

                    ContribGradePoints = source.StcCumContribGradePts,
                    ContribGpaCredits = source.StcCumContribGpaCred,
                    ContribCmplCredits = source.StcCumContribCmplCred,

                    ReplCode = source.StcReplCode,
                    RepeatAcademicCreditIds = source.StcRepeatedAcadCred,
                    VerifiedGradeDate = source.StcVerifiedGradeDate,
                    VerifiedGrade = source.StcVerifiedGrade,
                    StwebTranAltcumFlag = stwebTranAltcumFlag

                };

                if (!string.IsNullOrEmpty(source.StcStudentCourseSec))
                {
                    var courseSectionId = string.Empty;
                    scsCourseSectionDict.TryGetValue(source.StcStudentCourseSec, out courseSectionId);
                    if (string.IsNullOrEmpty(courseSectionId))
                    {
                        throw new KeyNotFoundException(string.Format("Unable to locate id for course section {0}", (source.StcStudentCourseSec)));
                    }
                    studentTranscriptGrade.CourseSection = courseSectionId;
                }


                studentTranscriptGrade.StudentTranscriptGradesHistory = GetStudentAcadCredHistoryFields(source.Recordkey, guidFromRecordInfo, studentAcadCredHistLogCollection, studentAcadCredHistCollection, source.StcIntgHistEntityAssociation);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError(guidFromRecordInfo, source.Recordkey, "BuildStudentTranscriptGrade", ex.Message));
               
            }

            return studentTranscriptGrade;
        }

        /// <summary>
        /// Get all StudentAcadCredHistory fields needed to build Collection of StudentTranscriptGradesHistory domain entity
        /// </summary>
        /// <param name="recordKey"></param>
        /// <param name="guid"></param>
        /// <param name="studentAcadCredHistLogCollection"></param>
        /// <param name="studentAcadCredHistCollection"></param>
        /// <param name="stcIntgHistEntityAssociation"></param>
        /// <returns>Collection of StudentTranscriptGradesHistory containing 1) changeReason 2) studentAcadCredHist record, 3)gradeChangeDate</returns>
        private List<StudentTranscriptGradesHistory> GetStudentAcadCredHistoryFields(string recordKey, string guid,
          Collection<StcHistLog> studentAcadCredHistLogCollection, Collection<Hist> studentAcadCredHistCollection,
          List<StudentAcadCredStcIntgHist> stcIntgHistEntityAssociation)
        {
            var studentTranscriptGradesHistories = new List<StudentTranscriptGradesHistory>();

            // If there are no HIST.LOG records, then return
            if ((studentAcadCredHistLogCollection == null) || (!studentAcadCredHistLogCollection.Any()))
            {
                return studentTranscriptGradesHistories;
            }

            // Get all STUDENT.ACAD.CRED.HIST.LOG records, associated with this academicCredentialID, from the STUDENT.ACAD.CRED.HIST.LOG collection
            var studentAcadCredHistLogRecords = studentAcadCredHistLogCollection
                .Where(x => x.StchlRecordId == recordKey)
                .OrderByDescending(s => s.StchlDate)
                .ThenByDescending(s => s.StchlTime);

            // If there are no related HIST.LOG records, then return
            if ((studentAcadCredHistLogRecords == null) || (!studentAcadCredHistLogRecords.Any()))
            {
                return studentTranscriptGradesHistories;
            }

            // if we have HIST.LOG records but there is no HIST collection, then throw an error
            if ((studentAcadCredHistCollection == null) || (!studentAcadCredHistCollection.Any()))
            {
                //throw new KeyNotFoundException(string.Format("Unable to locate STUDENT.ACAD.CRED.HIST record(s) for studentAcadCred: {0}.", recordKey));
                exception.AddError(new RepositoryError(guid, recordKey, "BuildStudentTranscriptGrade", string.Format("Unable to locate STUDENT.ACAD.CRED.HIST record(s) for studentAcadCred: '{0}'.", recordKey)));
            }

            // get a collecton of  HIST.LOG.ID that has been sorted by date 
            var studentAcadCredHistLogIds = studentAcadCredHistLogRecords
                .Select(log => log.Recordkey).ToArray();

            // Get all STUDENT.ACAD.CRED.HIST records, associated with the sublist of HIST.LOG.IDs, from the 
            // STUDENT.ACAD.CRED.HIST collection where STC.OLD.VALUES is not null.
            // The following OrderBy clause sorts the studentAcadCredHistRecord using the ordered keys from 
            // studentAcadCredHistLogRecords
            var studentAcadCredHists = studentAcadCredHistCollection
                .Where(x => studentAcadCredHistLogIds.Contains(x.HistLog) && !string.IsNullOrEmpty(x.HistOldValues))
                .OrderBy(k => studentAcadCredHistLogIds.ToList().IndexOf(k.HistLog));

            // There may not be any HIST records that qualify since we dont care about records with a NULL HIST.OLD.VALUE
            if ((studentAcadCredHists == null) || (!studentAcadCredHists.Any()))
            {
                return studentTranscriptGradesHistories;
            }

            foreach (var studentAcadCredHistory in studentAcadCredHists)
            {
                var changeReason = string.Empty;
                DateTime? gradeChangeDate = null;
                
                //Now that we know which HIST record meet the criteria, look back at the associated LOG record for the date
                var studentAcadCredHistLog = studentAcadCredHistLogRecords
                    .FirstOrDefault(log => log.Recordkey == studentAcadCredHistory.HistLog);
                if (studentAcadCredHistLog == null)
                {
                    //throw new KeyNotFoundException(string.Format("Unable to locate STUDENT.ACAD.CRED.HIST.LOG record for studentAcadCred: {0}.", recordKey));
                    exception.AddError(new RepositoryError(guid, recordKey, "BuildStudentTranscriptGrade", string.Format("Unable to locate STUDENT.ACAD.CRED.HIST.LOG record for studentAcadCred: '{0}'.", recordKey)));
                }

                if ((studentAcadCredHistLog.StchlDate.HasValue) && (studentAcadCredHistLog.StchlTime.HasValue))
                {
                    var date = studentAcadCredHistLog.StchlDate.Value;
                    var time = studentAcadCredHistLog.StchlTime.Value;
                    gradeChangeDate = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
                }
                if ((stcIntgHistEntityAssociation != null) && (stcIntgHistEntityAssociation.Any()))
                {
                    var stcIntgHistEntityAssociationRecord = stcIntgHistEntityAssociation.FirstOrDefault(c => c.StcIntgHistLogIdxAssocMember == studentAcadCredHistory.HistLog);
                    if (stcIntgHistEntityAssociationRecord != null)
                    {
                        changeReason = stcIntgHistEntityAssociationRecord.StcIntgHistLogReasonAssocMember;
                    }
                }

                var studentTranscriptGradesHistory = new StudentTranscriptGradesHistory(changeReason, studentAcadCredHistory.HistOldValues, gradeChangeDate);
                if (!studentTranscriptGradesHistories.Contains(studentTranscriptGradesHistory))
                    studentTranscriptGradesHistories.Add(studentTranscriptGradesHistory);
            }
            return studentTranscriptGradesHistories;
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
                throw new Exception(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
        }

        private async Task<Data.Student.DataContracts.StwebDefaults> GetStwebDefaultsAsync()
        {
            Data.Student.DataContracts.StwebDefaults studentWebDefaults = await GetOrAddToCacheAsync<Data.Student.DataContracts.StwebDefaults>("StudentWebDefaults",
            async () =>
            {
                Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false);
                if (stwebDefaults == null)
                {
                    var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                    logger.Info(errorMessage);
                    stwebDefaults = new StwebDefaults();
                }
                return stwebDefaults;
            }, Level1CacheTimeoutValue);
            return studentWebDefaults;
        }

    }
}