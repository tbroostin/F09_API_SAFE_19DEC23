// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentTermRepository : BaseColleagueRepository, IStudentTermRepository
    {
        public StudentTermRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get all Student Academic Terms for a list of Students.
        /// </summary>
        /// <param name="studentIds">List of Student IDs</param>
        /// <returns>Dictionary containing Student IDs and StudentAcademicTerms entity objects</returns>
        public async Task<IDictionary<string, List<StudentTerm>>> GetStudentTermsByStudentIdsAsync(IEnumerable<string> studentIds, string termId, string academicLevelId)
        {
            IDictionary<string, List<StudentTerm>> studentAcademicTerms = new Dictionary<string, List<StudentTerm>>();
            bool error = false;

            if (studentIds != null && studentIds.Count() > 0)
            {
                // Get STUDENT data
                Collection<Students> studentData = await DataReader.BulkReadRecordAsync<Students>(studentIds.ToArray());
                if (studentData == null || studentData.Count <= 0)
                {
                    logger.Error(string.Format("No PERSON ST records returned for students : {0}", string.Join(",", studentIds)));
                }
                else
                {
                    foreach (var student in studentData)
                    {
                        try // If we hit an exception, cut it off at the student level. Other students could still proceed.
                        {
                            List<string> studentTermIds = new List<string>();
                            // get the student acad cred list
                            if (student != null && student.StuAcadLevels != null && student.StuAcadLevels.Count > 0)
                            {
                                if (student.StuTerms != null && student.StuTerms.Count > 0)
                                {
                                    foreach (string academicLevel in student.StuAcadLevels)
                                    {
                                        // If we pass in an Academic Level, then we need to only build keys for that level.
                                        if (!string.IsNullOrEmpty(academicLevel) && (string.IsNullOrEmpty(academicLevelId) || academicLevel == academicLevelId))
                                        {
                                            foreach (string term in student.StuTerms)
                                            {
                                                // If we pass in a term, then we need to only build keys for that term.
                                                if (!string.IsNullOrEmpty(term) && (string.IsNullOrEmpty(termId) || term == termId))
                                                {
                                                    var studentTermId = student.Recordkey + "*" + term + "*" + academicLevel;
                                                    if (!studentTermIds.Contains(studentTermId))
                                                    {
                                                        studentTermIds.Add(studentTermId);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            List<StudentTerm> studentAcademicTerm = new List<StudentTerm>();
                            if (studentTermIds != null && studentTermIds.Count > 0)
                            {
                                Collection<StudentTerms> studentTermsData = await DataReader.BulkReadRecordAsync<StudentTerms>(studentTermIds.ToArray());
                                foreach (var studentTerms in studentTermsData)
                                {
                                   var studentId = studentTerms.Recordkey.Split('*')[0];
                                    var termKey = studentTerms.Recordkey.Split('*')[1];
                                    var levelId = studentTerms.Recordkey.Split('*')[2];
                                    if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(termKey) && !string.IsNullOrEmpty(levelId))
                                    {
                                        var studentTerm = new StudentTerm(studentId, termKey, levelId);
                                        studentTerm.StudentLoad = studentTerms.SttrStudentLoad;
                                        studentAcademicTerm.Add(studentTerm);
                                    }                               
                                }
                            }
                            studentAcademicTerms.Add(student.Recordkey, studentAcademicTerm);
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Could not build student academic terms for student {0}", student.Recordkey));
                            logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
                            error = true;
                        }
                    }
                }
            }
            if (error && !studentAcademicTerms.Any())
                throw new ColleagueWebApiException("Error prevented partial return of student academic terms batch.");

            return studentAcademicTerms;
        }

        /// <summary>
        /// Get Student Academic Term for a guid
        /// </summary>
        /// <param name="guid">IDs</param>
        /// <returns>StudentTerms entity objects</returns>
        public async Task<StudentTerm> GetStudentTermByGuidAsync(string guid)
        {
            StudentTerm studentTerm = null;
            if (!(string.IsNullOrEmpty(guid)))
            {
                var id = await GetStudentTermIdFromGuidAsync(guid);

                try
                {
                    if (!(string.IsNullOrEmpty(id)))
                    {
                        var studentTerms = await DataReader.ReadRecordAsync<StudentTerms>("STUDENT.TERMS", id);

                        studentTerm = BuildStudentTerm(studentTerms);
                    }
                }
                catch
                    (Exception e)
                {
                    logger.Error(string.Format("Could not build student academic terms for guid {0}", guid));
                    logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);

                }
            }
            return studentTerm;
        }

        /// <summary>
        /// Returns StudentTerm Entity as per the select criteria
        /// </summary>
        /// <returns>List of StudentTerm Entities</returns>
        public async Task<Tuple<IEnumerable<StudentTerm>, int>> GetStudentTermsAsync(int offset, int limit, bool bypassCache = false,
            string person = "", string academicPeriod = "")
        {
            try
            {
                string criteria = string.Empty;
                List<string> keys = new List<string>();
                if (!string.IsNullOrEmpty(person))
                {
                    //Get student record based on id passed and generate the compound key for STUDENT.TERMS
                    var student = await DataReader.ReadRecordAsync<Students>("STUDENTS", person);
                                        
                    if(student == null || 
                      (student.StuTerms == null || !student.StuTerms.Any()) || 
                      (student.StuAcadLevels == null || !student.StuAcadLevels.Any()))
                    {
                        return new Tuple<IEnumerable<StudentTerm>, int>(new List<StudentTerm>() , 0);
                    }

                    student.StuTerms.ForEach(trm => 
                    {
                        if(!string.IsNullOrEmpty(trm))
                        {
                            student.StuAcadLevels.ForEach(lvl =>
                            {
                                if (!string.IsNullOrEmpty(lvl))
                                {
                                    if (!string.IsNullOrEmpty(academicPeriod) && trm.Equals(academicPeriod, StringComparison.OrdinalIgnoreCase))
                                    {
                                        keys.Add(string.Concat(person, "*", academicPeriod, "*", lvl));
                                    }
                                    else if(string.IsNullOrEmpty(academicPeriod))
                                    {
                                        keys.Add(string.Concat(person, "*", trm, "*", lvl));
                                    }
                                }
                            });
                        }
                    });                   

                }
                else if (!string.IsNullOrEmpty(academicPeriod))
                {
                    criteria = "WITH STTR.TERM EQ '" + academicPeriod + "'";
                }

                var studentTermIds = await DataReader.SelectAsync("STUDENT.TERMS", keys.ToArray(), criteria);

                var totalCount = studentTermIds.Count();
                Array.Sort(studentTermIds);
                var subList = studentTermIds.Skip(offset).Take(limit).ToArray();
                var studentTermData = await DataReader.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", subList);                
                var studentProgEntities = studentTermData.Select(studentTerms => BuildStudentTerm(studentTerms)).ToList();
                
                return new Tuple<IEnumerable<StudentTerm>, int>(studentProgEntities.AsEnumerable(), totalCount);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<string> GetStudentTermIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("StudentTerm GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("StudentTerm GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STUDENT.TERMS")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, STUDENT.TERMS", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        private StudentTerm BuildStudentTerm(StudentTerms studentTerms)
        {
            StudentTerm studentTerm = null;

            var studentId = studentTerms.Recordkey.Split('*')[0];
            var termKey = studentTerms.Recordkey.Split('*')[1];
            var levelId = studentTerms.Recordkey.Split('*')[2];
            if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(termKey) && !string.IsNullOrEmpty(levelId))
            {
                studentTerm = new StudentTerm(studentTerms.RecordGuid, studentId, termKey, levelId)
                {
                    StudentLoad = studentTerms.SttrStudentLoad,
                    StudentAcademicCredentials = studentTerms.SttrStudentAcadCred
                };
            }
            else
            {
                throw new RepositoryException(string.Concat("Invalid record, Entity:'STUDENT.TERMS', Record ID:'", studentTerms.Recordkey, "'"));
            }
            if (studentTerm != null)
            {
                var studentTermStatuses = new List<StudentTermStatus>();
                foreach (var status in studentTerms.SttrStatusesEntityAssociation)
                {
                    studentTermStatuses.Add(new StudentTermStatus(status.SttrStatusAssocMember, status.SttrStatusDateAssocMember));
                }
                studentTerm.StudentTermStatuses = studentTermStatuses;

            }
    
            return studentTerm;
        }
    }
}