// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAcademicProgramRepository : BaseColleagueRepository, IStudentAcademicProgramRepository
    {
        private ApplValcodes studentProgramStatuses;
                public StudentAcademicProgramRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {

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

        ///// <summary>
        ///// Returns studentProgram Entity as per the select criteria
        ///// </summary>
        ///// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        /////  <param name="program">academic program Name Contains ...program...</param>
        /////  <param name="startDate">Student Academic Program starts on or after this date</param>
        /////  <param name="endDate">Student Academic Program ends on or before this date</param>
        ///// <param name="student">Id of the student enrolled on the academic program</param>
        ///// <param name="catalog">Student Academic Program catalog  equal to</param>
        ///// <param name="Status">Student Academic Program status equals to </param>
        ///// <param name="programOwner">Student Academic Program programOwner equals to </param>
        ///// <param name="site">Student Academic Program site equals to </param>
        ///// <param name="graduatedOn">Student Academic Program graduatedOn equals to </param>
        ///// <param name="ccdCredential">Student Academic Program ccdCredential equals to </param>
        ///// <param name="degreeCredential">Student Academic Program degreeCredential equals to </param>
        ///// <returns>StudentProgram Entities</returns>
        //public async Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicProgramsAsync(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
        //    string program = "", string startDate = "", string endDate = "", string student = "", string catalog = "", string status = "",
        //    string programOwner = "", string site = "", string academicLevel = "", string graduatedOn = "", string ccdCredential = "", string degreeCredential = "", string graduatedAcademicPeriod = "", string completeStatus = "")
        //{
        //    try
        //    {
        //        string criteria = "WITH STPR.STUDENT NE '' AND STPR.ACAD.PROGRAM NE '' AND STPR.START.DATE NE ''";
        //        var oldCriteria = criteria;
        //        string acadProgCriteria = string.Empty;
        //        string acadCredProgCriteria = string.Empty;
        //        string acadCredCriteria = string.Empty;
        //        List<string> acadCredStuProgIds = new List<string>();
        //        string[] studentProgramIds = new string[] { };
        //        string[] acadStuProgIds = new string[] { };
        //        string[] stuProgIds = new string[] { };
        //        if (!string.IsNullOrEmpty(program))
        //        {
        //            criteria += " AND WITH STPR.ACAD.PROGRAM EQ '" + program + "'";
        //        }
        //        if (!string.IsNullOrEmpty(startDate))
        //        {
        //            criteria += " AND WITH STPR.LATEST.START.DATE GE '" + startDate + "'";
        //        }
        //        if (!string.IsNullOrEmpty(endDate))
        //        {
        //            criteria += " AND WITH STPR.CURRENT.END.DATE NE '' AND WITH STPR.CURRENT.END.DATE LE '" + endDate + "'";
        //        }
        //        if (!string.IsNullOrEmpty(student))
        //        {
        //            criteria += " AND WITH STPR.STUDENT EQ '" + student + "'";
        //        }
        //        if (!string.IsNullOrEmpty(catalog))
        //        {
        //            criteria += " AND WITH STPR.CATALOG EQ '" + catalog + "'";
        //        }
        //        if (!string.IsNullOrEmpty(status))
        //        {
        //            criteria += " AND WITH STPR.CURRENT.STATUS EQ " + status;
        //        }
        //        if (!string.IsNullOrEmpty(programOwner))
        //        {
        //            criteria += " AND WITH STPR.DEPT EQ '" + programOwner + "'";
        //        }
        //        if (!string.IsNullOrEmpty(site))
        //        {
        //            criteria += " AND WITH STPR.LOCATION EQ '" + site + "'";
        //        }
        //        if (!string.IsNullOrEmpty(academicLevel))
        //        {
        //            criteria += " AND WITH STPR.ACAD.LEVEL EQ '" + academicLevel + "'";
        //        }
        //        if (!string.IsNullOrEmpty(graduatedOn))
        //        {
        //            if (string.IsNullOrEmpty(acadCredProgCriteria))
        //                acadCredProgCriteria += "WITH ACAD.END.DATE EQ '" + graduatedOn + "'";
        //        }
        //        if (!string.IsNullOrEmpty(graduatedAcademicPeriod))
        //        {
        //            if (string.IsNullOrEmpty(acadCredProgCriteria))
        //                acadCredProgCriteria += "WITH ACAD.TERM EQ '" + graduatedAcademicPeriod + "'";
        //            else
        //                acadCredProgCriteria += " AND WITH ACAD.TERM EQ '" + graduatedAcademicPeriod + "'";
        //        }
        //        if (!string.IsNullOrEmpty(ccdCredential))
        //        {
        //            if (string.IsNullOrEmpty(acadProgCriteria))
        //                acadProgCriteria += "WITH ACPG.CCDS EQ '" + ccdCredential + "'";
        //            acadCredCriteria += "WITH ACAD.DEGREE.DATE NE '' AND WITH ACAD.CCD EQ '" + ccdCredential + "'";
        //        }
        //        if (!string.IsNullOrEmpty(degreeCredential))
        //        {
        //            if (string.IsNullOrEmpty(acadProgCriteria))
        //                acadProgCriteria += "WITH ACPG.DEGREE EQ '" + degreeCredential + "'";
        //            else
        //                acadProgCriteria += " AND WITH ACPG.DEGREE EQ '" + degreeCredential + "'";
        //            if (string.IsNullOrEmpty(acadCredCriteria))
        //                acadCredCriteria += "WITH ACAD.DEGREE.DATE NE '' AND WITH ACAD.DEGREE EQ '" + degreeCredential + "'";
        //            else
        //                acadCredCriteria += " AND WITH ACAD.DEGREE EQ '" + degreeCredential + "'";
        //        }

        //        //get student Program IDs from ACAD.CREDENTIALS
        //        if (!string.IsNullOrEmpty(acadCredProgCriteria))
        //        {
        //            var acadCredProgIds = await DataReader.SelectAsync("ACAD.CREDENTIALS", acadCredProgCriteria);
        //            if (acadCredProgIds.Any())
        //            {
        //                var acadCredData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredProgIds);
        //                if (acadCredData.Any())
        //                {
        //                    foreach (var cred in acadCredData)
        //                    {
        //                        if (!string.IsNullOrEmpty(cred.AcadAcadProgram))
        //                            acadCredStuProgIds.Add(cred.AcadPersonId + "*" + cred.AcadAcadProgram);
        //                    }
        //                }
        //            }
        //        }
        //        //get student program IDs from ACAD.PROGRAMS
        //        if (!string.IsNullOrEmpty(acadProgCriteria))
        //        {
        //            var acadProgramIds = await DataReader.SelectAsync("ACAD.PROGRAMS", acadProgCriteria);
        //            if (acadProgramIds.Any())
        //            {
        //                // we are excluding those student programs that have graduated
        //                var newcriteria = oldCriteria + " AND WITH STPR.CURRENT.STATUS NE " + completeStatus + " AND WITH STPR.ACAD.PROGRAM EQ '?'";
        //                acadStuProgIds = await DataReader.SelectAsync("STUDENT.PROGRAMS", newcriteria, acadProgramIds);
        //            }
        //            if (!string.IsNullOrEmpty(ccdCredential))
        //            {
        //                // we are excluding those student programs that have graduated
        //                var ccdCriteria = oldCriteria + " AND WITH STPR.CURRENT.STATUS NE " + completeStatus + " AND WITH STPR.CCDS EQ '" + ccdCredential + "'";
        //                var stuProgCcds = await DataReader.SelectAsync("STUDENT.PROGRAMS", ccdCriteria);
        //                acadStuProgIds = acadStuProgIds.Union(stuProgCcds).ToArray();

        //            }
        //            //we need to include the ACAD.CREDENTIALS record for those who have graduated.
        //            if (!string.IsNullOrEmpty(acadCredCriteria))
        //            {
        //                var acadCredProgIds = await DataReader.SelectAsync("ACAD.CREDENTIALS", acadCredCriteria);
        //                List<string> acadCredStuProgs = new List<string>();
        //                if (acadCredProgIds.Any())
        //                {
        //                    var acadCredData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredProgIds);
        //                    if (acadCredData.Any())
        //                    {
        //                        foreach (var cred in acadCredData)
        //                        {
        //                            if (!string.IsNullOrEmpty(cred.AcadAcadProgram))
        //                                acadCredStuProgs.Add(cred.AcadPersonId + "*" + cred.AcadAcadProgram);
        //                        }
        //                        acadStuProgIds = acadStuProgIds.Union(acadCredStuProgs).ToArray();
        //                    }
        //                }
        //            }

        //        }

        //        //at this stage, we have student program IDs from ACAD.PROGRAMS & ACAD.CREDENTIALS
        //        //for GET ALL without any filter, the old criteria will be same as criteria and there is no 

        //        if (!acadCredStuProgIds.Any() && criteria.Equals(oldCriteria) && !acadStuProgIds.Any())
        //        {
        //            studentProgramIds = await DataReader.SelectAsync("STUDENT.PROGRAMS", criteria);
        //        }
        //        else
        //        {
        //            if (!criteria.Equals(oldCriteria))
        //            {
        //                stuProgIds = await DataReader.SelectAsync("STUDENT.PROGRAMS", criteria);
        //                //bypassCache = true;
        //            }
        //        }
        //        //intersect all the student program ids that we have got so far
        //        if (stuProgIds.Any())
        //        {
        //            studentProgramIds = studentProgramIds.Union(stuProgIds).ToArray();
        //        }
        //        if (!string.IsNullOrEmpty(acadCredProgCriteria))
        //        {
        //            if (studentProgramIds.Any())
        //                studentProgramIds = studentProgramIds.Intersect(acadCredStuProgIds).ToArray();
        //            else
        //                studentProgramIds = studentProgramIds.Union(acadCredStuProgIds).ToArray();
        //            if (!string.IsNullOrEmpty(acadProgCriteria))
        //                studentProgramIds = studentProgramIds.Intersect(acadStuProgIds).ToArray();

        //        }
        //        else if (!string.IsNullOrEmpty(acadProgCriteria))
        //        {
        //            if (studentProgramIds.Any())
        //                studentProgramIds = studentProgramIds.Intersect(acadStuProgIds).ToArray();
        //            else
        //                studentProgramIds = studentProgramIds.Union(acadStuProgIds).ToArray();
        //        }
        //        var totalCount = studentProgramIds.Count();
        //        Array.Sort(studentProgramIds);
        //        var subList = studentProgramIds.Skip(offset).Take(limit).ToArray();
        //        var studentProgramData = await DataReader.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", subList);
        //        var studentProgEntities = await BuildStudentAcademicProgramsAsync(studentProgramData, defaultInstitutionId);
        //        return new Tuple<IEnumerable<StudentAcademicProgram>, int>(studentProgEntities, totalCount);

        //    }
        //    catch (RepositoryException e)
        //    {
        //        throw e;
        //    }
        //}


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
                    string acadCredProgCriteria = "WITH ACAD.INSTITUTIONS.ID EQ '" + defaultInstitutionId + "'";
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
                                    var acadCredIds = await DataReader.SelectAsync("INSTITUTIONS.ATTEND",  instAttendIds.ToArray(), "SAVING UNIQUE INSTA.ACAD.CREDENTIALS");
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
                if ((ccdCredentials != null && ccdCredentials.Any())|| (degreeCredentials != null && degreeCredentials.Any()))
                {
                    var credStuProgIds = await ApplyCredentialsFilter(defaultInstitutionId, degreeCredentials, ccdCredentials, stuProgsLimitingKeys, acadCredLimitingKeys, completeStatus);
                    //merge it with existing list of student academic programs.
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
                    if (stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any())
                    {
                        List<string> studentIds = stuProgsLimitingKeys.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
                        List<string> instAttendIds = new List<string>();
                        foreach (var id in studentIds)
                            instAttendIds.Add(string.Concat(id, "*", defaultInstitutionId));
                        if (instAttendIds != null && instAttendIds.Any())
                        {
                            var acadCredIds = await DataReader.SelectAsync("INSTITUTIONS.ATTEND", instAttendIds.ToArray(), "SAVING UNIQUE INSTA.ACAD.CREDENTIALS");
                            if (acadCredIds != null && acadCredIds.Any())
                            {
                                acadCredLimitingKeys.AddRange(acadCredIds);
                                acadCredLimitingKeys.Distinct();
                            }
                        }
                    }

                }
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
                        acadCredLimitingKeys = (await DataReader.SelectAsync("INSTITUTIONS.ATTEND", instAttendIds.ToArray(), "SAVING UNIQUE INSTA.ACAD.CREDENTIALS")).ToList();
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
                        throw new Exception(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return studentProgramStatuses;
        }

        // process credential filter
        //it will take the list of acadprograms, student programss & acad credentials list and return appropriate student program list after applying the filter
        private async Task<List<string>> ApplyCredentialsFilter(string defaultInstitutionId, List<string> degreeCredentials, List<string> ccdCredentials, List<string> stuProgsLimitingKeys, List<string> acadCredLimitingKeys, string completeStatus)
        {
            var stuProg = new List<string>();
            string acadProgCriteria = string.Empty;
            string ccdCriteria = string.Empty;
            List<string> acadCredStuProgs = new List<string>();
            string acadCredCriteria = "WITH ACAD.ACAD.PROGRAM NE ''";
            List<string> progLimitingKeys = stuProgsLimitingKeys.Select(x => x.Split('*')[1]).Where(y => !string.IsNullOrEmpty(y)).ToList();
            string progCriteria = string.Concat("WITH STPR.START.DATE NE '' AND WITH STPR.CURRENT.STATUS NE ",  completeStatus);
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
                        ccdCriteria +=  string.Concat(progCriteria, " AND WITH STPR.CCDS EQ '" + ccdCredential + "'");
                    }
                    else
                    {
                        ccdCriteria += " AND WITH STPR.CCDS EQ '" + ccdCredential + "'";
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
                        stuProg.AddRange(acadCredStuProgs);
                    }
                }
            }
            if (!string.IsNullOrEmpty(acadProgCriteria))
            {
                var acadProgramIds = (await DataReader.SelectAsync("ACAD.PROGRAMS", progLimitingKeys != null && progLimitingKeys.Any() ? progLimitingKeys.ToArray() : null, acadProgCriteria)).ToList();
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
            if (!string.IsNullOrEmpty(ccdCriteria))
            {
                var stuProgCcds = await DataReader.SelectAsync("STUDENT.PROGRAMS", stuProgsLimitingKeys != null && stuProgsLimitingKeys.Any() ? stuProgsLimitingKeys.ToArray() : null, ccdCriteria);
                if (stuProgCcds != null && stuProgCcds.Any())
                stuProg.AddRange(stuProgCcds);
            }
            return stuProg.Distinct().ToList();
        }

        ///// <summary>
        ///// Builds StudentProgram Entity
        ///// </summary>
        ///// <param name="studentProgramData">Student Programs Data contracts</param>
        ///// <param name="defaultInstitutionId">Default Institution ID to get Acad Credentials record</param>
        ///// <returns>Returns StudentProgram</returns>
        //private async Task<IEnumerable<StudentAcademicProgram>> BuildStudentAcademicProgramsAsync(Collection<StudentPrograms> studentProgramData, string defaultInstitutionId)
        //{
        //    //get needed reference data
        //    List<StudentAcademicProgram> stuAcadPrograms = new List<StudentAcademicProgram>();

        //    //if no studentacadprograms passed in return empty list
        //    if (!studentProgramData.Any())
        //    {
        //        return stuAcadPrograms;
        //    }

        //    //get academic program data for all the programs in the list
        //    string[] progCodes = studentProgramData.Select(p => p.Recordkey).Where(x => !string.IsNullOrEmpty(x) && x.Contains("*")).Select(x => x.Split('*')[1]).Where(y => !string.IsNullOrEmpty(y)).ToArray();
        //    Collection<AcadPrograms> acadProgramCollection = new Collection<AcadPrograms>();
        //    if (progCodes != null && progCodes.Any())
        //    {
        //        acadProgramCollection = await DataReader.BulkReadRecordAsync<AcadPrograms>(progCodes);
        //    }
        //    //get the list of student IDs
        //    string[] studentIds = studentProgramData.Select(p => p.Recordkey).Where(x => !string.IsNullOrEmpty(x) && x.Contains("*")).Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToArray();
        //    //get the list of acad data contract objects
        //    Collection<AcadCredentials> acadCredentialsData = new Collection<AcadCredentials>();
        //    string[] acadCredIdListStu = null;
        //    string[] acadCredIdListProg = null;
        //    string[] acadCredIdList = null;
            
        //    if (studentIds.Any())
        //    {
        //        string studentcriteria = "WITH ACAD.PERSON.ID EQ '?'";
        //        acadCredIdListStu = await DataReader.SelectAsync("ACAD.CREDENTIALS", studentcriteria, studentIds);
        //    }
        //    if (progCodes.Any())
        //    {
        //        string programcriteria = "WITH ACAD.ACAD.PROGRAM EQ '?'";
        //        acadCredIdListProg = await DataReader.SelectAsync("ACAD.CREDENTIALS", programcriteria, progCodes);
        //    }
        //    acadCredIdList = acadCredIdListStu.Union(acadCredIdListProg).ToArray();
        //    if (!string.IsNullOrEmpty(defaultInstitutionId) && acadCredIdList.Any())
        //    {
        //        string defaultInstCriteria = "WITH ACAD.INSTITUTIONS.ID EQ '" + defaultInstitutionId + "'";
        //        acadCredIdList = await DataReader.SelectAsync("ACAD.CREDENTIALS", acadCredIdList, defaultInstCriteria);
               
        //    }
           
        //    if (acadCredIdList.Any())
        //    {
        //        acadCredentialsData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", acadCredIdList);
        //    }

        //    //process each of the student program records           

        //    foreach (var stuProg in studentProgramData)
        //    {
        //        try
        //        {
        //            string catcode = stuProg.StprCatalog;
        //            string studentid = stuProg.Recordkey.Split('*')[0];
        //            string progcode = stuProg.Recordkey.Split('*')[1];
        //            string guid = stuProg.RecordGuid;
        //            string status = string.Empty;
        //            if (stuProg.StprStatus.Any())
        //            {
        //                status = stuProg.StprStatus.ElementAt(0);
        //            }
        //            else
        //            {
        //                //in Colleague, if status is missing, it defaults to active if there is no end date and to withdrawn if there is end date.
        //                if (stuProg.StprEndDate.Any())
        //                {
        //                    if (stuProg.StprEndDate.ElementAt(0) != null)
        //                    {
        //                        var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "4");
        //                        if (codeAssoc != null)
        //                        {
        //                            status = codeAssoc.ValInternalCodeAssocMember;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "2");
        //                        if (codeAssoc != null)
        //                        {
        //                            status = codeAssoc.ValInternalCodeAssocMember;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValActionCode1AssocMember == "2");
        //                    if (codeAssoc != null)
        //                    {
        //                        status = codeAssoc.ValInternalCodeAssocMember;
        //                    }
        //                }
        //            }
        //            DateTime startDate = new DateTime();
        //            if (stuProg.StprStartDate != null && stuProg.StprStartDate.Any())
        //            {
        //                var studentProgramStartDate = stuProg.StprStartDate.ElementAt(0);
        //                if (studentProgramStartDate != null && studentProgramStartDate != DateTime.MinValue)
        //                {
        //                    startDate = studentProgramStartDate.Value;
        //                }
        //            }

        //            StudentAcademicProgram studentAcadProgEntity = new StudentAcademicProgram(studentid, progcode, catcode, guid, startDate, status);
        //            studentAcadProgEntity.AnticipatedCompletionDate = stuProg.StprAntCmplDate;
        //            if (stuProg.StprEndDate != null && stuProg.StprEndDate.Any())
        //            {
        //                var studentProgramEndDate = stuProg.StprEndDate.ElementAt(0);
        //                if (studentProgramEndDate != null && studentProgramEndDate != DateTime.MinValue)
        //                {
        //                    studentAcadProgEntity.EndDate = studentProgramEndDate;
        //                }
        //            }
        //            studentAcadProgEntity.DepartmentCode = stuProg.StprDept;
        //            studentAcadProgEntity.Location = stuProg.StprLocation;
        //            studentAcadProgEntity.StartTerm = stuProg.StprIntgStartTerm;
        //            studentAcadProgEntity.AnticipatedCompletionTerm = stuProg.StprIntgAntCmplTerm;
        //            studentAcadProgEntity.AnticipatedCompletionDate = stuProg.StprAntCmplDate;
        //            //get data from students to see if this program is primary for this student.
        //            // it is primary if it is the first one on the list.
        //            //depreciate the primary flag
        //            //var studentData = await DataReader.ReadRecordAsync<Students>(studentid);
        //            //if (studentData != null && studentData.StuAcadPrograms.Any() && progcode.Equals(studentData.StuAcadPrograms.FirstOrDefault()))
        //            //{
        //            //    studentAcadProgEntity.IsPrimary = true;
        //            //}
        //            //get academic level
        //            AcadPrograms acadProgramData = acadProgramCollection.FirstOrDefault(a => a.Recordkey == progcode);
        //            if (acadProgramData != null)
        //            {
        //                studentAcadProgEntity.AcademicLevelCode = acadProgramData.AcpgAcadLevel;
        //            }
        //                // get data from ACAD.CREDENTIALS. If the student has already graduated and has a record in ACAD.CREDENTIALS
        //                // then we need to display the credentials and disciplines from ACAD.CREDENTIALS instead of ACAD.PROGRAMS and STUDENT.PROGRAMS
        //                var acadCredential = acadCredentialsData.FirstOrDefault(cred => cred.AcadPersonId == studentid && cred.AcadAcadProgram == progcode);
        //            if (acadCredential != null)
        //            {
        //                foreach (var honor in acadCredential.AcadHonors)
        //                {
        //                    studentAcadProgEntity.AddHonors(honor);
        //                }
        //                studentAcadProgEntity.GradGPA = acadCredential.AcadGpa;
        //                studentAcadProgEntity.GraduationDate = acadCredential.AcadEndDate;
        //                studentAcadProgEntity.CredentialsDate = acadCredential.AcadDegreeDate;
        //                studentAcadProgEntity.ThesisTitle = acadCredential.AcadThesis;
        //                studentAcadProgEntity.DegreeCode = acadCredential.AcadDegree;
        //                studentAcadProgEntity.GradTerm = acadCredential.AcadTerm;
        //                // Add majors from the Academic Credentials
        //                if (acadCredential.AcadMajors.Any())
        //                {
        //                    foreach (var mjr in acadCredential.AcadMajors)
        //                    {
        //                        studentAcadProgEntity.AddMajors(mjr);
        //                    }
        //                }
        //                // Add minors from the Academic Credentials
        //                if (acadCredential.AcadMinors.Any())
        //                {
        //                    foreach (var minr in acadCredential.AcadMinors)
        //                    {
        //                        studentAcadProgEntity.AddMinors(minr);
        //                    }
        //                }
        //                // Add specializations from the Academic Credentials
        //                if (acadCredential.AcadSpecialization.Any())
        //                {
        //                    foreach (var sp in acadCredential.AcadSpecialization)
        //                    {
        //                        studentAcadProgEntity.AddSpecializations(sp);
        //                    }
        //                }
        //                // Add ccds from the Academic Credentials
        //                if (acadCredential.AcadCcd.Any())
        //                {
        //                    foreach (var ccd in acadCredential.AcadCcd)
        //                    {
        //                        studentAcadProgEntity.AddCcds(ccd);
        //                    }
        //                }

        //            }
        //            else
        //            {

        //                if (acadProgramData != null)
        //                {
        //                    studentAcadProgEntity.AcademicLevelCode = acadProgramData.AcpgAcadLevel;
        //                    studentAcadProgEntity.DegreeCode = acadProgramData.AcpgDegree;
        //                    // Add majors from the Academic Program
        //                    if (acadProgramData.AcpgMajors.Any())
        //                    {
        //                        foreach (var mjr in acadProgramData.AcpgMajors)
        //                        {
        //                            studentAcadProgEntity.AddMajors(mjr);
        //                        }
        //                    }
        //                    // Add minors from the Academic Program
        //                    if (acadProgramData.AcpgMinors.Any())
        //                    {
        //                        foreach (var minr in acadProgramData.AcpgMinors)
        //                        {
        //                            studentAcadProgEntity.AddMinors(minr);
        //                        }
        //                    }
        //                    // Add specializations from the Academic Program
        //                    if (acadProgramData.AcpgSpecializations.Any())
        //                    {
        //                        foreach (var sp in acadProgramData.AcpgSpecializations)
        //                        {
        //                            studentAcadProgEntity.AddSpecializations(sp);
        //                        }
        //                    }
        //                    // Add ccds from the Academic Program
        //                    if (acadProgramData.AcpgCcds.Any())
        //                    {
        //                        foreach (var ccd in acadProgramData.AcpgCcds)
        //                        {
        //                            studentAcadProgEntity.AddCcds(ccd);
        //                        }
        //                    }
        //                }

        //                // Additional Requirements from student programs
        //                // Add majors
        //                if (stuProg.StprMajorListEntityAssociation.Any())
        //                {
        //                    foreach (var mjr in stuProg.StprMajorListEntityAssociation)
        //                    {
        //                        if (mjr.StprAddnlMajorStartDateAssocMember <= DateTime.Now && mjr.StprAddnlMajorEndDateAssocMember == null || mjr.StprAddnlMajorEndDateAssocMember > DateTime.Now)
        //                        {
        //                            studentAcadProgEntity.AddMajors(mjr.StprAddnlMajorsAssocMember);
        //                        }
        //                    }
        //                }
        //                // Add minors 
        //                if (stuProg.StprMinorListEntityAssociation.Any())
        //                {
        //                    foreach (var minr in stuProg.StprMinorListEntityAssociation)
        //                    {
        //                        if (minr.StprMinorStartDateAssocMember <= DateTime.Now && minr.StprMinorEndDateAssocMember == null || minr.StprMinorEndDateAssocMember > DateTime.Now)
        //                        {
        //                            studentAcadProgEntity.AddMinors(minr.StprMinorsAssocMember);
        //                        }
        //                    }
        //                }
        //                // Add specializations 
        //                if (stuProg.StprSpecialtiesEntityAssociation.Any())
        //                {
        //                    foreach (var sps in stuProg.StprSpecialtiesEntityAssociation)
        //                    {
        //                        if (sps.StprSpecializationStartAssocMember <= DateTime.Now && sps.StprSpecializationEndAssocMember == null || sps.StprSpecializationEndAssocMember > DateTime.Now)
        //                        {
        //                            studentAcadProgEntity.AddSpecializations(sps.StprSpecializationsAssocMember);
        //                        }
        //                    }
        //                }
        //                // Add ccds 
        //                if (stuProg.StprCcdListEntityAssociation.Any())
        //                {
        //                    foreach (var ccd in stuProg.StprCcdListEntityAssociation)
        //                    {
        //                        if (ccd.StprCcdsStartDateAssocMember <= DateTime.Now && ccd.StprCcdsEndDateAssocMember == null || ccd.StprCcdsEndDateAssocMember > DateTime.Now)
        //                        {
        //                            studentAcadProgEntity.AddCcds(ccd.StprCcdsAssocMember);
        //                        }
        //                    }
        //                }
        //            }


        //            //get credits earned
        //            studentAcadProgEntity.CreditsEarned = stuProg.StprEvalCombinedCred;
        //            stuAcadPrograms.Add(studentAcadProgEntity);
        //        }
        //        catch (ArgumentException e)
        //        {
        //            throw e;
        //        }
        //        catch (Exception e)
        //        {
        //            throw new RepositoryException(string.Format("Could not build program {0} for student {1}", stuProg.Recordkey.Split('*')[1], stuProg.Recordkey.Split('*')[0]));
        //        }
        //    }
        //    return stuAcadPrograms;
        //}

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
                        if (stuProg.StprCcdListEntityAssociation!= null && stuProg.StprCcdListEntityAssociation.Any())
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
    }
}
