// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{

    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAcademicProgramRepository : BaseColleagueRepository, IStudentAcademicProgramRepository
    {
        private ApplValcodes studentProgramStatuses;
        private List<ApplicationStatuses> allAppStatuses;
        private readonly int readSize;
        protected const string AllStudentAcademicProgramsCache = "AllStudentAcademicPrograms";
        protected const int AllStudentAcademicProgramsCacheTimeout = 20; // Clear from cache every 20 minutes
        protected const string AllStudentAcademicProgramsPersonFilterCache = "AllStudentAcademicProgramsPersonFilter";
        protected const int AllStudentAcademicProgramsPersonFilterTimeout = 20; // Clear from cache every 20 minutes

        public StudentAcademicProgramRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            this.readSize = apiSettings != null && apiSettings.BulkReadSize > 0 ? apiSettings.BulkReadSize : 5000;
        }
        /// <summary>
        /// Gets Student Academic Programs for a guid
        /// </summary>
        /// <param name="id">Student Academic Programs GUID</param>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Returns StudentAcademicProgram</returns>
        public async Task<StudentAcademicProgram> GetStudentAcademicProgramByGuidAsync(string id, string defaultInstitutionId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id must be specified.");
            }

            var studentProgramsId = await GetRecordKeyFromGuidAsync(id);
            try
            {
                if (string.IsNullOrEmpty(studentProgramsId))
                {
                    throw new KeyNotFoundException("studentProgramsId");
                }

                StudentPrograms stuprog = await DataReader.ReadRecordAsync<StudentPrograms>(studentProgramsId);
                if (stuprog == null)
                {
                    throw new KeyNotFoundException("Invalid Student Programs ID: " + studentProgramsId);
                }
                //since get all is just return those "WITH STPR.START.DATE NE ''", issue exception if start date is missing affecting v6 and v11
                if (stuprog.StprStartDate == null || !stuprog.StprStartDate.Any())
                    throw new KeyNotFoundException(string.Concat("No Student Academic Program was found for guid '", id, "'. "));
                var studentProg = new Collection<StudentPrograms>() { stuprog };
                var acadCredData = new Collection<AcadCredentials>();
                var instAttendId = string.Concat(stuprog.Recordkey.Split('*')[0], "*", defaultInstitutionId);
                var instAttendRecord = await DataReader.ReadRecordAsync<InstitutionsAttend>(instAttendId);
                if (instAttendRecord != null && instAttendRecord.InstaAcadCredentials != null & instAttendRecord.InstaAcadCredentials.Any())
                    acadCredData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", instAttendRecord.InstaAcadCredentials.ToArray());
                var stuprogs = await BuildStudentAcademicPrograms2Async(studentProg, acadCredData);
                return stuprogs.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException(string.Concat("No Student Academic Program was found for guid '", id, "'. ", ex.Message));
            }
        }

        /// <summary>
        /// Gets Student Academic Programs for a guid
        /// </summary>
        /// <param name="id">Student Academic Programs GUID</param>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <param name="includeAcadCredentials"></param>
        /// <returns>Returns StudentAcademicProgram</returns>
        public async Task<StudentAcademicProgram> GetStudentAcademicProgramByGuid2Async(string id, string defaultInstitutionId, bool includeAcadCredentials = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id must be specified.");
            }

            var studentProgramsId = await GetRecordKeyFromGuidAsync(id);
            try
            {
                if (string.IsNullOrEmpty(studentProgramsId))
                {
                    throw new KeyNotFoundException("studentProgramsId");
                }

                StudentPrograms stuprog = await DataReader.ReadRecordAsync<StudentPrograms>(studentProgramsId);
                if (stuprog == null)
                {
                    throw new KeyNotFoundException("Invalid Student Programs ID: " + studentProgramsId);
                }

                var studentProg = new Collection<StudentPrograms>() { stuprog };
                var acadCredData = new Collection<AcadCredentials>();
                if (includeAcadCredentials)
                {
                    var instAttendId = string.Concat(stuprog.Recordkey.Split('*')[0], "*", defaultInstitutionId);
                    var instAttendRecord = await DataReader.ReadRecordAsync<InstitutionsAttend>(instAttendId);
                    if (instAttendRecord != null && instAttendRecord.InstaAcadCredentials != null & instAttendRecord.InstaAcadCredentials.Any())
                        acadCredData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", instAttendRecord.InstaAcadCredentials.ToArray());
                }
                var stuprogs = await BuildStudentAcademicPrograms3Async(studentProg, acadCredData);
                return stuprogs.FirstOrDefault();
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException(string.Concat("No Student Academic Program was found for guid '", id, "'. ", ex.Message));
            }
        }

        /// <summary>
        /// Creates a new Student Academic Program
        /// </summary>
        /// <param name="stuAcadProg">Student Academic Program to be created</param>        
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Created Academic Program Entity entity</returns>
        public async Task<StudentAcademicProgram> CreateStudentAcademicProgramAsync(StudentAcademicProgram stuAcadProg, string defaultInstitutionId)
        {
            return await MaintainStudentAcademicProgramAsync(stuAcadProg, false, defaultInstitutionId);
        }

        /// <summary>
        /// Updates an existing Student Academic Program
        /// </summary>
        /// <param name="stuAcadProg">Student Academic Program to be updated</param>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Updated Academic Program Entity entity</returns>
        public async Task<StudentAcademicProgram> UpdateStudentAcademicProgramAsync(StudentAcademicProgram stuAcadProg, string defaultInstitutionId)
        {
            return await MaintainStudentAcademicProgramAsync(stuAcadProg, true, defaultInstitutionId);
        }

        /// <summary>
        /// Creates or updates a Student Academic Program
        /// </summary>
        /// <param name="stuAcadProg">Student Academic Program to be maintained</param>
        /// <param name="isUpdating">Indicates whether an Student Academic Program is being updated</param>        
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Created or updated Student Academic Program entity</returns>
        private async Task<StudentAcademicProgram> MaintainStudentAcademicProgramAsync(StudentAcademicProgram stuAcadProg, bool isUpdating, string defaultInstitutionId)
        {
            if (stuAcadProg == null)
            {
                throw new ArgumentNullException(" Student Academic Program", " Student Academic Program must be provided.");
            }

            string studentProgId = null;
            if (isUpdating)
            {
                if (!string.IsNullOrEmpty(stuAcadProg.Guid))
                {
                    studentProgId = GetRecordKeyFromGuid(stuAcadProg.Guid);
                    if (string.IsNullOrEmpty(studentProgId))
                    {
                        isUpdating = false;
                    }
                }
                else
                {
                    isUpdating = false;
                }
            }

            var request = new UpdateStuAcadProgramRequest()
            {
                StuProgGuid = stuAcadProg.Guid,
                AcadProg = stuAcadProg.ProgramCode,
                Catalog = stuAcadProg.CatalogCode,
                StudentId = stuAcadProg.StudentId,
                degrees = stuAcadProg.DegreeCode,
                ccds = stuAcadProg.StudentProgramCcds,
                Majors = stuAcadProg.StudentProgramMajors,
                Minors = stuAcadProg.StudentProgramMinors,
                Specializations = stuAcadProg.StudentProgramSpecializations,
                StartDate = stuAcadProg.StartDate,
                EndDate = stuAcadProg.EndDate,
                Status = stuAcadProg.Status,
                Location = stuAcadProg.Location,
                StartTerm = stuAcadProg.StartTerm,
                AcademicLevel = stuAcadProg.AcademicLevelCode,
                Dept = stuAcadProg.DepartmentCode,
                Gpa = stuAcadProg.GradGPA,
                GradDate = stuAcadProg.GraduationDate,
                CredDate = stuAcadProg.CredentialsDate,
                ThesisTitle = stuAcadProg.ThesisTitle,
                Honors = stuAcadProg.StudentProgramHonors,
                CreditEarned = stuAcadProg.CreditsEarned,
                AntCmplTerm = stuAcadProg.AnticipatedCompletionTerm,
                GradTerm = stuAcadProg.GradTerm,
                IsPrimary = stuAcadProg.IsPrimary,
                AntCmplDate = stuAcadProg.AnticipatedCompletionDate
            };
            var extendedDataTuple = GetEthosExtendedDataLists();

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }
            var response = await transactionInvoker.ExecuteAsync<UpdateStuAcadProgramRequest, UpdateStuAcadProgramResponse>(request);

            if (response.Error)
            {
                var exception = new RepositoryException("Errors encountered while updating student programs " + request.StudentId + "*" + request.AcadProg);
                foreach (var error in response.UpdateStuAcadProgramError)
                {
                    if (!string.IsNullOrEmpty(error.ErrorCode))
                    {
                        exception.AddError(new RepositoryError(error.ErrorCode, error.ErrorMessage));
                    }
                    else
                    {
                        exception.AddError(new RepositoryError(error.ErrorMessage));
                    }

                }
                throw exception;
            }

            // Create the new object to be returned
            var createdAcad = await GetStudentAcademicProgramByGuidAsync(response.StuProgGuid, defaultInstitutionId);

            return createdAcad;
        }

        /// <summary>
        /// Creates a new Student Academic Program via student-academic-program-submissions resource
        /// </summary>
        /// <param name="stuAcadProg">Student Academic Program to be created</param>        
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Created Academic Program Entity entity</returns>
        public async Task<StudentAcademicProgram> CreateStudentAcademicProgram2Async(StudentAcademicProgram stuAcadProg, string defaultInstitutionId)
        {
            return await MaintainStudentAcademicProgram2Async(stuAcadProg, false, defaultInstitutionId);
        }

        /// <summary>
        /// Updates an existing Student Academic Program via student-academic-program-submissions resource
        /// </summary>
        /// <param name="stuAcadProg">Student Academic Program to be updated</param>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Updated Academic Program Entity entity</returns>
        public async Task<StudentAcademicProgram> UpdateStudentAcademicProgram2Async(StudentAcademicProgram stuAcadProg, string defaultInstitutionId)
        {
            return await MaintainStudentAcademicProgram2Async(stuAcadProg, true, defaultInstitutionId);
        }

        /// <summary>
        /// Creates or updates a Student Academic Program
        /// </summary>
        /// <param name="stuAcadProg">Student Academic Program to be maintained</param>
        /// <param name="isUpdating">Indicates whether an Student Academic Program is being updated</param>        
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Created or updated Student Academic Program entity</returns>
        private async Task<StudentAcademicProgram> MaintainStudentAcademicProgram2Async(StudentAcademicProgram stuAcadProg, bool isUpdating, string defaultInstitutionId)
        {
            if (stuAcadProg == null)
            {
                throw new ArgumentNullException(" Student Academic Program", " Student Academic Program must be provided.");
            }

            string studentProgId = null;
            if (isUpdating)
            {
                if (!string.IsNullOrEmpty(stuAcadProg.Guid))
                {
                    studentProgId = GetRecordKeyFromGuid(stuAcadProg.Guid);
                    if (string.IsNullOrEmpty(studentProgId))
                    {
                        isUpdating = false;
                    }
                }
                else
                {
                    isUpdating = false;
                }
            }

            var request = new UpdateStudentProgramRequest()
            {
                StuProgGuid = stuAcadProg.Guid,
                AcadProg = stuAcadProg.ProgramCode,
                Catalog = stuAcadProg.CatalogCode,
                StudentId = stuAcadProg.StudentId,
                degrees = stuAcadProg.DegreeCode,
                ccds = stuAcadProg.StudentProgramCcds,
                StartDate = stuAcadProg.StartDate,
                EndDate = stuAcadProg.EndDate,
                Status = stuAcadProg.Status,
                Location = stuAcadProg.Location,
                StartTerm = stuAcadProg.StartTerm,
                AcademicLevel = stuAcadProg.AcademicLevelCode,
                Dept = stuAcadProg.DepartmentCode,
                AntCmplTerm = stuAcadProg.AnticipatedCompletionTerm,
                AntCmplDate = stuAcadProg.AnticipatedCompletionDate,
                AdmitStatus = stuAcadProg.AdmitStatus,
                StuProgToReplace = stuAcadProg.StudentProgramToReplace
            };
            //add majors info to the CTX
            if ((stuAcadProg.StudentProgramMajorsTuple != null) && (stuAcadProg.StudentProgramMajorsTuple.Any()))
            {
                foreach (var major in stuAcadProg.StudentProgramMajorsTuple)
                {
                    if (!string.IsNullOrEmpty(major.Item1))
                    {
                        var maj = new UpdateStudentProgramMajors();
                        maj.Majors = major.Item1;
                        maj.MajorStartDate = major.Item2;
                        maj.MajorEndDate = major.Item3;
                        request.UpdateStudentProgramMajors.Add(maj);
                    }
                }
            }
            //add minors  info to the CTX
            if ((stuAcadProg.StudentProgramMinorsTuple != null) && (stuAcadProg.StudentProgramMinorsTuple.Any()))
            {
                foreach (var minor in stuAcadProg.StudentProgramMinorsTuple)
                {
                    if (!string.IsNullOrEmpty(minor.Item1))
                    {
                        var min = new UpdateStudentProgramMinors();
                        min.Minors = minor.Item1;
                        min.MinorStartDate = minor.Item2;
                        min.MinorEndDate = minor.Item3;
                        request.UpdateStudentProgramMinors.Add(min);
                    }
                }
            }
            //add specializations  info to the CTX
            if ((stuAcadProg.StudentProgramSpecializationsTuple != null) && (stuAcadProg.StudentProgramSpecializationsTuple.Any()))
            {
                foreach (var sp in stuAcadProg.StudentProgramSpecializationsTuple)
                {
                    if (!string.IsNullOrEmpty(sp.Item1))
                    {
                        var special = new UpdateStudentProgramSpecials();
                        special.Specializations = sp.Item1;
                        special.SpecialStartDate = sp.Item2;
                        special.SpecialEndDate = sp.Item3;
                        request.UpdateStudentProgramSpecials.Add(special);
                    }
                }
            }
            var extendedDataTuple = GetEthosExtendedDataLists();

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }
            var response = await transactionInvoker.ExecuteAsync<UpdateStudentProgramRequest, UpdateStudentProgramResponse>(request);

            if (response.Error)
            {
                var exception = new RepositoryException();
                foreach (var error in response.UpdateStudentProgramError)
                {

                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCode, " - ", error.ErrorMessage))
                    {
                        SourceId = request.StudentId + "*" + request.AcadProg,
                        Id = request.StuProgGuid

                    });
                }
                throw exception;
            }

            // Create the new object to be returned
            var createdAcad = await GetStudentAcademicProgramByGuid2Async(response.StuProgGuid, defaultInstitutionId);

            return createdAcad;
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        public async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await GetInternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }

        /// <summary>
        /// Returns studentProgram Entity as per the select criteria
        /// </summary>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        ///  <param name="program">academic program Name Contains ...program...</param>
        ///  <param name="startDate">Student Academic Program starts on or after this date</param>
        ///  <param name="endDate">Student Academic Program ends on or before this date</param>
        /// <param name="student">Id of the student enrolled on the academic program</param>
        /// <param name="catalog">Student Academic Program catalog  equal to</param>
        /// <param name="Status">Student Academic Program status equals to </param>
        /// <param name="programOwner">Student Academic Program programOwner equals to </param>
        /// <param name="site">Student Academic Program site equals to </param>
        /// <param name="graduatedOn">Student Academic Program graduatedOn equals to </param>
        /// <param name="ccdCredentials">Student Academic Program ccdCredential equals to </param>
        /// <param name="degreeCredentials">Student Academic Program degreeCredential equals to </param>
        /// <returns>StudentProgram Entities</returns>
        public async Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms2Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
            string program = "", string startDate = "", string endDate = "", string student = "", string catalog = "", string status = "", string programOwner = "",
            string site = "", string academicLevel = "", string graduatedOn = "", List<string> ccdCredentials = null, List<string> degreeCredentials = null,
            string graduatedAcademicPeriod = "", string completeStatus = "")
        {
            try
            {
                List<string> stuProgsLimitingKeys = new List<string>();
                List<string> acadCredLimitingKeys = new List<string>();
                List<string> acadProgLimitingKeys = new List<string>();
                string criteria = "WITH STPR.START.DATE NE ''";
                var oldCriteria = criteria;
                string acadProgCriteria = string.Empty;
                //string acadCredProgCriteria = string.Empty;
                string acadCredCriteria = string.Empty;
                string[] studentProgramIds = new string[] { };
                string[] acadStuProgIds = new string[] { };
                string[] stuProgIds = new string[] { };

                //do student first to get limiting keys
                #region student filter
                if (!string.IsNullOrEmpty(student))
                {
                    //since we have student Id, we should just read the student record and create the student program ids.
                    List<string> IdsFromStuFil = new List<string>();
                    var studentRecord = await DataReader.ReadRecordAsync<DataContracts.Students>(student);
                    if (studentRecord == null)
                        return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                    if (studentRecord.StuAcadPrograms != null && studentRecord.StuAcadPrograms.Any())
                    {
                        acadProgLimitingKeys.AddRange(studentRecord.StuAcadPrograms);
                        foreach (var prog in studentRecord.StuAcadPrograms)
                        {
                            IdsFromStuFil.Add(string.Concat(student, "*", prog));
                        }
                    }
                    if (IdsFromStuFil != null && IdsFromStuFil.Any())
                        stuProgsLimitingKeys.AddRange(IdsFromStuFil);
                    //since we know the student Id, using institution.attend record for the default institution, we can create the limiting keys for acad.credentials
                    var instAttendId = string.Concat(student, "*", defaultInstitutionId);
                    var instAttendRecord = await DataReader.ReadRecordAsync<InstitutionsAttend>(instAttendId);
                    if (instAttendRecord != null && instAttendRecord.InstaAcadCredentials != null & instAttendRecord.InstaAcadCredentials.Any())
                        acadCredLimitingKeys.AddRange(instAttendRecord.InstaAcadCredentials);
                }
                #endregion

                #region student program data items filter
                //if there is program and catalog in the filter, we can use an index to create a limiting list.
                if ((!string.IsNullOrEmpty(program)) && (!string.IsNullOrEmpty(catalog)))
                {
                    //if there is acad program limiting keys, we need to use it.
                    if (!string.IsNullOrEmpty(program))
                    {
                        if (acadProgLimitingKeys != null & acadProgLimitingKeys.Any())
                        {
                            var acad = acadProgLimitingKeys.FirstOrDefault(pr => pr.Equals(program));
                            if (acad == null)
                                return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);

                        }
                        else
                        {
                            if (!acadProgLimitingKeys.Contains(program))
                                acadProgLimitingKeys.Add(program);
                        }
                    }
                    var prgCriteria = "WITH STU.PGM.INDEX EQ '" + program + catalog + "'";
                    stuProgsLimitingKeys = (await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, prgCriteria)).ToList();
                    if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(program))
                    {

                        if (acadProgLimitingKeys != null & acadProgLimitingKeys.Any())
                        {
                            var acad = acadProgLimitingKeys.FirstOrDefault(pr => pr.Equals(program));
                            if (acad == null)
                                return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                        }
                        else
                        {
                            if (!acadProgLimitingKeys.Contains(program))
                                acadProgLimitingKeys.Add(program);
                        }
                        criteria += " AND WITH STPR.ACAD.PROGRAM EQ '" + program + "'";
                    }
                    if (!string.IsNullOrEmpty(catalog))
                    {
                        criteria += " AND WITH STPR.CATALOG EQ '" + catalog + "'";
                    }
                }
                //create a limiting list using filters that are data element
                if (!string.IsNullOrEmpty(programOwner))
                {
                    criteria += " AND WITH STPR.DEPT EQ '" + programOwner + "'";
                }
                if (!string.IsNullOrEmpty(site))
                {
                    criteria += " AND WITH STPR.LOCATION EQ '" + site + "'";
                }
                if (criteria != oldCriteria)
                {
                    stuProgsLimitingKeys = (await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, criteria)).ToList();
                    if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                    }
                    criteria = oldCriteria;
                }
                #endregion

                #region student program CC filter
                //this is a CC
                if (!string.IsNullOrEmpty(startDate))
                {
                    criteria += " AND WITH STPR.LATEST.START.DATE GE '" + startDate + "'";
                }
                //this is a CC
                if (!string.IsNullOrEmpty(endDate))
                {
                    criteria += " AND WITH STPR.CURRENT.END.DATE NE '' AND WITH STPR.CURRENT.END.DATE LE '" + endDate + "'";
                }
                // this is a CC
                if (!string.IsNullOrEmpty(status))
                {
                    criteria += " AND WITH STPR.CURRENT.STATUS EQ " + status;
                }
                //this is a CC
                if (!string.IsNullOrEmpty(academicLevel))
                {
                    criteria += " AND WITH STPR.ACAD.LEVEL EQ '" + academicLevel + "'";
                    //if (string.IsNullOrEmpty(acadProgCriteria))
                    //    acadProgCriteria += "WITH ACPG.ACAD.LEVEL EQ '" + academicLevel + "'";
                    //else
                    //    acadProgCriteria += "AND WITH ACPG.ACAD.LEVEL EQ '" + academicLevel + "'";
                }

                if (criteria != oldCriteria)
                {
                    stuProgsLimitingKeys = (await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, criteria)).ToList();
                    if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                    }
                    criteria = oldCriteria;
                }

                #endregion

                #region acad cred filter
                if (!string.IsNullOrEmpty(graduatedOn) || !string.IsNullOrEmpty(graduatedAcademicPeriod))
                {
                    string acadCredProgCriteria = string.Empty;
                    if (!string.IsNullOrEmpty(program))
                        acadCredProgCriteria = "WITH ACAD.ACAD.PROGRAM EQ '" + program + "'";
                    else
                        acadCredProgCriteria = "WITH ACAD.INSTITUTIONS.ID EQ '" + defaultInstitutionId + "'";
                    if (!string.IsNullOrEmpty(graduatedOn))
                    {
                        if (string.IsNullOrEmpty(acadCredProgCriteria))
                            acadCredProgCriteria += "WITH ACAD.END.DATE EQ '" + graduatedOn + "'";
                        else
                            acadCredProgCriteria += " AND WITH ACAD.END.DATE EQ '" + graduatedOn + "'";
                    }
                    if (!string.IsNullOrEmpty(graduatedAcademicPeriod))
                    {
                        if (string.IsNullOrEmpty(acadCredProgCriteria))
                            acadCredProgCriteria += "WITH ACAD.TERM EQ '" + graduatedAcademicPeriod + "'";
                        else
                            acadCredProgCriteria += " AND WITH ACAD.TERM EQ '" + graduatedAcademicPeriod + "'";
                    }

                    if (!string.IsNullOrEmpty(acadCredProgCriteria))
                    {
                        //if there is no limiting keys then we can create using student program limiting keys here. 
                        if (acadCredLimitingKeys == null || !acadCredLimitingKeys.Any())
                        {
                            if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                            {
                                List<string> studentIds = stuProgsLimitingKeys.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
                                List<string> instAttendIds = new List<string>();
                                foreach (var id in studentIds)
                                    instAttendIds.Add(string.Concat(id, "*", defaultInstitutionId));
                                if (instAttendIds != null && instAttendIds.Any())
                                {
                                    var acadCredIds = await DataReader.SelectAsync("INSTITUTIONS.ATTEND", instAttendIds.ToArray(), "WITH INSTA.ACAD.CREDENTIALS BY.EXP INSTA.ACAD.CREDENTIALS SAVING INSTA.ACAD.CREDENTIALS");
                                    if (acadCredIds != null && acadCredIds.Any())
                                    {
                                        acadCredLimitingKeys.AddRange(acadCredIds);
                                        acadCredLimitingKeys.Distinct();
                                    }
                                }
                            }
                        }

                        acadCredLimitingKeys = (await DataReader.SelectAsync("ACAD.CREDENTIALS", acadCredLimitingKeys != null && acadCredLimitingKeys.Any() ? acadCredLimitingKeys.ToArray() : null, acadCredProgCriteria)).ToList();
                        if (acadCredLimitingKeys == null || !acadCredLimitingKeys.Any())
                        {
                            return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                        }
                        if (acadCredLimitingKeys.Any())
                        {
                            var acadCredData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredLimitingKeys.ToArray());
                            List<string> acadCredStuProgIds = new List<string>();
                            if (acadCredData.Any())
                            {
                                foreach (var cred in acadCredData)
                                {
                                    if (!string.IsNullOrEmpty(cred.AcadAcadProgram))
                                        acadCredStuProgIds.Add(cred.AcadPersonId + "*" + cred.AcadAcadProgram);
                                }
                                //merge it with existing list of student academic programs.
                                if (acadCredStuProgIds != null && acadCredStuProgIds.Any())
                                {
                                    if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                                    {
                                        stuProgsLimitingKeys = acadCredStuProgIds;
                                        if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                                        {
                                            return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                                        }

                                    }
                                    else
                                    {
                                        stuProgsLimitingKeys.AddRange(acadCredStuProgIds);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region degree & ccd filter
                //we are applying the credentials and degree filters here 
                if ((ccdCredentials != null && ccdCredentials.Any()) || (degreeCredentials != null && degreeCredentials.Any()))
                {
                    var credStuProgIds = await ApplyCredentialsFilter(program, degreeCredentials, ccdCredentials, stuProgsLimitingKeys, acadCredLimitingKeys, completeStatus);
                    //if this returns the list of student programs then that becomes our list. 
                    if (credStuProgIds != null && credStuProgIds.Any())
                    {
                        if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                        {
                            stuProgsLimitingKeys = credStuProgIds;
                            if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                            {
                                return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                            }

                        }
                        else
                        {
                            stuProgsLimitingKeys.AddRange(credStuProgIds);
                        }
                    }
                    else
                    {
                        return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                    }
                    //if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                    //{
                    //    List<string> studentIds = stuProgsLimitingKeys.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
                    //    List<string> instAttendIds = new List<string>();
                    //    foreach (var id in studentIds)
                    //        instAttendIds.Add(string.Concat(id, "*", defaultInstitutionId));
                    //    if (instAttendIds != null && instAttendIds.Any())
                    //    {
                    //        var acadCredIds = await DataReader.SelectAsync("INSTITUTIONS.ATTEND", instAttendIds.ToArray(), "WITH INSTA.ACAD.CREDENTIALS BY.EXP INSTA.ACAD.CREDENTIALS SAVING INSTA.ACAD.CREDENTIALS");
                    //        if (acadCredIds != null && acadCredIds.Any())
                    //        {
                    //            acadCredLimitingKeys.AddRange(acadCredIds);
                    //            acadCredLimitingKeys.Distinct();
                    //        }
                    //    }
                    //}

                }
                #endregion

                #region Default Selection Criteria
                // Failed Inst Enrollment (WINR) leaves behind orphaned items.  Some items are invalid because
                // the person record doesn't exist and other items are invalid because the key is invalid (missing program)
                // This default criteria is set to insure we only select valid STUDENT.PROGRAMS records.
                if (string.IsNullOrEmpty(criteria))
                    criteria = "WITH STPR.ACAD.PROGRAM NE '' AND WITH STPR.STUDENT.LAST.NAME NE ''";
                else
                    criteria += " AND WITH STPR.ACAD.PROGRAM NE '' AND WITH STPR.STUDENT.LAST.NAME NE ''";
                #endregion

                if (!stuProgsLimitingKeys.Any())
                {
                    stuProgsLimitingKeys = (await DataReader.SelectAsync("STUDENT.PROGRAMS", criteria)).ToList();
                }
                var totalCount = stuProgsLimitingKeys.Count();
                stuProgsLimitingKeys.Sort();
                var subList = stuProgsLimitingKeys.Skip(offset).Take(limit).ToArray();
                var studentProgramData = await DataReader.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", subList);
                var acadCredRecords = new Collection<AcadCredentials>();
                if (subList != null && subList.Any())
                {
                    List<string> studentIds = subList.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
                    List<string> instAttendIds = new List<string>();
                    foreach (var id in studentIds)
                        instAttendIds.Add(string.Concat(id, "*", defaultInstitutionId));
                    if (instAttendIds != null && instAttendIds.Any())
                    {
                        acadCredLimitingKeys = (await DataReader.SelectAsync("INSTITUTIONS.ATTEND", instAttendIds.ToArray(), "WITH INSTA.ACAD.CREDENTIALS BY.EXP INSTA.ACAD.CREDENTIALS SAVING INSTA.ACAD.CREDENTIALS")).ToList();
                    }
                    acadCredRecords = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredLimitingKeys.ToArray());

                }
                var studentProgEntities = await BuildStudentAcademicPrograms2Async(studentProgramData, acadCredRecords);
                return new Tuple<IEnumerable<StudentAcademicProgram>, int>(studentProgEntities, totalCount);

            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Returns studentProgram Entity as per the select criteria
        /// </summary>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        ///  <param name="program">academic program Name Contains ...program...</param>
        ///  <param name="startDate">Student Academic Program starts on or after this date</param>
        ///  <param name="endDate">Student Academic Program ends on or before this date</param>
        /// <param name="student">Id of the student enrolled on the academic program</param>
        /// <param name="catalog">Student Academic Program catalog  equal to</param>
        /// <param name="Status">Student Academic Program status equals to </param>
        /// <param name="programOwner">Student Academic Program programOwner equals to </param>
        /// <param name="site">Student Academic Program site equals to </param>
        /// <param name="graduatedOn">Student Academic Program graduatedOn equals to </param>
        /// <param name="ccdCredentials">Student Academic Program ccdCredential equals to </param>
        /// <param name="degreeCredentials">Student Academic Program degreeCredential equals to </param>
        /// <returns>StudentProgram Entities</returns>
        public async Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms3Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
            string program = "", string startDate = "", string endDate = "", string student = "", string catalog = "", string status = "", string programOwner = "",
            string site = "", string academicLevel = "", string graduatedOn = "", List<string> ccdCredentials = null, List<string> degreeCredentials = null,
            string graduatedAcademicPeriod = "", string completeStatus = "", CurriculumObjectiveCategory curriculumObjective = CurriculumObjectiveCategory.NotSet,
            bool includeAcademicCredentials = true)
        {
            try
            {
                var acadCredLimitingKeys = new List<string>();

                var stuProgsLimitingKeysKeyCache = await GetStudentAcademicProgramsFilterCriteriaAsync(defaultInstitutionId, program, startDate, endDate, student, catalog, status,
                    programOwner, site, academicLevel, graduatedOn, ccdCredentials, degreeCredentials, graduatedAcademicPeriod, completeStatus, curriculumObjective, includeAcademicCredentials);

                if ((stuProgsLimitingKeysKeyCache == null) || (stuProgsLimitingKeysKeyCache.NoQualifyingRecords == true))
                {
                    return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                }

                var stuProgsLimitingKeys = (await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeysKeyCache.limitingKeys.Any() ? stuProgsLimitingKeysKeyCache.limitingKeys.ToArray() : null,
                       stuProgsLimitingKeysKeyCache.criteria)).ToList();

                if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                }

                var totalCount = stuProgsLimitingKeys.Count();
                stuProgsLimitingKeys.Sort();
                var subList = stuProgsLimitingKeys.Skip(offset).Take(limit).ToArray();

                var studentProgramData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", subList);

                if (studentProgramData.Equals(default(BulkReadOutput<DataContracts.StudentPrograms>)))
                {
                    return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), totalCount);
                }
                if ((studentProgramData.InvalidKeys != null && studentProgramData.InvalidKeys.Any())
                        || (studentProgramData.InvalidRecords != null && studentProgramData.InvalidRecords.Any()))
                {
                    var repositoryException = new RepositoryException();

                    if (studentProgramData.InvalidKeys.Any())
                    {
                        repositoryException.AddErrors(studentProgramData.InvalidKeys
                            .Select(key => new RepositoryError("invalid.key",
                            string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                    }
                    if (studentProgramData.InvalidRecords.Any())
                    {
                        repositoryException.AddErrors(studentProgramData.InvalidRecords
                           .Select(r => new RepositoryError("invalid.record",
                           string.Format("Error: '{0}' ", r.Value))
                           { SourceId = r.Key }));
                    }
                    throw repositoryException;
                }

                var acadCredRecords = new Collection<AcadCredentials>();

                if (includeAcademicCredentials && subList != null && subList.Any())
                {
                    List<string> studentIds = subList.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
                    List<string> instAttendIds = new List<string>();

                    foreach (var id in studentIds)
                        instAttendIds.Add(string.Concat(id, "*", defaultInstitutionId));
                    if (instAttendIds != null && instAttendIds.Any())
                    {
                        acadCredLimitingKeys = (await DataReader.SelectAsync("INSTITUTIONS.ATTEND", instAttendIds.ToArray(), "WITH INSTA.ACAD.CREDENTIALS BY.EXP INSTA.ACAD.CREDENTIALS SAVING INSTA.ACAD.CREDENTIALS")).ToList();
                    }
                    acadCredRecords = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredLimitingKeys.ToArray());

                }
                var studentProgEntities = await BuildStudentAcademicPrograms3Async(studentProgramData.BulkRecordsRead, acadCredRecords);
                return new Tuple<IEnumerable<StudentAcademicProgram>, int>(studentProgEntities, totalCount);

            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Returns studentProgram Entity as per the select criteria
        /// </summary>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        ///  <param name="program">academic program Name Contains ...program...</param>
        ///  <param name="startDate">Student Academic Program starts on or after this date</param>
        ///  <param name="endDate">Student Academic Program ends on or before this date</param>
        /// <param name="student">Id of the student enrolled on the academic program</param>
        /// <param name="catalog">Student Academic Program catalog  equal to</param>
        /// <param name="Status">Student Academic Program status equals to </param>
        /// <param name="programOwner">Student Academic Program programOwner equals to </param>
        /// <param name="site">Student Academic Program site equals to </param>
        /// <param name="graduatedOn">Student Academic Program graduatedOn equals to </param>
        /// <param name="ccdCredentials">Student Academic Program ccdCredential equals to </param>
        /// <param name="degreeCredentials">Student Academic Program degreeCredential equals to </param>
        /// <returns>StudentProgram Entities</returns>
        public async Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms4Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
            string program = "", string startDate = "", string endDate = "", string student = "", string catalog = "", string status = "", string programOwner = "",
            string site = "", string academicLevel = "", string graduatedOn = "", List<string> ccdCredentials = null, List<string> degreeCredentials = null,
            string graduatedAcademicPeriod = "", string completeStatus = "", CurriculumObjectiveCategory curriculumObjective = CurriculumObjectiveCategory.NotSet,
            bool includeAcademicCredentials = true)
        {
            try
            {
                List<string> stuProgsLimitingKeys = new List<string>();
                List<string> acadCredLimitingKeys = new List<string>();

                int totalCount = 0;
                string[] subList = null;

                string studentAcademicProgramsCacheKey = CacheSupport.BuildCacheKey(AllStudentAcademicProgramsCache, program, startDate, endDate, student, catalog, status,
                    programOwner, site, academicLevel, graduatedOn, ccdCredentials, degreeCredentials, graduatedAcademicPeriod,
                    completeStatus, curriculumObjective.ToString(), includeAcademicCredentials);

                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    studentAcademicProgramsCacheKey,
                    "STUDENT.PROGRAMS",
                    offset,
                    limit,
                    AllStudentAcademicProgramsCacheTimeout,
                    async () =>
                    {
                        return await GetStudentAcademicProgramsFilterCriteriaAsync(defaultInstitutionId, program, startDate, endDate, student, catalog, status,
                             programOwner, site, academicLevel, graduatedOn, ccdCredentials, degreeCredentials, graduatedAcademicPeriod, completeStatus, curriculumObjective, includeAcademicCredentials);

                    }
                );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                }

                subList = keyCache.Sublist.ToArray();

                totalCount = keyCache.TotalCount.Value;
                var studentProgramData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", subList);

                if (studentProgramData.Equals(default(BulkReadOutput<DataContracts.StudentPrograms>)))
                {
                    return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), totalCount);
                }
                if ((studentProgramData.InvalidKeys != null && studentProgramData.InvalidKeys.Any())
                        || (studentProgramData.InvalidRecords != null && studentProgramData.InvalidRecords.Any()))
                {
                    var repositoryException = new RepositoryException();

                    if (studentProgramData.InvalidKeys.Any())
                    {
                        repositoryException.AddErrors(studentProgramData.InvalidKeys
                            .Select(key => new RepositoryError("invalid.key",
                            string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                    }
                    if (studentProgramData.InvalidRecords.Any())
                    {
                        repositoryException.AddErrors(studentProgramData.InvalidRecords
                           .Select(r => new RepositoryError("invalid.record",
                           string.Format("Error: '{0}' ", r.Value))
                           { SourceId = r.Key }));
                    }
                    throw repositoryException;
                }

                var acadCredRecords = new Collection<AcadCredentials>();

                if (includeAcademicCredentials && subList != null && subList.Any())
                {
                    List<string> studentIds = subList.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
                    List<string> instAttendIds = new List<string>();

                    foreach (var id in studentIds)
                        instAttendIds.Add(string.Concat(id, "*", defaultInstitutionId));
                    if (instAttendIds != null && instAttendIds.Any())
                    {
                        acadCredLimitingKeys = (await DataReader.SelectAsync("INSTITUTIONS.ATTEND", instAttendIds.ToArray(), "WITH INSTA.ACAD.CREDENTIALS BY.EXP INSTA.ACAD.CREDENTIALS SAVING INSTA.ACAD.CREDENTIALS")).ToList();
                    }
                    acadCredRecords = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredLimitingKeys.ToArray());

                }
                var studentProgEntities = await BuildStudentAcademicPrograms3Async(studentProgramData.BulkRecordsRead, acadCredRecords);
                return new Tuple<IEnumerable<StudentAcademicProgram>, int>(studentProgEntities, totalCount);

            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Returns StudentAcademicPrograms Filter sublist
        /// </summary>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        ///  <param name="program">academic program Name Contains ...program...</param>
        ///  <param name="startDate">Student Academic Program starts on or after this date</param>
        ///  <param name="endDate">Student Academic Program ends on or before this date</param>
        /// <param name="student">Id of the student enrolled on the academic program</param>
        /// <param name="catalog">Student Academic Program catalog  equal to</param>
        /// <param name="Status">Student Academic Program status equals to </param>
        /// <param name="programOwner">Student Academic Program programOwner equals to </param>
        /// <param name="site">Student Academic Program site equals to </param>
        /// <param name="graduatedOn">Student Academic Program graduatedOn equals to </param>
        /// <param name="ccdCredentials">Student Academic Program ccdCredential equals to </param>
        /// <param name="degreeCredentials">Student Academic Program degreeCredential equals to </param>
        /// <returns>StudentProgram Entities</returns>
        public async Task<CacheSupport.KeyCacheRequirements> GetStudentAcademicProgramsFilterCriteriaAsync(string defaultInstitutionId,
            string program = "", string startDate = "", string endDate = "", string student = "", string catalog = "", string status = "", string programOwner = "",
            string site = "", string academicLevel = "", string graduatedOn = "", List<string> ccdCredentials = null, List<string> degreeCredentials = null,
            string graduatedAcademicPeriod = "", string completeStatus = "", CurriculumObjectiveCategory curriculumObjective = CurriculumObjectiveCategory.NotSet,
            bool includeAcademicCredentials = true)
        {


            List<string> stuProgsLimitingKeys = new List<string>();
            List<string> acadCredLimitingKeys = new List<string>();
            List<string> acadProgLimitingKeys = new List<string>();
            string criteria = string.Empty;
            try
            {
                string acadProgCriteria = string.Empty;
                string acadCredCriteria = string.Empty;
                string[] studentProgramIds = new string[] { };
                string[] acadStuProgIds = new string[] { };
                string[] stuProgIds = new string[] { };

                //do student first to get limiting keys
                #region student filter
                if (!string.IsNullOrEmpty(student))
                {
                    criteria = "WITH STPR.STUDENT EQ '" + student + "'";
                }
                #endregion

                #region student program data items filter
                //if there is program and catalog in the filter, we can use an index to create a limiting list.
                if ((!string.IsNullOrEmpty(program)) && (!string.IsNullOrEmpty(catalog)))
                {
                    //if there is acad program limiting keys, we need to use it.
                    if (!string.IsNullOrEmpty(program))
                    {
                        if (acadProgLimitingKeys != null & acadProgLimitingKeys.Any())
                        {
                            var acad = acadProgLimitingKeys.FirstOrDefault(pr => pr.Equals(program));
                            if (acad == null)
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };

                        }
                        else
                        {
                            if (!acadProgLimitingKeys.Contains(program))
                                acadProgLimitingKeys.Add(program);
                        }
                    }
                    //var prgCriteria = "WITH STU.PGM.INDEX EQ '" + program + catalog + "'"; 
                    if (string.IsNullOrEmpty(criteria))
                        criteria = "WITH STU.PGM.INDEX EQ '" + program + catalog + "'";
                    else
                        criteria += " AND WITH STU.PGM.INDEX EQ '" + program + catalog + "'";
                    //stuProgsLimitingKeys = (await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, prgCriteria)).ToList();
                    //if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                    //{
                    //    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                    //}
                }
                else
                {
                    if (!string.IsNullOrEmpty(program))
                    {

                        if (acadProgLimitingKeys != null & acadProgLimitingKeys.Any())
                        {
                            var acad = acadProgLimitingKeys.FirstOrDefault(pr => pr.Equals(program));
                            if (acad == null)
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        else
                        {
                            if (!acadProgLimitingKeys.Contains(program))
                                acadProgLimitingKeys.Add(program);
                        }
                        if (string.IsNullOrEmpty(criteria))
                            criteria = "WITH STPR.ACAD.PROGRAM EQ '" + program + "'";
                        else
                            criteria += " AND WITH STPR.ACAD.PROGRAM EQ '" + program + "'";
                    }
                    if (!string.IsNullOrEmpty(catalog))
                    {
                        if (string.IsNullOrEmpty(criteria))
                            criteria = "WITH STPR.CATALOG EQ '" + catalog + "'";
                        else
                            criteria += " AND WITH STPR.CATALOG EQ '" + catalog + "'";
                    }
                }
                //create a limiting list using filters that are data element
                if (!string.IsNullOrEmpty(programOwner))
                {
                    if (string.IsNullOrEmpty(criteria))
                        criteria = "WITH STPR.DEPT EQ '" + programOwner + "'";
                    else
                        criteria += " AND WITH STPR.DEPT EQ '" + programOwner + "'";
                }
                if (!string.IsNullOrEmpty(site))
                {
                    if (string.IsNullOrEmpty(criteria))
                        criteria = "WITH STPR.LOCATION EQ '" + site + "'";
                    else
                        criteria += " AND WITH STPR.LOCATION EQ '" + site + "'";
                }
                //if (criteria != string.Empty) //oldCriteria)
                //{
                //    stuProgsLimitingKeys = (await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, criteria)).ToList();
                //    if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                //    {
                //        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                //    }
                //    criteria = string.Empty; // oldCriteria;
                //}
                #endregion

                #region student program CC filter
                //this is a CC
                if (!string.IsNullOrEmpty(startDate))
                {
                    if (string.IsNullOrEmpty(criteria))
                        criteria = "WITH STPR.LATEST.START.DATE GE '" + startDate + "'";
                    else
                        criteria += " AND WITH STPR.LATEST.START.DATE GE '" + startDate + "'";
                }
                //this is a CC
                if (!string.IsNullOrEmpty(endDate))
                {
                    if (string.IsNullOrEmpty(criteria))
                        criteria = "WITH STPR.CURRENT.END.DATE NE '' AND WITH STPR.CURRENT.END.DATE LE '" + endDate + "'";
                    else
                        criteria += " AND WITH STPR.CURRENT.END.DATE NE '' AND WITH STPR.CURRENT.END.DATE LE '" + endDate + "'";
                }
                // this is a CC
                if (!string.IsNullOrEmpty(status))
                {
                    if (string.IsNullOrEmpty(criteria))
                        criteria = "WITH STPR.CURRENT.STATUS EQ " + status;
                    else
                        criteria += " AND WITH STPR.CURRENT.STATUS EQ " + status;
                }
                //this is a CC
                if (!string.IsNullOrEmpty(academicLevel))
                {
                    if (string.IsNullOrEmpty(criteria))
                        criteria = "WITH STPR.ACAD.LEVEL EQ '" + academicLevel + "'";
                    else
                        criteria += " AND WITH STPR.ACAD.LEVEL EQ '" + academicLevel + "'";
                    //if (string.IsNullOrEmpty(acadProgCriteria))
                    //    acadProgCriteria += "WITH ACPG.ACAD.LEVEL EQ '" + academicLevel + "'";
                    //else
                    //    acadProgCriteria += "AND WITH ACPG.ACAD.LEVEL EQ '" + academicLevel + "'";
                }

                //if (criteria != string.Empty) //oldCriteria)
                //{
                //    stuProgsLimitingKeys = (await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, criteria)).ToList();
                //    if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                //    {
                //        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                //    }
                //    criteria = string.Empty; // oldCriteria;
                //}

                #endregion

                #region acad cred filter
                if (!string.IsNullOrEmpty(graduatedOn) || !string.IsNullOrEmpty(graduatedAcademicPeriod))
                {
                    string acadCredProgCriteria = string.Empty;
                    if (!string.IsNullOrEmpty(program))
                        acadCredProgCriteria = "WITH ACAD.ACAD.PROGRAM EQ '" + program + "'";
                    else
                        acadCredProgCriteria = "WITH ACAD.INSTITUTIONS.ID EQ '" + defaultInstitutionId + "'";
                    if (!string.IsNullOrEmpty(graduatedOn))
                    {
                        if (string.IsNullOrEmpty(acadCredProgCriteria))
                            acadCredProgCriteria += "WITH ACAD.END.DATE EQ '" + graduatedOn + "'";
                        else
                            acadCredProgCriteria += " AND WITH ACAD.END.DATE EQ '" + graduatedOn + "'";
                    }
                    if (!string.IsNullOrEmpty(graduatedAcademicPeriod))
                    {
                        if (string.IsNullOrEmpty(acadCredProgCriteria))
                            acadCredProgCriteria += "WITH ACAD.TERM EQ '" + graduatedAcademicPeriod + "'";
                        else
                            acadCredProgCriteria += " AND WITH ACAD.TERM EQ '" + graduatedAcademicPeriod + "'";
                    }

                    if (!string.IsNullOrEmpty(acadCredProgCriteria))
                    {
                        //if there is no limiting keys then we can create using student program limiting keys here. 
                        if (acadCredLimitingKeys == null || !acadCredLimitingKeys.Any())
                        {
                            if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                            {
                                List<string> studentIds = stuProgsLimitingKeys.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
                                List<string> instAttendIds = new List<string>();
                                foreach (var id in studentIds)
                                    instAttendIds.Add(string.Concat(id, "*", defaultInstitutionId));
                                if (instAttendIds != null && instAttendIds.Any())
                                {
                                    var acadCredIds = await DataReader.SelectAsync("INSTITUTIONS.ATTEND", instAttendIds.ToArray(), "WITH INSTA.ACAD.CREDENTIALS BY.EXP INSTA.ACAD.CREDENTIALS SAVING INSTA.ACAD.CREDENTIALS");
                                    if (acadCredIds != null && acadCredIds.Any())
                                    {
                                        acadCredLimitingKeys.AddRange(acadCredIds);
                                        acadCredLimitingKeys.Distinct();
                                    }
                                }
                            }
                        }

                        acadCredLimitingKeys = (await DataReader.SelectAsync("ACAD.CREDENTIALS", acadCredLimitingKeys != null && acadCredLimitingKeys.Any() ? acadCredLimitingKeys.ToArray() : null, acadCredProgCriteria)).ToList();
                        if (acadCredLimitingKeys == null || !acadCredLimitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        if (acadCredLimitingKeys.Any())
                        {
                            var acadCredData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredLimitingKeys.ToArray());
                            List<string> acadCredStuProgIds = new List<string>();
                            if (acadCredData.Any())
                            {
                                foreach (var cred in acadCredData)
                                {
                                    if (!string.IsNullOrEmpty(cred.AcadAcadProgram))
                                        acadCredStuProgIds.Add(cred.AcadPersonId + "*" + cred.AcadAcadProgram);
                                }
                                //merge it with existing list of student academic programs.
                                if (acadCredStuProgIds != null && acadCredStuProgIds.Any())
                                {
                                    if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                                    {
                                        stuProgsLimitingKeys = acadCredStuProgIds;
                                        if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }

                                    }
                                    else
                                    {
                                        stuProgsLimitingKeys.AddRange(acadCredStuProgIds);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region degree & ccd filter
                if ((ccdCredentials != null && ccdCredentials.Any()) || (degreeCredentials != null && degreeCredentials.Any()))
                {
                    var ccdCreds = ccdCredentials != null && ccdCredentials.Any() ? ccdCredentials.Distinct().ToList() : null;
                    var degrCreds = degreeCredentials != null && degreeCredentials.Any() ? degreeCredentials.Distinct().ToList() : null;
                    var credStuProgIds = await ApplyCredentialsFilter2(program, degrCreds, ccdCreds, stuProgsLimitingKeys, acadCredLimitingKeys, completeStatus, includeAcademicCredentials);
                    //merge it with existing list of student academic programs.
                    //if this returns the list of student programs then that becomes our list. 
                    if (credStuProgIds != null && credStuProgIds.Any())
                    {
                        if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                        {
                            stuProgsLimitingKeys = credStuProgIds;
                            if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                            {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            }

                        }
                        else
                        {
                            stuProgsLimitingKeys.AddRange(credStuProgIds);
                        }
                    }
                    else
                    {
                        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                    }
                }
                #endregion

                #region curriculumObjective
                if (curriculumObjective != CurriculumObjectiveCategory.NotSet)
                {
                    #region matriculated/coutcome
                    if ((curriculumObjective == CurriculumObjectiveCategory.Matriculated) || (curriculumObjective == CurriculumObjectiveCategory.Outcome))
                    {
                        if (string.IsNullOrEmpty(criteria))
                            criteria = "WITH STPR.START.DATE NE ''";
                        else
                            criteria += " AND WITH STPR.START.DATE NE ''";

                        var codeAssocs = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.Where(v => v.ValActionCode1AssocMember == "3");
                        int index = 0;
                        foreach (var codeAssoc in codeAssocs)
                        {
                            //matriculated - Select any STUDENT.PROGRAMS records where the START.DATE is populated and the STPR.CURRENT.STATUS is not one w/ a special processing code of 3.
                            if (curriculumObjective == CurriculumObjectiveCategory.Matriculated)
                            {
                                criteria += " AND WITH STPR.CURRENT.STATUS NE '" + codeAssoc.ValInternalCodeAssocMember + "'";
                            }
                            //outcome - Select any STUDENT.PROGRAMS records where the START.DATE is populated and the STPR.CURRENT.STATUS is a code w/ a special processing code of 3.
                            else
                            {
                                if (index == 0)
                                    criteria += " AND WITH STPR.CURRENT.STATUS EQ '" + codeAssoc.ValInternalCodeAssocMember + "'";
                                else
                                    criteria += "'" + codeAssoc.ValInternalCodeAssocMember + "'";
                            }
                            index++;
                        }
                    }
                    #endregion

                    #region recruiter/applied
                    if ((curriculumObjective == CurriculumObjectiveCategory.Recruited) || (curriculumObjective == CurriculumObjectiveCategory.Applied))
                    {
                        //select any STUDENT.PROGRAMS records where every STPR.START.DATE is null.
                        if (string.IsNullOrEmpty(criteria))
                            criteria = "WITH EVERY STPR.START.DATE EQ ''";
                        else
                            criteria += " AND WITH EVERY STPR.START.DATE EQ ''";

                        var allApplicationStatuses = await GetApplicationStatusesAsync();
                        if (allApplicationStatuses == null)
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }

                        string[] applStatusesSpCodeIds = null;

                        if (curriculumObjective == CurriculumObjectiveCategory.Recruited)
                        {
                            //Retrieve the APPLICATION.STATUSES codes that do not have any value in APPS.SPECIAL.PROCESSING.CODE. Then select all APPLICATIONS that have one of those statuses. 
                            applStatusesSpCodeIds = (allApplicationStatuses.Where(sp1 => string.IsNullOrEmpty(sp1.AppsSpecialProcessingCode)).Select(c => c.Recordkey)).ToArray();
                        }
                        else
                        {
                            // retrieve the APPLICATION.STATUSES codes where APPS.SPECIAL.PROCESSING.CODE is populated. Then select all APPLICATIONS that have one of those statuses.                                                  
                            applStatusesSpCodeIds = (allApplicationStatuses.Where(sp1 => !string.IsNullOrEmpty(sp1.AppsSpecialProcessingCode)).Select(c => c.Recordkey)).ToArray();
                        }

                        var appStatusCriteria = "WITH APPL.CURRENT.STATUS EQ '?' SAVING APPL.STUDENT.PROGRAMS.ID";
                        // get all the applicationId that meet the conditions for either recruiter (no SP codes) or applied (sp codes)
                        var applStudentProgramIds = await DataReader.SelectAsync("APPLICATIONS", appStatusCriteria, applStatusesSpCodeIds.Distinct().ToArray(), "?");
                        if (applStudentProgramIds == null || !applStudentProgramIds.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }

                        applStudentProgramIds = applStudentProgramIds.Distinct().ToArray();

                        var initialStuProgsLimitingKeys = new List<string>(stuProgsLimitingKeys);

                        if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                            stuProgsLimitingKeys = stuProgsLimitingKeys.Intersect(applStudentProgramIds).ToList();
                        else
                            stuProgsLimitingKeys.AddRange(applStudentProgramIds);

                        //if applied, and we dont have any limiting records, then we can return.  However, we need to continue to determine if there are potential recruited records
                        //that meet the criteria
                        if ((curriculumObjective == CurriculumObjectiveCategory.Applied) && (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any()))
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }

                        if (curriculumObjective == CurriculumObjectiveCategory.Recruited)
                        {
                            // we previously got the list of appls where the APPS.SPECIAL.PROCESSING.CODE was empty, now we need to retrieve the APPLICATIONS where APPS.SPECIAL.PROCESSING.CODE is populated.                                                   
                            // (ie - get all the applicationId that meet the conditions for applied (has sp codes))                                                 
                            applStatusesSpCodeIds = (allApplicationStatuses.Where(sp1 => !string.IsNullOrEmpty(sp1.AppsSpecialProcessingCode)).Select(c => c.Recordkey)).ToArray();

                            var appliedApplStudentProgramIds = await DataReader.SelectAsync("APPLICATIONS", appStatusCriteria, applStatusesSpCodeIds.Distinct().ToArray(), "?");
                            appliedApplStudentProgramIds = appliedApplStudentProgramIds.Distinct().ToArray();

                            // we have to get the entire list of student.programs (using the initial list of limiting keys) to determine which ones do not have an application record.
                            var allStudentProgramIDs = (await DataReader.SelectAsync("STUDENT.PROGRAMS", initialStuProgsLimitingKeys != null && initialStuProgsLimitingKeys.Any() ? initialStuProgsLimitingKeys.Distinct().ToArray() : null, criteria)).ToList();

                            var studentProgramsWithoutApplications = allStudentProgramIDs.Except(appliedApplStudentProgramIds);

                            if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                                stuProgsLimitingKeys = stuProgsLimitingKeys.Union(studentProgramsWithoutApplications).ToList();
                            else
                                stuProgsLimitingKeys.AddRange(studentProgramsWithoutApplications);

                            if (stuProgsLimitingKeys == null || !stuProgsLimitingKeys.Any())
                            {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                return new CacheSupport.KeyCacheRequirements()
                {
                    limitingKeys = stuProgsLimitingKeys,
                    criteria = criteria
                };
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }
        }


        /// <summary>
        /// GetStudentAcademicProgramsPersonFilter.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="filterPersonIds"></param>
        /// <param name="personFilter"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicProgramsPersonFilterAsync(int offset, int limit,
          string[] filterPersonIds = null, string personFilter = "", bool bypassCache = false)
        {
            if (filterPersonIds == null || !filterPersonIds.Any())
            {
                return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
            }

            var stuProgsLimitingKeys = new List<string>();

            int totalCount = 0;
            string criteria = "";
            string[] subList = null;

            try
            {
                string studentAcademicProgramPersonFilterCacheKey = CacheSupport.BuildCacheKey(AllStudentAcademicProgramsPersonFilterCache, filterPersonIds, personFilter);

                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    studentAcademicProgramPersonFilterCacheKey,
                    "",
                    offset,
                    limit,
                    AllStudentAcademicProgramsPersonFilterTimeout,
                    async () =>
                    {
                        List<string> IdsFromStuFil = new List<string>();

                        var columns = await DataReader.BatchReadRecordColumnsAsync("STUDENTS", filterPersonIds, new string[] { "STU.ACAD.PROGRAMS" });

                        foreach (KeyValuePair<string, Dictionary<string, string>> entry in columns)
                        {
                            var studentId = entry.Key;
                            foreach (KeyValuePair<string, string> stuPrograms in entry.Value)
                            {
                                var programs = stuPrograms.Value.Split(DmiString._VM);
                                foreach (var program in programs)
                                {
                                    if (!string.IsNullOrEmpty(program))
                                    {
                                        IdsFromStuFil.Add(string.Concat(studentId, "*", program));
                                    }
                                }
                            }
                        }

                        if (IdsFromStuFil != null && IdsFromStuFil.Any())
                            stuProgsLimitingKeys.AddRange(IdsFromStuFil);

                        CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                        {
                            limitingKeys = stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.Distinct().ToList() : null,
                            criteria = criteria.ToString(),
                        };

                        return requirements;
                    }
                );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), 0);
                }

                subList = keyCache.Sublist.ToArray();

                totalCount = keyCache.TotalCount.Value;

                var studentProgramData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", subList);

                if (studentProgramData.Equals(default(BulkReadOutput<DataContracts.StudentPrograms>)))
                {
                    return new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>(), totalCount);
                }
                if ((studentProgramData.InvalidKeys != null && studentProgramData.InvalidKeys.Any())
                        || (studentProgramData.InvalidRecords != null && studentProgramData.InvalidRecords.Any()))
                {
                    var repositoryException = new RepositoryException();

                    if (studentProgramData.InvalidKeys.Any())
                    {
                        repositoryException.AddErrors(studentProgramData.InvalidKeys
                            .Select(key => new RepositoryError("invalid.key",
                            string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                    }
                    if (studentProgramData.InvalidRecords.Any())
                    {
                        repositoryException.AddErrors(studentProgramData.InvalidRecords
                           .Select(r => new RepositoryError("invalid.record",
                           string.Format("Error: '{0}' ", r.Value))
                           { SourceId = r.Key }));
                    }
                    throw repositoryException;
                }

                var studentProgEntities = await BuildStudentAcademicPrograms3Async(studentProgramData.BulkRecordsRead, new Collection<AcadCredentials>());
                return new Tuple<IEnumerable<StudentAcademicProgram>, int>(studentProgEntities, totalCount);

            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetStudentAcademicProgramIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("StudentAcademicProgram GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("StudentAcademicProgram GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STUDENT.PROGRAMS" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new RepositoryException("GUID '" + guid + "' is not valid for student-academic-programs.");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Using a collection of Student Academic Program ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of Student Academic Program ids</param>
        /// <returns>Dictionary consisting of a Student Academic Program (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetStudentAcademicProgramGuidsCollectionAsync(IEnumerable<string> ids)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var stAcadProgramGuidCollection = new Dictionary<string, string>();

            var personGuidLookup = ids
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList()
                .ConvertAll(p => new RecordKeyLookup("STUDENT.PROGRAMS", p, false)).ToArray();
            var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!stAcadProgramGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        stAcadProgramGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
                catch (Exception ex) // Do not throw error.
                {
                    throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", "STUDENT.PROGRAMS"), ex);
                }
            }

            return stAcadProgramGuidCollection;
        }

        /// <summary>
        /// Returns Student.Program.Statuses valcode data
        /// </summary>
        /// <returns>Student.Program.Statuses Valcode Data contract.</returns>
        private async Task<ApplValcodes> GetStudentProgramStatusesAsync()
        {
            if (studentProgramStatuses != null)
            {
                return studentProgramStatuses;
            }

            // Overriding cache timeout to be 240.
            studentProgramStatuses = await GetOrAddToCacheAsync<ApplValcodes>("StudentProgramStatuses",
                async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access STUDENT.PROGRAM.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return studentProgramStatuses;
        }

        // Process credential filter
        // This will take the list of acadprograms, student programss & acad credentials list and return an appropriate student program list after applying the filter
        private async Task<List<string>> ApplyCredentialsFilter(string acadProgram, List<string> degreeCredentials, List<string> ccdCredentials, List<string> stuProgsLimitingKeys, List<string> acadCredLimitingKeys, string completeStatus)
        {
            var stuProg = new List<string>();
            string acadProgCriteria = string.Empty;
            string ccdCriteria = string.Empty;
            List<string> acadCredStuProgs = new List<string>();
            string acadCredCriteria = string.Empty;
            List<string> progLimitingKeys;
            if (!string.IsNullOrEmpty(acadProgram))
            {
                acadCredCriteria = string.Concat("WITH ACAD.ACAD.PROGRAM EQ '", acadProgram, "'");
                progLimitingKeys = new List<string> { acadProgram };
            }
            else
            {
                acadCredCriteria = "WITH ACAD.ACAD.PROGRAM NE ''";
                progLimitingKeys = stuProgsLimitingKeys.Select(x => x.Split('*')[1]).Where(y => !string.IsNullOrEmpty(y)).ToList();
            }
            string progCriteria = string.Concat("WITH STPR.START.DATE NE '' AND WITH STPR.CURRENT.STATUS NE ", completeStatus);
            if (ccdCredentials != null && ccdCredentials.Any())
            {
                foreach (var ccdCredential in ccdCredentials)
                {
                    if (string.IsNullOrEmpty(acadProgCriteria))
                    {
                        acadProgCriteria += "WITH ACPG.CCDS EQ '" + ccdCredential + "'";
                    }
                    else
                    {
                        acadProgCriteria += " AND WITH ACPG.CCDS EQ '" + ccdCredential + "'";
                    }

                    if (string.IsNullOrEmpty(acadCredCriteria))
                    {
                        acadCredCriteria += "WITH ACAD.CCD EQ '" + ccdCredential + "'";
                    }
                    else
                    {
                        acadCredCriteria += " AND WITH ACAD.CCD EQ '" + ccdCredential + "'";
                    }
                    if (string.IsNullOrEmpty(ccdCriteria))
                    {
                        ccdCriteria += string.Concat(progCriteria, " AND WITH STPR.CURRENT.ADDNL.CCDS EQ '" + ccdCredential + "'");
                    }
                    else
                    {
                        ccdCriteria += " AND WITH STPR.CURRENT.ADDNL.CCDS EQ '" + ccdCredential + "'";
                    }
                }
            }
            if (degreeCredentials != null && degreeCredentials.Any())
            {
                foreach (var degreeCredential in degreeCredentials)
                {
                    if (string.IsNullOrEmpty(acadProgCriteria))
                    {
                        acadProgCriteria += "WITH ACPG.DEGREE EQ '" + degreeCredential + "'";
                    }
                    else
                    {
                        acadProgCriteria += " AND WITH ACPG.DEGREE EQ '" + degreeCredential + "'";
                    }

                    if (string.IsNullOrEmpty(acadCredCriteria))
                    {
                        acadCredCriteria += "WITH ACAD.DEGREE EQ '" + degreeCredential + "'";
                    }
                    else
                    {
                        acadCredCriteria += " AND WITH ACAD.DEGREE EQ '" + degreeCredential + "'";
                    }
                }
            }
            //get student programs for those who have graduated from those ccds & degree
            if (!string.IsNullOrEmpty(acadCredCriteria))
            {
                var acadCredIds = (await DataReader.SelectAsync("ACAD.CREDENTIALS", acadCredLimitingKeys != null && acadCredLimitingKeys.Any() ? acadCredLimitingKeys.ToArray() : null, acadCredCriteria)).ToList();
                if (acadCredIds != null && acadCredIds.Any())
                {
                    var acadCredData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredIds.ToArray());
                    if (acadCredData != null && acadCredData.Any())
                    {
                        foreach (var cred in acadCredData)
                        {
                            if (!string.IsNullOrEmpty(cred.AcadAcadProgram) & !string.IsNullOrEmpty(cred.AcadPersonId))
                                acadCredStuProgs.Add(cred.AcadPersonId + "*" + cred.AcadAcadProgram);
                        }
                        //if there is stuProgsLimitingKeys list then we need to honor that
                        if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                            stuProg = stuProgsLimitingKeys.Intersect(acadCredStuProgs).ToList();
                        else
                            stuProg.AddRange(acadCredStuProgs);
                    }
                }
            }
            if (!string.IsNullOrEmpty(acadProgCriteria))
            {
                var acadProgramIds = (await DataReader.SelectAsync("ACAD.PROGRAMS", progLimitingKeys != null && progLimitingKeys.Any() ? progLimitingKeys.ToArray() : null, acadProgCriteria)).ToList();
                if (acadProgramIds != null && acadProgramIds.Any())
                {
                    var queryAttributeLimit = Configuration.ColleagueSDKParameters.QueryAttributeLimit;
                    if (queryAttributeLimit == 0) queryAttributeLimit = 100;
                    string[] studentProgramIds = null;
                    var stuprogQuery = string.Empty;
                    for (var i = 0; i < (acadProgramIds.Count / queryAttributeLimit) + 1; i++)
                    {
                        var dataToQuery = string.Empty;

                        // Retrieve the range of attributes
                        var filteredElements = acadProgramIds.Take(queryAttributeLimit * (i + 1)).Skip(i * queryAttributeLimit).ToArray();

                        // Concatenate the list of attributes in the specified range
                        dataToQuery = filteredElements.Aggregate(dataToQuery, (current, element) => current + string.Concat("'", element, "'"));
                        stuprogQuery = string.Concat(progCriteria, " AND WITH STPR.ACAD.PROGRAM EQ  ", dataToQuery);
                        if ((studentProgramIds == null) || (!studentProgramIds.Any()))
                            studentProgramIds = await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys.ToArray(), stuprogQuery);
                        else
                            studentProgramIds = studentProgramIds.Concat(await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys.ToArray(), stuprogQuery)).ToArray();
                    }
                    if (studentProgramIds != null && studentProgramIds.Any())
                        stuProg.AddRange(studentProgramIds);
                }


            }
            if (!string.IsNullOrEmpty(ccdCriteria))
            {
                var stuProgCcds = await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, ccdCriteria);
                if (stuProgCcds != null && stuProgCcds.Any())
                    stuProg.AddRange(stuProgCcds);
            }
            return stuProg.Distinct().ToList();
        }

        // Process the credential filter
        // This will take the list of acadprograms, student programss & acad credentials list and return an appropriate student program list after applying the filter
        private async Task<List<string>> ApplyCredentialsFilter2(string acadProgram, List<string> degreeCredentials, List<string> ccdCredentials, List<string> stuProgsLimitingKeys,
            List<string> acadCredLimitingKeys, string completeStatus, bool includeAcademicCredentials = true)
        {
            var stuProg = new List<string>();
            string acadProgCriteria = string.Empty;
            string ccdCriteria = string.Empty;
            List<string> acadCredStuProgs = new List<string>();
            string acadCredCriteria = string.Empty;
            List<string> progLimitingKeys;
            if (!string.IsNullOrEmpty(acadProgram))
            {
                acadCredCriteria = string.Concat("WITH ACAD.ACAD.PROGRAM EQ '", acadProgram, "'");
                progLimitingKeys = new List<string> { acadProgram };
            }
            else
            {
                acadCredCriteria = "WITH ACAD.ACAD.PROGRAM NE ''";
                progLimitingKeys = stuProgsLimitingKeys.Select(x => x.Split('*')[1]).Where(y => !string.IsNullOrEmpty(y)).ToList();
            }
            string progCriteria = string.Concat("WITH STPR.CURRENT.STATUS NE ", completeStatus);
            if (ccdCredentials != null && ccdCredentials.Any())
            {
                foreach (var ccdCredential in ccdCredentials)
                {
                    if (string.IsNullOrEmpty(acadProgCriteria))
                    {
                        acadProgCriteria += "WITH ACPG.CCDS EQ '" + ccdCredential + "'";
                    }
                    else
                    {
                        acadProgCriteria += " AND WITH ACPG.CCDS EQ '" + ccdCredential + "'";
                    }

                    if (includeAcademicCredentials)
                    {
                        if (string.IsNullOrEmpty(acadCredCriteria))
                        {
                            acadCredCriteria += "WITH ACAD.CCD EQ '" + ccdCredential + "'";
                        }
                        else
                        {
                            acadCredCriteria += " AND WITH ACAD.CCD EQ '" + ccdCredential + "'";
                        }
                    }
                    if (string.IsNullOrEmpty(ccdCriteria))
                    {
                        if (includeAcademicCredentials)
                        {
                            ccdCriteria += string.Concat(progCriteria, " AND WITH STPR.CURRENT.ADDNL.CCDS EQ '" + ccdCredential + "'");
                        }
                        else
                        {
                            ccdCriteria += "WITH STPR.CURRENT.ADDNL.CCDS EQ '" + ccdCredential + "'";
                        }
                    }
                    else
                    {
                        ccdCriteria += " AND WITH STPR.CURRENT.ADDNL.CCDS EQ '" + ccdCredential + "'";
                    }
                }
            }
            if (degreeCredentials != null && degreeCredentials.Any())
            {
                foreach (var degreeCredential in degreeCredentials)
                {
                    if (string.IsNullOrEmpty(acadProgCriteria))
                    {
                        acadProgCriteria += "WITH ACPG.DEGREE EQ '" + degreeCredential + "'";
                    }
                    else
                    {
                        acadProgCriteria += " AND WITH ACPG.DEGREE EQ '" + degreeCredential + "'";
                    }

                    if (includeAcademicCredentials)
                    {
                        if (string.IsNullOrEmpty(acadCredCriteria))
                        {
                            acadCredCriteria += "WITH ACAD.DEGREE EQ '" + degreeCredential + "'";
                        }
                        else
                        {
                            acadCredCriteria += " AND WITH ACAD.DEGREE EQ '" + degreeCredential + "'";
                        }
                    }
                }
            }
            //get student programs for those who have graduated from those ccds & degree
            if (includeAcademicCredentials && !string.IsNullOrEmpty(acadCredCriteria))
            {
                var acadCredIds = (await DataReader.SelectAsync("ACAD.CREDENTIALS", acadCredLimitingKeys != null && acadCredLimitingKeys.Any() ? acadCredLimitingKeys.ToArray() : null, acadCredCriteria)).ToList();
                if (acadCredIds != null && acadCredIds.Any())
                {
                    var acadCredData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredIds.ToArray());
                    if (acadCredData != null && acadCredData.Any())
                    {
                        foreach (var cred in acadCredData)
                        {
                            if (!string.IsNullOrEmpty(cred.AcadAcadProgram) & !string.IsNullOrEmpty(cred.AcadPersonId))
                                acadCredStuProgs.Add(cred.AcadPersonId + "*" + cred.AcadAcadProgram);
                        }
                        //if there is stuProgsLimitingKeys list then we need to honor that
                        if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                            stuProg = stuProgsLimitingKeys.Intersect(acadCredStuProgs).ToList();
                        else
                            stuProg.AddRange(acadCredStuProgs);
                    }
                }
            }
            if (!string.IsNullOrEmpty(acadProgCriteria))
            {
                var acadProgramIds = (await DataReader.SelectAsync("ACAD.PROGRAMS", progLimitingKeys != null && progLimitingKeys.Any() ? progLimitingKeys.ToArray() : null, acadProgCriteria)).ToList();
                if (acadProgramIds != null && acadProgramIds.Any())
                {
                    var queryAttributeLimit = Configuration.ColleagueSDKParameters.QueryAttributeLimit;
                    if (queryAttributeLimit == 0) queryAttributeLimit = 100;
                    string[] studentProgramIds = null;
                    var stuprogQuery = string.Empty;
                    for (var i = 0; i < (acadProgramIds.Count / queryAttributeLimit) + 1; i++)
                    {
                        var dataToQuery = string.Empty;

                        // Retrieve the range of attributes
                        var filteredElements = acadProgramIds.Take(queryAttributeLimit * (i + 1)).Skip(i * queryAttributeLimit).ToArray();

                        // Concatenate the list of attributes in the specified range
                        dataToQuery = filteredElements.Aggregate(dataToQuery, (current, element) => current + string.Concat("'", element, "'"));
                        if (includeAcademicCredentials)
                        {
                            stuprogQuery = string.Concat(progCriteria, " AND WITH STPR.ACAD.PROGRAM EQ  ", dataToQuery);
                        }
                        else
                        {
                            stuprogQuery = string.Concat("WITH STPR.ACAD.PROGRAM EQ  ", dataToQuery);
                        }
                        if ((studentProgramIds == null) || (!studentProgramIds.Any()))
                            studentProgramIds = await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys.ToArray(), stuprogQuery);
                        else
                            studentProgramIds = studentProgramIds.Concat(await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys.ToArray(), stuprogQuery)).ToArray();
                    }
                    if (studentProgramIds != null && studentProgramIds.Any())
                        stuProg.AddRange(studentProgramIds);
                }


            }
            if (!string.IsNullOrEmpty(ccdCriteria))
            {
                var stuProgCcds = await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, ccdCriteria);
                if (stuProgCcds != null && stuProgCcds.Any())
                    stuProg.AddRange(stuProgCcds);
            }
            return stuProg.Distinct().ToList();
        }

        /// <summary>
        /// Returns application statuses data and update the cache
        /// </summary>
        /// <returns>ApplicationStatuses Data contract.</returns>
        private async Task<List<ApplicationStatuses>> GetApplicationStatusesAsync()
        {
            if (allAppStatuses != null)
            {
                return allAppStatuses;
            }

            // Overriding cache timeout to be 240.
            allAppStatuses = await GetOrAddToCacheAsync<List<ApplicationStatuses>>("StudentProgramApplicationStatuses",
                async () =>
                {
                    List<ApplicationStatuses> applStat = (await DataReader.BulkReadRecordAsync<ApplicationStatuses>("APPLICATION.STATUSES", "")).ToList();
                    if (applStat == null)
                    {
                        var errorMessage = "Unable to access APPLICATION.STATUSES table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return applStat;
                }, Level1CacheTimeoutValue);
            return allAppStatuses;
        }

        /// <summary>
        /// Builds StudentProgram Entity
        /// </summary>
        /// <param name="studentProgramData">Student Programs Data contracts</param>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Returns StudentProgram</returns>
        private async Task<IEnumerable<StudentAcademicProgram>> BuildStudentAcademicPrograms2Async(Collection<StudentPrograms> studentProgramData, Collection<AcadCredentials> acadCredentialsData)
        {
            //get needed reference data
            List<StudentAcademicProgram> stuAcadPrograms = new List<StudentAcademicProgram>();

            //if no studentacadprograms passed in return empty list
            if (!studentProgramData.Any())
            {
                return stuAcadPrograms;
            }

            //get academic program data for all the programs in the list
            string[] progCodes = studentProgramData.Select(p => p.Recordkey).Where(x => !string.IsNullOrEmpty(x) && x.Contains("*")).Select(x => x.Split('*')[1]).Where(y => !string.IsNullOrEmpty(y)).ToArray();
            Collection<AcadPrograms> acadProgramCollection = new Collection<AcadPrograms>();
            if (progCodes != null && progCodes.Any())
            {
                acadProgramCollection = await DataReader.BulkReadRecordAsync<AcadPrograms>(progCodes);
            }
            //get the list of student IDs
            string[] studentIds = studentProgramData.Select(p => p.Recordkey).Where(x => !string.IsNullOrEmpty(x) && x.Contains("*")).Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToArray();
            //process each of the student program records           

            foreach (var stuProg in studentProgramData)
            {
                try
                {
                    string catcode = stuProg.StprCatalog;
                    string studentid = stuProg.Recordkey.Split('*')[0];
                    string progcode = stuProg.Recordkey.Split('*')[1];
                    string guid = stuProg.RecordGuid;
                    string status = string.Empty;
                    if (stuProg.StprStatus.Any())
                    {
                        status = stuProg.StprStatus.ElementAt(0);
                    }
                    else
                    {
                        //in Colleague, if status is missing, it defaults to active if there is no end date and to withdrawn if there is end date.
                        if (stuProg.StprEndDate.Any())
                        {
                            if (stuProg.StprEndDate.ElementAt(0) != null)
                            {
                                var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "4");
                                if (codeAssoc != null)
                                {
                                    status = codeAssoc.ValInternalCodeAssocMember;
                                }
                            }
                            else
                            {
                                var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "2");
                                if (codeAssoc != null)
                                {
                                    status = codeAssoc.ValInternalCodeAssocMember;
                                }
                            }
                        }
                        else
                        {
                            var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "2");
                            if (codeAssoc != null)
                            {
                                status = codeAssoc.ValInternalCodeAssocMember;
                            }
                        }
                    }
                    DateTime startDate = new DateTime();
                    if (stuProg.StprStartDate != null && stuProg.StprStartDate.Any())
                    {
                        var studentProgramStartDate = stuProg.StprStartDate.ElementAt(0);
                        if (studentProgramStartDate != null && studentProgramStartDate != DateTime.MinValue)
                        {
                            startDate = studentProgramStartDate.Value;
                        }
                    }

                    StudentAcademicProgram studentAcadProgEntity = new StudentAcademicProgram(studentid, progcode, catcode, guid, startDate, status);
                    studentAcadProgEntity.AnticipatedCompletionDate = stuProg.StprAntCmplDate;
                    if (stuProg.StprEndDate != null && stuProg.StprEndDate.Any())
                    {
                        var studentProgramEndDate = stuProg.StprEndDate.ElementAt(0);
                        if (studentProgramEndDate != null && studentProgramEndDate != DateTime.MinValue)
                        {
                            studentAcadProgEntity.EndDate = studentProgramEndDate;
                        }
                    }
                    studentAcadProgEntity.DepartmentCode = stuProg.StprDept;
                    studentAcadProgEntity.Location = stuProg.StprLocation;
                    studentAcadProgEntity.StartTerm = stuProg.StprIntgStartTerm;
                    studentAcadProgEntity.AnticipatedCompletionTerm = stuProg.StprIntgAntCmplTerm;
                    studentAcadProgEntity.AnticipatedCompletionDate = stuProg.StprAntCmplDate;
                    //get academic level
                    AcadPrograms acadProgramData = null;
                    if (acadProgramCollection != null && acadProgramCollection.Any())
                    {
                        acadProgramData = acadProgramCollection.FirstOrDefault(a => a.Recordkey == progcode);
                        if (acadProgramData != null)
                        {
                            studentAcadProgEntity.AcademicLevelCode = acadProgramData.AcpgAcadLevel;
                        }
                    }
                    // get data from ACAD.CREDENTIALS. If the student has already graduated and has a record in ACAD.CREDENTIALS
                    // then we need to display the credentials and disciplines from ACAD.CREDENTIALS instead of ACAD.PROGRAMS and STUDENT.PROGRAMS
                    AcadCredentials acadCredential = null;
                    if (acadCredentialsData != null && acadCredentialsData.Any())
                    {
                        acadCredential = acadCredentialsData.FirstOrDefault(cred => cred.AcadPersonId == studentid && cred.AcadAcadProgram == progcode);
                    }
                    if (acadCredential != null)
                    {
                        foreach (var honor in acadCredential.AcadHonors)
                        {
                            studentAcadProgEntity.AddHonors(honor);
                        }
                        studentAcadProgEntity.GradGPA = acadCredential.AcadGpa;
                        studentAcadProgEntity.GraduationDate = acadCredential.AcadEndDate;
                        //ACAD.DEGREE.DATE and, if null, should publish the first value in ACAD.CCD.DATE.  
                        if (acadCredential.AcadDegreeDate.HasValue)
                            studentAcadProgEntity.CredentialsDate = acadCredential.AcadDegreeDate;
                        else
                        {
                            if (acadCredential.AcadCcdDate != null && acadCredential.AcadCcdDate.Any())
                                studentAcadProgEntity.CredentialsDate = acadCredential.AcadCcdDate.FirstOrDefault();
                        }
                        studentAcadProgEntity.ThesisTitle = acadCredential.AcadThesis;
                        studentAcadProgEntity.DegreeCode = acadCredential.AcadDegree;
                        studentAcadProgEntity.GradTerm = acadCredential.AcadTerm;
                        // Add majors from the Academic Credentials
                        if (acadCredential.AcadMajors.Any())
                        {
                            foreach (var mjr in acadCredential.AcadMajors)
                            {
                                studentAcadProgEntity.AddMajors(mjr);
                            }
                        }
                        // Add minors from the Academic Credentials
                        if (acadCredential.AcadMinors.Any())
                        {
                            foreach (var minr in acadCredential.AcadMinors)
                            {
                                studentAcadProgEntity.AddMinors(minr);
                            }
                        }
                        // Add specializations from the Academic Credentials
                        if (acadCredential.AcadSpecialization.Any())
                        {
                            foreach (var sp in acadCredential.AcadSpecialization)
                            {
                                studentAcadProgEntity.AddSpecializations(sp);
                            }
                        }
                        // Add ccds from the Academic Credentials
                        if (acadCredential.AcadCcd.Any())
                        {
                            foreach (var ccd in acadCredential.AcadCcd)
                            {
                                studentAcadProgEntity.AddCcds(ccd);
                            }
                        }
                    }

                    else
                    {

                        if (acadProgramData != null)
                        {
                            studentAcadProgEntity.AcademicLevelCode = acadProgramData.AcpgAcadLevel;
                            studentAcadProgEntity.DegreeCode = acadProgramData.AcpgDegree;
                            // Add majors from the Academic Program
                            if (acadProgramData.AcpgMajors.Any())
                            {
                                foreach (var mjr in acadProgramData.AcpgMajors)
                                {
                                    studentAcadProgEntity.AddMajors(mjr);
                                }
                            }
                            // Add minors from the Academic Program
                            if (acadProgramData.AcpgMinors.Any())
                            {
                                foreach (var minr in acadProgramData.AcpgMinors)
                                {
                                    studentAcadProgEntity.AddMinors(minr);
                                }
                            }
                            // Add specializations from the Academic Program
                            if (acadProgramData.AcpgSpecializations.Any())
                            {
                                foreach (var sp in acadProgramData.AcpgSpecializations)
                                {
                                    studentAcadProgEntity.AddSpecializations(sp);
                                }
                            }
                            // Add ccds from the Academic Program
                            if (acadProgramData.AcpgCcds.Any())
                            {
                                foreach (var ccd in acadProgramData.AcpgCcds)
                                {
                                    studentAcadProgEntity.AddCcds(ccd);
                                }
                            }
                        }

                        // Additional Requirements from student programs
                        // Add majors
                        if (stuProg.StprMajorListEntityAssociation != null && stuProg.StprMajorListEntityAssociation.Any())
                        {
                            foreach (var mjr in stuProg.StprMajorListEntityAssociation)
                            {
                                if (mjr.StprAddnlMajorStartDateAssocMember <= DateTime.Now && mjr.StprAddnlMajorEndDateAssocMember == null || mjr.StprAddnlMajorEndDateAssocMember > DateTime.Now)
                                {
                                    studentAcadProgEntity.AddMajors(mjr.StprAddnlMajorsAssocMember);
                                }
                            }
                        }
                        // Add minors 
                        if (stuProg.StprMinorListEntityAssociation != null && stuProg.StprMinorListEntityAssociation.Any())
                        {
                            foreach (var minr in stuProg.StprMinorListEntityAssociation)
                            {
                                if (minr.StprMinorStartDateAssocMember <= DateTime.Now && minr.StprMinorEndDateAssocMember == null || minr.StprMinorEndDateAssocMember > DateTime.Now)
                                {
                                    studentAcadProgEntity.AddMinors(minr.StprMinorsAssocMember);
                                }
                            }
                        }
                        // Add specializations 
                        if (stuProg.StprSpecialtiesEntityAssociation != null && stuProg.StprSpecialtiesEntityAssociation.Any())
                        {
                            foreach (var sps in stuProg.StprSpecialtiesEntityAssociation)
                            {
                                if (sps.StprSpecializationStartAssocMember <= DateTime.Now && sps.StprSpecializationEndAssocMember == null || sps.StprSpecializationEndAssocMember > DateTime.Now)
                                {
                                    studentAcadProgEntity.AddSpecializations(sps.StprSpecializationsAssocMember);
                                }
                            }
                        }
                        // Add ccds 
                        if (stuProg.StprCcdListEntityAssociation != null && stuProg.StprCcdListEntityAssociation.Any())
                        {
                            foreach (var ccd in stuProg.StprCcdListEntityAssociation)
                            {
                                if (ccd.StprCcdsStartDateAssocMember <= DateTime.Now && ccd.StprCcdsEndDateAssocMember == null || ccd.StprCcdsEndDateAssocMember > DateTime.Now)
                                {
                                    studentAcadProgEntity.AddCcds(ccd.StprCcdsAssocMember);
                                }
                            }
                        }
                    }

                    //get credits earned
                    studentAcadProgEntity.CreditsEarned = stuProg.StprEvalCombinedCred;
                    stuAcadPrograms.Add(studentAcadProgEntity);
                }
                catch (ArgumentException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    throw new RepositoryException(string.Format("Could not build program {0} for student {1}", stuProg.Recordkey.Split('*')[1], stuProg.Recordkey.Split('*')[0]));
                }
            }
            return stuAcadPrograms;
        }

        /// <summary>
        /// Builds StudentProgram Entity
        /// </summary>
        /// <param name="studentProgramData">Student Programs Data contracts</param>
        /// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /// <returns>Returns StudentProgram</returns>
        private async Task<IEnumerable<StudentAcademicProgram>> BuildStudentAcademicPrograms3Async(Collection<StudentPrograms> studentProgramData, Collection<AcadCredentials> acadCredentialsData)
        {
            //get needed reference data
            var stuAcadPrograms = new List<StudentAcademicProgram>();

            //if no studentacadprograms passed in return empty list
            if (!studentProgramData.Any())
            {
                return stuAcadPrograms;
            }

            var exception = new RepositoryException();
            string[] studentIds = null;
            ApplValcodesVals codeAssoc3 = null;
            string[] progCodes = null;
            var acadProgramCollection = new Collection<AcadPrograms>();
            try
            {
                //get academic program data for all the programs in the list
                progCodes = studentProgramData.Select(p => p.Recordkey).Where(x => !string.IsNullOrEmpty(x) && x.Contains("*")).Select(x => x.Split('*')[1]).Where(y => !string.IsNullOrEmpty(y)).ToArray();

                if (progCodes != null && progCodes.Any())
                {
                    acadProgramCollection = await DataReader.BulkReadRecordAsync<AcadPrograms>(progCodes);
                }
                //get the list of student IDs
                studentIds = studentProgramData.Select(p => p.Recordkey).Where(x => !string.IsNullOrEmpty(x) && x.Contains("*")).Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToArray();

                codeAssoc3 = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "3");
            }
            catch (Exception ex)
            {
                exception.AddError(
                      new RepositoryError("Student.Program.Status.Error", ex.Message)
                      );
                throw exception;
            }


            //process each of the student program records           
            foreach (var stuProg in studentProgramData)
            {
                string guid = stuProg.RecordGuid;
                string id = stuProg.Recordkey;
                try
                {
                    #region build domain entity
                    string catcode = stuProg.StprCatalog;
                    string studentid = stuProg.Recordkey.Split('*')[0];
                    string progcode = stuProg.Recordkey.Split('*')[1];

                    string status = string.Empty;
                    if ((stuProg.StprStatus != null) && (stuProg.StprStatus.Any()))
                    {
                        status = stuProg.StprStatus.ElementAt(0);
                    }
                    else
                    {
                        //in Colleague, if status is missing, it defaults to active if there is no end date and to withdrawn if there is end date.
                        if ((stuProg.StprEndDate != null) && (stuProg.StprEndDate.Any()))
                        {
                            if (stuProg.StprEndDate.ElementAt(0) != null)
                            {
                                var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "4");
                                if (codeAssoc != null)
                                {
                                    status = codeAssoc.ValInternalCodeAssocMember;
                                }
                            }
                            else
                            {
                                var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "2");
                                if (codeAssoc != null)
                                {
                                    status = codeAssoc.ValInternalCodeAssocMember;
                                }
                            }
                        }
                        else
                        {
                            var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "2");
                            if (codeAssoc != null)
                            {
                                status = codeAssoc.ValInternalCodeAssocMember;
                            }
                        }
                    }
                    DateTime startDate = new DateTime();
                    if (stuProg.StprStartDate != null && stuProg.StprStartDate.Any())
                    {
                        var studentProgramStartDate = stuProg.StprStartDate.ElementAt(0);
                        if (studentProgramStartDate != null && studentProgramStartDate != DateTime.MinValue)
                        {
                            startDate = studentProgramStartDate.Value;
                        }
                    }

                    StudentAcademicProgram studentAcadProgEntity = new StudentAcademicProgram(studentid, progcode, catcode, guid, startDate, status);
                    studentAcadProgEntity.AnticipatedCompletionDate = stuProg.StprAntCmplDate;
                    if (stuProg.StprEndDate != null && stuProg.StprEndDate.Any())
                    {
                        var studentProgramEndDate = stuProg.StprEndDate.ElementAt(0);
                        if (studentProgramEndDate != null && studentProgramEndDate != DateTime.MinValue)
                        {
                            studentAcadProgEntity.EndDate = studentProgramEndDate;
                        }
                    }
                    studentAcadProgEntity.DepartmentCode = stuProg.StprDept;
                    studentAcadProgEntity.Location = stuProg.StprLocation;
                    studentAcadProgEntity.StartTerm = stuProg.StprIntgStartTerm;
                    studentAcadProgEntity.AnticipatedCompletionTerm = stuProg.StprIntgAntCmplTerm;
                    studentAcadProgEntity.AnticipatedCompletionDate = stuProg.StprAntCmplDate;
                    //get academic level
                    AcadPrograms acadProgramData = null;
                    if (acadProgramCollection != null && acadProgramCollection.Any())
                    {
                        acadProgramData = acadProgramCollection.FirstOrDefault(a => a.Recordkey == progcode);
                        if (acadProgramData != null)
                        {
                            studentAcadProgEntity.AcademicLevelCode = acadProgramData.AcpgAcadLevel;
                        }
                    }
                    // get data from ACAD.CREDENTIALS. If the student has already graduated and has a record in ACAD.CREDENTIALS
                    // then we need to display the credentials and disciplines from ACAD.CREDENTIALS instead of ACAD.PROGRAMS and STUDENT.PROGRAMS
                    AcadCredentials acadCredential = null;
                    if (acadCredentialsData != null && acadCredentialsData.Any())
                    {
                        acadCredential = acadCredentialsData.FirstOrDefault(cred => cred.AcadPersonId == studentid && cred.AcadAcadProgram == progcode);
                    }
                    if (acadCredential != null)
                    {
                        foreach (var honor in acadCredential.AcadHonors)
                        {
                            studentAcadProgEntity.AddHonors(honor);
                        }
                        studentAcadProgEntity.GradGPA = acadCredential.AcadGpa;
                        studentAcadProgEntity.GraduationDate = acadCredential.AcadEndDate;
                        //ACAD.DEGREE.DATE and, if null, should publish the first value in ACAD.CCD.DATE.  
                        if (acadCredential.AcadDegreeDate.HasValue)
                            studentAcadProgEntity.CredentialsDate = acadCredential.AcadDegreeDate;
                        else
                        {
                            if (acadCredential.AcadCcdDate != null && acadCredential.AcadCcdDate.Any())
                                studentAcadProgEntity.CredentialsDate = acadCredential.AcadCcdDate.FirstOrDefault();
                        }
                        studentAcadProgEntity.ThesisTitle = acadCredential.AcadThesis;
                        studentAcadProgEntity.DegreeCode = acadCredential.AcadDegree;
                        studentAcadProgEntity.GradTerm = acadCredential.AcadTerm;
                        // Add majors from the Academic Credentials



                        if (acadCredential.AcadMajors.Any())
                        {
                            foreach (var mjr in acadCredential.AcadMajors)
                            {
                                studentAcadProgEntity.AddMajors(mjr);
                            }
                        }
                        // Add minors from the Academic Credentials
                        if (acadCredential.AcadMinors.Any())
                        {
                            foreach (var minr in acadCredential.AcadMinors)
                            {
                                studentAcadProgEntity.AddMinors(minr);
                            }
                        }
                        // Add specializations from the Academic Credentials
                        if (acadCredential.AcadSpecialization.Any())
                        {
                            foreach (var sp in acadCredential.AcadSpecialization)
                            {
                                studentAcadProgEntity.AddSpecializations(sp);
                            }
                        }
                        // Add ccds from the Academic Credentials
                        if (acadCredential.AcadCcd.Any())
                        {
                            foreach (var ccd in acadCredential.AcadCcd)
                            {
                                studentAcadProgEntity.AddCcds(ccd);
                            }
                        }

                    }

                    else
                    {

                        if (acadProgramData != null)
                        {
                            studentAcadProgEntity.AcademicLevelCode = acadProgramData.AcpgAcadLevel;
                            studentAcadProgEntity.DegreeCode = acadProgramData.AcpgDegree;
                            // Add majors from the Academic Program
                            if (acadProgramData.AcpgMajors.Any())
                            {
                                foreach (var mjr in acadProgramData.AcpgMajors)
                                {
                                    studentAcadProgEntity.AddMajors(mjr);
                                }
                            }
                            // Add minors from the Academic Program
                            if (acadProgramData.AcpgMinors.Any())
                            {
                                foreach (var minr in acadProgramData.AcpgMinors)
                                {
                                    studentAcadProgEntity.AddMinors(minr);
                                }
                            }
                            // Add specializations from the Academic Program
                            if (acadProgramData.AcpgSpecializations.Any())
                            {
                                foreach (var sp in acadProgramData.AcpgSpecializations)
                                {
                                    studentAcadProgEntity.AddSpecializations(sp);
                                }
                            }
                            // Add ccds from the Academic Program
                            if (acadProgramData.AcpgCcds.Any())
                            {
                                foreach (var ccd in acadProgramData.AcpgCcds)
                                {
                                    studentAcadProgEntity.AddCcds(ccd);
                                }
                            }
                        }

                        // Additional Requirements from student programs
                        // Add majors
                        if (stuProg.StprMajorListEntityAssociation != null && stuProg.StprMajorListEntityAssociation.Any())
                        {
                            foreach (var mjr in stuProg.StprMajorListEntityAssociation)
                            {
                                studentAcadProgEntity.AddMajors(mjr.StprAddnlMajorsAssocMember, mjr.StprAddnlMajorStartDateAssocMember, mjr.StprAddnlMajorEndDateAssocMember);
                            }
                        }
                        // Add minors 
                        if (stuProg.StprMinorListEntityAssociation != null && stuProg.StprMinorListEntityAssociation.Any())
                        {
                            foreach (var minr in stuProg.StprMinorListEntityAssociation)
                            {
                                studentAcadProgEntity.AddMinors(minr.StprMinorsAssocMember, minr.StprMinorStartDateAssocMember, minr.StprMinorEndDateAssocMember);
                            }
                        }
                        // Add specializations 
                        if (stuProg.StprSpecialtiesEntityAssociation != null && stuProg.StprSpecialtiesEntityAssociation.Any())
                        {
                            foreach (var sps in stuProg.StprSpecialtiesEntityAssociation)
                            {
                                studentAcadProgEntity.AddSpecializations(sps.StprSpecializationsAssocMember, sps.StprSpecializationStartAssocMember, sps.StprSpecializationEndAssocMember);
                            }
                        }
                        // Add ccds 
                        if (stuProg.StprCcdListEntityAssociation != null && stuProg.StprCcdListEntityAssociation.Any())
                        {
                            var stprCCDList = stuProg.StprCcdListEntityAssociation
                                             .Where(l => l.StprCcdsStartDateAssocMember.HasValue && (!l.StprCcdsEndDateAssocMember.HasValue ||
                                                        (l.StprCcdsEndDateAssocMember.HasValue && l.StprCcdsEndDateAssocMember.Value >= DateTime.Today)));
                            foreach (var ccd in stprCCDList)
                            {
                                studentAcadProgEntity.AddCcds(ccd.StprCcdsAssocMember);
                            }
                        }
                    }

                    //get credits earned
                    studentAcadProgEntity.CreditsEarned = stuProg.StprEvalCombinedCred;
                    studentAcadProgEntity.AdmitStatus = stuProg.StprAdmitStatus;


                    var curriculumObjective = CurriculumObjectiveCategory.NotSet;

                    if (startDate == new DateTime())
                    {
                        var applicationStatus = string.Empty;

                        //If the STPR.START.DATE is not populated, then read the corresponding APPLICATIONS record by selecting the APPL.APPLICANT 
                        // that matches the student's ID (STPR.STUDENT) and the APPL.ACAD.PROGRAM that matches the current program (STPR.ACAD.PROGRAM). 
                        // From APPLICATIONS, retrieve the current APPL.STATUS (the first value is the most current status)
                        var applicationDataContract = await DataReader.BulkReadRecordAsync<Applications>("APPLICATIONS", "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "'");

                        if (applicationDataContract != null && applicationDataContract.Any())
                        {
                            var appStatAssoc = new List<ApplicationsApplStatuses>();
                            foreach (var app in applicationDataContract)
                            {
                                appStatAssoc.AddRange(app.ApplStatusesEntityAssociation);
                            }

                            var appStatAssocOrdered = appStatAssoc.OrderByDescending(x => x.ApplStatusDateAssocMember).ThenByDescending(y => y.ApplStatusTimeAssocMember);
                            if (appStatAssocOrdered != null && appStatAssocOrdered.Any())
                            {
                                applicationStatus = appStatAssocOrdered.FirstOrDefault().ApplStatusAssocMember;
                            }
                        }

                        if ((applicationStatus == null) || !(applicationStatus.Any()))
                        {
                            curriculumObjective = CurriculumObjectiveCategory.Recruited;
                        }
                        else
                        {
                            ApplicationStatuses appStatusRecord = null;
                            var applicationStatusRecords = await GetApplicationStatusesAsync();
                            if (applicationStatusRecords != null && applicationStatusRecords.Any())
                            {
                                appStatusRecord = applicationStatusRecords.FirstOrDefault(x => x.Recordkey == applicationStatus);
                            }
                            curriculumObjective = (appStatusRecord != null && !string.IsNullOrEmpty(appStatusRecord.AppsSpecialProcessingCode))
                                    ? CurriculumObjectiveCategory.Applied : CurriculumObjectiveCategory.Recruited;
                        }
                    }
                    else
                    {
                        //If the STPR.START.DATE is populated, then make the following checks:
                        //   If the STPR.STATUS < 1,1 > has a special processing code of 3, then use the enumeration "outcome" here.
                        //  Otherwise, use the enumeration "matriculated"

                        curriculumObjective = ((codeAssoc3 != null) && (status == codeAssoc3.ValInternalCodeAssocMember))
                            ? CurriculumObjectiveCategory.Outcome : CurriculumObjectiveCategory.Matriculated;
                    }

                    studentAcadProgEntity.CurriculumObjective = curriculumObjective;
                    #endregion

                    stuAcadPrograms.Add(studentAcadProgEntity);
                }
                catch (Exception ex)
                {
                    //throw new RepositoryException(string.Format("Could not build program {0} for student {1}", stuProg.Recordkey.Split('*')[1], stuProg.Recordkey.Split('*')[0]));
                    exception.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = guid,
                           SourceId = id
                       });
                    //throw new RepositoryException(string.Format("Could not build program {0} for student {1}", stuProg.Recordkey.Split('*')[1], stuProg.Recordkey.Split('*')[0]));
                    exception.AddError(
                       new RepositoryError("Bad.Data", string.Format("Could not build program {0} for student {1}", stuProg.Recordkey.Split('*')[1], stuProg.Recordkey.Split('*')[0]))
                       {
                           Id = guid,
                           SourceId = id
                       });
                }
            }

            if (exception.Errors.Any())
            {
                throw exception;
            }

            return stuAcadPrograms;
        }

    }
}