// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Services;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentStandingRepository : BaseColleagueRepository, IStudentStandingRepository
    {
        const string AllStudentStandingsRecordsCache = "AllStudentStandingsRecordKeys";
        const int AllStudentStandingsRecordsCacheTimeout = 20;
        private RepositoryException exception = new RepositoryException();

        public StudentStandingRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get Student Standings for multiple students
        /// </summary>
        /// <param name="studentIds">List of student Ids to get Student Standings for</param>
        /// <param name="term">Term to filter student standings</param>
        /// <returns>Returns a list of Student Standing Entities</returns>
        public async Task<IEnumerable<StudentStanding>> GetAsync(IEnumerable<string> studentIds, string term = null)
        {
            List<StudentStanding> studentStandings = new List<StudentStanding>();
            bool error = false;

            if (studentIds != null && studentIds.Count() > 0)
            {
                // get students data
                string[] studentStandingIds = await DataReader.SelectAsync("STUDENT.STANDINGS", " WITH STS.STUDENT = '?'", studentIds.ToArray());
                Collection<StudentStandings> studentStandingData = await DataReader.BulkReadRecordAsync<StudentStandings>("STUDENT.STANDINGS", studentStandingIds);
                if (studentStandingData == null)
                    logger.Debug(string.Format("No student standing data returned for students {0}", string.Join(",", studentIds)));
                foreach (var studentStanding in studentStandingData)
                {
                    if (studentStanding != null)
                    {
                        try
                        {
                            var studentStandingEntity = BuildStandings(studentStanding, term);
                            if (studentStandingEntity != null)
                            {
                                studentStandings.Add(studentStandingEntity);
                            }
                        }
                        catch (Ellucian.Data.Colleague.Exceptions.ColleagueSessionExpiredException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Failed to build student standing {0} for student {1}", studentStanding.Recordkey, studentStanding.StsStudent));
                            logger.Error(e.GetBaseException().Message);
                            logger.Error(e.GetBaseException().StackTrace);
                            error = true;
                        }
                    }
                }
            }
            if (error && studentStandings.Count() == 0)
                throw new ColleagueWebApiException("Unexpected errors occurred. No student standings records returned. Check API error log.");

            return studentStandings;
        }

        /// <summary>
        /// Build the Student Standings Entity from the Data Contract
        /// </summary>
        /// <param name="studentStanding">Data Contract object for Student Standings</param>
        /// <param name="term">Term to filter student standings</param>
        /// <returns>A single Entity object for Student Standings</returns>
        private StudentStanding BuildStandings(StudentStandings studentStanding, string term = null)
        {
            // For now we are only returning book price information, but will eventually need book details.
            StudentStanding studentStandingEntity = null;
            if (studentStanding != null)
            {
                try
                {
                    studentStandingEntity = new StudentStanding(studentStanding.Recordkey, studentStanding.StsStudent, studentStanding.StsAcadStanding, studentStanding.StsAcadStandingDate.GetValueOrDefault());
                    studentStandingEntity.Level = studentStanding.StsAcadLevel;
                    studentStandingEntity.Program = studentStanding.StsAcadProgram;
                    studentStandingEntity.Term = studentStanding.StsTerm;
                    studentStandingEntity.CalcStandingCode = studentStanding.StsCalcAcadStanding;
                    studentStandingEntity.OverrideReason = studentStanding.StsOverrideReason;

                    switch (studentStanding.StsType)
                    {
                        case "ACLV":
                            {
                                studentStandingEntity.Type = StudentStandingType.AcademicLevel;
                                break;
                            }
                        case "ACPG":
                            {
                                studentStandingEntity.Type = StudentStandingType.Program;
                                break;
                            }
                        case "TERM":
                            {
                                studentStandingEntity.Type = StudentStandingType.Term;
                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException("Unexpected condition encountered in Student Standing of Type " + studentStanding.StsType);
                            }
                    }
                }
                catch (Exception ex)
                {
                    LogDataError("Student Standings", studentStanding.Recordkey, studentStanding, ex);
                    //throw;
                    return null;
                }
                if (term != null)
                {
                    if (term == studentStandingEntity.Term)
                    {
                        return studentStandingEntity;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return studentStandingEntity;
                }
            }
            return studentStandingEntity;
        }

        public async Task<Dictionary<string, List<StudentStanding>>> GetGroupedAsync(IEnumerable<string> studentIds)
        {
            Dictionary<string, List<StudentStanding>> standingsByStudent = new Dictionary<string, List<StudentStanding>>();

            bool error = false;

            if (studentIds != null && studentIds.Count() > 0)
            {
                // get students data
                string[] studentStandingIds = await DataReader.SelectAsync("STUDENT.STANDINGS", " WITH STS.STUDENT = '?'", studentIds.ToArray());
                Collection<StudentStandings> studentStandingData = await DataReader.BulkReadRecordAsync<StudentStandings>("STUDENT.STANDINGS", studentStandingIds);
                if (studentStandingData == null)
                    logger.Debug(string.Format("No student standing data returned for students {0}", string.Join(",", studentIds)));

                foreach (var studentStanding in studentStandingData)
                {
                    if (studentStanding != null)
                    {
                        try
                        {
                            var studentStandingEntity = BuildStandings(studentStanding);
                            if (studentStandingEntity != null)
                            {
                                if (!standingsByStudent.ContainsKey(studentStandingEntity.StudentId))
                                    standingsByStudent[studentStandingEntity.StudentId] = new List<StudentStanding>();
                                standingsByStudent[studentStandingEntity.StudentId].Add(studentStandingEntity);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Failed to build student standing {0} for student {1}", studentStanding.Recordkey, studentStanding.StsStudent));
                            logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
                            error = true;
                        }
                    }
                }
            }
            if (error && standingsByStudent.Count() == 0)
                throw new ColleagueWebApiException("Errors prevented partial return of student standings batch.");

            return standingsByStudent;
        }

        /// <summary>
        ///  Get collection of StudentStandings
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <returns>collection of StudentStandings</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentStanding>, int>> GetStudentStandingsAsync(int offset, int limit)
        {
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllStudentStandingsRecordsCache);

            int totalCount = 0;
            var selectionCriteria = new StringBuilder();

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                "STUDENT.STANDINGS",
                offset,
                limit,
                AllStudentStandingsRecordsCacheTimeout,
                async () =>
                {
                    selectionCriteria.Append("WITH STS.STUDENT NE '' AND WITH STS.TYPE NE '' AND WITH STS.ACAD.STANDING NE ''");

                    return new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = selectionCriteria.ToString()
                    };
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<StudentStanding>, int>(new List<StudentStanding>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            var subList = keyCacheObject.Sublist.ToArray();

            if (subList == null || !subList.Any())
            {
                return new Tuple<IEnumerable<StudentStanding>, int>(new List<StudentStanding>(), 0);
            }

            var studentStandingData = await DataReader.BulkReadRecordAsync<DataContracts.StudentStandings>("STUDENT.STANDINGS", subList);

            if (studentStandingData == null)
            {
                return new Tuple<IEnumerable<StudentStanding>, int>(new List<StudentStanding>(), 0);
            }

            var studentStandings = BuildStudentStandings(studentStandingData);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<StudentStanding>, int>(studentStandings, totalCount);

        }

        /// <summary>
        /// Get StudentStanding domain entity By Guid
        /// </summary>
        /// <param name="guid">Unique identifier</param>
        /// <returns>StudentStanding domain entity</returns>
        public async Task<StudentStanding> GetStudentStandingByGuidAsync(string guid)
        {
            try
            {
                var id = await GetStudentStandingIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for StudentStanding guid:", guid));
                }
                return await GetStudentStandingAsync(id);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("StudentStanding GUID " + guid + " lookup failed.");
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("StudentStanding GUID " + guid + " lookup failed.");
            }
        }

        /// <summary>
        /// Get a single StudentStanding domain entity using an ID
        /// </summary>
        /// <param name="id">The StudentStanding ID</param>
        /// <returns>The StudentStanding domain entity</returns>
        private async Task<StudentStanding> GetStudentStandingAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a StudentStanding.");
            }

            // Now we have an ID, so we can read the record
            var studentStanding = await DataReader.ReadRecordAsync<StudentStandings>(id);
            if (studentStanding == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found for StudentStanding with ID ", id, "invalid."));
            }

            // for Ethos APIs student-academic-standings we don't expose the date
            // but the Entity StudentStandings requires the date or it will not
            // build the entity recrd.  Just set the date to today's date so that
            // we get the entity result back from the BuildStandings method (shared).
            // If we ever decide to expose the date, then missing dates will have
            // to produce an error/exception or be excluded from the GET all selection.
            var originalDate = studentStanding.StsAcadStandingDate;
            if (originalDate == null || !originalDate.HasValue)
            {
                studentStanding.StsAcadStandingDate = DateTime.Now;
            }
            var result = BuildStandings(studentStanding); 
            
            if (result == null || string.IsNullOrEmpty(studentStanding.RecordGuid))
            {
                var message = "Invalid STUDENT.STANDINGS record.";
                if (studentStanding.StsType != "ACLV" && studentStanding.StsType != "ACPG" && studentStanding.StsType != "TERM")
                {

                    exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STS.TYPE of '{1}' is invalid.", message, studentStanding.StsType))
                    {
                        Id = studentStanding.RecordGuid,
                        SourceId = studentStanding.Recordkey
                    });
                }
                if (string.IsNullOrEmpty(studentStanding.RecordGuid))
                {
                    exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The GUID is missing.", message))
                    {
                        Id = studentStanding.RecordGuid,
                        SourceId = studentStanding.Recordkey
                    });
                }
                if (string.IsNullOrEmpty(studentStanding.StsStudent))
                {
                    exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STS.STUDENT of '{1}' is invalid.", message, studentStanding.StsStudent))
                    {
                        Id = studentStanding.RecordGuid,
                        SourceId = studentStanding.Recordkey
                    });
                }
                if (string.IsNullOrEmpty(studentStanding.StsAcadStanding))
                {
                    exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STS.ACAD.STANDING of '{1}' is invalid.", message, studentStanding.StsAcadStanding))
                    {
                        Id = studentStanding.RecordGuid,
                        SourceId = studentStanding.Recordkey
                    });
                }
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            result.Guid = studentStanding.RecordGuid;
            return result;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<string> GetStudentStandingIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("StudentStanding GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("StudentStanding GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STUDENT.STANDINGS")
            {
                exception.AddError(new RepositoryError("GUID.Wrong.Type", "GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, STUDENT.STANDINGS")
                {
                    Id = guid
                });
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        ///  Build collection of StudentStanding domain entities from collection of StudentStandings data contacts
        /// </summary>
        /// <param name="studentStandings">Collection of StudentStandings data contracts</param>
        /// <returns>Collection of StudentStanding domain entity</returns>
        private IEnumerable<StudentStanding> BuildStudentStandings(IEnumerable<StudentStandings> studentStandings)
        {
            var studentStandingsCollection = new List<StudentStanding>();

            foreach (var studentStanding in studentStandings)
            {
                // for Ethos APIs student-academic-standings we don't expose the date
                // but the Entity StudentStandings requires the date or it will not
                // build the entity recrd.  Just set the date to today's date so that
                // we get the entity result back from the BuildStandings method (shared).
                // If we ever decide to expose the date, then missing dates will have
                // to produce an error/exception or be excluded from the GET all selection.
                var originalDate = studentStanding.StsAcadStandingDate;
                if (originalDate == null || !originalDate.HasValue)
                {
                    studentStanding.StsAcadStandingDate = DateTime.Now;
                }
                var result = BuildStandings(studentStanding);
                if (result == null || string.IsNullOrEmpty(studentStanding.RecordGuid))
                {
                    var message = "Invalid STUDENT.STANDINGS record.";
                    if (studentStanding.StsType != "ACLV" && studentStanding.StsType != "ACPG" && studentStanding.StsType != "TERM")
                    {

                        exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STS.TYPE of '{1}' is invalid.", message, studentStanding.StsType))
                        {
                            Id = studentStanding.RecordGuid,
                            SourceId = studentStanding.Recordkey
                        });
                    }
                    if (string.IsNullOrEmpty(studentStanding.RecordGuid))
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The GUID is missing.", message))
                        {
                            Id = studentStanding.RecordGuid,
                            SourceId = studentStanding.Recordkey
                        });
                    }
                    if (string.IsNullOrEmpty(studentStanding.StsStudent))
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STS.STUDENT of '{1}' is invalid.", message, studentStanding.StsStudent))
                        {
                            Id = studentStanding.RecordGuid,
                            SourceId = studentStanding.Recordkey
                        });
                    }
                    if (string.IsNullOrEmpty(studentStanding.StsAcadStanding))
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STS.ACAD.STANDING of '{1}' is invalid.", message, studentStanding.StsAcadStanding))
                        {
                            Id = studentStanding.RecordGuid,
                            SourceId = studentStanding.Recordkey
                        });
                    }
                }
                else
                {
                    result.Guid = studentStanding.RecordGuid;
                    studentStandingsCollection.Add(result);
                }
            }

            return studentStandingsCollection.AsEnumerable();
        }
    }
}