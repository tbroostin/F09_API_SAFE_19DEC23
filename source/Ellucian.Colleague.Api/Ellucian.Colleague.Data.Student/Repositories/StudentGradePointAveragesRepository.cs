// Copyright 2018-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
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
    public class StudentGradePointAveragesRepository : BaseColleagueRepository, IStudentGradePointAveragesRepository
    {
        private int readSize;
        private RepositoryException exception;
        const string AllStudentGradePointAveragesCache = "AllStudentGPAs";
        const int AllStudentGPACacheTimeout = 20;

        public StudentGradePointAveragesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using Level 1 Cache Timeout Value for data that changes rarely.
            CacheTimeout = Level1CacheTimeoutValue;
            readSize = settings.BulkReadSize;
            exception = new RepositoryException();
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
        public async Task<Tuple<IEnumerable<StudentAcademicCredit>, int>> GetStudentGpasAsync(int offset, int limit, StudentAcademicCredit sgpa, string gradeDate = "")
        {
            List<StudentAcademicCredit> studentGpas = new List<StudentAcademicCredit>();
            Collection<PersonSt> personStDataContracts = null;
            List<StudentAcadCred> stcDataContracts = new List<StudentAcadCred>();

            var totalCount = 0;
            string[] personStIds = null;
            string[] limitingKeys = null;
            string[] stAcadCredIds = null;
            IEnumerable<string> subList = null;

            string acadIdsCriteria = "WITH STC.ATT.CRED NE ''";
            string criteria = await GenerateCriteriaAsync(sgpa, gradeDate);
            if (string.IsNullOrEmpty(criteria))
            {
                //Now use sac limiting list to get person ids who have sac records.
                criteria = "WITH STC.ATT.CRED NE '' SAVING UNIQUE STC.PERSON.ID";
            }
            else
            {
                if (sgpa != null && !string.IsNullOrEmpty(sgpa.StudentId))
                {
                    acadIdsCriteria = string.Format("WITH STC.ATT.CRED NE '' AND STC.PERSON.ID EQ '{0}' AND {1}", sgpa.StudentId, criteria);
                }
                else
                {
                    acadIdsCriteria = string.Format("WITH STC.ATT.CRED NE '' AND {0}", criteria);
                }
                criteria = string.Format("WITH STC.ATT.CRED NE '' AND {0} SAVING UNIQUE STC.PERSON.ID", criteria);
            }

            try
            {
                string gpaCacheKey = CacheSupport.BuildCacheKey(AllStudentGradePointAveragesCache,
                                     (sgpa != null && sgpa.AcademicPeriods != null && sgpa.AcademicPeriods.Any()) ? sgpa.AcademicPeriods : null,
                                      sgpa != null ? sgpa.StudentId : string.Empty, gradeDate);


                var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                        this,
                        ContainsKey,
                        GetOrAddToCacheAsync,
                        AddOrUpdateCacheAsync,
                        transactionInvoker,
                        gpaCacheKey,
                        "STUDENT.ACAD.CRED",
                        offset,
                        limit,
                        AllStudentGPACacheTimeout,
                        async () =>
                        {
                            //student id filter
                            if (sgpa != null && !string.IsNullOrEmpty(sgpa.StudentId))
                            {
                                //readr the person st record to get st acad id's.
                                var student = await DataReader.ReadRecordAsync<PersonSt>(sgpa.StudentId);
                                if (student == null)
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                personStIds = student.PstStudentAcadCred.ToArray();
                                if (personStIds == null || !personStIds.Any())
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                            }                            
                            
                            CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                            {
                                limitingKeys = personStIds != null && personStIds.Any() ? personStIds.Distinct().ToList() : null,
                                criteria = criteria.ToString(),
                            };
                            return requirements;
                        }
                );

                if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
                }

                totalCount = keyCacheObject.TotalCount.Value;
                subList = keyCacheObject.Sublist.ToArray();
                personStDataContracts = await DataReader.BulkReadRecordAsync<PersonSt>(subList.Distinct().ToArray());

                if (personStDataContracts == null || !personStDataContracts.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
                }

                //Make sure we have all the st acad creds, just incase if total # is more than read size
                List<StudentAcadCred> tempCreds = new List<StudentAcadCred>();
                var stAcadKeys = personStDataContracts.Where(sac => sac.PstStudentAcadCred != null && sac.PstStudentAcadCred.Any())
                                                      .SelectMany(i => i.PstStudentAcadCred).Distinct().ToList();
                int totalAcadCount = stAcadKeys.Count();
                for (var i = 0; i < totalAcadCount; i += readSize)
                {
                    var courseSubList = stAcadKeys.Skip(i).Take(readSize);
                    var records = await DataReader.BulkReadRecordAsync<StudentAcadCred>(courseSubList.ToArray());
                    if (records != null)
                    {
                        tempCreds.AddRange(records);
                    }
                }

                /*
                    we still need these id's based criteria. Reason, when we do Read record async, it only returns ids in student acad cred & so, 
                    we dont know how many of those id's qualify for STC.ATT.CRED NE '' condition.
                */
                if (limitingKeys == null) limitingKeys = stAcadKeys.ToArray();
                stAcadCredIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, acadIdsCriteria);
                // Need to loop through the subList PERSON.ST keys in case the record is missing or doesn't have
                // the appropriate references to PST.STUDENT.ACAD.CRED so that we can report on invalid PERSON.ST
                // records.  When we were looping through the data contracts, then we might have a situation where
                // the subList contains a full page but the data contracts only have a subset of the full page.
                foreach (var personStId in subList)
                {
                    var personStDataContract = personStDataContracts.Where(pst => pst.Recordkey.Equals(personStId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (personStDataContract == null || personStDataContract.PstStudentAcadCred == null || !personStDataContract.PstStudentAcadCred.Any())
                    {
                        exception.AddError(new RepositoryError("Data.Access", string.Format("Unable to locate a PERSON.ST record with  STC references for id: '{0}'", personStId)));
                    }
                    else
                    {
                        string[] credIds = personStDataContract.PstStudentAcadCred.Intersect(stAcadCredIds).ToArray();
                        var creds = tempCreds.Where(cr => credIds.Contains(cr.Recordkey)).Select(rec => rec);
                        if ((personStDataContract != null) && (!string.IsNullOrWhiteSpace(personStDataContract.RecordGuid)))
                        {
                            studentGpas.Add(BuildStudentAcadCred(personStDataContract, creds));
                        }
                        else
                        {
                            exception.AddError(new RepositoryError("Data.Access", string.Format("Unable to locate guid for PERSON.ST id: '{0}'", personStId)));
                        }
                    }
                }

                if (exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }

                return studentGpas.Any() ? new Tuple<IEnumerable<StudentAcademicCredit>, int>(studentGpas, totalCount) :
                    new Tuple<IEnumerable<StudentAcademicCredit>, int>(new List<StudentAcademicCredit>(), 0);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                exception.AddError(new RepositoryError("Bad.Data", e.Message));
                throw exception;
            }
        }

        /// <summary>
        /// Gets criteria.
        /// </summary>
        /// <param name="filter">filter criteria</param>
        /// <param name="gradeDateNQ">grade date named query</param>
        /// <returns></returns>
        private async Task<string> GenerateCriteriaAsync(StudentAcademicCredit filter, string gradeDateNQ)
        {
            string criteria = string.Empty;
            //Grade date(verified date) named query
            if (!string.IsNullOrEmpty(gradeDateNQ))
            {
                DateTime gradesDate;
                var gradeFormattedDate = (DateTime.TryParse(gradeDateNQ, out gradesDate)) ? await GetUnidataFormatDateAsync(gradesDate) : gradeDateNQ;

                criteria = string.Format("STC.VERIFIED.GRADE.DATE EQ '{0}'", gradeFormattedDate);
            }

            //Academic period named query                    
            if (filter != null && filter.AcademicPeriods != null && filter.AcademicPeriods.Any())
            {
                if (string.IsNullOrEmpty(criteria))
                {
                    var acadPeriodIds = string.Join(" ", filter.AcademicPeriods.Select(i => string.Format("'{0}'", i)));
                    criteria = string.Format("STC.TERM EQ {0}", acadPeriodIds);
                }
                else
                {
                    var acadPeriodIds = string.Join(" ", filter.AcademicPeriods.Select(i => string.Format("'{0}'", i)));
                    criteria = string.Format("{0} AND STC.TERM EQ {1}", criteria, acadPeriodIds);
                }
            }            

            return criteria;
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
                studentGPAInfo.SourceKey = stAcadCredDC.Recordkey;
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
            List<RepositoryError> _errors = new List<RepositoryError>();

            try
            {
                var acadCredDCs = await DataReader.BulkReadRecordWithInvalidKeysAsync<AcadCredentials>( markAcadCredIds.Distinct().ToArray() );

                if( acadCredDCs.InvalidKeys != null && acadCredDCs.InvalidKeys.Any() )
                {
                    foreach( var invalidKey in acadCredDCs.InvalidKeys )
                    {
                        //Log that error, don't throw. err5 per specs
                        string errorMessage = string.Format( "student-grade-point-averages: ACAD.CREDENTIALS '{0}' does not exist, credential omitted from earnedDegrees.", invalidKey );
                        logger.Error( errorMessage );
                    }
                }

                if( acadCredDCs.BulkRecordsRead != null || acadCredDCs.BulkRecordsRead.Any() )
                {
                    var stProgIds = acadCredDCs.BulkRecordsRead.Where( i => !string.IsNullOrEmpty( i.AcadPersonId ) && !string.IsNullOrEmpty( i.AcadAcadProgram ) )
                                               .Select( p => string.Join( "*", p.AcadPersonId, p.AcadAcadProgram ) ).ToList();
                    if( stProgIds != null && stProgIds.Any() )
                    {
                        var stProgRecs = await DataReader.BulkReadRecordAsync<StudentPrograms>( stProgIds.Distinct().ToArray() );

                        if( stProgRecs != null && stProgRecs.Any() )
                        {
                            foreach( var acadCredDC in acadCredDCs.BulkRecordsRead )
                            {
                                if( !string.IsNullOrEmpty( acadCredDC.AcadAcadProgram ) && !string.IsNullOrEmpty( acadCredDC.AcadPersonId ) )
                                {
                                    var studProgramId = string.Join( "*", acadCredDC.AcadPersonId, acadCredDC.AcadAcadProgram );
                                    var studProg = stProgRecs.FirstOrDefault( i => i.Recordkey.Equals( studProgramId, StringComparison.InvariantCultureIgnoreCase ) );
                                    if( studProg != null )
                                    {
                                        StudentAcademicCredentialProgramInfo studentCredentialsProgramInfo = new StudentAcademicCredentialProgramInfo();
                                        studentCredentialsProgramInfo.AcadCcdDate = acadCredDC.AcadCcdDate.FirstOrDefault().HasValue ? acadCredDC.AcadCcdDate.FirstOrDefault().Value : default( DateTime? );
                                        studentCredentialsProgramInfo.AcadDegreeDate = acadCredDC.AcadDegreeDate.HasValue ? acadCredDC.AcadDegreeDate.Value : default( DateTime? );
                                        studentCredentialsProgramInfo.AcademicAcadProgram = acadCredDC.AcadAcadProgram;
                                        studentCredentialsProgramInfo.AcademicCredentialsId = acadCredDC.Recordkey;
                                        studentCredentialsProgramInfo.AcadPersonId = acadCredDC.AcadPersonId;
                                        studentCredentialsProgramInfo.StudentProgramGuid = studProg.RecordGuid;
                                        list.Add( studentCredentialsProgramInfo );
                                    }
                                    else
                                    {
                                        //err7 per specs
                                        string message = string.Format( "student-grade-point-averages: STUDENT.PROGRAMS '{0}' does not exist for ACAD.CREDENTIALS '{1}', credential omitted from earnedDegrees.", studProgramId, acadCredDC.Recordkey );
                                        RepositoryError error = new RepositoryError( acadCredDC.RecordGuid, acadCredDC.Recordkey, "Bad.Data", message );
                                        _errors.Add( error );
                                    }
                                }
                                else if( string.IsNullOrEmpty( acadCredDC.AcadAcadProgram ) )
                                {
                                    //err6 per specs
                                    string message = string.Format( "student-grade-point-averages: ACAD.ACAD.PROGRAMS not populated for ACAD.CREDENTIALS '{0}', credential omitted from earnedDegrees.", acadCredDC.Recordkey );
                                    RepositoryError error = new RepositoryError( acadCredDC.RecordGuid, acadCredDC.Recordkey, "Bad.Data", message );
                                    _errors.Add( error );
                                }
                            }

                            if( _errors.Any() )
                            {
                                exception.AddErrors( _errors );
                                throw exception;
                            }
                        }
                        else
                        {
                            //err7 per specs
                            foreach( var stProgId in stProgIds.ToList() )
                            {
                                try
                                {
                                    string[] key = stProgId.Split( new string[] { "*" }, StringSplitOptions.None );
                                    string stId = key[ 0 ];
                                    string acadacadId = key[ 1 ];
                                    var record = acadCredDCs.BulkRecordsRead.FirstOrDefault( i => i.AcadPersonId.Equals( stId, StringComparison.InvariantCultureIgnoreCase ) && i.AcadAcadProgram.Equals( key[ 1 ], StringComparison.InvariantCultureIgnoreCase ) );
                                    string message = string.Format( "student-grade-point-averages: STUDENT.PROGRAMS '{0}' does not exist for ACAD.CREDENTIALS '{1}', credential omitted from earnedDegrees.", stProgId, record.Recordkey );
                                    logger.Error( message );
                                }
                                catch( Exception e )
                                {
                                    //Do Nothing, don't error out.
                                    logger.Error( e.Message );
                                }
                            }
                        }
                    }
                }
            }
            catch(RepositoryException e)
            {
                //Don't throw exception but just log.
                if(e.Errors != null && e.Errors.Any())
                {
                    e.Errors.ToList().ForEach( er =>
                     {
                         logger.Error( er.Message );
                     } );
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
                throw new ColleagueWebApiException("Unable to access STWEB.DEFAULTS values");
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