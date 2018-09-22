// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Applicant Repository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ApplicationStatusRepository : BaseColleagueRepository, IApplicationStatusRepository
    {
        public static char _VM = Convert.ToChar(DynamicArray.VM);
        public ApplicationStatusRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Gets all admission statusess.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>> GetApplicationStatusesAsync(int offset, int limit, string applicationId, bool bypassCache = false)
        {
            int totalCount = 0;
            string criteria = string.Empty;
            string[] applStatusesNoSpCodeIds;

            var admissionStatusesEntities = new List<Domain.Student.Entities.ApplicationStatus2>();

            if (!string.IsNullOrEmpty(applicationId))
            {
                var applId = await GetRecordKeyFromGuidAsync(applicationId);
                criteria = string.Format("WITH APPLICATIONS.ID EQ '{0}' AND WITH APPL.STATUS.DATE NE '' AND WITH APPL.STATUS.TIME NE '' AND WITH APPL.STATUS EQ '?' BY.EXP APPL.STATUS.DATE.TIME.IDX", applId);
            }
            else
            {
                criteria = "WITH APPL.STATUS.DATE NE '' AND WITH APPL.STATUS.TIME NE '' AND WITH APPL.STATUS EQ '?' BY.EXP APPL.STATUS.DATE.TIME.IDX";
            }

            applStatusesNoSpCodeIds = await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE NE ''");

            var applIds = await DataReader.SelectAsync("APPLICATIONS", criteria, applStatusesNoSpCodeIds.Distinct().ToArray(), "?");

            if (applIds != null && !applIds.Any())
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>(new List<Domain.Student.Entities.ApplicationStatus2>(), 0);
            }

            var applIds2 = new List<string>();
            foreach (var applId in applIds)
            {
                var perposKey = applId.Split(_VM)[0];
                applIds2.Add(perposKey);
            }

            var criteria2 = "";

            if (!string.IsNullOrEmpty(applicationId))
            {
                var applId = await GetRecordKeyFromGuidAsync(applicationId);
                criteria2 = string.Format("WITH APPLICATIONS.ID EQ '{0}' AND WITH APPL.STATUS.DATE NE '' AND WITH APPL.STATUS.TIME NE '' AND WITH APPL.STATUS EQ '?' BY.EXP APPL.STATUS.DATE.TIME.IDX SAVING APPL.STATUS.DATE.TIME.IDX", applId);
            }
            else
            {
                criteria2 = "WITH APPL.STATUS.DATE NE '' AND WITH APPL.STATUS.TIME NE '' AND WITH APPL.STATUS EQ '?' BY.EXP APPL.STATUS.DATE.TIME.IDX SAVING APPL.STATUS.DATE.TIME.IDX";
            }

            var applIdxs = await DataReader.SelectAsync("APPLICATIONS", criteria2.ToString(), applStatusesNoSpCodeIds.Distinct().ToArray(), "?");

            var keys = new List<string>();
            var idx = 0;

            foreach (var applId2 in applIds2)
            {
                var statusCode = applIdxs.ElementAt(idx).Split(new[] { '*' })[0];
                if (applStatusesNoSpCodeIds.Contains(statusCode))
                {
                    keys.Add(String.Concat(applId2, "|", applIdxs.ElementAt(idx)));
                    idx++;
                }
                else
                {
                    idx++;
                }
            }

            totalCount = keys.Count();
            keys.Sort();
            var keysSubList = keys.Skip(offset).Take(limit).Distinct().ToArray();

            if (keysSubList.Any())
            {
                var subList = new List<string>();

                foreach (var key in keysSubList)
                {
                    var applKey = key.Split('|')[0];
                    subList.Add(applKey);
                }
                var applications = await DataReader.BulkReadRecordAsync<Applications>("APPLICATIONS", subList.Distinct().ToArray());

                foreach (var key in keysSubList)
                {
                    var splitKey = key.Split('|');
                    var application = applications.FirstOrDefault(x => x.Recordkey == splitKey[0]);
                    var applicationKey = splitKey[0];
                    var applStatusIdx = splitKey[1];
                    var splitValues = applStatusIdx.Split(new[] { '*' });
                    var applStatus = splitValues[0];
                    var applStatusDate = Convert.ToDateTime(DmiString.PickDateToDateTime(Convert.ToInt32(splitValues[1])));
                    var applStatusTime = new DateTime(1, 1, 1, DmiString.PickTimeToDateTime(Convert.ToInt32(splitValues[2])).Hours,
                                                               DmiString.PickTimeToDateTime(Convert.ToInt32(splitValues[2])).Minutes,
                                                               DmiString.PickTimeToDateTime(Convert.ToInt32(splitValues[2])).Seconds);

                    var entity = application.ApplStatusesEntityAssociation.FirstOrDefault(i =>
                                    i.ApplStatusAssocMember.Equals(applStatus, StringComparison.OrdinalIgnoreCase) &&
                                    i.ApplStatusDateAssocMember.Equals(applStatusDate) &&
                                    i.ApplStatusTimeAssocMember.Equals(applStatusTime));

                    if (entity != null)
                    {
                        if (applStatusesNoSpCodeIds.Contains(entity.ApplStatusAssocMember))
                        {
                            var applStatusKey = string.Format("{0}*{1}*{2}",
                                                entity.ApplStatusAssocMember,
                                                DmiString.DateTimeToPickDate(entity.ApplStatusDateAssocMember.Value),
                                                DmiString.DateTimeToPickTime(entity.ApplStatusTimeAssocMember.Value));

                            var applGuidInfo = await GetGuidFromRecordInfoAsync("APPLICATIONS", applicationKey, "APPL.STATUS.DATE.TIME.IDX", applStatusKey);
                            if (!string.IsNullOrEmpty(applicationId) && applGuidInfo.Equals(applicationId, StringComparison.OrdinalIgnoreCase))
                            {
                                return new Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>(new List<Domain.Student.Entities.ApplicationStatus2>(), 0);
                            }

                            if (applStatusIdx.Equals(applStatusKey, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(applGuidInfo))
                                    {
                                        Domain.Student.Entities.ApplicationStatus2 adminStatus = new Domain.Student.Entities.ApplicationStatus2(applGuidInfo, application.Recordkey,
                                            entity.ApplStatusAssocMember, entity.ApplStatusDateAssocMember.Value, entity.ApplStatusTimeAssocMember.Value);
                                        admissionStatusesEntities.Add(adminStatus);
                                    }
                                }
                                catch (KeyNotFoundException)
                                {
                                    throw new KeyNotFoundException(string.Format("Application record not found for key {0}.", applStatusKey));
                                }
                            }
                        }
                    }
                }
            }

            return admissionStatusesEntities.Any() ? new Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>(admissionStatusesEntities, totalCount) :
                new Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>(new List<Domain.Student.Entities.ApplicationStatus2>(), 0);
        }

        /// <summary>
        /// Gets admission status by id.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Domain.Student.Entities.ApplicationStatus2> GetApplicationStatusByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Admission decision guid is required.");
            }

            var applicationId = await GetRecordInfoFromGuidAsync(guid);

            if (applicationId == null)
                throw new KeyNotFoundException(string.Format("Application record not found for guid {0}", guid));

            var applStatusesNoSpCodeIds = await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            var application = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", applicationId.PrimaryKey);

            Domain.Student.Entities.ApplicationStatus2 adminStatus = null;

            foreach (var applStatusEntity in application.ApplStatusesEntityAssociation)
            {
                var applStatusKey = string.Format("{0}*{1}*{2}",
                                    applStatusEntity.ApplStatusAssocMember,
                                    DmiString.DateTimeToPickDate(applStatusEntity.ApplStatusDateAssocMember.Value),
                                    DmiString.DateTimeToPickTime(applStatusEntity.ApplStatusTimeAssocMember.Value));

                var applGuidInfo = await GetGuidFromRecordInfoAsync("APPLICATIONS", applicationId.PrimaryKey, "APPL.STATUS.DATE.TIME.IDX", applStatusKey);

                if (!string.IsNullOrEmpty(applGuidInfo))
                {
                    if (applGuidInfo.Equals(guid, StringComparison.OrdinalIgnoreCase))
                    {
                        if (applStatusesNoSpCodeIds.Contains(applStatusEntity.ApplStatusAssocMember))
                        {
                            break;
                        }
                        adminStatus = new Domain.Student.Entities.ApplicationStatus2(applGuidInfo, application.Recordkey, applStatusEntity.ApplStatusAssocMember,
                                    applStatusEntity.ApplStatusDateAssocMember.Value, applStatusEntity.ApplStatusTimeAssocMember.Value);
                        break;
                    }
                }
            }

            return adminStatus;
        }

        /// <summary>
        /// Creates admission decision.
        /// </summary>
        /// <param name="appStatusEntity"></param>
        /// <returns></returns>
        public async Task<ApplicationStatus2> UpdateAdmissionDecisionAsync(ApplicationStatus2 appStatusEntity)
        {
            if(appStatusEntity == null)
            {
                throw new ArgumentNullException("Admission Decision", "Admission decision must be provided.");
            }

            var request = new UpdateAdmApplStatusesRequest()
            {
                AdmdecGuid = appStatusEntity.Guid,
                AdmdecApplication = appStatusEntity.ApplicantRecordKey,
                AdmdecDate = appStatusEntity.DecidedOnDate,
                AdmdecTime = appStatusEntity.DecidedOnTime,
                AdmdecStatus = appStatusEntity.DecisionType
            };

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateAdmApplStatusesRequest, UpdateAdmApplStatusesResponse>(request);

            if (response.ApplicationStatusErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating address '{0}':", appStatusEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                response.ApplicationStatusErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCode, e.ErrorMsg)));
                logger.Error(errorMessage);
                throw exception;
            }


            return await GetApplicationStatusByGuidAsync(response.AdmdecGuid, true);
        }

        /// <summary>
        /// Gets tuple with entity, primary key & secondary key.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Tuple<string, string, string>> GetApplicationStatusKey(string guid)
        {
            var result = await GetRecordInfoFromGuidAsync(guid);
            return result == null? null : new Tuple<string, string, string>(result.Entity, result.PrimaryKey, result.SecondaryKey);
        }
    }
}