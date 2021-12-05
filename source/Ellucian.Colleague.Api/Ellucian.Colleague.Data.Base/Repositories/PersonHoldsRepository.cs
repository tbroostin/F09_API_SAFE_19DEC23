// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Colleague.Data.Base.Transactions;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.Base.Services;
using System.Text;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonHoldsRepository : BaseColleagueRepository, IPersonHoldsRepository
    {
        const string AllStudentRestrictionsRecordsCache = "AllStudentRestrictionsRecordKeys";
        const int AllStudentRestrictionsRecordsCacheTimeout = 20;
        private RepositoryException exception = new RepositoryException();
        readonly int readSize;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public PersonHoldsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }
        #region GET Methods

        /// <summary>
        /// Returns a list of all active holds recorded for any person in the database
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<PersonRestriction>, int>> GetPersonHoldsAsync(int offset, int limit)
        {
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllStudentRestrictionsRecordsCache);

            int totalCount = 0;
            var selectionCriteria = new StringBuilder();

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                "STUDENT.RESTRICTIONS",
                offset,
                limit,
                AllStudentRestrictionsRecordsCacheTimeout,
                async () =>
                {
                    var date = await GetUnidataFormatDateAsync(DateTime.Today);
                    var criteria =
                        string.Format("WITH STR.STUDENT NE '' AND WITH STR.RESTRICTION NE '' AND WITH STR.END.DATE EQ '' OR STR.END.DATE GE '{0}' AND WITH STR.CORP.INDICATOR NE 'Y'",
                            date);
                    selectionCriteria.Append(criteria);

                    return new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = selectionCriteria.ToString()
                    };
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<PersonRestriction>, int>(new List<PersonRestriction>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            var subList = keyCacheObject.Sublist.ToArray();

            if (subList == null || !subList.Any())
            {
                return new Tuple<IEnumerable<PersonRestriction>, int>(new List<PersonRestriction>(), 0);
            }

            var studentHolds = new Collection<StudentRestrictions>();
            var personHoldsList = new List<PersonRestriction>();
            try
            {
                studentHolds = await DataReader.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", subList);
                personHoldsList = BuildPersonHolds(studentHolds);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<PersonRestriction>, int>(personHoldsList, totalCount);
        }

        /// <summary>
        /// Returns a hold for a specified Student Restrictions key.
        /// </summary>
        /// <param name="ids">Key to Student Restrictions to be returned</param>
        /// <returns>List of PersonRestrictions Objects</returns>
        public async Task<PersonRestriction> GetPersonHoldByIdAsync(string id)
        {
            var personHoldId = await GetStudentHoldIdFromGuidAsync(id);
            if (personHoldId == null )
            {
                throw new KeyNotFoundException("PersonHold GUID " + id + " not found.");
            }
            var studentHold = await DataReader.ReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", personHoldId);
            var personHold = BuildPersonHold(studentHold);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return personHold;
        }

        /// <summary>
        /// Returns all restrictions for a specified person Id
        /// </summary>
        /// <param name="personId">Person Id for whom restrictions are requested</param>
        /// <returns>List of PersonRestrictions</returns>
        public async Task<IEnumerable<PersonRestriction>> GetPersonHoldsByPersonIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                return new List<PersonRestriction>();
                //throw new ArgumentNullException("personId", "Must provide a person Id");
            }

            IEnumerable<PersonRestriction> holdsList = new List<PersonRestriction>();

            try
            {
                var personKey = await GetPersonIdFromGuidAsync(personId);
            
                //if (string.IsNullOrEmpty(personKey))
                //{
                //    throw new ArgumentNullException("personKey", "No person record with person Id: " + personId);
                //}

                var date = await GetUnidataFormatDateAsync(DateTime.Today);
                var criteria = string.Format("WITH STR.STUDENT EQ '{0}' AND WITH STR.RESTRICTION NE '' AND WITH STR.END.DATE EQ '' OR STR.END.DATE GE '{1}' AND WITH STR.CORP.INDICATOR NE 'Y'", 
                    personKey, date);

                // If there is no STUDENT.RESTRICTIONS record for this person in Colleague returns no restrictions.
                Collection<StudentRestrictions> personHolds = await DataReader.BulkReadRecordAsync<StudentRestrictions>(criteria);
                holdsList = BuildPersonHolds(personHolds);

                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }

                return holdsList;
            }
            catch (ArgumentNullException)
            {
                return new List<PersonRestriction>();
            }
        }
       
        #endregion

        #region DELETE Method
        /// <summary>
        /// Delete a person hold based on person hold id
        /// </summary>
        /// <param name="personHoldsId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonHoldResponse>> DeletePersonHoldsAsync(string personHoldsId)
        {
            var request = new DeleteRestrictionRequest()
            {
                StudentRestrictionsId = await GetStudentHoldIdFromGuidAsync(personHoldsId),
                StrGuid = personHoldsId
            };
            
            //Delete
            var response = await transactionInvoker.ExecuteAsync<DeleteRestrictionRequest, DeleteRestrictionResponse>(request);
            
            //if there are any errors throw
            if (response.DeleteRestrictionErrors.Any())
            {
                var exception = new RepositoryException("Errors encountered while deleting person hold: " + personHoldsId);
                response.DeleteRestrictionErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                throw exception;
            }

            //log any warnings
            List<PersonHoldResponse> warningList = new List<PersonHoldResponse>();
            if (response.DeleteRestrictionWarnings.Any())
            {
                foreach (var restrictionWarning in response.DeleteRestrictionWarnings)
                {
                    PersonHoldResponse restrictionResponse = new PersonHoldResponse();
                    restrictionResponse.WarningCode = restrictionWarning.WarningCodes;
                    restrictionResponse.WarningMessage = restrictionWarning.WarningMessages;

                    warningList.Add(restrictionResponse);
                }
            }
            return warningList;
        }

        #endregion

        #region Updates

        /// <summary>
        /// Updates or creates the person hold
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PersonHoldResponse> UpdatePersonHoldAsync(PersonHoldRequest request)
        {
            var extendedDataTuple = GetEthosExtendedDataLists();
            UpdateRestrictionRequest updateRequest = new UpdateRestrictionRequest();
            updateRequest.StudentRestrictionsId = request.Id;            
            updateRequest.StrComments = string.IsNullOrEmpty(request.Comments) ? string.Empty : request.Comments;
            updateRequest.StrEndDate = request.EndOn.HasValue ? request.EndOn.Value.Date : default(DateTime?);
            updateRequest.StrGuid = request.PersonHoldGuid;
            updateRequest.StrNotify = request.NotificationIndicator.Equals("Y", StringComparison.InvariantCultureIgnoreCase) ? true : false;
            updateRequest.StrRestriction = request.RestrictionType;
            updateRequest.StrStartDate = request.StartOn.Value.Date;
            updateRequest.StrStudent = request.PersonId;
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }
            UpdateRestrictionResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdateRestrictionRequest, UpdateRestrictionResponse>(updateRequest);

            if (updateResponse.RestrictionErrorMessages.Any())
            {
                var errorMessage = string.Empty;
                errorMessage = string.Format("Error occurred updating person hold key '{0}', id '{1}'. ", request.Id, request.PersonHoldGuid);
                exception = new RepositoryException(errorMessage);
                foreach (var message in updateResponse.RestrictionErrorMessages)
                {
                    errorMessage += string.Join(Environment.NewLine, message.ErrorMsg);
                    logger.Error(errorMessage.ToString());
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(message.ErrorCode, " ", message.ErrorMsg))
                    {
                        Id = request.PersonHoldGuid,
                        SourceId = request.Id
                    });
                }
                throw exception;
            }

            PersonHoldResponse response = new PersonHoldResponse();
            response.PersonHoldGuid = updateResponse.StrGuid;
            response.PersonHoldId = updateResponse.StudentRestrictionsId;
            
            return response;
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Builds many person holds
        /// </summary>
        /// <param name="studentHolds"></param>
        /// <returns></returns>
        private List<PersonRestriction> BuildPersonHolds(IEnumerable<StudentRestrictions> studentHolds)
        {
            List<PersonRestriction> personHolds = new List<PersonRestriction>();
            var filteredHolList = studentHolds.Where(h => h.StrEndDate > DateTime.Today.Date || h.StrEndDate == null);

            foreach (var hold in filteredHolList)
            {
                PersonRestriction personHold = BuildPersonHold(hold);
                personHolds.Add(personHold);
            }
            return personHolds;
        }

        /// <summary>
        /// Builds single person hold
        /// </summary>
        /// <param name="studentHold"></param>
        /// <returns></returns>
        private PersonRestriction BuildPersonHold(StudentRestrictions studentHold)
        {
            PersonRestriction stuRestriction = null;
            if (studentHold != null)
            {
                try
                {
                    stuRestriction = new PersonRestriction(studentHold.RecordGuid, studentHold.Recordkey, studentHold.StrStudent, studentHold.StrRestriction, studentHold.StrStartDate, studentHold.StrEndDate, studentHold.StrSeverity, studentHold.StrPrtlDisplayFlag);
                    stuRestriction.Comment = string.IsNullOrEmpty(studentHold.StrComments) ? string.Empty : studentHold.StrComments;
                    stuRestriction.NotificationIndicator = studentHold.StrPrtlDisplayFlag;
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(studentHold.RecordGuid) && !string.IsNullOrEmpty(studentHold.StrStudent) && !string.IsNullOrEmpty(studentHold.StrRestriction))
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Format("Invalid STUDENT.RESTRICTIONS record. {0}", ex.Message))
                        {
                            Id = studentHold.RecordGuid,
                            SourceId = studentHold.Recordkey
                        });
                    }
                }
                
                var message = "Invalid STUDENT.RESTRICTIONS record.";
                if (string.IsNullOrEmpty(studentHold.RecordGuid))
                {
                    exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The GUID is missing.", message))
                    {
                        Id = studentHold.RecordGuid,
                        SourceId = studentHold.Recordkey
                    });
                }
                if (string.IsNullOrEmpty(studentHold.StrStudent))
                {
                    exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STR.STUDENT of '' is invalid.", message))
                    {
                        Id = studentHold.RecordGuid,
                        SourceId = studentHold.Recordkey
                    });
                }
                if (string.IsNullOrEmpty(studentHold.StrRestriction))
                {
                    exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STR.RESTRICTION of '' is invalid.", message))
                    {
                        Id = studentHold.RecordGuid,
                        SourceId = studentHold.Recordkey
                    });
                }
                if (studentHold.StrStartDate == null || !studentHold.StrStartDate.HasValue)
                {
                    exception.AddError(new RepositoryError("Bad.Data", string.Format("{0} The STR.START.DATE of '' is invalid.", message))
                    {
                        Id = studentHold.RecordGuid,
                        SourceId = studentHold.Recordkey
                    });
                }
                return stuRestriction;
            }
            return stuRestriction;
        }

        /// <summary>
        /// Get the GUID for a section using its ID
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Section GUID</returns>
        public async Task<string> GetStudentHoldGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("STUDENT.RESTRICTIONS", id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("student.restriction.guid.NotFound", "GUID not found for student restriction " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetStudentHoldIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Person Holds GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Person Holds GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STUDENT.RESTRICTIONS")
            {
                var errorMessage = string.Format("GUID '{0}' has different entity, '{1}', than expected, 'STUDENT.RESTRICTIONS'", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("GUID.Wrong.Type", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<string> GetPersonIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Person GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Person GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "PERSON")
            {
                var errorMessage = string.Format("GUID '{0}' has different entity, '{1}', than expected, 'PERSON'", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("GUID.Wrong.Type", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }
        #endregion

    }
}
