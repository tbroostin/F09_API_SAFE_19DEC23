// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentGradePointAveragesRepository : BaseColleagueRepository, IStudentGradePointAveragesRepository
    {
        private int readSize;

        public StudentGradePointAveragesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using Level 1 Cache Timeout Value for data that changes rarely.
            CacheTimeout = Level1CacheTimeoutValue;
            readSize = settings.BulkReadSize;
        }

        #region GET Method

        /// <summary>
        /// Gets entities for student grade point averages.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="sgpa"></param>
        /// <param name="acadPeriod"></param>
        /// <param name="gradeDate"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<StudentAcademicCredit>, int>> GetStudentGpasAsync(int offset, int limit, StudentAcademicCredit sgpa, string gradeDate)
        {
            List<StudentAcademicCredit> studentGpas = new List<StudentAcademicCredit>();
            Collection<PersonSt> personStDataContracts = null;
            List<StudentAcadCred> stcDataContracts = new List<StudentAcadCred>();
            List<string> personStIds = new List<string>();
            IEnumerable<string> subList = null;
            string[] limitingKeys = null;
            string[] stAcadCredIds = null;
            var totalCount = 0;

            string criteria = string.Empty;

            //student id filter
            if (sgpa != null && !string.IsNullOrEmpty(sgpa.StudentId))
            {
                criteria = string.Format("WITH STC.ATT.CRED NE '' AND STC.PERSON.ID EQ '{0}'", sgpa.StudentId);
                limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
                }
            }

            //Academic period named query
            if (sgpa != null && sgpa.AcademicPeriods!= null && sgpa.AcademicPeriods.Any())
            {
                criteria = "WITH STC.ATT.CRED NE '' AND STC.TERM EQ '?'";
                stAcadCredIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", criteria, sgpa.AcademicPeriods.ToArray());
                if (stAcadCredIds == null || !stAcadCredIds.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
                }
                if(limitingKeys == null)
                {
                    limitingKeys = stAcadCredIds;
                }
                else
                {
                    limitingKeys = limitingKeys.Intersect(stAcadCredIds).ToArray();
                }
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
                }
            }

            //Grade date(verified date) named query
            if (!string.IsNullOrEmpty(gradeDate)) 
            {
                criteria = string.Format("WITH STC.ATT.CRED NE '' AND STC.VERIFIED.GRADE.DATE EQ '{0}'", gradeDate);
                limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
                }
            }

            if (string.IsNullOrEmpty(criteria))
            {
                limitingKeys = await DataReader.SelectAsync("PERSON.ST", "WITH PST.STUDENT.ACAD.CRED BY.EXP PST.STUDENT.ACAD.CRED SAVING PST.STUDENT.ACAD.CRED");                

                //Now use sac limiting list to get person ids who have sac records.
                personStIds = (await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, "WITH STC.ATT.CRED NE '' SAVING UNIQUE STC.PERSON.ID")).ToList();
                if (personStIds == null || !personStIds.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
                }
            }
            else
            {
                criteria = "SAVING UNIQUE STC.PERSON.ID";
                personStIds = (await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, criteria)).ToList();
                if (personStIds == null || !personStIds.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
                }
            }

            totalCount = personStIds.Count();
            Array.Sort(personStIds.ToArray());
            subList = personStIds.Skip(offset).Take(limit);
            personStDataContracts = await DataReader.BulkReadRecordAsync<PersonSt>(subList.Distinct().ToArray());

            if (personStDataContracts == null || !personStDataContracts.Any())
            {
                return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
            }

            List<StudentAcadCred> tempCreds = new List<StudentAcadCred>();
            int totalAcadCount = personStDataContracts.Where(sac => sac.PstStudentAcadCred != null && sac.PstStudentAcadCred.Any())
                                                      .SelectMany(i => i.PstStudentAcadCred).Distinct().Count();

            var stAcadKeys = personStDataContracts.Where(sac => sac.PstStudentAcadCred != null && sac.PstStudentAcadCred.Any())
                                                  .SelectMany(i => i.PstStudentAcadCred).Distinct();

            for (var i = 0; i < totalAcadCount; i += readSize)
            {
                var courseSubList = stAcadKeys.Skip(i).Take(readSize);
                var records = await DataReader.BulkReadRecordAsync<StudentAcadCred>(courseSubList.ToArray());
                if (records != null)
                {
                    tempCreds.AddRange(records);
                }
            }

            foreach (var personStDataContract in personStDataContracts)
            {
                string[] credIds = personStDataContract.PstStudentAcadCred.Intersect(limitingKeys).ToArray();
                var creds = tempCreds.Where(cr => credIds.Contains(cr.Recordkey)).Select(rec => rec);
                studentGpas.Add(BuildStudentAcadCred(personStDataContract, creds));
            }

            return studentGpas.Any() ? new Tuple<IEnumerable<StudentAcademicCredit>, int>(studentGpas, totalCount) :
                new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
        }

        /// <summary>
        /// Gets student acad credit record based on an id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StudentAcademicCredit> GetStudentCredProgramInfoByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a student grade point average.");
            }

            var personStDataContract = await DataReader.ReadRecordAsync<PersonSt>(id);

            if (personStDataContract == null)
            {
                throw new KeyNotFoundException(string.Format("Student grade point average not found for id {0}.", id));
            }
            var creds = await DataReader.BulkReadRecordAsync<StudentAcadCred>(personStDataContract.PstStudentAcadCred.ToArray());

            var stAttcred = creds.Where(rec => rec.StcAttCred.HasValue).ToList();
            if (stAttcred == null || !stAttcred.Any())
            {
                return null;
            }
            return BuildStudentAcadCred(personStDataContract, stAttcred);
        }

        /// <summary>
        /// Builds student acad cred entities.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stAcadCredDCs"></param>
        /// <returns></returns>
        private StudentAcademicCredit BuildStudentAcadCred(PersonSt source, IEnumerable<StudentAcadCred> stAcadCredDCs)
        {
            StudentAcademicCredit sac = new StudentAcademicCredit(source.RecordGuid, source.Recordkey);
            sac.StudentGPAInfoList = new List<StudentGPAInfo>();

            foreach (var stAcadCredDC in stAcadCredDCs)
            {
                StudentGPAInfo studentGPAInfo = new StudentGPAInfo();

                studentGPAInfo.Term = stAcadCredDC.StcTerm;
                studentGPAInfo.StcReportingTerm = stAcadCredDC.StcReportingTerm;
                studentGPAInfo.StcAttCredit = stAcadCredDC.StcAttCred;
                studentGPAInfo.AcademicLevel = stAcadCredDC.StcAcadLevel;
                studentGPAInfo.StcStudentCourseSec = stAcadCredDC.StcStudentCourseSec;
                studentGPAInfo.CreditType = stAcadCredDC.StcCredType;
                studentGPAInfo.StcCumContribGradePoint = stAcadCredDC.StcCumContribGradePts;
                studentGPAInfo.StcCumContribGpaCredit = stAcadCredDC.StcCumContribGpaCred;
                studentGPAInfo.StcCumContribAttCredit = stAcadCredDC.StcCumContribAttCred;
                //Alt gpas
                studentGPAInfo.StcAltcumContribGradePoint = stAcadCredDC.StcAltcumContribGradePts;
                studentGPAInfo.StcAltCumContribGpaCredit = stAcadCredDC.StcAltcumContribGpaCred;
                studentGPAInfo.StcAltCumContribAttCredit = stAcadCredDC.StcAltcumContribAttCred;
                //Credential Ids
                if (studentGPAInfo.MarkAcadCredentials == null) studentGPAInfo.MarkAcadCredentials = new List<string>();
                if (stAcadCredDC.StcMarkAcadCredentials != null && stAcadCredDC.StcMarkAcadCredentials.Any())
                {
                    studentGPAInfo.MarkAcadCredentials.AddRange(stAcadCredDC.StcMarkAcadCredentials);
                }

                sac.StudentGPAInfoList.Add(studentGPAInfo);
            }
            return sac;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetStudentGradePointAverageIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(string.Format("Student grade point average GUID {0} not found.", guid));
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(string.Format("Student grade point average GUID {0} lookup failed.", guid));
            }

            if (foundEntry.Value.Entity != "PERSON.ST")
            {
                throw new RepositoryException(string.Format("GUID '{0}' has different entity, '{1}' than expected, PERSON.ST", guid, foundEntry.Value.Entity));
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Gets student program info based om marked credentials ids in student acad cred file/table.
        /// </summary>
        /// <param name="markAcadCredIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<StudentAcademicCredentialProgramInfo>> GetStudentCredProgramInfoAsync(IEnumerable<string> markAcadCredIds)
        {
            if(markAcadCredIds == null || !markAcadCredIds.Any())
            {
                return null;
            }
            List<StudentAcademicCredentialProgramInfo> list = new List<StudentAcademicCredentialProgramInfo>();
            var acadCredDCs = await DataReader.BulkReadRecordAsync<AcadCredentials>(markAcadCredIds.Distinct().ToArray());

            if(acadCredDCs != null || acadCredDCs.Any())
            {
                foreach (var acadCredDC in acadCredDCs)
                {
                    if(!string.IsNullOrEmpty(acadCredDC.AcadAcadProgram) && !string.IsNullOrEmpty(acadCredDC.AcadPersonId))
                    {
                        var studProgramId = string.Join("*", acadCredDC.AcadPersonId, acadCredDC.AcadAcadProgram);
                        var studProg = await DataReader.ReadRecordAsync<StudentPrograms>(studProgramId );
                        if(studProg != null)
                        {
                            StudentAcademicCredentialProgramInfo studentCredentialsProgramInfo = new StudentAcademicCredentialProgramInfo();
                            studentCredentialsProgramInfo.AcadCcdDate = acadCredDC.AcadCcdDate.FirstOrDefault().HasValue? acadCredDC.AcadCcdDate.FirstOrDefault().Value : default(DateTime?);
                            studentCredentialsProgramInfo.AcadDegreeDate = acadCredDC.AcadDegreeDate.HasValue ? acadCredDC.AcadDegreeDate.Value : default(DateTime?);
                            studentCredentialsProgramInfo.AcademicAcadProgram = acadCredDC.AcadAcadProgram;
                            studentCredentialsProgramInfo.AcademicCredentialsId = acadCredDC.Recordkey;
                            studentCredentialsProgramInfo.AcadPersonId = acadCredDC.AcadPersonId;
                            studentCredentialsProgramInfo.StudentProgramGuid = studProg.RecordGuid;
                            list.Add(studentCredentialsProgramInfo);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets true/false based on the value Y/N set in StwebTranAltcumFlag field.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UseAlternativeCumulativeValuesAsync()
        {
            var stwebDefaults = await GetStwebDefaultsAsync();
            if (stwebDefaults == null)
            {
                throw new Exception("Unable to access STWEB.DEFAULTS values");
            }

            var flag = stwebDefaults.StwebTranAltcumFlag;
            if(flag.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets defaults used in conversion for StwebTranAltcumFlag.
        /// </summary>
        /// <returns></returns>
        private async Task<StwebDefaults> GetStwebDefaultsAsync()
        {

            var result = await GetOrAddToCacheAsync<StwebDefaults>("StudentWebDefaults",
            async () =>
            {
                StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true);
                if (stwebDefaults == null)
                {
                    if (logger.IsInfoEnabled)
                    {
                        var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                        logger.Info(errorMessage);
                    }
                    stwebDefaults = new StwebDefaults();
                }
                return stwebDefaults;
            }, Level1CacheTimeoutValue);

            return result;
        }
        
        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type.
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        public async Task<string> GetUnidataFormattedDate(string date)
        {
            _internationalParameters = _internationalParameters == null ? await base.GetInternationalParametersAsync() : _internationalParameters;
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, _internationalParameters.HostShortDateFormat, _internationalParameters.HostDateDelimiter);
        }       

        #endregion Helper Methods
    }
}