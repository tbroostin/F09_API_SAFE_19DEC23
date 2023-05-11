// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Accesses Colleague for a person's proxy information.
    /// </summary>
    [RegisterType]
    public class ProxyRepository : BaseColleagueRepository, IProxyRepository
    {
        protected const int ProxyRepositoryCacheTimeout = 60;

        private const string EmployeeProxyGroupSpecialProcessingCode = "EMP";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public ProxyRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get the proxy configuration.
        /// </summary>
        /// <returns>The <see cref="ProxyConfiguration">proxy configuration</see></returns>
        public async Task<ProxyConfiguration> GetProxyConfigurationAsync()
        {
            return await GetOrAddToCacheAsync<ProxyConfiguration>("ProxyConfiguration",
                async () =>
                {
                    ProxyConfiguration config;

                    // Get the master data from MAP.PROXY.USER.PERMISSIONS
                    var proxyAndUserPermissionsMapEntiies = await GetProxyAndUserPermissionsMappingInfoAsync();

                    var proxyDefaults = await this.DataReader.ReadRecordAsync<ProxyDefaults>("UT.PARMS", "PROXY.DEFAULTS");
                    // Checks to see if Dflts is using a custom subroutine to format SSN
                    var ldapContextSubroutine = await this.DataReader.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS");
                    bool customSubroutine = ldapContextSubroutine.DfltsSocIdSubr != "";
                    if (proxyDefaults == null)
                    {
                        // PROXY.DEFAULTS must exist for Proxy functionality to function properly
                        throw new ConfigurationException("Proxy setup not complete.");
                    }

                    config = new ProxyConfiguration(proxyDefaults.PrxdProxyEnabled.ToUpperInvariant() == "Y", proxyDefaults.PrxdDisclosureReleaseDoc, proxyDefaults.PrxdProxyEmailDocument, proxyDefaults.PrxdAddNewProxyAllowed.ToUpperInvariant() == "Y", customSubroutine, proxyAndUserPermissionsMapEntiies)
                    {
                        DisclosureReleaseText = proxyDefaults.PrxdDisclosureReleaseText,
                        AddProxyHeaderText = proxyDefaults.PrxdGrantAccessText,
                        ProxyFormHeaderText = proxyDefaults.PrxdHeaderText,
                        ReauthorizationText = proxyDefaults.PrxdReauthorizationText,
                        ProxyEmailAddressHierarchy = proxyDefaults.PrxdEmailHierarchy
                    };

                    var enabledWorkflows = await GetProxyWorkflowGroupsAsync();
                    if (enabledWorkflows != null)
                    {
                        enabledWorkflows.ForEach(w => config.AddWorkflowGroup(w));
                    }

                    if (proxyDefaults.PrxdRelationships != null && proxyDefaults.PrxdRelationships.Any())
                    {
                        proxyDefaults.PrxdRelationships.ForEach(r => config.AddRelationshipTypeCode(r));
                    }

                    if (proxyDefaults.PrxdDemographicsEntityAssociation != null && proxyDefaults.PrxdDemographicsEntityAssociation.Count > 0)
                    {
                        foreach (var field in proxyDefaults.PrxdDemographicsEntityAssociation)
                        {
                            if (field != null)
                            {
                                config.AddDemographicField(new DemographicField(field.PrxdDemoElementsAssocMember, field.PrxdDemoDescsAssocMember,
                                    ConvertStringToDemographicFieldRequirement(field.PrxdDemoRqmtAssocMember)));
                            }
                        }
                    }

                    return config;
                }, ProxyRepositoryCacheTimeout);
        }

        /// <summary>
        /// Posts a user's grants and revocations for proxy access.
        /// </summary>
        /// <param name="assignment">Proxy permission assignment object</param>
        /// <param name="useEmployeeGroups">Optional parameter used to differentiate between employee proxy and person proxy</param>
        public async Task<IEnumerable<ProxyAccessPermission>> PostUserProxyPermissionsAsync(ProxyPermissionAssignment assignment, bool useEmployeeGroups = false)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment", "PostUserProxyPermissionAsync requires an input object");
            }
            // If email address supplied in the DTO, first submit a request to update email address and type. A failure or error of any
            // type is logged but does not prevent further update.
            if (assignment.Permissions != null && assignment.Permissions.Count() > 0 && !string.IsNullOrEmpty(assignment.ProxyEmailAddress))
            {
                var emailUpdateRequest = new UpdateProxyEmailRequest();
                emailUpdateRequest.GrantorId = assignment.ProxySubjectId;
                emailUpdateRequest.RelatedPersonId = assignment.Permissions.ElementAt(0).ProxyUserId;
                emailUpdateRequest.EmailAddress = assignment.ProxyEmailAddress;
                emailUpdateRequest.EmailAddressType = assignment.ProxyEmailType;
                var emailUpdateResponse = new UpdateProxyEmailResponse();
                try
                {
                    emailUpdateResponse = await transactionInvoker.ExecuteAsync<UpdateProxyEmailRequest, UpdateProxyEmailResponse>(emailUpdateRequest);
                    if (!string.IsNullOrEmpty(emailUpdateResponse.Msg))
                    {
                        logger.Error(emailUpdateResponse.Msg);
                    }
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var message = "Error processing Colleague Transaction UPDATE.PROXY.EMAIL";
                    logger.Error(ex, message);
                }
            }

            var request = new CreateUpdateProxyAccessRequest();
            request.PrincipalIdentifier = assignment.ProxySubjectId;
            request.DisclosureDocumentText.AddRange(assignment.ProxySubjectApprovalDocumentText);
            foreach (var permission in assignment.Permissions)
            {
                ProxyPermissions proxyPermission;
                // Employee proxy specific changes
                if (useEmployeeGroups)
                {
                    proxyPermission = new ProxyPermissions()
                    {
                        ProxyIdentifiers = permission.ProxyUserId,
                        ProxyPermissionIdentifiers = permission.ProxyWorkflowCode,
                        ProxyPermissionGrantedIndicators = permission.IsGranted,
                        ProxyPermissionStartDates = DateTime.SpecifyKind(permission.StartDate, DateTimeKind.Unspecified),
                        ProxyPermissionEndDates = permission.EndDate.HasValue ? DateTime.SpecifyKind(permission.EndDate.Value, DateTimeKind.Unspecified) : permission.EndDate
                    };
                }
                else
                {
                    proxyPermission = new ProxyPermissions()
                    {
                        ProxyIdentifiers = permission.ProxyUserId,
                        ProxyPermissionIdentifiers = permission.ProxyWorkflowCode,
                        ProxyPermissionGrantedIndicators = permission.IsGranted
                    };
                }
                request.ProxyPermissions.Add(proxyPermission);
            }
            request.ReauthorizationIndicator = assignment.IsReauthorizing;

            var response = new CreateUpdateProxyAccessResponse();
            try
            {
                response = await transactionInvoker.ExecuteAsync<CreateUpdateProxyAccessRequest, CreateUpdateProxyAccessResponse>(request);

                if (response.ErrorIndicator)
                {
                    foreach (var msg in response.Messages)
                    {
                        logger.Error(msg);
                    }
                    var message = "Error returned by Colleague Transaction CREATE.UPDATE.PROXY.ACCESS.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                if (response.WarningIndicator)
                {
                    foreach (var msg in response.Messages)
                    {
                        logger.Warn(msg);
                    }
                }

                var data = await DataReader.BulkReadRecordAsync<ProxyAccess>(response.ProxyAccessIdentifiers.ToArray());
                if (data == null || data.Count != response.ProxyAccessIdentifiers.Count)
                {
                    var message = "Could not read all proxy access records.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                return BuildProxyPermissions(data, useEmployeeGroups);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Error processing Colleague Transaction CREATE.UPDATE.PROXY.ACCESS.";
                logger.Error(ex, message);
                throw new RepositoryException(message, ex);
            }
        }

        /// <summary>
        /// Posts demographic information and people possibly meeting that information, for resolution; as well as the permissions to grant to the resolved person
        /// </summary>
        /// <param name="candidate">A <see cref="ProxyCandidate"/> containing the information to post</param>
        /// <returns>The created <see cref="ProxyCandidate"/></returns>
        public async Task<ProxyCandidate> PostProxyCandidateAsync(ProxyCandidate candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException("candidate");
            }

            var request = BuildCreateProxyCandidateRequest(candidate);
            var response = new CreateProxyCandidateResponse();
            try
            {
                response = await transactionInvoker.ExecuteAsync<CreateProxyCandidateRequest, CreateProxyCandidateResponse>(request);

                if (response.ErrorOccurred)
                {
                    foreach (var msg in response.ErrorMessages)
                    {
                        logger.Error(msg);
                    }
                    var message = "Error returned by Colleague Transaction CREATE.PROXY.CANDIDATE.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                var data = await DataReader.ReadRecordAsync<ProxyCandidates>(response.ProxyCandidateId);
                if (data == null)
                {
                    var message = "Could not read ProxyCandidates record.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                return BuildProxyCandidate(data);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Error processing Colleague Transaction CREATE.PROXY.CANDIDATE.";
                logger.Error(ex, message);
                throw new RepositoryException(message, ex);
            }

        }

        /// <summary>
        /// Gets all the proxy candidates for a proxy user.
        /// </summary>
        /// <param name="grantorId">ID of the user granting access</param>
        /// <returns>
        /// A collection of proxy candidates
        /// </returns>
        public async Task<IEnumerable<Domain.Base.Entities.ProxyCandidate>> GetUserProxyCandidatesAsync(string grantorId)
        {
            if (string.IsNullOrEmpty(grantorId))
            {
                throw new ArgumentNullException("grantorId");
            }

            List<ProxyCandidate> proxyCandidates = new List<ProxyCandidate>();
            List<Domain.Base.Entities.ProxyAccessPermission> permissions = new List<Domain.Base.Entities.ProxyAccessPermission>();

            // Get all proxy.access records associated with current user and are unexpired
            // so that we can gather the list of proxy subjects who have granted 
            // proxy access that are valid as of today.
            string proxyAccessQuery = "WITH PRXC.PROXY.SUBJECT EQ '{0}'";
            var proxyCandidatesRecs = await this.DataReader.BulkReadRecordAsync<Data.Base.DataContracts.ProxyCandidates>(
                string.Format(proxyAccessQuery, grantorId));

            if (proxyCandidatesRecs == null || !proxyCandidatesRecs.Any())
            {
                // return empty proxySubjects collection
                return proxyCandidates;
            }

            foreach (var pc in proxyCandidatesRecs)
            {
                try
                {
                    proxyCandidates.Add(BuildProxyCandidate(pc));
                }
                catch (Exception ex)
                {
                    // Removed the logDataError which would spit out all the info for a candidate if data could not be retrieved.
                  
                    logger.Error("Unable to retrieve information for proxy candidate " + pc.Recordkey + ". " + ex.Message);
                }
            }

            return proxyCandidates;
        }

        /// <summary>
        /// Gets a collection of proxy access permissions, by user, for the supplied person
        /// </summary>
        /// <param name="id">The identifier of the entity of interest</param>
        /// <param name="useEmployeeGroups">Optional parameter used to differentiate between employee proxy and person proxy</param>
        /// <returns>A collection of proxy access permissions, by user, for the supplied person</returns>
        public async Task<IEnumerable<ProxyUser>> GetUserProxyPermissionsAsync(string id, bool useEmployeeGroups = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            List<ProxyUser> proxyUsers = new List<ProxyUser>();

            List<Domain.Base.Entities.ProxyAccessPermission> permissions = new List<Domain.Base.Entities.ProxyAccessPermission>();
            var data = await this.DataReader.BulkReadRecordAsync<Data.Base.DataContracts.ProxyAccess>("PROXY.ACCESS", string.Format("WITH PRAC.PRINCIPAL.WEB.USER = '{0}'", id));
            if (data == null || !data.Any())
            {
                logger.Error("User " + id + " has not currently granted permissions for proxy access to any users for any workflows.");
                return proxyUsers;
            }
            permissions = BuildProxyPermissions(data, useEmployeeGroups);
            var proxyUserIds = permissions.Select(p => p.ProxyUserId).Distinct().ToList();
            proxyUserIds.ForEach(userId => proxyUsers.Add(new ProxyUser(userId)));

            foreach (var user in proxyUsers)
            {
                var permissionsForUser = permissions.Where(p => p.ProxyUserId == user.Id).ToList();
                permissionsForUser.ForEach(pfu => user.AddPermission(pfu));
            }
            return proxyUsers;
        }

        /// <summary>
        /// Gets all the proxy subjects who have granted the proxy user permissions.
        /// </summary>
        /// <param name="proxyPersonId"></param>
        /// <returns>
        /// A collection of proxy subjects
        /// </returns>
        public async Task<IEnumerable<Domain.Base.Entities.ProxySubject>> GetUserProxySubjectsAsync(string proxyPersonId)
        {
            if (string.IsNullOrEmpty(proxyPersonId))
            {
                throw new ArgumentNullException("proxyPersonId");
            }

            List<ProxySubject> proxySubjects = new List<ProxySubject>();
            List<Domain.Base.Entities.ProxyAccessPermission> permissions = new List<Domain.Base.Entities.ProxyAccessPermission>();

            // Get all proxy.access records associated with current user and are unexpired
            // so that we can gather the list of proxy subjects who have granted 
            // proxy access that are valid as of today.
            string todaysDate = UniDataFormatter.UnidataFormatDate(DateTime.Today,
                InternationalParameters.HostShortDateFormat, InternationalParameters.HostDateDelimiter);
            string proxyAccessQuery = "WITH PRAC.PROXY.WEB.USER EQ '{0}' " +
                " AND (PRAC.BEGIN.DATE NE '' AND PRAC.BEGIN.DATE LE '{1}')" +
                " AND (PRAC.END.DATE EQ '' OR PRAC.END.DATE GT '{1}')";
            var proxyAccessRecs = await this.DataReader.BulkReadRecordAsync<Data.Base.DataContracts.ProxyAccess>(
                string.Format(proxyAccessQuery, proxyPersonId, todaysDate));

            if (proxyAccessRecs == null || !proxyAccessRecs.Any())
            {
                // return empty proxySubjects collection
                return proxySubjects;
            }

            foreach (var pa in proxyAccessRecs)
            {
                try
                {
                    permissions.Add(new Domain.Base.Entities.ProxyAccessPermission(
                        pa.Recordkey, pa.PracPrincipalWebUser, pa.PracProxyWebUser,
                        pa.PracProxyAccessPermission, pa.PracBeginDate.GetValueOrDefault(), pa.PracEndDate)
                    {
                        ApprovalEmailDocumentId = pa.PracProxyApprovalEmail,
                        DisclosureReleaseDocumentId = pa.PracDisclosureReleaseDoc,                       
                        ReauthorizationDate = pa.PracReauthReqDate
                    });
                }
                catch (Exception ex)
                {
                    LogDataError("PROXY.ACCESS", pa.Recordkey, pa, ex);
                }
            }
            var proxySubjectIds = permissions.Select(p => p.ProxySubjectId).Distinct().ToList();
            proxySubjectIds.ForEach(id => proxySubjects.Add(new ProxySubject(id)));

            foreach (var proxySubject in proxySubjects)
            {
                var permissionsByProxySubject = permissions.Where(p => p.ProxySubjectId == proxySubject.Id).ToList();
                permissionsByProxySubject.ForEach(perm => proxySubject.AddPermission(perm));
            }

            return proxySubjects;
        }

        #region Private methods
        private List<ProxyAccessPermission> BuildProxyPermissions(ICollection<ProxyAccess> proxyAccesses, bool useEmployeeGroups = false)
        {
            var permissions = new List<ProxyAccessPermission>();
            foreach (var pa in proxyAccesses)
            {
                try
                {
                    var permission = new ProxyAccessPermission(pa.Recordkey, pa.PracPrincipalWebUser, pa.PracProxyWebUser, pa.PracProxyAccessPermission, pa.PracBeginDate.GetValueOrDefault(), pa.PracEndDate, useEmployeeGroups)
                    {
                        ApprovalEmailDocumentId = pa.PracProxyApprovalEmail,
                        DisclosureReleaseDocumentId = pa.PracDisclosureReleaseDoc,
                        ReauthorizationDate = pa.PracReauthReqDate
                    };
                    permissions.Add(permission);
                }
                catch (Exception ex)
                {
                    LogDataError("PROXY.ACCESS", pa.Recordkey, pa, ex);
                }
            }

            return permissions;
        }

        private async Task<List<ProxyWorkflowGroup>> GetProxyWorkflowGroupsAsync()
        {
            var workflowGroups = new List<ProxyWorkflowGroup>();

            var proxyGroups = await this.DataReader.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "PROXY.ACCESS.GROUPS");
            if (proxyGroups == null)
            {
                throw new ConfigurationException("Proxy workflow group setup not complete.");
            }
            foreach (var group in proxyGroups.ValsEntityAssociation)
            {
                var groupWorkflows = new List<Domain.Base.Entities.ProxyWorkflow>();
                var proxyWorkflows = await this.DataReader.BulkReadRecordAsync<ProxyAccessPerms>("PROXY.ACCESS.PERMS", string.Format("WITH PRCP.GROUP = '{0}'", group.ValInternalCodeAssocMember));
                if (proxyWorkflows == null || proxyWorkflows.Count == 0)
                {
                    throw new ConfigurationException("Proxy workflow setup not complete for group " + group.ValInternalCodeAssocMember + ".");
                }
                var groupEntity = new ProxyWorkflowGroup(group.ValInternalCodeAssocMember, group.ValExternalRepresentationAssocMember, string.Equals(group.ValActionCode1AssocMember, EmployeeProxyGroupSpecialProcessingCode, StringComparison.InvariantCultureIgnoreCase));
                foreach (var workflow in proxyWorkflows)
                {
                    groupEntity.AddWorkflow(new ProxyWorkflow(workflow.Recordkey, workflow.PrcpDescription, workflow.PrcpGroup, workflow.PrcpEnabled.ToUpper() == "Y")
                    {
                        WorklistCategorySpecialProcessCode = workflow.PrcpWorklistCatSpecProc
                    });
                }
                workflowGroups.Add(groupEntity);
            }

            return workflowGroups;
        }

        private DemographicFieldRequirement ConvertStringToDemographicFieldRequirement(string dfr)
        {
            if (string.IsNullOrEmpty(dfr))
            {
                throw new ArgumentNullException("dfr", "A demographic field requirement must be specified.");
            }
            dfr = dfr.ToUpperInvariant();
            switch (dfr)
            {
                case "R":
                    return DemographicFieldRequirement.Required;
                case "O":
                    return DemographicFieldRequirement.Optional;
                case "N":
                    return DemographicFieldRequirement.Hidden;
                default:
                    throw new ApplicationException("Demographic field requirement is not a valid value.");
            }
        }

        private CreateProxyCandidateRequest BuildCreateProxyCandidateRequest(ProxyCandidate candidate)
        {
            return new CreateProxyCandidateRequest()
            {
                CandidateBirthDate = candidate.BirthDate,
                CandidateEmailAddress = candidate.EmailAddress,
                CandidateEmailType = candidate.EmailType,
                CandidateFirstName = candidate.FirstName,
                CandidateFormerFirstName = candidate.FormerFirstName,
                CandidateFormerLastName = candidate.FormerLastName,
                CandidateFormerMiddleName = candidate.FormerMiddleName,
                CandidateGender = candidate.Gender,
                CandidateGrantedPerms = candidate.GrantedPermissions.ToList(),
                CandidateLastName = candidate.LastName,
                CandidateMiddleName = candidate.MiddleName,
                CandidatePhone = candidate.Phone,
                CandidatePhoneExtension = candidate.PhoneExtension,
                CandidatePhoneType = candidate.PhoneType,
                CandidatePrefix = candidate.Prefix,
                CandidateProxySubject = candidate.ProxySubject,
                CandidateRelationType = candidate.RelationType,
                CandidateSsn = candidate.GovernmentId,
                CandidateSuffix = candidate.Suffix,
                PossibleDuplicates =
                    candidate.ProxyMatchResults.Select(x => new PossibleDuplicates()
                    {
                        CandidateDuplPersonId = x.PersonId,
                        CandidateDuplScore = x.MatchScore,
                        CandidateDuplCategory = x.MatchCategory == PersonMatchCategoryType.Definite ? "D" : "P",
                    }).ToList(),
            };
        }

        private ProxyCandidate BuildProxyCandidate(ProxyCandidates data)
        {
            if (data == null || data.ProxyMatchResultsEntityAssociation == null || !data.ProxyMatchResultsEntityAssociation.Any())
            {
                var msg = "Could not retrieve ProxyCandidate " + data.Recordkey;
                logger.Error(msg);
                throw new RepositoryException(msg);
            }
            var result = new ProxyCandidate(
                data.PrxcProxySubject,
                data.PrxcRelationType,
                data.PrxcGrantedPerms,
                data.PrxcFirstName,
                data.PrxcLastName,
                data.PrxcEmailAddress,
                data.ProxyMatchResultsEntityAssociation.Select(x => new PersonMatchResult(x.PrxcPersonIdAssocMember,
                    x.PrxcScoreAssocMember,
                    x.PrxcCategoryAssocMember)).ToList()
                );

            result.BirthDate = data.PrxcBirthDate;
            result.EmailType = data.PrxcEmailType;
            result.FormerFirstName = data.PrxcFormerFirstName;
            result.FormerLastName = data.PrxcFormerLastName;
            result.FormerMiddleName = data.PrxcFormerMiddleName;
            result.Gender = data.PrxcGender;
            result.MiddleName = data.PrxcMiddleName;
            result.Phone = data.PrxcPhone;
            result.PhoneExtension = data.PrxcPhoneExtension;
            result.PhoneType = data.PrxcPhoneType;
            result.Prefix = data.PrxcPrefix;
            result.Id = data.Recordkey;
            result.GovernmentId = data.PrxcSsn;
            result.Suffix = data.PrxcSuffix;

            return result;
        }

        private async Task<List<ProxyAndUserPermissionsMap>> GetProxyAndUserPermissionsMappingInfoAsync()
        {
            var proxyAndUserPermissionsMapEnties = new List<ProxyAndUserPermissionsMap>();
            var proxyAndUserPermissionsMapRecords = new Collection<MapProxyUserPerms>();
            var ProxyAndUserPermissionsMappingKeys = await DataReader.SelectAsync("MAP.PROXY.USER.PERMS", "");
            if (ProxyAndUserPermissionsMappingKeys != null && ProxyAndUserPermissionsMappingKeys.Length != 0)
            {
                proxyAndUserPermissionsMapRecords = await DataReader.BulkReadRecordAsync<MapProxyUserPerms>(ProxyAndUserPermissionsMappingKeys);
            }
            if (proxyAndUserPermissionsMapRecords.Any())
            {
                foreach (var proxyAndUserPermissionsMapRecord in proxyAndUserPermissionsMapRecords)
                {
                    proxyAndUserPermissionsMapEnties.Add(new ProxyAndUserPermissionsMap(proxyAndUserPermissionsMapRecord.Recordkey,
                        proxyAndUserPermissionsMapRecord.MpupPermission,
                        proxyAndUserPermissionsMapRecord.MpupProxyAccessPermission));
                }
            }

            return proxyAndUserPermissionsMapEnties;
        }
        #endregion
    }
}
