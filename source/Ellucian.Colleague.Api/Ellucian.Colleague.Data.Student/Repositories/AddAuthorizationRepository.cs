// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
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
    public class AddAuthorizationRepository : BaseColleagueRepository, IAddAuthorizationRepository
    {
        private string colleagueTimeZone;

        public AddAuthorizationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Not cached
            CacheTimeout = 0;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Update add authorization information for a section
        /// </summary>
        /// <param name="addAuthorization">The AddAuthorization to update</param>
        /// <returns>Updated AddAuthorization</returns>
        public async Task<AddAuthorization> UpdateAddAuthorizationAsync(AddAuthorization addAuthorization)
        {
            AddAuthorization updatedAddAuthorization = null;
            if (addAuthorization == null)
            {
                throw new ArgumentNullException("addAuthorization", "Must provide the add authorization item to be updated.");
            }
            UpdateAddAuthorizationRequest updateRequest = new UpdateAddAuthorizationRequest();
            updateRequest.AddAuthorizationId = addAuthorization.Id;
            updateRequest.SectionId = addAuthorization.SectionId;
            updateRequest.AddCode = addAuthorization.AddAuthorizationCode;
            updateRequest.StudentId = addAuthorization.StudentId;
            updateRequest.AssignedBy = addAuthorization.AssignedBy;
            if (addAuthorization.AssignedTime.HasValue)
            {
                var assignedDate = addAuthorization.AssignedTime.Value.ToLocalDateTime(colleagueTimeZone);
                updateRequest.AssignedDate = assignedDate;
                updateRequest.AssignedTime = assignedDate;
            }
            updateRequest.IsRevoked = addAuthorization.IsRevoked;
            updateRequest.RevokedBy = addAuthorization.RevokedBy;
            if (addAuthorization.RevokedTime.HasValue)
            {
                var revokedDate = addAuthorization.RevokedTime.Value.ToLocalDateTime(colleagueTimeZone);
                updateRequest.RevokedDate = revokedDate;
                updateRequest.RevokedTime = revokedDate;
            }
            UpdateAddAuthorizationResponse updateResponse = null;
            try
            {
                updateResponse = await transactionInvoker.ExecuteAsync<UpdateAddAuthorizationRequest, UpdateAddAuthorizationResponse>(updateRequest);
            }
            catch
            {
                logger.Error("Error occurred during UpdateAddAuthorization transaction execution.");
                throw new ApplicationException("Error occurred during Add Authorization Update.");
            }
            if (updateResponse == null)
            {
                logger.Error("Null response returned by update add authorization transaction.");
                throw new ApplicationException("Null response returned by Add Authorization Update.");
            }
            if (updateResponse.ErrorOccurred)
            {
                // Send different error depending on whether it was locked, not found, or had another problem.
                if (updateResponse.ErrorMessage.Contains("Not Found"))
                {
                    logger.Error("Could not find the add authorization with id " + addAuthorization.Id);
                    throw new KeyNotFoundException("Unable to find add authorization with ID " + addAuthorization.Id);
                }
                if (updateResponse.ErrorMessage.Contains("Locked"))
                {
                    logger.Error("Add authorization with id " + addAuthorization.Id + " is locked");
                    throw new RecordLockException("Add authorization with id " + addAuthorization.Id + " is locked", "AddAuthorizations", addAuthorization.Id);
                }
                else
                {
                    string otherError = updateResponse.ErrorMessage + " Authorization Id " + addAuthorization.Id;
                    logger.Error(otherError);
                    throw new ApplicationException(otherError);
                }
            }
            if (!updateResponse.ErrorOccurred)
            {
                try
                {
                    // Update successful and we have a returned ID - Get the newly updated authorization from Colleague. 
                    // If we don't have an id or can't get the item it throw an error.
                    updatedAddAuthorization = await GetAsync(updateResponse.UpdatedAddAuthorizationId);

                }
                catch (KeyNotFoundException)
                {
                    logger.Error("Could not find the updated authorization specified by id " + updateResponse.UpdatedAddAuthorizationId);
                    throw new ApplicationException();
                }
                catch (Exception)
                {
                    logger.Error("Error occurred while retrieving newly added request using id " + updateResponse.UpdatedAddAuthorizationId);
                    throw;
                }

            }
            return updatedAddAuthorization;
        }


        /// <summary>
        /// Get an add authorization
        /// </summary>
        /// <param name="addAuthorizationId">Id of the add authorization</param>
        /// <returns>The <see cref="Domain.Student.Entities.AddAuthorization">Add Authorization</see>> is returned</returns>
        public async Task<AddAuthorization> GetAsync(string addAuthorizationId)
        {

            if (string.IsNullOrEmpty(addAuthorizationId))
            {
                throw new ArgumentNullException("addAuthorizationId", "Add Authorization Id must be provided.");
            }

            var authorizationRecord = await DataReader.ReadRecordAsync<AddAuthorizations>(addAuthorizationId);

            if (authorizationRecord == null)
            {
                logger.Error("Get: Add Authorization record not found for request Id " + addAuthorizationId);
                throw new KeyNotFoundException();
            }


            List<AddAuthorization> addAuthorizationEntities = BuildAddAuthorizations(new List<AddAuthorizations>() { authorizationRecord });
            return addAuthorizationEntities.FirstOrDefault();


        }

        /// <summary>
        /// Gets an add authorization by Section and Add Code
        /// </summary>
        /// <param name="sectionId">Section Id of the add authorization to retrieve (required)</param>
        /// <param name="addAuthorizationCode">Add Code of authorization to retrieve (required)</param>
        /// <returns>Add Authorization entity</returns>
        public async Task<AddAuthorization> GetAddAuthorizationByAddCodeAsync(string sectionId, string addAuthorizationCode)
        {
            if (string.IsNullOrEmpty(sectionId) || string.IsNullOrEmpty(addAuthorizationCode))
            {
                logger.Error("Must provide a section Id and an authorization code to lookup authorization by add code.");
                throw new ArgumentNullException("Must provide a section Id and an authorization code to lookup authorization by add code.");
            }
            Collection<AddAuthorizations> addAuthorizations;
            try
            {
                string addCriteria = "WITH AAU.COURSE.SECTION = '" + sectionId + "' AND WITH AAU.AUTHORIZATION.CODE = '" + addAuthorizationCode + "'";
                logger.Info("Retrieving add authorizations by add code using criteria '" + addCriteria + "'");
                addAuthorizations = await DataReader.BulkReadRecordAsync<AddAuthorizations>(addCriteria);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Bulk read failed for section Id " + sectionId + " and add code " + addAuthorizationCode);
                throw new ApplicationException("Failed to retrieve add authorization for section Id " + sectionId + " and add code " + addAuthorizationCode);
            }
            if (addAuthorizations == null || !addAuthorizations.Any())
            {
                logger.Error("Add Authorization record not found for section Id " + sectionId + " with add code " + addAuthorizationCode);
                throw new KeyNotFoundException("Add Authorization record not found for section Id " + sectionId + " with add code " + addAuthorizationCode);
            }
            if (addAuthorizations.Count() > 1)
            {
                logger.Error("More than one Add Authorization record found for section Id " + sectionId + " and add code " + addAuthorizationCode + ". INVALID.");
                throw new ApplicationException("More than one Add Authorization record found for section Id " + sectionId + " and add code " + addAuthorizationCode + ". INVALID.");
            }

            List<AddAuthorization> result = BuildAddAuthorizations(addAuthorizations.ToList());
            return result == null ? null : result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves add authorizations for a section
        /// </summary>
        /// <param name="sectionId">Id of section</param>
        /// <returns>Add Authorization for the section</returns>
        public async Task<IEnumerable<AddAuthorization>> GetSectionAddAuthorizationsAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section Id must be provided to retrieve section add authorizations.");
            }

            Collection<AddAuthorizations> addAuthorizations;
            try
            {
                string addCriteria = "WITH AAU.COURSE.SECTION = '" + sectionId + "'";
                logger.Info("Retrieving section add authorizations using criteria '" + addCriteria + "'");
                addAuthorizations = await DataReader.BulkReadRecordAsync<AddAuthorizations>(addCriteria);
            }
            catch (Exception ex)
            {
                logger.Error(ex,"Bulk read failed for section Id " + sectionId);
                throw new ApplicationException("Add Authorization read failed for section Id " + sectionId);

            }
            if (addAuthorizations == null)
            {
                return new List<AddAuthorization>();
            }
            else
            {
                logger.Info("Retrieved " + addAuthorizations.Count() + " add authorizations for section " + sectionId);
                List<AddAuthorization> result = BuildAddAuthorizations(addAuthorizations.ToList());
                return result;
            }
        }

        /// <summary>
        /// Retrieve add authorizations for a student
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>Add Authorizations for the student</returns>
        public async Task<IEnumerable<AddAuthorization>> GetStudentAddAuthorizationsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID must be provided to retrieve student add authorizations.");
            }
            Collection<AddAuthorizations> addAuthorizations;
            try
            {
                string addCriteria = "WITH AAU.STUDENT = '" + studentId + "'";
                logger.Info("Retrieving student add authorizations using criteria '" + addCriteria + "'");
                addAuthorizations = await DataReader.BulkReadRecordAsync<AddAuthorizations>(addCriteria);
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Bulk read failed for student {0} ", studentId));
                throw new ApplicationException(string.Format("Add Authorization read failed for student {0}", studentId));
            }
            if (addAuthorizations == null)
            {
                logger.Error(string.Format("No add authorizations retrieved for student {0}", studentId));
                return new List<AddAuthorization>();
            }
            logger.Error(string.Format("Retrieved {0} add authorizations for student {1}", addAuthorizations.Count(), studentId));
            List<AddAuthorization> result = BuildAddAuthorizations(addAuthorizations.ToList());
            return result;
        }

        /// <summary>
        /// Builds a collection of <see cref="AddAuthorization"/> objects from a collection of <see cref="AddAuthorizations">ADD.AUTHORIZATIONS</see> data objects.
        /// </summary>
        /// <param name="authorizationData">Collection of <see cref="AddAuthorizations">ADD.AUTHORIZATIONS</see> data objects</param>
        /// <returns>Collection of <see cref="AddAuthorization"/> objects</returns>
        /// <summary>
        /// Create an add authorization information for a student in a section
        /// </summary>
        /// <param name="addAuthorizationInput">The AddAuthorization to create</param>
        /// <returns>Created AddAuthorization</returns>
        public async Task<AddAuthorization> CreateAddAuthorizationAsync(AddAuthorization addAuthorizationInput)
        {
            AddAuthorization newAddAuthorization = null;
            if (addAuthorizationInput == null)
            {
                throw new ArgumentNullException("addAuthorizationInput", "Must provide the add authorization input needed to create a new add authorization.");
            }
            CreateAddAuthorizationRequest newRequest = new CreateAddAuthorizationRequest();
            newRequest.SectionId = addAuthorizationInput.SectionId;
            newRequest.StudentId = addAuthorizationInput.StudentId;
            newRequest.AssignedBy = addAuthorizationInput.AssignedBy;
            if (addAuthorizationInput.AssignedTime.HasValue)
            {
                var assignedDate = addAuthorizationInput.AssignedTime.Value.ToLocalDateTime(colleagueTimeZone);
                newRequest.AssignedDate = assignedDate;
                newRequest.AssignedTime = assignedDate;
            }
            
            CreateAddAuthorizationResponse createResponse = null;
            try
            {
                createResponse = await transactionInvoker.ExecuteAsync<CreateAddAuthorizationRequest, CreateAddAuthorizationResponse>(newRequest);
            }
            catch
            {
                logger.Error("Error occurred during CreateAddAuthorization transaction execution.");
                throw new ApplicationException("Error occurred during Add Authorization creation.");
            }
            if (createResponse == null)
            {
                logger.Error("Null response returned by create add authorization transaction.");
                throw new ApplicationException("Null response returned by Add Authorization creation.");
            }
            if (createResponse.ErrorOccurred)
            {
                if (createResponse.ErrorMessage.Contains("Conflict"))
                {
                    logger.Error(createResponse.ErrorMessage);
                    throw new ExistingResourceException();
                }
                else
                {
                    logger.Error(createResponse.ErrorMessage);
                    throw new ApplicationException(createResponse.ErrorMessage);
                }
              
            }
            if (!createResponse.ErrorOccurred)
            {
                try
                {
                    // Create successful and we have a returned ID - Get the new authorization from Colleague. 
                    // If we don't have an id or can't get the item it throw an error.
                    newAddAuthorization = await GetAsync(createResponse.NewAddAuthorizationId);

                }
                catch (KeyNotFoundException)
                {
                    logger.Error("Could not find the newly created authorization specified by id " + createResponse.NewAddAuthorizationId);
                    throw new ApplicationException();
                }
                catch (Exception)
                {
                    logger.Error("Error occurred while retrieving new add authorization with id " + createResponse.NewAddAuthorizationId);
                    throw;
                }

            }
            return newAddAuthorization;
        }

        private List<AddAuthorization> BuildAddAuthorizations(List<AddAuthorizations> authorizationData)
        {
            // Get all add authorization details
            List<AddAuthorization> authorizationList = new List<AddAuthorization>();
            if (authorizationData != null && authorizationData.Any())
            {
                foreach (var authorizationContract in authorizationData)
                {
                    try
                    {
                        AddAuthorization addAuthorization = new AddAuthorization(authorizationContract.Recordkey, authorizationContract.AauCourseSection)
                        {
                            AddAuthorizationCode = authorizationContract.AauAuthorizationCode,
                            StudentId = authorizationContract.AauStudent,
                            AssignedTime = authorizationContract.AauAssignedTime.HasValue && authorizationContract.AauAssignedDate.HasValue ? authorizationContract.AauAssignedTime.ToPointInTimeDateTimeOffset(authorizationContract.AauAssignedDate, colleagueTimeZone) : null,
                            AssignedBy = authorizationContract.AauAssignedBy,
                            IsRevoked = !string.IsNullOrEmpty(authorizationContract.AauRevokedFlag) && authorizationContract.AauRevokedFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase),
                            RevokedBy = authorizationContract.AauRevokedBy,
                            RevokedTime = authorizationContract.AauRevokedTime.HasValue && authorizationContract.AauRevokedDate.HasValue ? authorizationContract.AauRevokedTime.ToPointInTimeDateTimeOffset(authorizationContract.AauRevokedDate, colleagueTimeZone) : null
                        };
                        authorizationList.Add(addAuthorization);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("AddAuthorizations", authorizationContract.Recordkey, authorizationContract, ex);
                    }
                }
                logger.Info("Number of add authorization entities retreived is " + authorizationList.Count());
            }
            return authorizationList;
        }
    }
}
