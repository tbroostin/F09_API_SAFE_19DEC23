// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using System;
using slf4net;
using Ellucian.Web.Cache;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Web.Dependency;
using System.Collections.Generic;
using Ellucian.Web.Http.Configuration;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Repositories;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class OfficeHoursRepository : BaseColleagueRepository, IOfficeHoursRepository
    {
        private string colleagueTimeZone;

        public OfficeHoursRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = 0;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Add office hours for faculty
        /// </summary>
        /// <param name="addOfficeHours">The AddOfficeHours to create</param>
        /// <returns>Added OfficeHours</returns>
        public async Task<AddOfficeHours> AddOfficeHoursAsync(AddOfficeHours addOfficeHours)
        {
            AddOfficeHours newOfficeHours = new AddOfficeHours();

            if (addOfficeHours == null)
            {
                throw new ArgumentNullException("addOfficeHours", "Must provide the add OfficeHours input to add a new office hours.");
            }

            var localStartDate = addOfficeHours.StartDate.ToPointInTimeDateTimeOffset(addOfficeHours.StartDate, colleagueTimeZone);
            var localEndDate = addOfficeHours.EndDate.ToPointInTimeDateTimeOffset(addOfficeHours.EndDate, colleagueTimeZone);
            DateTime? localStartTime = !string.IsNullOrEmpty(addOfficeHours.StartsByTime) ? Convert.ToDateTime(addOfficeHours.StartsByTime) : (DateTime?)null;
            DateTime? localEndTime = !string.IsNullOrEmpty(addOfficeHours.EndsByTime) ? Convert.ToDateTime(addOfficeHours.EndsByTime) : (DateTime?)null;

            AddOfficeHoursRequest newRequest = new AddOfficeHoursRequest()
            {
                InFacultyId = addOfficeHours.Id,
                InStartDate = localStartDate.Value.Date,
                InEndDate = localEndDate.HasValue ? localEndDate.Value.Date : (DateTime?)null,
                InStartTime = localStartTime.HasValue ? localStartTime.Value : (DateTime?)null,
                InEndTime = localEndTime.HasValue ? localEndTime.Value : (DateTime?)null,
                InBuilding = addOfficeHours.BuildingCode,
                InRoom = addOfficeHours.RoomCode,
                InRepeat = addOfficeHours.Frequency,
            };

            string[] strDaysOfWeek = addOfficeHours.DaysOfWeek.Split(',');
            foreach (var code in strDaysOfWeek)
            {
                switch (code.ToUpper())
                {
                    case "M":
                        newRequest.InMonday = "Y";
                        break;
                    case "T":
                        newRequest.InTuesday = "Y";
                        break;
                    case "W":
                        newRequest.InWednesday = "Y";
                        break;
                    case "TH":
                        newRequest.InThursday = "Y";
                        break;
                    case "F":
                        newRequest.InFriday = "Y";
                        break;
                    case "SA":
                        newRequest.InSaturday = "Y";
                        break;
                    case "SU":
                        newRequest.InSunday = "Y";
                        break;
                    default:
                        break;
                }
            }

            AddOfficeHoursResponse createResponse = null;
            try
            {
                createResponse = await transactionInvoker.ExecuteAsync<AddOfficeHoursRequest, AddOfficeHoursResponse>(newRequest);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred during addOfficeHours transaction execution.");
                throw new ApplicationException("Error occurred during Add OfficeHours creation.");
            }
            if (createResponse == null)
            {
                logger.Error("Null response returned by add OfficeHours transaction.");
                throw new ApplicationException("Null response returned by add OfficeHours creation.");
            }
            if (createResponse.OutErrorMsgs != null && createResponse.OutErrorMsgs.Count > 0)
            {
                string error = "An error occurred while trying to add officehours";
                logger.Error(error);
                createResponse.OutErrorMsgs.ForEach(message => logger.Error(message));
                throw new ApplicationException(error);
            }

            // Clear Cache after adding successfully
            ClearCache(new List<string> { "FacultyOfficeHours" + addOfficeHours.Id });

            return newOfficeHours;
        }

        /// <summary>
        /// Update office hours information for a faculty
        /// </summary>
        /// <param name="UpdateOfficeHours">The office hours information to update</param>
        /// <returns>Updated OfficeHours</returns>
        public async Task<UpdateOfficeHours> UpdateOfficeHoursAsync(UpdateOfficeHours updateOfficeHours)
        {
            UpdateOfficeHours newOfficeHours = new UpdateOfficeHours();

            if (updateOfficeHours == null)
            {
                throw new ArgumentNullException("updateOfficeHours", "Office hours item must be provided to update.");
            }

            var oldLocalStartDate = updateOfficeHours.OldStartDate.ToPointInTimeDateTimeOffset(updateOfficeHours.OldStartDate, colleagueTimeZone);
            var oldLocalEndDate = updateOfficeHours.OldEndDate.ToPointInTimeDateTimeOffset(updateOfficeHours.OldEndDate, colleagueTimeZone);
            DateTime? oldLocalStartTime = !string.IsNullOrEmpty(updateOfficeHours.OldStartsByTime) ? Convert.ToDateTime(updateOfficeHours.OldStartsByTime) : (DateTime?)null;
            DateTime? oldLocalEndTime = !string.IsNullOrEmpty(updateOfficeHours.OldEndsByTime) ? Convert.ToDateTime(updateOfficeHours.OldEndsByTime) : (DateTime?)null;

            var newLocalStartDate = updateOfficeHours.NewStartDate.ToPointInTimeDateTimeOffset(updateOfficeHours.NewStartDate, colleagueTimeZone);
            var newLocalEndDate = updateOfficeHours.NewEndDate.ToPointInTimeDateTimeOffset(updateOfficeHours.NewEndDate, colleagueTimeZone);
            DateTime? newLocalStartTime = !string.IsNullOrEmpty(updateOfficeHours.NewStartsByTime) ? Convert.ToDateTime(updateOfficeHours.NewStartsByTime) : (DateTime?)null;
            DateTime? newLocalEndTime = !string.IsNullOrEmpty(updateOfficeHours.NewEndsByTime) ? Convert.ToDateTime(updateOfficeHours.NewEndsByTime) : (DateTime?)null;


            var oldStartTime = oldLocalStartTime.ToPointInTimeDateTimeOffset(oldLocalStartTime, colleagueTimeZone);
            var newStartTime = newLocalStartTime.ToPointInTimeDateTimeOffset(newLocalStartTime, colleagueTimeZone);

            UpdateOfficeHoursRequest updateRequest = new UpdateOfficeHoursRequest()
            {
                InFacultyId = updateOfficeHours.Id,
                InOldStartDate = oldLocalStartDate.Value.Date,
                InNewStartDate = newLocalStartDate.Value.Date,
                InOldEndDate = oldLocalEndDate.HasValue ? oldLocalEndDate.Value.Date : (DateTime?)null,
                InNewEndDate = newLocalEndDate.HasValue ? newLocalEndDate.Value.Date : (DateTime?)null,
                InOldStartTime = oldStartTime.HasValue ? oldStartTime.Value.DateTime : (DateTime?)null,
                InNewStartTime = newStartTime.HasValue ? newStartTime.Value.DateTime : (DateTime?)null,
                InOldEndTime = oldLocalEndTime.HasValue ? oldLocalEndTime.Value : (DateTime?)null,
                InNewEndTime = newLocalEndTime.HasValue ? newLocalEndTime.Value : (DateTime?)null,
                InOldBuilding = updateOfficeHours.OldBuildingCode,
                InNewBuilding = updateOfficeHours.NewBuildingCode,
                InOldRoom = updateOfficeHours.OldRoomCode,
                InNewRoom = updateOfficeHours.NewRoomCode,
                InOldRepeat = updateOfficeHours.OldFrequency,
                InNewRepeat = updateOfficeHours.NewFrequency
            };

            string[] strOldDaysOfWeek = updateOfficeHours.OldDaysOfWeek.Split(',');
            foreach (var code in strOldDaysOfWeek)
            {
                switch (code.ToUpper())
                {
                    case "M":
                        updateRequest.InOldMonday = "Y";
                        break;
                    case "T":
                        updateRequest.InOldTuesday = "Y";
                        break;
                    case "W":
                        updateRequest.InOldWednesday = "Y";
                        break;
                    case "TH":
                        updateRequest.InOldThursday = "Y";
                        break;
                    case "F":
                        updateRequest.InOldFriday = "Y";
                        break;
                    case "SA":
                        updateRequest.InOldSaturday = "Y";
                        break;
                    case "SU":
                        updateRequest.InOldSunday = "Y";
                        break;
                    default:
                        break;
                }
            }

            string[] strNewDaysOfWeek = updateOfficeHours.NewDaysOfWeek.Split(',');
            foreach (var code in strNewDaysOfWeek)
            {
                switch (code.ToUpper())
                {
                    case "M":
                        updateRequest.InNewMonday = "Y";
                        break;
                    case "T":
                        updateRequest.InNewTuesday = "Y";
                        break;
                    case "W":
                        updateRequest.InNewWednesday = "Y";
                        break;
                    case "TH":
                        updateRequest.InNewThursday = "Y";
                        break;
                    case "F":
                        updateRequest.InNewFriday = "Y";
                        break;
                    case "SA":
                        updateRequest.InNewSaturday = "Y";
                        break;
                    case "SU":
                        updateRequest.InNewSunday = "Y";
                        break;
                    default:
                        break;
                }
            }

            UpdateOfficeHoursResponse updateResponse = null;
            try
            {
                updateResponse = await transactionInvoker.ExecuteAsync<UpdateOfficeHoursRequest, UpdateOfficeHoursResponse>(updateRequest);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred during updateOfficeHours transaction execution.");
                throw new ApplicationException("Error occurred during OfficeHours updation.");
            }
            if (updateResponse == null)
            {
                logger.Error("Null response returned by update OfficeHours transaction.");
                throw new ApplicationException("Null response returned by OfficeHours updation.");
            }
            if (updateResponse.OutErrorMsgs != null && updateResponse.OutErrorMsgs.Count > 0)
            {
                string error = "An error occurred while trying to update officehours";
                logger.Error(error);
                updateResponse.OutErrorMsgs.ForEach(message => logger.Error(message));
                throw new ApplicationException(error);
            }

            // Clear Cache after updating successfully
            ClearCache(new List<string> { "FacultyOfficeHours" + updateOfficeHours.Id });

            return newOfficeHours;
        }

        /// <summary>
        /// delete office hours information for a faculty
        /// </summary>
        /// <param name="deleteOfficeHours">The office hours information to delete</param>
        /// <returns>Deleted OfficeHours</returns>
        public async Task<DeleteOfficeHours> DeleteOfficeHoursAsync(DeleteOfficeHours deleteOfficeHours)
        {
            DeleteOfficeHours deletedOfficeHours = new DeleteOfficeHours();

            if (deleteOfficeHours == null)
            {
                throw new ArgumentNullException("deleteOfficeHours", "Must provide the OfficeHours input to delete office hours.");
            }

            var localStartDate = deleteOfficeHours.StartDate.ToPointInTimeDateTimeOffset(deleteOfficeHours.StartDate, colleagueTimeZone);
            var localEndDate = deleteOfficeHours.EndDate.ToPointInTimeDateTimeOffset(deleteOfficeHours.EndDate, colleagueTimeZone);
            DateTime? localStartTime = !string.IsNullOrEmpty(deleteOfficeHours.StartsByTime) ? Convert.ToDateTime(deleteOfficeHours.StartsByTime) : (DateTime?)null;
            DateTime? localEndTime = !string.IsNullOrEmpty(deleteOfficeHours.EndsByTime) ? Convert.ToDateTime(deleteOfficeHours.EndsByTime) : (DateTime?)null;

            DeleteOfficeHoursRequest deleteRequest = new DeleteOfficeHoursRequest()
            {
                InFacultyId = deleteOfficeHours.Id,
                InStartDate = localStartDate.Value.Date,
                InEndDate = localEndDate.HasValue ? localEndDate.Value.Date : (DateTime?)null,
                InStartTime = localStartTime.HasValue ? localStartTime.Value : (DateTime?)null,
                InEndTime = localEndTime.HasValue ? localEndTime.Value : (DateTime?)null,
                InBuilding = deleteOfficeHours.BuildingCode,
                InRoom = deleteOfficeHours.RoomCode,
                InRepeat = deleteOfficeHours.Frequency,
            };

            string[] strDaysOfWeek = deleteOfficeHours.DaysOfWeek.Split(',');
            foreach (var code in strDaysOfWeek)
            {
                switch (code.ToUpper())
                {
                    case "M":
                        deleteRequest.InMonday = "Y";
                        break;
                    case "T":
                        deleteRequest.InTuesday = "Y";
                        break;
                    case "W":
                        deleteRequest.InWednesday = "Y";
                        break;
                    case "TH":
                        deleteRequest.InThursday = "Y";
                        break;
                    case "F":
                        deleteRequest.InFriday = "Y";
                        break;
                    case "SA":
                        deleteRequest.InSaturday = "Y";
                        break;
                    case "SU":
                        deleteRequest.InSunday = "Y";
                        break;
                    default:
                        break;
                }
            }

            DeleteOfficeHoursResponse deleteResponse = null;
            try
            {
                deleteResponse = await transactionInvoker.ExecuteAsync<DeleteOfficeHoursRequest, DeleteOfficeHoursResponse>(deleteRequest);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred during deleteOfficeHours transaction execution.");
                throw new ApplicationException("Error occurred during OfficeHours deletion.");
            }
            if (deleteResponse == null)
            {
                logger.Error("Null response returned by delete OfficeHours transaction.");
                throw new ApplicationException("Null response returned by OfficeHours deletion.");
            }
            if (deleteResponse.OutErrorMsgs != null && deleteResponse.OutErrorMsgs.Count > 0)
            {
                string error = "An error occurred while trying to delete officehours";
                logger.Error(error);
                deleteResponse.OutErrorMsgs.ForEach(message => logger.Error(message));
                throw new ApplicationException(error);
            }

            // Clear Cache after adding successfully
            ClearCache(new List<string> { "FacultyOfficeHours" + deleteOfficeHours.Id });

            return deletedOfficeHours;
        }
    }
}
